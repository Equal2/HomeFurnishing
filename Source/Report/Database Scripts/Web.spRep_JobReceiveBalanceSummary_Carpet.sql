USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobReceiveBalanceSummary_Carpet]    Script Date: 04/Aug/2015 12:26:14 PM ******/
IF OBJECT_ID ('[Web].[spRep_JobReceiveBalanceSummary_Carpet]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobReceiveBalanceSummary_Carpet]	
GO

/****** Object:  StoredProcedure [Web].[spRep_JobReceiveBalanceSummary_Carpet]    Script Date: 04/Aug/2015 12:26:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_JobReceiveBalanceSummary_Carpet]
 @StatusOnDate VARCHAR(20) = NULL,
 @Site VARCHAR(20) = NULL,
 @Division VARCHAR(20) = NULL,
 @FromDate VARCHAR(20) = NULL,
 @ToDate VARCHAR(20) = NULL,
 @DocumentType VARCHAR(20) = NULL,
 @JobWorker VARCHAR(Max) = NULL,
 @Process VARCHAR(Max) = NULL,
 @Product VARCHAR(Max) = NULL,
 @JobreceiveHeaderId VARCHAR(Max) = NULL,
 @ProductGroup VARCHAR(Max)=NULL,
 @ProductNature VARCHAR(Max)=NULL,
 @ProductCustomGroup VARCHAR(Max)=NULL,
 @PersonCustomGroup VARCHAR(Max)=NULL,
 @ReportType VARCHAR(Max) = 'Product Wise Summary',
 @AreaOnUnit VARCHAR(10) = 'Sq.Feet'
AS 
BEGIN

SELECT 
max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 				 
				WHEN @ReportType ='Product Size Wise Summary' THEN  Replace(Str(VRS.StandardSizeArea,15,4), ' ','0') 			   
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='JobWorker Wise Summary' THEN Pp.Name
				WHEN @ReportType ='Process Wise Summary' THEN Pro.ProcessName
				WHEN @ReportType ='Receive No Wise Summary' THEN H.JobReceiveNO
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.ReceiveDate,11),0,6)
				ELSE P.ProductName
      End) AS GroupOnValue,
max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName   
				WHEN @ReportType ='Product Size Wise Summary' THEN VRS.StandardSizeName 			   
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='JobWorker Wise Summary' THEN Pp.Name
				WHEN @ReportType ='Process Wise Summary' THEN Pro.ProcessName
				WHEN @ReportType ='Receive No Wise Summary' THEN H.JobReceiveNO
				WHEN @ReportType ='Month Wise Summary' THEN STUFF(CONVERT(CHAR(11), H.ReceiveDate, 100), 5,5, '-')
				ELSE P.ProductName
End) AS GroupOnText,
 Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
				WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'	
				WHEN @ReportType ='Product Size Wise Summary' THEN 'Product Size'		   
				WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Nature'
				WHEN @ReportType ='JobWorker Wise Summary' THEN 'Worker'
				WHEN @ReportType ='Process Wise Summary' THEN 'Process'
				WHEN @ReportType ='Receive No Wise Summary' THEN 'Job Receive No'
				WHEN @ReportType ='Month Wise Summary' THEN 'Month'
				ELSE 'Product'
					END) AS GroupTitle, 
		CASE WHEN Min(U.UnitName) =  Max(U.UnitName) THEN Max(U.UnitName) ELSE 'Mix' END AS UnitName,
		 Max(AU.UnitName) AS AreaUnit,		 
		isnull(sum(H.BalQty),0) AS  BalanceQty,
		 Sum(H.BalQty*(CASE WHEN @AreaOnUnit = 'Sq.Yard' THEN Ra.SqYardPerPcs ELSE Ra.Area END)) AS TotalArea,
		IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces,
		Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
		S.SiteCode AS SiteName,D.DivisionName,
'Carpet Job Receive Balance Summery' AS ReportTitle, NULL AS SubReportProcList,'JobReceiveBalanceSummary.rdl' AS ReportName

FROM Web.FJobReceiveBalance (@StatusOnDate,@Site,@Division,@FromDate,@ToDate,@DocumentType,@JobWorker,@Process,@Product,@JobreceiveHeaderId) AS H
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonId 
LEFT JOIN Web.Processes Pro ON Pro.ProcessId=H.ProcessId
LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
LEFT JOIN Web.ViewRugArea Ra WITH (nolock) ON P.ProductId = Ra.ProductId
LEFT JOIN Web.Units Au WITH (nolock) ON @AreaOnUnit = Au.UnitName
LEFT JOIN Web.ViewRugSize VRS WITH (nolock) ON VRS.ProductId = P.ProductId
WHERE 1=1 AND Pt.ProductTypeName='Rug'
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
GROUP BY CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Size Wise Summary' THEN  Replace(Str(VRS.StandardSizeArea,15,4), ' ','0') 			   
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='JobWorker Wise Summary' THEN Pp.Name
				WHEN @ReportType ='Process Wise Summary' THEN Pro.ProcessName
				WHEN @ReportType ='Receive No Wise Summary' THEN H.JobReceiveNO
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.ReceiveDate,11),0,6)
				ELSE P.ProductName
			END,S.SiteCode,D.DivisionName
ORDER BY GroupOnValue

END
GO


