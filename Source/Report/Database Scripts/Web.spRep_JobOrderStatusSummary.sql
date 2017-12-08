IF object_id ('[Web].[spRep_JobOrderStatusSummary]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_JobOrderStatusSummary]
GO 


CREATE PROCEDURE [Web].[spRep_JobOrderStatusSummary]
	@StatusOnDate VARCHAR(20) = NULL,
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


DECLARE @LossQtySum INT 


SELECT @LossQtySum =  
( SELECT 
IsNull(Sum(VJobOrderStatus.LossQty),0) AS LossQty
FROM 
(
	SELECT Pp.Name AS JobWorkerName, P.ProductName, PR.ProcessName, U.UnitName, Pg.ProductGroupName, Pt.ProductTypeName, H.DocDate,
	H.OrderQty, H.AmendmentQty, H.CancelQty, IsNull(VReceive.ReceiveQty,0) AS ReceiveQty, IsNull(VReceive.LossQty,0) AS LossQty,
	H.OrderQty + H.AmendmentQty - H.CancelQty - IsNull(VReceive.ReceiveQty,0) -IsNull(VReceive.LossQty,0) AS BalanceQty,
	H.OrderAmount, H.AmendmentAmount, H.CancelAmount, IsNull(VReceive.ReceiveAmount,0) AS  ReceiveAmount,IsNull(VReceive.LossAmount,0) AS  LossAmount,
	H.OrderAmount + H.AmendmentAmount - H.CancelAmount - IsNull(VReceive.ReceiveAmount,0) - IsNull(VReceive.LossAmount,0)  AS BalanceAmount,
	IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
	H.SiteId AS SiteId, H.DivisionId AS DivisionId,
	S.SiteName AS SiteName, D.DivisionName AS DivisionName
	FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
	LEFT JOIN (
		SELECT L.JobOrderLineId, Sum(L.ReceiveQty) AS ReceiveQty, Sum(L.LossQty) AS LossQty, Sum(L.Amount) AS ReceiveAmount, Sum(L.LossAmount) AS LossAmount
		FROM Web.FJobReceive(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Product, DEFAULT) AS L 
		GROUP BY L.JobOrderLineId
	) AS VReceive ON H.JobOrderLineId = VReceive.JobOrderLineId
	LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
	LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
	LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
	LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
	LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonID 
	LEFT JOIN Web.ProdOrderLines Pil ON H.ProdOrderLineId = Pil.ProdOrderLineId
	LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
	LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
	LEFT JOIN web.Processes PR ON PR.ProcessId = H.ProcessId
	WHERE 1=1
	AND ( @ProdOrderHeaderId IS NULL OR Pil.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
	AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) AS VJobOrderStatus

)


SELECT Max(CASE WHEN @ReportType ='Product Wise Summary' THEN VJobOrderStatus.ProductName
			WHEN @ReportType ='Product Group Wise Summary' THEN VJobOrderStatus.ProductGroupName 
			WHEN @ReportType ='Product Type Wise Summary' THEN VJobOrderStatus.ProductTypeName
			WHEN @ReportType ='JobWorker Wise Summary' THEN VJobOrderStatus.JobWorkerName
			WHEN @ReportType ='Process Wise Summary' THEN VJobOrderStatus.ProcessName
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,VJobOrderStatus.DocDate,11),0,6)
			ELSE VJobOrderStatus.ProductName
		END) AS GroupOnValue,
Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
			WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'
			WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Type'
			WHEN @ReportType ='JobWorker Wise Summary' THEN 'JobWorker'
			WHEN @ReportType ='Process Wise Summary' THEN 'Process'
			WHEN @ReportType ='Month Wise Summary' THEN 'Month'
			ELSE 'Product'
END) AS GroupTitle,
		
CASE WHEN Min(VJobOrderStatus.UnitName) =  Max(VJobOrderStatus.UnitName) 
	  THEN Max(VJobOrderStatus.UnitName) ELSE 'Mix' END AS UnitName,
