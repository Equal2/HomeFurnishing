USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_ShadeFNSAnalysisReport]    Script Date: 17/Jul/2015 4:02:35 PM ******/
IF object_id ('[Web].[spRep_ShadeMovementReport]') IS NOT NULL 
DROP Procedure  [Web].[spRep_ShadeMovementReport]
GO 

/****** Object:  StoredProcedure [Web].[spRep_ShadeFNSAnalysisReport]    Script Date: 17/Jul/2015 4:02:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



	CREATE procedure   [Web].[spRep_ShadeMovementReport]
		@FromDate VARCHAR(20) = NULL,
		@ToDate VARCHAR(20) = NULL,
		@Site VARCHAR(20) = NULL,
		@Division VARCHAR(20) = NULL		
	as 
	begin 

	DECLARE @CountDivision NUMERIC(18,4) 
	DECLARE @CountSite NUMERIC(18,4) 

	DECLARE @TotalQty NUMERIC(18,4) 
	DECLARE @sumrow NUMERIC(18,4)
	DECLARE @max NUMERIC(18,4)
	DECLARE @min NUMERIC(18,4)

	SELECT @TotalQty=IsNull(Sum(Qty),0),@sumrow=count(Distinct H.JobOrderHeaderId),@CountSite=count( Distinct H.SiteId),@CountDivision=count(Distinct H.DivisionId) FROM (
	SELECT H.JobOrderHeaderId,H.SiteId,H.DivisionId from web._JobOrderHeaders H 
	Left Join Web.DocumentTypes DT with (nolock) on H.DocTypeId=DT.DocumentTypeId 
		  where 1=1 AND DT.DocumentCategoryId =59
	AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
	AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	) H
	LEFT JOIN  Web._JobOrderLines L WITH (nolock) ON L.JobOrderHeaderId = H.JobOrderHeaderId
	where 1=1 AND Dimension1Id IS NOT NULL


	select @max=Max(Persentage),@min=min(Persentage) from(
	SELECT Dimension1Id,round((count(Distinct H.JobOrderHeaderId)/@sumrow) * 100,4) AS Persentage FROM (
	SELECT H.JobOrderHeaderId from web._JobOrderHeaders H
	Left Join Web.DocumentTypes DT with (nolock) on H.DocTypeId=DT.DocumentTypeId 	
	where 1=1 AND DT.DocumentCategoryId =59	
	AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
	AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	) H
	LEFT JOIN  Web._JobOrderLines L WITH (nolock) ON L.JobOrderHeaderId = H.JobOrderHeaderId
	where 1=1 AND Dimension1Id IS NOT NULL 
	GROUP BY Dimension1Id
	) K

	select D.Dimension1Name,IsNull(Sum(L.Qty),0) AS TotalQty,count(Distinct H.JobOrderHeaderId)AS cnt,Round(IsNull(Sum(L.Qty),0)/count(Distinct H.JobOrderHeaderId),2) AS Average,
	Round((IsNull(Sum(L.Qty),0)/@TotalQty)*100,2) AS Consumption,round((count(Distinct H.JobOrderHeaderId)/@sumrow) * 100,4) AS Persentage,
	Round((((count(Distinct H.JobOrderHeaderId)/@sumrow) * 100)/(@max+@min))* 100,2) AS MovementRatio,
	CASE WHEN Round((((count(Distinct H.JobOrderHeaderId)/@sumrow) * 100)/(@max+@min))* 100,2) > ((@max+@min)/2) THEN 'Fast' ELSE 'slow' END AS  Movindrating,
	NULL AS SubReportProcList,'ShadeMovementReport.rdl' AS ReportName, 'Shade Movement Report' AS ReportTitle,
	case When @CountDivision > 1 then 0 else Max(H.DivisionId) end AS DivisionId,case when @CountSite >1 then 0 else Max(H.SiteId) end as SiteId

	 from
	(
	SELECT H.JobOrderHeaderId,H.DivisionId,H.SiteId from web._JobOrderHeaders H 
	Left Join Web.DocumentTypes DT with (nolock) on H.DocTypeId=DT.DocumentTypeId 	
	where 1=1 AND DT.DocumentCategoryId =59		
	AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
	AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	) H
	LEFT JOIN  Web._JobOrderLines L WITH (nolock) ON L.JobOrderHeaderId = H.JobOrderHeaderId
	LEFT join  web.Dimension1 D  WITH (nolock) ON D.Dimension1Id=L.Dimension1Id
	where 1=1 AND L.Dimension1Id IS NOT NULL 
	group by D.Dimension1Name
	ORDER BY Round((((count(Distinct H.JobOrderHeaderId)/@sumrow) * 100)/(@max+@min))* 100,2) DESC
	end

GO





