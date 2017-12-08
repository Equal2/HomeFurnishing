IF object_id ('[Web].[spRep_ProdOrderStatus]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_ProdOrderStatus]
GO 


CREATE PROCEDURE [Web].[spRep_ProdOrderStatus]
	@StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	    
    @Process VARCHAR(Max) = NULL,
    @ProductNature VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,  
    @Product VARCHAR(Max) = NULL,  
	@ProdOrderHeaderId VARCHAR(Max) = NULL
AS 
BEGIN 


--- *************** Check Conditions For ReportName *********************** ----
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 
DECLARE @ReportName NVARCHAR(100) 




SELECT @Dimension1IdCnt = Count(*)
FROM Web.FProdOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process,@Product, @ProdOrderHeaderId) AS H 
LEFT JOIN Web.Products P WITH (Nolock) ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg WITH (Nolock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (Nolock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines POL WITH (Nolock) ON H.ProdOrderLineId = POL.ProdOrderLineId
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND H.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt = Count(*)
FROM Web.FProdOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process,@Product, @ProdOrderHeaderId) AS H 
LEFT JOIN Web.Products P WITH (Nolock) ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg WITH (Nolock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (Nolock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines POL WITH (Nolock) ON H.ProdOrderLineId = POL.ProdOrderLineId
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND H.Dimension2Id IS NOT NULL




SET @ReportName =
(
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'ProdOrderStatusWithDoubleDimension.rdl'
	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'ProdOrderStatusWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'ProdOrderStatusWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'ProdOrderStatus.rdl'
END 
)







--- *************** Main Qry *********************** ----

SELECT H.DocDate AS OrderDate, H.DocNo AS OrderNo, PR.ProcessName, PR.ProcessSr, H.ProdOrderLineId,
CASE WHEN H.OrderQty + H.AmendmentQty - H.CancelQty - IsNull(VGoodsReceipt.Qty,0) > 0 
	 THEN DateDiff(Day,H.DueDate,GetDate())
	 ELSE DateDiff(Day,H.DueDate,IsNull(VGoodsReceipt.LastOrderDate,H.DueDate)) END AS OverDueDays,
P.ProductName, U.UnitName, H.Specification, Pt.ProductTypeName,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
H.OrderQty, H.AmendmentQty, H.CancelQty, IsNull(VGoodsReceipt.Qty,0) AS ReceiveQty, 
H.OrderQty + H.AmendmentQty - H.CancelQty - IsNull(VGoodsReceipt.Qty,0)   AS BalanceQty,
IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
H.SiteId AS SiteId, H.DivisionId AS DivisionId,
S.SiteName AS SiteName, D.DivisionName AS DivisionName, 
@ReportName ReportName, 'Production Order Status' AS ReportTitle, NULL AS SubReportProcList
FROM Web.FProdOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process,@Product, @ProdOrderHeaderId) AS H 
LEFT JOIN 
(
	SELECT L.ProdOrderLineId, Sum(L.Qty) AS Qty, Max(L.DocDate) AS LastOrderDate
	FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, NULL , NULL ,@Process, @Product, NULL ) L
	GROUP BY L.ProdOrderLineId
) AS VGoodsReceipt ON H.ProdOrderLineId = VGoodsReceipt.ProdOrderLineId
LEFT JOIN Web.Products P WITH (Nolock) ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg WITH (Nolock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (Nolock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1 D1 WITH (Nolock) ON H.Dimension1Id = D1.Dimension1Id
LEFT JOIN Web.Dimension2 D2 WITH (Nolock) ON H.Dimension2Id = D2.Dimension2Id
LEFT JOIN Web.Dimension1Types Dt1 WITH (Nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (Nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN Web.Units U WITH (Nolock) ON P.UnitId = U.UnitId
LEFT JOIN Web.Sites S WITH (Nolock) ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D WITH (Nolock) ON H.DivisionId = D.DivisionId
LEFT JOIN web.Processes PR WITH (Nolock) ON PR.ProcessId = H.ProcessId
WHERE 1=1
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 IS NULL OR H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 IS NULL OR H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
ORDER BY H.DocDate
End
GO

