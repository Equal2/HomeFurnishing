USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingReceiveSummary]    Script Date: 02/Sep/2015 1:50:32 PM ******/
IF object_id ('[Web].[spRep_WeavingReceiveSummary]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_WeavingReceiveSummary]
GO
/****** Object:  StoredProcedure [Web].[spRep_WeavingReceiveSummary]    Script Date: 02/Sep/2015 1:50:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[spRep_WeavingReceiveSummary]
        @Site VARCHAR(20) = NULL,
	    @Division VARCHAR(20) = NULL,
	    @FromDate VARCHAR(20) = NULL,
	    @ToDate VARCHAR(20) = NULL,
	    @DocumentType VARCHAR(20) = NULL,	
	    @JobworkerId VARCHAR(Max) = NULL,
	    @ProductType VARCHAR(Max) = NULL,    
	    @ProductGroup VARCHAR(Max) = NULL,   
	    @ProductNature VARCHAR(Max) = NULL, 
	    @Product VARCHAR(Max) = NULL,      
	    @ProductCustomGroup VARCHAR(Max) = NULL,
	    @JobOrderHeaderId VARCHAR(Max) = NULL,
        @JobReceiveHeaderId VARCHAR(Max) = NULL, 
	    @ProcessId  VARCHAR(Max) = NULL,
	    @ProductCategory VARCHAR(Max)=NULL,
	    @ProductQuality VARCHAR(Max)=NULL,
	    @ReportType VARCHAR(Max) = 'JobWorker Wise Summary',
	    @BranchRecord VARCHAR(Max)=NULL,
	    @SampleRecord VARCHAR(Max)=NULL
	    AS 
	    BEGIN 
	    
	   
	     SET @SampleRecord=(CASE WHEN @SampleRecord ='Exclude Sample Records' THEN 0 WHEN @SampleRecord='Only Sample Records' THEN 1 ELSE NULL End)
	     SET @BranchRecord=(CASE WHEN @BranchRecord ='Exclude Branch Records' THEN 0 WHEN @BranchRecord='Only Branch Records' THEN 1 ELSE NULL End)
	    
    SELECT
             Max(CASE WHEN @ReportType ='JobWorker Wise Summary' THEN P.Name
			WHEN @ReportType ='Construction Wise Summary' THEN PC.ProductCategoryName
			WHEN @ReportType ='Design Wise Summary' THEN PG.ProductGroupName
			WHEN @ReportType ='Quality Wise Summary' THEN PQ.ProductQualityName
			WHEN @ReportType ='Size Wise Summary' THEN (Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END) 
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,11)
			ELSE P.Name		END) AS GroupOnValue,
		Max(CASE WHEN @ReportType ='JobWorker Wise Summary' THEN P.Name
			WHEN @ReportType ='Construction Wise Summary' THEN PC.ProductCategoryName
			WHEN @ReportType ='Design Wise Summary' THEN PG.ProductGroupName
			WHEN @ReportType ='Quality Wise Summary' THEN PQ.ProductQualityName
			WHEN @ReportType ='Size Wise Summary' THEN (Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END) 
			WHEN @ReportType ='Month Wise Summary' THEN STUFF(CONVERT(CHAR(11), H.DocDate, 100), 5,5, '-') 
			WHEN @ReportType ='Date Wise Summary' THEN replace(convert(VARCHAR,H.DocDate, 106), ' ', '/')
			ELSE P.Name		END) AS GroupOnText,
		
		Max(CASE WHEN @ReportType ='JobWorker Wise Summary' THEN 'Job Worker'
			WHEN @ReportType ='Construction Wise Summary' THEN 'Construction'
			WHEN @ReportType ='Design Wise Summary' THEN 'Design'
			WHEN @ReportType ='Quality Wise Summary' THEN 'Quality'
			WHEN @ReportType ='Size Wise Summary' THEN 'Size'
			WHEN @ReportType ='Month Wise Summary' THEN 'Month'
			WHEN @ReportType ='Date Wise Summary' THEN 'Date'
			ELSE 'Job Worker'		END) AS GroupTitle,
		 Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
		IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces,		
sum(JOL.DealQty) AS Area,
sum(L.Qty) AS PCS,
sum(L.Weight) AS Weight,
NULL AS  SubReportProcList,'WeavingReceiveSummary.rdl' AS ReportName,'Weaving Receive Summary' AS  ReportTitle
 FROM 
(
    SELECT * FROM web.JobReceiveHeaders H WHERE 1=1
	AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
	AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ',')))
	AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
	AND ( @JobReceiveHeaderId is null or H.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeaderId, ','))) 
	AND (@JobworkerId is null OR H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ',')))
)H
LEFT JOIN   Web._JobReceiveLines L ON L.JobReceiveHeaderId=H.JobReceiveHeaderId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = H.JobWorkerId
LEFT JOIN Web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderLineId = L.JobOrderLineId
LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JoL.JobOrderHeaderId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
LEFT JOIN  Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = PD.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON PG.ProductTypeId = Pt.ProductTypeId 
LEFT Join Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId=PD.ProductId 
LEFT JOIN Web.ProductQualities PQ WITH (nolock)  ON FP.ProductQualityId = PQ.ProductQualityId
LEFT JOIN Web.ProductCategories PC WITH (Nolock) ON FP.ProductCategoryId=PC.ProductCategoryId
LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=PD.ProductId
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId
where 1=1
	AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
	AND ( @ProductCustomGroup IS NULL OR PD.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ',')))
	AND ( @ProductCategory is null or FP.ProductCategoryId IN (SELECT Items FROM [dbo].[Split] (@ProductCategory, ','))) 
	AND ( @ProductQuality is null or FP.ProductQualityId IN (SELECT Items FROM [dbo].[Split] (@ProductQuality, ','))) 
    AND ( @SampleRecord IS NULL OR FP.IsSample IN  (SELECT Items FROM [dbo].[Split] (@SampleRecord, ',')))
    AND ( @BranchRecord  IS NULL OR  P.IsSisterConcern IN (SELECT Items FROM [dbo].[Split] (@BranchRecord, ',')))   
GROUP BY CASE WHEN @ReportType  ='JobWorker Wise Summary' THEN P.Name
		    WHEN @ReportType ='Construction Wise Summary' THEN PC.ProductCategoryName
			WHEN @ReportType ='Design Wise Summary' THEN PG.ProductGroupName
			WHEN @ReportType ='Quality Wise Summary' THEN PQ.ProductQualityName
			WHEN @ReportType ='Size Wise Summary' THEN (Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END) 
			WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,11)
			ELSE P.Name END
	ORDER BY GroupOnValue

END
GO


