USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobInvoiceReport]    Script Date: 7/7/2015 12:02:47 PM ******/

IF OBJECT_ID ('[Web].[spRep_JobInvoiceReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobInvoiceReport]
GO
/****** Object:  StoredProcedure [Web].[spRep_JobInvoiceReport]    Script Date: 7/7/2015 12:02:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure   [Web].[spRep_JobInvoiceReport]
    @ReportType VARCHAR(Max) = NULL,
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
	@JobInvoiceHeaderId VARCHAR(Max) = NULL, 
	@ProcessId  VARCHAR(Max) = NULL 
AS
Begin



DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 


SELECT @Dimension1IdCnt =  Count(*)
FROM  
(
SELECT * FROM Web._JobInvoiceHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
--AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM [dbo].[Split] (@Currency, ','))) 
AND ( @JobInvoiceHeaderId is null or H.JobInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobInvoiceHeaderId, ','))) 
--AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT * FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN Web._JobInvoiceLines L ON L.JobInvoiceHeaderId = H.JobInvoiceHeaderId
LEFT JOIN Web._JobReceivelines JRL ON JRL.JobReceiveLineId= L.JobReceiveLineid 
LEFT JOIN Web._JobOrderLines JOL ON JOL.JobOrderLineId = JRL.JobOrderLineId
LEFT JOIN Web.Products P ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND (@JobworkerId is null OR L.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ',')))
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobReceiveHeadersId is null or JRL.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND JOL.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt = Count(*)
FROM  
(
 SELECT * FROM Web._JobInvoiceHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
--AND ( @Currency is null or H.CurrencyId IN (SELECT Items FROM [dbo].[Split] (@Currency, ','))) 
AND ( @JobInvoiceHeaderId is null or H.JobInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobInvoiceHeaderId, ','))) 
--AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT * FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
) H 
LEFT JOIN Web._JobInvoiceLines L ON L.JobInvoiceHeaderId = H.JobInvoiceHeaderId
LEFT JOIN Web._JobReceivelines JRL ON JRL.JobReceiveLineId= L.JobReceiveLineid 
LEFT JOIN Web._JobOrderLines JOL ON JOL.JobOrderLineId = JRL.JobOrderLineId
LEFT JOIN Web.Products P ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND (@JobworkerId is null OR L.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ',')))
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobReceiveHeadersId is null or JRL.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ','))) 
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND JOL.Dimension2Id IS NOT NULL




SELECT H.SiteId,H.DivisionId, H.JobInvoiceHeaderId, DT.DocumentTypeName, H.DocDate, 
DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo, H.JobWorkerDocNo, H.JobWorkerDocDate, H.Remark,
PS.Name AS JobworkerName, DTRH.DocumentTypeShortName + '-' + JRH.DocNo AS JobReciptNo,
DTOH.DocumentTypeShortName + '-' + JOH.DocNo AS OrderNo,
Pt.ProductTypeName,
 isnull(H.CreditDays,0) AS CreditDays,  
L.JobInvoiceLineId, P.ProductName, JRL.Qty,L.DealQty AS DealQty,
L.Rate, L.Amount AS Amount,L.Remark AS LineRemark, 
U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 4 AS DeliveryUnitDecimalPlace,
H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
JRL.LotNo, WP.ProcessName,
CASE WHEN isnull(L.Amount,0) <> 0 
THEN L.Amount/(CASE  WHEN  isnull(JRL.Qty,0) =0 THEN 1 ELSE JRL.Qty END ) ELSE 0 END AS RatePerPcs,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	 
CASE WHEN @ReportType = '1' THEN 'JobInvoiceDetail.rdl' ELSE 
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobInvoiceProductWiseDetailWithDoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobInvoiceProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobInvoiceProductWiseDetailWithSingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobInvoiceProductWiseDetail.rdl'
	END 
END AS ReportName, 
'Job Invoice Report' AS ReportTitle
FROM  
( 
SELECT * FROM Web._JobInvoiceHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobInvoiceHeaderId is null or H.JobInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobInvoiceHeaderId, ','))) 
) H 
LEFT JOIN  Web._JobInvoiceLines L WITH (nolock) ON L.JobInvoiceHeaderId = H.JobInvoiceHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web._People PS WITH (nolock) ON PS.PersonID = L.JobWorkerId 
LEFT JOIN Web._JobReceivelines JRL WITH (nolock) ON JRL.JobReceiveLineId = L.JobReceiveLineId
LEFT JOIN  web._JobReceiveHeaders JRH WITH (nolock) ON JRH.JobReceiveHeaderId = JRL.JobReceiveHeaderId
LEFT JOIN Web.DocumentTypes DTRH WITH (nolock) ON DTRH.DocumentTypeId = JRH.DocTypeId 
LEFT JOIN Web._JobOrderLines JOL WITH (nolock) ON JOL.JobOrderLineId = JRL.JobOrderLineId 
LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JoL.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes DTOH WITH (nolock) ON DTOH.DocumentTypeId = JOH.DocTypeId 
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = P.UnitId  
--LEFT JOIN [Web].Currencies C WITH (nolock) ON C.ID = H.CurrencyId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = JOL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = JOL.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN Web.Processes WP WITH (nolock) ON H.ProcessId=WP.ProcessId
WHERE 1=1 AND JOL.ProductId IS NOT NULL 
AND (@JobworkerId is null OR L.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ',')))
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or JOL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or JOL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @JobReceiveHeadersId is null or JRL.JobReceiveHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobReceiveHeadersId, ',')))
AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ','))) 
AND (@ProcessId IS NULL OR WP.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',')))
ORDER BY L.JobInvoiceLineId
End
GO