IsNull(Sum(VJobOrderStatus.OrderQty),0) AS OrderQty, 
IsNull(Sum(VJobOrderStatus.CancelQty),0) AS CancelQty, 
IsNull(Sum(VJobOrderStatus.AmendmentQty),0) AS AmendmentQty, 
IsNull(Sum(VJobOrderStatus.ReceiveQty),0) AS ReceiveQty, 
IsNull(Sum(VJobOrderStatus.LossQty),0) AS LossQty, 
IsNull(Sum(VJobOrderStatus.BalanceQty),0) AS BalanceQty, 
IsNull(Sum(VJobOrderStatus.OrderAmount),0) AS OrderAmount, 
IsNull(Sum(VJobOrderStatus.CancelAmount),0) AS CancelAmount, 
IsNull(Sum(VJobOrderStatus.AmendmentAmount),0) AS AmendmentAmount,
IsNull(Sum(VJobOrderStatus.ReceiveAmount),0) AS ReceiveAmount,
IsNull(Sum(VJobOrderStatus.LossAmount),0) AS LossAmount,
IsNull(Sum(VJobOrderStatus.BalanceAmount),0) AS BalanceAmount,
IsNull(Max(VJobOrderStatus.DecimalPlaces),0) AS DecimalPlaces,
Max(VJobOrderStatus.SiteId) AS SiteId, Max(VJobOrderStatus.DivisionId) AS DivisionId,
Max(VJobOrderStatus.SiteName) AS SiteName, Max(VJobOrderStatus.DivisionName) AS DivisionName, 
@ReportType AS ReportType, 
'Job Order Status Summary' AS ReportTitle,
CASE WHEN @LossQtySum > 0 THEN 'JobOrderStatusSummary_WithLoss.rdl' ELSE 'JobOrderStatusSummary.rdl' END AS ReportName, NULL AS SubReportProcList
FROM 
(
	SELECT Pp.Name AS JobWorkerName, P.ProductName, PR.ProcessName, U.UnitName, Pg.ProductGroupName, Pt.ProductTypeName, H.DocDate,
	H.OrderQty, H.AmendmentQty, H.CancelQty, IsNull(VReceive.ReceiveQty,0) AS ReceiveQty, IsNull(VReceive.LossQty,0) AS LossQty,
	H.OrderQty + H.AmendmentQty - H.CancelQty - IsNull(VReceive.ReceiveQty,0) -IsNull(VReceive.LossQty,0) AS BalanceQty,
	H.OrderAmount, H.AmendmentAmount, H.CancelAmount, IsNull(VReceive.ReceiveAmount,0) AS  ReceiveAmount,IsNull(VReceive.LossAmount,0) AS  LossAmount,
	H.OrderAmount + H.AmendmentAmount - H.CancelAmount - IsNull(VReceive.ReceiveAmount,0) - IsNull(VReceive.LossAmount,0)  AS BalanceAmount,
	IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
	H.SiteId AS SiteId, H.DivisionId AS DivisionId,
	S.SiteName AS SiteName, D.DivisionName AS DivisionName
	FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
	LEFT JOIN (
		SELECT L.JobOrderLineId, Sum(L.ReceiveQty) AS ReceiveQty, Sum(L.LossQty) AS LossQty, Sum(L.ReceiveAmount) AS ReceiveAmount, Sum(L.LossAmount) AS LossAmount
		FROM Web.FJobReceive(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Product, DEFAULT) AS L 
		GROUP BY L.JobOrderLineId
	) AS VReceive ON H.JobOrderLineId = VReceive.JobOrderLineId
	LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
	LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
	LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
	LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
	LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonID 
	LEFT JOIN Web.ProdOrderLines Pil ON H.ProdOrderLineId = Pil.ProdOrderLineId
	LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
	LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
	LEFT JOIN web.Processes PR ON PR.ProcessId = H.ProcessId
	WHERE 1=1
	AND ( @ProdOrderHeaderId IS NULL OR Pil.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
	AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) AS VJobOrderStatus
GROUP BY CASE WHEN @ReportType ='Product Wise Summary' THEN VJobOrderStatus.ProductName
			WHEN @ReportType ='Product Group Wise Summary' THEN VJobOrderStatus.ProductGroupName 
			WHEN @ReportType ='Product Type Wise Summary' THEN VJobOrderStatus.ProductTypeName
			WHEN @ReportType ='JobWorker Wise Summary' THEN VJobOrderStatus.JobWorkerName
			WHEN @ReportType ='Process Wise Summary' THEN VJobOrderStatus.ProcessName
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,VJobOrderStatus.DocDate,11),0,6)
			ELSE VJobOrderStatus.ProductName END
ORDER BY GroupOnValue
End
GO

