USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiveBalance]    Script Date: 04/Aug/2015 12:07:37 PM ******/

IF OBJECT_ID ('[Web].[spRep_PurchaseGoodsReceiveBalance]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_PurchaseGoodsReceiveBalance]	
GO

/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiveBalance]    Script Date: 04/Aug/2015 12:07:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[spRep_PurchaseGoodsReceiveBalance]
   @StatusOnDate VARCHAR(20)=null,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @Supplier VARCHAR(Max) = NULL, 	   
    @ProductNature VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @PersonCustomGroup VARCHAR(Max) = NULL,
    @Product VARCHAR(Max) = NULL,  
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,  
    @PurchaseIndentHeaderId VARCHAR(Max) = NULL,	
	@PurchaseGoodReceiptHeaderId VARCHAR(Max) = NULL
as
begin 
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


/*
IF (@ReportOnDate = 'Receive Date')
BEGIN
	SET @StatusOnDate = NULL
END 
ELSE
BEGIN
	SET @StatusOnDate = @ToDate
END 
*/



SELECT @Dimension1IdCnt = Count(*)
FROM Web.FPurchaseGoodsReceiveBalance(@StatusOnDate,  @Site,  @Division , @FromDate,@ToDate , @DocumentType, @Supplier, @Product , @PurchaseGoodReceiptHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.PurchaseIndentLines Pil ON H.PurchaseIndentLineId = Pil.PurchaseIndentLineId
WHERE 1=1
AND ( @PurchaseIndentHeaderId IS NULL OR Pil.PurchaseIndentHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseIndentHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.supplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND H.Dimension1Id IS NOT NULL


SELECT @Dimension2IdCnt = Count(*)
FROM Web.FPurchaseGoodsReceiveBalance(@StatusOnDate,  @Site,  @Division , @FromDate,@ToDate , @DocumentType, @Supplier, @Product , @PurchaseGoodReceiptHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.PurchaseIndentLines Pil ON H.PurchaseIndentLineId = Pil.PurchaseIndentLineId
WHERE 1=1
AND ( @PurchaseIndentHeaderId IS NULL OR Pil.PurchaseIndentHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseIndentHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.supplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND H.Dimension2Id IS NOT NULL


SELECT H.ReceiveDate, H.ReceiveNo, Pp.Name AS SupplierName, P.ProductName, H.Specification, 
Pt.ProductTypeName,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
H.Qty,
U.UnitName AS UnitName,
IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
H.SiteId AS SiteId, H.DivisionId AS DivisionId,
S.SiteName AS SiteName, D.DivisionName AS DivisionName, 
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'PurchaseGoodsReceiveBalanceWithDoubleDimension.rdl'
	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'PurchaseGoodsReceiveBalanceWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'PurchaseGoodsReceiveBalanceWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'PurchaseGoodsReceiveBalance.rdl'
END AS ReportName, 
'Purchase Goods Receive Balance' AS ReportTitle, NULL AS SubReportProcList
FROM Web.FPurchaseGoodsReceiveBalance(@StatusOnDate,  @Site,  @Division , @FromDate,@ToDate , @DocumentType, @Supplier, @Product , @PurchaseGoodReceiptHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1Types Dt1 ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
LEFT JOIN Web._People Pp ON H.supplierId = Pp.PersonID 
LEFT JOIN Web.PurchaseIndentLines Pil ON H.PurchaseIndentLineId = Pil.PurchaseIndentLineId
LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
LEFT JOIN Web.Dimension1 D1 ON H.Dimension1Id = D1.Dimension1Id
LEFT JOIN Web.Dimension2 D2 ON H.Dimension2Id = D2.Dimension2Id
WHERE 1=1
AND ( @PurchaseIndentHeaderId IS NULL OR Pil.PurchaseIndentHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseIndentHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.supplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND ( @Dimension1 is null or H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ',')))
ORDER BY H.ReceiveDate


end
GO


