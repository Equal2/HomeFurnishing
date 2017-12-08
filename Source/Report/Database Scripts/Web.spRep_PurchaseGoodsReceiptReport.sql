USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiptReport]    Script Date: 7/7/2015 11:59:52 AM ******/

IF OBJECT_ID ('[Web].[spRep_PurchaseGoodsReceiptReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_PurchaseGoodsReceiptReport]	
GO
/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiptReport]    Script Date: 7/7/2015 11:59:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create  procedure   [Web].[spRep_PurchaseGoodsReceiptReport]    
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @Supplier VARCHAR(Max) = NULL,   
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,        
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,        
	@PurchaseOrderHeaderId VARCHAR(Max) = NULL,
	@PurchaseGoodsReceiptHeaderId VARCHAR(Max) = NULL		   
AS
Begin



DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM [Web]._PurchaseGoodsReceiptHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
AND ( @PurchaseGoodsReceiptHeaderId is null or H.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._PurchaseGoodsReceiptLines L WITH (nolock) ON L.PurchaseGoodsReceiptHeaderId = H.PurchaseGoodsReceiptHeaderId 
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN web.PurchaseOrderLines POL WITH (nolock) ON POL.PurchaseOrderLineId = L.PurchaseOrderLineId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @PurchaseOrderHeaderId is null or POL.PurchaseOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
AND L.Dimension1Id IS NOT NULL


SELECT @Dimension2IdCnt = Count(*)
FROM  
( 
SELECT * FROM [Web]._PurchaseGoodsReceiptHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
AND ( @PurchaseGoodsReceiptHeaderId is null or H.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._PurchaseGoodsReceiptLines L WITH (nolock) ON L.PurchaseGoodsReceiptHeaderId = H.PurchaseGoodsReceiptHeaderId 
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN web.PurchaseOrderLines POL ON POL.PurchaseOrderLineId = L.PurchaseOrderLineId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @PurchaseOrderHeaderId is null or POL.PurchaseOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
AND L.Dimension2Id IS NOT NULL



SELECT H.SiteId, H.DivisionId, H.PurchaseGoodsReceiptHeaderId, DT.DocumentTypeName, H.DocDate, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo, H.SupplierDocNo, H.SupplierDocDate , H.Remark,
PS.Name AS SupplierName,  DTPI.DocumentTypeShortName + '-' + POH.DocNo AS OrderNo,
Pt.ProductTypeName,   L.PurchaseGoodsReceiptLineId,  L.PurchaseOrderLineId, P.ProductName, L.Qty,
L.Specification, L.Remark AS LineRemark, 
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
L.LotNo, 
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	 

	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'PurchaseGoodsReceiptProductWiseDetailWithDoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'PurchaseGoodsReceiptProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'PurchaseGoodsReceiptProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'PurchaseGoodsReceiptProductWiseDetail.rdl'
	END AS ReportName, 
'Purchase Goods Receipt Report' AS ReportTitle
FROM  
( 
SELECT * FROM [Web]._PurchaseGoodsReceiptHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
AND ( @PurchaseGoodsReceiptHeaderId is null or H.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseGoodsReceiptHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web]._PurchaseGoodsReceiptLines L WITH (nolock) ON L.PurchaseGoodsReceiptHeaderId = H.PurchaseGoodsReceiptHeaderId 
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.SupplierId  
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = L.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = L.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN Web.PurchaseOrderLines POl WITH (nolock) ON L.PurchaseOrderLineId = POL.PurchaseOrderLineId
LEFT JOIN Web.PurchaseOrderHeaders POH WITH (nolock) ON POL.PurchaseOrderHeaderId = POH.PurchaseOrderHeaderId
LEFT JOIN [Web].DocumentTypes DTPI WITH (nolock) ON DTPI.DocumentTypeId = POH.DocTypeId 
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @PurchaseOrderHeaderId is null or POL.PurchaseOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
End
GO


