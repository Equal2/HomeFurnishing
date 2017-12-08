USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobReceiveReport]    Script Date: 7/7/2015 12:01:15 PM ******/

IF OBJECT_ID ('[Web].[spRep_JobReceiveReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobReceiveReport]
GO
/****** Object:  StoredProcedure [Web].[spRep_JobReceiveReport]    Script Date: 7/7/2015 12:01:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  procedure   [Web].[spRep_JobReceiveReport]    
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
    @PersonCustomGroup VARCHAR(Max) = NULL,        
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,        
	@JobOrderHeaderId VARCHAR(Max) = NULL,
	@JobReceiveHeadersId VARCHAR(Max) = NULL, 
	@ProcessId  VARCHAR(Max) = NULL 
AS
Begin



DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 
DECLARE @LossQtySum INT  


SELECT @LossQtySum =  IsNull(Sum(L.LossQty),0) 
FROM  
(
SELECT * FROM web._JobReceiveHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobworkerId is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobReceiveHeadersId is null or H.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
)  H
LEFT JOIN Web._JobReceiveLines L WITH (nolock) ON L.JobReceiveHeaderId = H.JobReceiveHeaderId
LEFT JOIN web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderlineId=L.JobOrderLineId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
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
AND ( @JobOrderHeaderId is null or JOL.JobOrderheaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @ProcessId IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
AND L.LossQty IS NOT NULL
/*SELECT @LossQtySum =
(
SELECT IsNull(Sum(L.LossQty),0) AS LossQty FROM web._JobReceiveHeaders H WITH (nolock)   
LEFT JOIN Web._JobReceiveLines L WITH (nolock) ON L.JobReceiveHeaderId = H.JobReceiveHeaderId
LEFT JOIN web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderlineId=L.JobOrderLineId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobworkerId is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobReceiveHeadersId is null or H.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderheaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @ProcessId IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
AND L.LossQty IS NOT NULL
)*/


SELECT @Dimension1IdCnt =  Count(*)
FROM  
(
SELECT * FROM web._JobReceiveHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobworkerId is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobReceiveHeadersId is null or H.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
)  H
LEFT JOIN Web._JobReceiveLines L WITH (nolock) ON L.JobReceiveHeaderId = H.JobReceiveHeaderId
LEFT JOIN web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderlineId=L.JobOrderLineId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
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
AND ( @JobOrderHeaderId is null or JOL.JobOrderheaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @ProcessId IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
AND JOL.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt =  Count(*)
FROM  
(
SELECT * FROM web._JobReceiveHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobworkerId is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ','))) 
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
AND ( @JobReceiveHeadersId is null or H.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
)  H
LEFT JOIN Web._JobReceiveLines L WITH (nolock) ON L.JobReceiveHeaderId = H.JobReceiveHeaderId
LEFT JOIN web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderlineId=L.JobOrderLineId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
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
AND ( @JobOrderHeaderId is null or JOL.JobOrderheaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @ProcessId IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
AND JOL.Dimension2Id IS NOT NULL



DECLARE @ReportName NVARCHAR(100) 

SET @ReportName =
(
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobReceiveProductWiseDetailWithDoubleDimension'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobReceiveProductWiseDetailWithSingleDimension'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobReceiveProductWiseDetailWithSingleDimension'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobReceiveProductWiseDetail'
	 END 	 	 
)
IF (@LossQtySum > 0) SET @ReportName = @ReportName + '_WithLossQty'



SELECT H.SiteId, H.DivisionId, H.JobReceiveHeaderId, DT.DocumentTypeName,H.DocDate AS DocDate
,DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo,H.Remark,
PS.Name AS JobworkerName, WDT.DocumentTypeShortName + '-' + JOH.DocNo AS OrderNo,Pt.ProductTypeName,
 L.JobReceiveLineId,  L.JobOrderLineId, P.ProductName, L.Qty,isnull(L.LossQty,0) AS LossQty,WPS.ProcessName,
 L.Remark AS LineRemark, 
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
L.LotNo,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name,
   @ReportName +'.rdl' AS ReportName, 
'Job Receive Report' AS ReportTitle	 
	 

FROM  
(
SELECT * FROM web._JobReceiveHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null OR @ToDate>= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobworkerId is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ',')))
AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',')))  
AND ( @JobReceiveHeadersId is null or H.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
)  H
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN   Web._JobReceiveLines L WITH (nolock) ON L.JobReceiveHeaderId=H.JobReceiveHeaderId
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId 
LEFT JOIN  web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderLineId=L.JobOrderLineId
LEFT JOIN Web.products P WITH (nolock) ON JOL.ProductId=P.ProductId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = JOL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = JOL.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN web._JobOrderHeaders JOH WITH (nolock) ON JOL.JobOrderHeaderId = JOH.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes WDT WITH (nolock) ON JOH.DocTypeId=WDT.DocumentTypeId
LEFT JOIN Web.Processes WPS WITH (nolock) ON H.ProcessId=WPS.ProcessId
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND (@ProcessId IS NULL OR WPS.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
 ORDER BY  L.JobReceiveLineId
End
GO


