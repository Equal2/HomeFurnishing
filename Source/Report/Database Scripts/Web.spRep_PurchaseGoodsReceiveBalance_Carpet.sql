USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiveBalance_Carpet]    Script Date: 04/Aug/2015 12:02:13 PM ******/

IF OBJECT_ID ('[Web].[spRep_PurchaseGoodsReceiveBalance_Carpet]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_PurchaseGoodsReceiveBalance_Carpet]	
GO
/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiveBalance_Carpet]    Script Date: 04/Aug/2015 12:02:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [Web].[spRep_PurchaseGoodsReceiveBalance_Carpet]
    @StatusOnDate VARCHAR(20)=null,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,	
    @Supplier VARCHAR(Max) = NULL,    
    @ProductNature VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @PersonCustomGroup VARCHAR(Max) = NULL,
    @Product VARCHAR(Max) = NULL,      
    @PurchaseIndentHeaderId VARCHAR(Max) = NULL,	
	@PurchaseGoodReceiptHeaderId VARCHAR(Max) = NULL,
	@AreaOnUnit VARCHAR(10) = 'Sq.Feet'	
AS 
BEGIN
Declare @DocumentType VARCHAR(20) = NULL
SELECT 
H.ReceiveDate, H.ReceiveNo,Pp.Name AS SupplierName,P.ProductName, H.Specification, 
Pt.ProductTypeName,H.Qty,
CASE WHEN @AreaOnUnit = 'Sq.Yard' THEN IsNull(H.Qty,0) * Ra.SqYardPerPcs ELSE IsNull(H.Qty,0) * Ra.Area END AS TotalArea,
U.UnitName AS UnitName,VRS.StandardSizeName AS SizeName,AU.UnitName AS AreaUnit,
IsNull(U.DecimalPlaces,0) AS DecimalPlaces,H.SiteId AS SiteId, H.DivisionId AS DivisionId,
S.SiteCode AS SiteName, D.DivisionName AS DivisionName, 
'Carpet Purchase Goods Receive Balance ' AS ReportTitle, NULL AS SubReportProcList,'PurchaseGoodsReceiveBalance_Carpet.rdl' AS ReportName
FROM Web.FPurchaseGoodsReceiveBalance(@StatusOnDate,  @Site,  @Division , @FromDate,@ToDate , @DocumentType, @Supplier, @Product , @PurchaseGoodReceiptHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
LEFT JOIN Web._People Pp ON H.supplierId = Pp.PersonID 
LEFT JOIN Web.PurchaseIndentLines Pil ON H.PurchaseIndentLineId = Pil.PurchaseIndentLineId
LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
LEFT JOIN Web.ViewRugArea Ra WITH (nolock) ON P.ProductId = Ra.ProductId
LEFT JOIN Web.Units Au WITH (nolock) ON @AreaOnUnit = Au.UnitName
LEFT JOIN Web.ViewRugSize VRS WITH (nolock) ON VRS.ProductId = P.ProductId
WHERE 1=1 AND Pt.ProductTypeName='Rug'
AND ( @PurchaseIndentHeaderId IS NULL OR Pil.PurchaseIndentHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseIndentHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.supplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
ORDER BY H.ReceiveDate
end

GO


