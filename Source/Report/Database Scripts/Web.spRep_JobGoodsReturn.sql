USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobGoodsReturn]    Script Date: 24/Jul/2015 11:45:02 AM ******/

IF OBJECT_ID ('[Web].[spRep_JobGoodsReturn]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobGoodsReturn]
GO
/****** Object:  StoredProcedure [Web].[spRep_JobGoodsReturn]    Script Date: 24/Jul/2015 11:45:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [Web].[spRep_JobGoodsReturn]
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @jobworker VARCHAR(Max) = NULL,   
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,         
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,        	
    @JobReceiveHeaderId VARCHAR(Max) = NULL,		   
	@JobReturnHeaders VARCHAR(Max) = NULL,
	 @Process VARCHAR(Max) = NULL
AS
BEGIN
    DECLARE @Dimension1IdCnt INT 
    DECLARE @Dimension2IdCnt INT 

 SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM [Web]._JobReturnHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @jobworker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@jobworker, ','))) 
AND ( @JobReturnHeaders is null or H.JobReturnHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReturnHeaders, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN Web._JobReturnLines L WITH (nolock) ON L.JobReturnHeaderId = H.JobReturnHeaderId
LEFT JOIN web.jobreceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId = L.JobReceiveLineId
LEFT JOIN web.joborderlines JOL WITH (nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobReceiveHeaderId is null or JRL.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeaderId, ','))) 
AND JOL.Dimension1Id IS NOT NULL

    
 SELECT @Dimension2IdCnt =  Count(*)
FROM  
( 
SELECT * FROM [Web]._JobReturnHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @jobworker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@jobworker, ','))) 
AND ( @JobReturnHeaders is null or H.JobReturnHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReturnHeaders, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN Web._JobReturnLines L WITH (nolock) ON L.JobReturnHeaderId = H.JobReturnHeaderId
LEFT JOIN web.jobreceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId = L.JobReceiveLineId
LEFT JOIN web.joborderlines JOL WITH (nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobReceiveHeaderId is null or JRL.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeaderId, ','))) 
AND JOL.Dimension2Id IS NOT NULL
   
    
    /*SELECT * FROM Web.JobReturnHeaders
SELECT * FROM Web.JobReturnLines

SELECT * FROM Web.JobReturnLines
SELECT TOP 1 * FROM web.jobreceivelines
SELECT * FROM web.joborderlines*/

SELECT H.SiteId, H.DivisionId, H.JobReturnHeaderId, DT.DocumentTypeName, H.DocDate, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo, H.Remark, R.ReasonName, 
PS.Name AS WorkerName,DTJRH.DocumentTypeShortName + '-' + JRH.DocNo AS ReceiptNo ,
Pt.ProductTypeName,   L.JobReceiveLineId,  L.JobReturnLineId, P.ProductName, L.Qty,
JOL.Specification, L.Remark AS LineRemark,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,  
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
JOL.LotNo,  PR.ProcessName, 
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName,  	 
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName,	 
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	 
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobReturnProductWiseDetailWithDoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobReturnProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobReturnProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobReturnProductWiseDetail.rdl'
	END AS ReportName, 
'Job Return Report' AS ReportTitle,NULL AS SubReportProcList

FROM
(
SELECT * FROM [Web]._JobReturnHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @jobworker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@jobworker, ','))) 
AND ( @JobReturnHeaders is null or H.JobReturnHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReturnHeaders, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H
LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId=H.DocTypeId
LEFT JOIN Web._JobReturnLines L WITH (nolock) ON L.JobReturnHeaderId=H.JobReturnHeaderId
LEFT JOIN web.jobreceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId = L.JobReceiveLineId
LEFT JOIN Web.JobReceiveHeaders JRH WITH (nolock) ON JRH.JobReceiveHeaderId=JRL.JobReceiveHeaderId
LEFT JOIN [Web].DocumentTypes DTJRH WITH (nolock) ON DTJRH.DocumentTypeId = JRH.DocTypeId 
LEFT JOIN web.joborderlines JOL WITH (nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId  
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = JOL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = JOL.Dimension2Id
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN Web.Reasons R WITH (nolock) ON R.ReasonId=H.ReasonId
LEFT JOIN Web.People  PS WITH (nolock) ON PS.PersonID=H.JobWorkerId
LEFT JOIN Web.Processes PR WITH (nolock) ON PR.ProcessId = H.ProcessId
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ',')))
AND ( @JobReceiveHeaderId is null or JRL.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeaderId, ','))) 
END

GO


