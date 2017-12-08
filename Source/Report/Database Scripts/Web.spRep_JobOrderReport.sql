

IF OBJECT_ID ('[Web].[spRep_JobOrderReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobOrderReport]
GO


Create procedure   [Web].[spRep_JobOrderReport]
    @ReportType VARCHAR(Max) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
    @FromDate VARCHAR(20) = NULL,
    @ToDate VARCHAR(20) = NULL,
    @DocumentType VARCHAR(20) = NULL,	
    @JobWorker VARCHAR(Max) = NULL,
    @Process VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,       
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,    
    @ProdOrderHeaderId VARCHAR(Max) = NULL,  
	@JoborderHeaderId VARCHAR(Max) = NULL	   
AS
Begin


DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM [Web]._JobOrderHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderHeaderId is null or H.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._JobOrderLines L ON L.JobOrderHeaderId = H.JobOrderHeaderId 
LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId
LEFT JOIN [Web].Products P ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND L.Dimension1Id IS NOT NULL


SELECT @Dimension2IdCnt =  Count(*)
FROM  
( 
SELECT * FROM [Web]._JobOrderHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderHeaderId is null or H.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web]._JobOrderLines L ON L.JobOrderHeaderId = H.JobOrderHeaderId 
LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId
LEFT JOIN [Web].Products P ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND L.Dimension1Id IS NOT NULL


SELECT H.SiteId, H.DivisionId, H.JobOrderHeaderId, DT.DocumentTypeName, H.DocDate, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo,H.DueDate,  H.Remark,
PS.Name AS JobWorkerName, DTPO.DocumentTypeShortName + '-' + POH.DocNo AS ProdOrderNo, Pt.ProductTypeName,
H.CreditDays, H.TermsAndConditions,   
L.JobOrderLineId, P.ProductName, L.Qty, L.Specification, L.DealQty AS DealQty,
L.DueDate AS LineDueDate, L.Rate, L.Amount AS Amount, L.Remark AS LineRemark, 
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 4 AS DeliveryUnitDecimalPlace,
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
L.LotNo, PR.ProcessName, 
CASE WHEN isnull(L.Amount,0) <> 0 THEN L.Amount/L.Qty ELSE 0 END AS RatePerPcs,

CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
	 
CASE WHEN @ReportType = '1' THEN 'JobOrderDetail.rdl' ELSE 
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderProductWiseDetailWithDoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderProductWiseDetail.rdl'
	END 
END AS ReportName, 

PR.ProcessName+ ' Order Report' AS ReportTitle
FROM  
( 
SELECT * FROM [Web]._JobOrderHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @JobOrderHeaderId is null or H.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web]._JobOrderLines L WITH (nolock) ON L.JobOrderHeaderId = H.JobOrderHeaderId 
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId  
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId  
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = L.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = L.Dimension2Id 
LEFT JOIN Web.Processes PR WITH (nolock) ON PR.ProcessId = H.ProcessId
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN Web.ProdOrderLines POL WITH (nolock) ON L.ProdOrderLineId = POL.ProdOrderLineId 
LEFT JOIN Web.ProdOrderHeaders POH WITH (nolock) ON POL.ProdOrderHeaderId  = POH.ProdOrderHeaderId
LEFT JOIN [Web].DocumentTypes DTPO WITH (nolock) ON DTPO.DocumentTypeId = POH.DocTypeId 
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
End
GO

