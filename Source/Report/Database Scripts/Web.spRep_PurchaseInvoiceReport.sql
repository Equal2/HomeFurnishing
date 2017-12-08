USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_PurchaseInvoiceReport]    Script Date: 7/7/2015 12:04:05 PM ******/
IF OBJECT_ID ('[Web].[spRep_PurchaseInvoiceReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_PurchaseInvoiceReport]
GO
/****** Object:  StoredProcedure [Web].[spRep_PurchaseInvoiceReport]    Script Date: 7/7/2015 12:04:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create  procedure   [Web].[spRep_PurchaseInvoiceReport]
    @ReportType VARCHAR(Max) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @Supplier VARCHAR(Max) = NULL,
    @Currency VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,      
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,        
	@PurchaseOrderHeaderId VARCHAR(Max) = NULL,
	@PurchaseGoodsReceiptHeaderId VARCHAR(Max) = NULL,
	@PurchaseInvoiceHeaderId VARCHAR(Max) = NULL	   
AS
Begin

DECLARE @CompCurrency NVARCHAR(100);

SELECT TOP 1 @CompCurrency= Cr.Name FROM Web.Companies C LEFT JOIN web.Currencies Cr ON c.CurrencyId = Cr.ID  ;


DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM [Web]._PurchaseInvoiceHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM [dbo].[Split] (@Currency, ','))) 
AND ( @PurchaseInvoiceHeaderId is null or H.PurchaseInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseInvoiceHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._PurchaseInvoiceLines L ON L.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId
LEFT JOIN web.PurchaseGoodsReceiptLines PGRL ON PGRL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId 
LEFT JOIN web.PurchaseOrderLines POL ON POL.PurchaseOrderLineId = PGRL.PurchaseOrderLineId 
LEFT JOIN [Web].Products P ON P.ProductId = PGRL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND PGRL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or PGRL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or PGRL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or PGRL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @PurchaseGoodsReceiptHeaderId is null or PGRL.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseGoodsReceiptHeaderId, ','))) 
AND ( @PurchaseOrderHeaderId is null or POL.PurchaseOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
AND PGRL.Dimension1Id IS NOT NULL


SELECT @Dimension2IdCnt = Count(*)
FROM  
( 
SELECT * FROM [Web]._PurchaseInvoiceHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM [dbo].[Split] (@Currency, ','))) 
AND ( @PurchaseInvoiceHeaderId is null or H.PurchaseInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseInvoiceHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._PurchaseInvoiceLines L ON L.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId
LEFT JOIN web.PurchaseGoodsReceiptLines PGRL ON PGRL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId 
LEFT JOIN web.PurchaseOrderLines POL ON POL.PurchaseOrderLineId = PGRL.PurchaseOrderLineId 
LEFT JOIN [Web].Products P ON P.ProductId = PGRL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND PGRL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or PGRL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or PGRL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or PGRL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @PurchaseGoodsReceiptHeaderId is null or PGRL.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseGoodsReceiptHeaderId, ','))) 
AND ( @PurchaseOrderHeaderId is null or POL.PurchaseOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
AND PGRL.Dimension2Id IS NOT NULL



SELECT H.SiteId, H.DivisionId, H.PurchaseInvoiceHeaderId, DT.DocumentTypeName, H.DocDate, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo, H.SupplierDocNo, H.SupplierDocDate, H.Remark,
PS.Name AS SupplierName, DTGR.DocumentTypeShortName + '-' + PGRH.DocNo AS GoodsReciptNo,
DTPO.DocumentTypeShortName + '-' + POH.DocNo AS OrderNo,
@CompCurrency AS Currency,Pt.ProductTypeName,
H.CreditDays, H.TermsAndConditions,   
L.PurchaseInvoiceLineId, P.ProductName, PGRL.Qty, PGRL.Specification, PGRL.DealQty AS DealQty,
L.Rate, L.Amount*C.BaseCurrencyRate AS Amount, L.Remark AS LineRemark, 
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 4 AS DeliveryUnitDecimalPlace,
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
PGRL.LotNo, 
CASE WHEN isnull(L.Amount,0) <> 0 THEN L.Amount*C.BaseCurrencyRate/PGRL.Qty ELSE 0 END AS RatePerPcs,

CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
	 
CASE WHEN @ReportType = '1' THEN 'PurchaseInvoiceDetail.rdl' ELSE 
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'PurchaseInvoiceProductWiseDetailWithDoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'PurchaseInvoiceProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'PurchaseInvoiceProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'PurchaseInvoiceProductWiseDetail.rdl'
	END 
END AS ReportName, 

'Purchase Invoice Report' AS ReportTitle
FROM  
( 
SELECT * FROM [Web]._PurchaseInvoiceHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM [dbo].[Split] (@Currency, ','))) 
AND ( @PurchaseInvoiceHeaderId is null or H.PurchaseInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseInvoiceHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._PurchaseInvoiceLines L WITH (nolock) ON L.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.SupplierId  
LEFT JOIN web.PurchaseGoodsReceiptLines PGRL WITH (nolock) ON PGRL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId 
LEFT JOIN web.PurchaseGoodsReceiptHeaders PGRH WITH (nolock) ON PGRH.PurchaseGoodsReceiptHeaderId = PGRL.PurchaseGoodsReceiptHeaderId
LEFT JOIN [Web].DocumentTypes DTGR WITH (nolock) ON DTGR.DocumentTypeId = PGRH.DocTypeId 
LEFT JOIN web.PurchaseOrderLines POL WITH (nolock) ON POL.PurchaseOrderLineId = PGRL.PurchaseOrderLineId 
LEFT JOIN web.PurchaseOrderHeaders POH WITH (nolock) ON POH.PurchaseOrderHeaderId = POL.PurchaseOrderHeaderId 
LEFT JOIN [Web].DocumentTypes DTPO WITH (nolock) ON DTPO.DocumentTypeId = POH.DocTypeId 
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = PGRL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId  
LEFT JOIN [Web].Currencies C WITH (nolock) ON C.ID = H.CurrencyId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = PGRL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = PGRL.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
WHERE 1=1 AND PGRL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or PGRL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or PGRL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or PGRL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @PurchaseGoodsReceiptHeaderId is null or PGRL.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseGoodsReceiptHeaderId, ','))) 
AND ( @PurchaseOrderHeaderId is null or POL.PurchaseOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseOrderHeaderId, ','))) 
End
GO


