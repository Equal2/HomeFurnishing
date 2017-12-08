USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobOrderRateAmendamentReport]    Script Date: 7/7/2015 12:12:26 PM ******/

IF OBJECT_ID ('[Web].[spRep_JobOrderRateAmendamentReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobOrderRateAmendamentReport]	
GO

/****** Object:  StoredProcedure [Web].[spRep_JobOrderRateAmendamentReport]    Script Date: 7/7/2015 12:12:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE procedure   [Web].[spRep_JobOrderRateAmendamentReport]   
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @JobWorker VARCHAR(Max) = NULL,
    @ProcessId VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,       
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,    
    @JobOrderAmendmentHeaderId VARCHAR(Max) = NULL,  
	@JoborderHeaderId VARCHAR(Max) = NULL	   
AS
Begin


DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM Web._JobOrderAmendmentHeaders  H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderAmendmentHeaderId is null or H.JobOrderAmendmentHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderAmendmentHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN Web._JobOrderRateAmendmentLines L ON L.JobOrderAmendmentHeaderId= H.JobOrderAmendmentHeaderId
LEFT JOIN  web._JobOrderLines JOL ON JOL.JobOrderLineId = L.JobOrderLineId
LEFT JOIN  Web._JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND JOL.Dimension1Id IS NOT NULL


SELECT @Dimension2IdCnt =  Count(*)
FROM  
( 
SELECT * FROM Web._JobOrderAmendmentHeaders  H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderAmendmentHeaderId is null or H.JobOrderAmendmentHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderAmendmentHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN Web._JobOrderRateAmendmentLines L ON L.JobOrderAmendmentHeaderId= H.JobOrderAmendmentHeaderId
LEFT JOIN  web._JobOrderLines JOL ON JOL.JobOrderLineId = L.JobOrderLineId
LEFT JOIN  Web._JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND JOL.Dimension2Id IS NOT NULL


SELECT H.SiteId, H.DivisionId, H.JobOrderAmendmentHeaderId, H.DocDate, Pt.ProductTypeName, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo,
PS.Name AS JobWorkerName, DTPO.DocumentTypeShortName + '-' + JOH.DocNo AS JobOrderNo,
L.JobOrderRateAmendmentLineId, P.ProductName, L.Qty,L.JobOrderRate,L.AmendedRate,L.Rate AS NetRate,L.Amount AS Affectedamt,
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces
,SI.SiteName, DI.DivisionName,PR.ProcessName,NULL AS SubReportProcList ,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	 
	 
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderRateAmendmentReport_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderRateAmendmentReport_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderRateAmendmentReport_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderRateAmendmentReport_ProductWiseDetail.rdl'
	END AS ReportName, 
'Job Order Rate Amendment Report' AS ReportTitle
FROM  Web._JobOrderRateAmendmentLines L
/*( 
SELECT * FROM Web._JobOrderAmendmentHeaders  H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderAmendmentHeaderId is null or H.JobOrderAmendmentHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderAmendmentHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H */
LEFT JOIN Web._JobOrderAmendmentHeaders  H ON L.JobOrderAmendmentHeaderId= H.JobOrderAmendmentHeaderId
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId  
LEFT JOIN [Web]._JobOrderLines JOL WITH (nolock) ON L.JobOrderLineId = JOL.JobOrderLineId
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId  
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = JOL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = JOL.Dimension2Id 
LEFT JOIN Web.Processes PR WITH (nolock) ON PR.ProcessId = H.ProcessId
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN Web.JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId
LEFT JOIN [Web].DocumentTypes DTPO WITH (nolock) ON DTPO.DocumentTypeId = JOH.DocTypeId 
WHERE 1=1  
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderAmendmentHeaderId is null or H.JobOrderAmendmentHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderAmendmentHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
End

GO


