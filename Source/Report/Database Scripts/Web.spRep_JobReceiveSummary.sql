IF object_id ('[Web].[spRep_JobReceiveSummary]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_JobReceiveSummary]
GO 


CREATE PROCEDURE [Web].[spRep_JobReceiveSummary]     
	@ReportOnDate VARCHAR(20) = 'Receive Date',
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
    @JobOrderHeaderId VARCHAR(Max) = NULL,  
	@JobReceiveHeaderId VARCHAR(Max) = NULL,
	@ReportType VARCHAR(Max) = 'Product Wise Summary'
AS
BEGIN

DECLARE @StatusOnDate NVARCHAR(20)

IF (@ReportOnDate = 'Receive Date')
BEGIN
	SET @StatusOnDate = NULL
END 
ELSE
BEGIN
	SET @StatusOnDate = @ToDate
END 


DECLARE @LossQtySum INT 


SELECT @LossQtySum =  
( SELECT IsNull(Sum(H.LossQty),0) AS LossQty
FROM Web.FJobReceive(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process, @JobWorker, @JobReceiveHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonID 
LEFT JOIN Web.JobReceiveLines JRL ON H.JobReceiveLineId = JRL.JobReceiveLineId 
LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
LEFT JOIN web.Processes PR ON PR.ProcessId = H.ProcessId
LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
WHERE 1=1 --Pg.ProductTypeId = 1
AND ( @JobReceiveHeaderId IS NULL OR JRL.JobReceiveHeaderId  IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeaderId, ','))) 
AND ( @Product IS NULL OR P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
)


SELECT PT.ProductTypeName,
Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
			WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
			WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
			WHEN @ReportType ='JobWorker Wise Summary' THEN Pp.Name
			WHEN @ReportType ='Process Wise Summary' THEN PR.ProcessName 
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE P.ProductName
		END) AS GroupOnValue,
		
Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
			WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'
			WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Nature'
			WHEN @ReportType ='JobWorker Wise Summary' THEN 'JobWorker'
			WHEN @ReportType ='Process Wise Summary' THEN 'Process'
			WHEN @ReportType ='Month Wise Summary' THEN 'Month'
			ELSE 'Product'
		END) AS GroupTitle,

		
CASE WHEN Min(U.UnitName) =  Max(U.UnitName) THEN Max(U.UnitName) ELSE 'Mix' END AS UnitName,
IsNull(Sum(H.ReceiveQty),0) AS ReceiveQty, IsNull(Sum(H.LossQty),0) AS LossQty,  IsNull(Sum(H.ReturnQty),0) AS ReturnQty, 
IsNull(Sum(H.Qty),0) AS Qty, 
IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces,
Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
Max(S.SiteName) AS SiteName, Max(D.DivisionName) AS DivisionName, 
CASE WHEN @LossQtySum > 0 THEN 'JobReceiveSummary_WithLoss.rdl' ELSE 'JobReceiveSummary.rdl' END AS ReportName, 'Job Receive Summary' AS ReportTitle, NULL AS SubReportProcList
FROM Web.FJobReceive(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process, @JobWorker, @JobReceiveHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonID 
LEFT JOIN Web.JobReceiveLines JRL ON H.JobReceiveLineId = JRL.JobReceiveLineId 
LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
LEFT JOIN web.Processes PR ON PR.ProcessId = H.ProcessId
LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
WHERE 1=1 --Pg.ProductTypeId = 1
AND ( @JobReceiveHeaderId IS NULL OR JRL.JobReceiveHeaderId  IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeaderId, ','))) 
AND ( @Product IS NULL OR P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
GROUP BY PT.ProductTypeName,
			CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
			WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
			WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
			WHEN @ReportType ='JobWorker Wise Summary' THEN Pp.Name
			WHEN @ReportType ='Process Wise Summary' THEN PR.ProcessName 
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE P.ProductName END
ORDER BY GroupOnValue
End
GO

