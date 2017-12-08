USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobOrderCancelReport]    Script Date: 6/29/2015 1:11:22 PM ******/

IF OBJECT_ID ('[Web].[spRep_JobOrderCancelReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobOrderCancelReport]	
GO

/****** Object:  StoredProcedure [Web].[spRep_JobOrderCancelReport]    Script Date: 6/29/2015 1:11:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  procedure   [Web].[spRep_JobOrderCancelReport]
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@FilterDateOn VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @Worker VARCHAR(Max) = NULL,   
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,    
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,        
	@JobOrderHeaderId VARCHAR(Max) = NULL,
	@JobOrderCancelHeaderId VARCHAR(Max) = NULL		   
AS
BEGIN
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM Web._JobOrderCancelHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Worker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@Worker, ','))) 
AND ( @JobOrderCancelHeaderId is null or H.JobOrderCancelHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderCancelHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN  Web.JobOrderCancelLines L ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId 
LEFT JOIN  web._JobOrderLines JOL ON JOL.JobOrderLineId = L.JobOrderLineId
LEFT JOIN  Web._JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
WHERE 1=1 AND P.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Job Order Date' THEN JOH.DocDate ELSE H.DocDate END ) 
AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Job Order Date' THEN JOH.DocDate ELSE H.DocDate END ) 
AND JOL.Dimension1Id IS NOT NULL

SELECT @Dimension2IdCnt =  Count(*)
FROM  
( 
SELECT * FROM Web._JobOrderCancelHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Worker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@Worker, ','))) 
AND ( @JobOrderCancelHeaderId is null or H.JobOrderCancelHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderCancelHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN  Web.JobOrderCancelLines L ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId 
LEFT JOIN  web._JobOrderLines JOL ON JOL.JobOrderLineId = L.JobOrderLineId
LEFT JOIN  Web._JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
WHERE 1=1 AND P.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Job Order Date' THEN JOH.DocDate ELSE H.DocDate END ) 
AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Job Order Date' THEN JOH.DocDate ELSE H.DocDate END ) 
AND JOL.Dimension2Id IS NOT NULL



SELECT H.SiteId, H.DivisionId, H.JobOrderCancelHeaderId, DT.DocumentTypeName, H.DocDate, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo, H.Remark,
PS.Name AS WorkerName,  DTPI.DocumentTypeShortName + '-' + JOH.DocNo AS OrderNo,
Pt.ProductTypeName,   L.JobOrderCancelLineId,  L.JobOrderLineId, P.ProductName, L.Qty,
JOL.Specification, L.Remark AS LineRemark, R.ReasonName,
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
JOL.LotNo,NULL AS SubReportProcList ,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	 
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderCancelProductWiseDetailWithDoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderCancelProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderCancelProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderCancelProductWiseDetail.rdl'
	END AS ReportName, 
'Job Order Cancel Report' AS ReportTitle
FROM  
( 
SELECT * FROM [Web]._JobOrderCancelHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Worker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@Worker, ','))) 
AND ( @JobOrderCancelHeaderId is null or H.JobOrderCancelHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderCancelHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
/*SELECT * FROM Web._JobOrderCancelHeaders
SELECT * FROM Web.PersonCustomGroupLines
SELECT * FROM Web.JobOrderCancelLines
SELECT * FROM web._JobOrderLines
SELECT * FROM web._JobOrderHeaders
SELECT * FROM Web.products
SELECT * FROM Web.ProductGroups 
SELECT * FROM Web.ProductTypes*/
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web]._JobOrderCancelLines L WITH (nolock) ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId 
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId  
LEFT JOIN Web.JobOrderLines JOl WITH (nolock) ON L.JobOrderLineId = JOL.JobOrderLineId
LEFT JOIN Web.JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock)  ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock)  ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = JOL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = JOL.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN [Web].DocumentTypes DTPI WITH (nolock) ON DTPI.DocumentTypeId = JOH.DocTypeId 
LEFT JOIN web.Reasons R WITH (nolock) ON R.ReasonId = H.ReasonId
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Job Order Date' THEN JOH.DocDate ELSE H.DocDate END ) 
AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Job Order Date' THEN JOH.DocDate ELSE H.DocDate END )
ORDER BY H.JobOrderCancelHeaderId,L.JobOrderLineId,JOL.JobOrderLineId
End
GO


