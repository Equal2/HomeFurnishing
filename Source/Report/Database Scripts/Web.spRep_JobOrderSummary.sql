IF object_id ('[Web].[spRep_JobOrderSummary]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_JobOrderSummary]
GO 


CREATE PROCEDURE [Web].[spRep_JobOrderSummary]     
	@ReportOnDate VARCHAR(20) = 'Order Date',
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @JobWorker VARCHAR(Max) = NULL,
    @Process VARCHAR(Max) = NULL,
    @ProductNature VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @PersonCustomGroup VARCHAR(Max) = NULL,
    @Product VARCHAR(Max) = NULL,  
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,  
    @ProdOrderHeaderId VARCHAR(Max) = NULL,  
	@JobOrderHeaderId VARCHAR(Max) = NULL,
	@ReportType VARCHAR(Max) = 'Product Wise Summary'
AS
BEGIN

DECLARE @StatusOnDate NVARCHAR(20)

IF (@ReportOnDate = 'Order Date')
BEGIN
	SET @StatusOnDate = NULL
END 
ELSE
BEGIN
	SET @StatusOnDate = @ToDate
END 


SELECT PT.ProductTypeName,
 Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
			WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
			WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
			WHEN @ReportType ='Job Worker Wise Summary' THEN Pp.Name
			WHEN @ReportType ='Process Wise Summary' THEN PR.ProcessName 
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE P.ProductName
		END) AS GroupOnValue,
		
Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
			WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'
			WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Nature'
			WHEN @ReportType ='Job Worker Wise Summary' THEN 'Job Worker'
			WHEN @ReportType ='Process Wise Summary' THEN 'Process'
			WHEN @ReportType ='Month Wise Summary' THEN 'Month'
			ELSE 'Product'
		END) AS GroupTitle,
		
CASE WHEN Min(U.UnitName) =  Max(U.UnitName) THEN Max(U.UnitName) ELSE 'Mix' END AS UnitName,
IsNull(Sum(H.OrderQty),0) AS OrderQty, IsNull(Sum(H.CancelQty),0) AS CancelQty, 
IsNull(Sum(H.AmendmentQty),0) AS AmendmentQty, IsNull(Sum(H.Qty),0) AS Qty, 
IsNull(Sum(H.OrderAmount),0) AS OrderAmount, IsNull(Sum(H.CancelAmount),0) AS CancelAmount, 
IsNull(Sum(H.AmendmentAmount),0) AS AmendmentAmount,IsNull(Sum(H.Amount),0) AS Amount,
IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces, Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
Max(S.SiteName) AS SiteName, Max(D.DivisionName) AS DivisionName, 
'JobOrderSummary.rdl' AS ReportName, 'Job Order Summary' AS ReportTitle, NULL AS SubReportProcList
FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
LEFT JOIN Web.Products P WITH (NoLock) ON H.ProductId = P.ProductId
LEFT JOIN Web.Units U WITH (NoLock) ON P.UnitId = U.UnitId
LEFT JOIN Web.ProductGroups Pg WITH (NoLock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (NoLock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web._People Pp WITH (NoLock) ON H.JobWorkerId = Pp.PersonID 
LEFT JOIN Web.ProdOrderLines POL WITH (NoLock) ON H.ProdOrderLineId = POL.ProdOrderLineId
LEFT JOIN Web.Processes PR WITH (NoLock) ON H.ProcessId = PR.ProcessId
LEFT JOIN Web.Sites S WITH (NoLock) ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D WITH (NoLock) ON H.DivisionId = D.DivisionId
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
GROUP BY PT.ProductTypeName,
			CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
			WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
			WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
			WHEN @ReportType ='Job Worker Wise Summary' THEN Pp.Name
			WHEN @ReportType ='Process Wise Summary' THEN PR.ProcessName 
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE P.ProductName END
ORDER BY GroupOnValue
End
GO

