USE [RUG]
GO

/****** Object:  UserDefinedFunction [Web].[FPurchaseGoodsReceiveBalance]    Script Date: 04/Aug/2015 12:04:02 PM ******/

IF OBJECT_ID ('[Web].[FPurchaseGoodsReceiveBalance]') IS NOT NULL
	DROP FUNCTION [Web].[FPurchaseGoodsReceiveBalance]	
GO
/****** Object:  UserDefinedFunction [Web].[FPurchaseGoodsReceiveBalance]    Script Date: 04/Aug/2015 12:04:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE 	FUNCTION [Web].[FPurchaseGoodsReceiveBalance]
(
  @StatusOnDate VARCHAR(20) = NULL,
  @Site VARCHAR(20) = NULL,
  @Division VARCHAR(20) = NULL,
  @FromDate VARCHAR(20) = NULL,
  @ToDate VARCHAR(20) = NULL,
  @DocumentType VARCHAR(20) = NULL,	
  @Supplier VARCHAR(Max) = NULL,
  @Product VARCHAR(Max) = NULL,
  @PurchaseGoodReceiptHeaderId VARCHAR(Max) = NULL
)
RETURNS TABLE
AS 

RETURN 

SELECT 
Max(VPurchaseGoodreceipt.PurchaseGoodsReceiptHeaderId) AS PurchaseGoodsReceiptHeaderId,
VPurchaseGoodreceipt.PurchaseGoodsReceiptLineId AS PurchRecLineId,
isnull(sum(VPurchaseGoodreceipt.Qty),0) AS  Qty,
Max(VPurchaseGoodreceipt.DocTypeId) AS DocTypeId,
Max(VPurchaseGoodreceipt.DocumentTypeShortName) AS documenttypeshortName,
Max(VPurchaseGoodreceipt.PurchaseReceiveNo) AS ReceiveNo,
 Max(VPurchaseGoodreceipt.ProductId) AS  ProductId,
  Max(VPurchaseGoodreceipt.Dimension1Id) AS Dimension1Id,
  Max(VPurchaseGoodreceipt.Specification) AS Specification, 
  Max(VPurchaseGoodreceipt.Dimension2Id) AS Dimension2Id,
  Max(VPurchaseGoodreceipt.SupplierId) AS supplierId,
  Max(VPurchaseGoodreceipt.DocDate) AS ReceiveDate,
  Max(VPurchaseGoodreceipt.DivisionId) AS DivisionId,
  Max(VPurchaseGoodreceipt.SiteId) AS SiteId,
  max(VPurchaseGoodreceipt.PurchaseIndentLineId) AS PurchaseIndentLineId
FROM 
(
SELECT L.PurchaseGoodsReceiptLineId,L.Qty, H.PurchaseGoodsReceiptHeaderId, H.DocTypeId, Dt.DocumentTypeShortName, Dt.DocumentTypeShortName + '-' +  H.DocNo AS PurchaseReceiveNo,  L.ProductId, L.Dimension1Id, L.Specification, L.Dimension2Id, H.SupplierId AS SupplierId, H.DocDate, H.DivisionId, H.SiteId,L.PurchaseIndentLineId 
  FROM
(
       SELECT H.*
		FROM Web._PurchaseGoodsReceiptHeaders H WITH (NoLock)
		WHERE 1=1    
		AND ( @PurchaseGoodReceiptHeaderId IS NULL OR H.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM  Web.[Split] (@PurchaseGoodReceiptHeaderId, ',')))     
		AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
		AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)     
		AND ( @DocumentType IS NULL OR H.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
		AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
		AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))     
		AND ( @Supplier IS NULL OR H.SupplierId IN (SELECT Items FROM  Web.[Split] (@Supplier, ','))) 
)AS H
LEFT JOIN Web._PurchaseGoodsReceiptLines L ON L.PurchaseGoodsReceiptHeaderId = H.PurchaseGoodsReceiptHeaderId
LEFT JOIN Web.DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId 

UNION ALL  

SELECT L.PurchaseGoodsReceiptLineId,- L.Qty,NULL AS PurchaseGoodsReceiptHeaderId,NULL AS DocTypeId,NULL AS DocumentTypeShortName,NULL AS  PurchaseReceiveNo,NULL AS ProductId,NULL AS Dimension1Id,NULL AS Specification,NULL AS Dimension2Id,NULL AS SupplierId,NULL AS DocDate,NULL AS DivisionId,NULL AS SiteId,NULL AS PurchaseIndentLineId 
FROM Web._PurchaseGoodsReturnLines L
LEFT JOIN web._PurchaseGoodsReturnHeaders H ON L.PurchaseGoodsReturnHeaderId=H.PurchaseGoodsReturnHeaderId
WHERE 1=1 
AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)     

UNION ALL

SELECT L.PurchaseGoodsReceiptLineId,- Gr.Qty,NULL AS PurchaseGoodsReceiptHeaderId,NULL AS DocTypeId,NULL AS DocumentTypeShortName,NULL AS  PurchaseReceiveNo,NULL AS ProductId,NULL AS Dimension1Id,NULL AS Specification,NULL AS Dimension2Id,NULL AS SupplierId,NULL AS DocDate,NULL AS DivisionId,NULL AS SiteId,NULL AS PurchaseIndentLineId  
FROM Web._PurchaseInvoiceLines L
LEFT JOIN Web._PurchaseGoodsReceiptLines Gr ON L.PurchaseGoodsReceiptLineId = Gr.PurchaseGoodsReceiptLineId
LEFT JOIN web._PurchaseInvoiceheaders H ON L.PurchaseInvoiceHeaderId=H.PurchaseInvoiceHeaderId
AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)
) AS  VPurchaseGoodreceipt
WHERE 1=1
AND VPurchaseGoodreceipt.PurchaseGoodsReceiptLineId IS NOT NULL
AND ( @Product IS NULL OR ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ',')))
GROUP BY VPurchaseGoodreceipt.PurchaseGoodsReceiptLineId
HAVING IsNull(Sum(VPurchaseGoodreceipt.Qty),0) > 0
GO


