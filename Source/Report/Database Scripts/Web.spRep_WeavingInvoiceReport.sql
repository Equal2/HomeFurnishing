USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingInvoiceReport]    Script Date: 28/Aug/2015 6:36:07 PM ******/
IF object_id ('[Web].[spRep_WeavingInvoiceReport]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_WeavingInvoiceReport]
GO 

/****** Object:  StoredProcedure [Web].[spRep_WeavingInvoiceReport]    Script Date: 28/Aug/2015 6:36:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_WeavingInvoiceReport]
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
		@JobInvoiceHeaderId VARCHAR(Max) = NULL, 
		@ProcessId  VARCHAR(Max) = NULL,
		@ProductCategory VARCHAR(Max)=NULL,
		@ProductQuality VARCHAR(Max)=NULL 
	as
	Begin
	SELECT max(H.SiteId) AS SiteId,max(H.DivisionId) AS DivisionId,max(H.DocDate) AS RecDate,max(DT.DocumentTypeShortName + '-' + H.DocNo) AS RecNo,
	CC.CostCenterName as cost, max(PQ.ProductQualityName) AS Quality, max(PG.ProductGroupName) AS Design,
	max(Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END) as size,
	sum(JRL.Qty) AS Pcs,
	max(PS.Name) AS JobWorkerName,Sum(L.DealQty) AS Area,
	sum(JRL.Weight) AS Weight,L.Rate,sum(isnull(L.Amount,0)) AS Amount,JILC.IncentiveRate, 
	sum(isnull(JILC.Incentive,0)) as Incentive,sum(isnull(JILC.Penalty,0)) as Penalty,
	sum(isnull(L.Amount,0))+sum(isnull(JILC.Incentive,0)) - sum(isnull(JILC.Penalty,0)) AS NetAmt,
	isnull(max(U.DecimalPlaces),0) AS DecimalPlaces,
	'WeavingInvoiceReport.rdl' AS  ReportName,'Weaving Invoice Report' AS  ReportTitle
	 FROM
	(
	SELECT * FROM Web._JobInvoiceHeaders H WHERE 1=1
	AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
	AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ',')))
	AND ( @ProcessId is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
	AND ( @JobInvoiceHeaderId is null or H.JobInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobInvoiceHeaderId, ','))) 
	) H
	LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
	LEFT JOIN Web._JobInvoiceLines L WITH (nolock) ON L.JobInvoiceHeaderId = H.JobInvoiceHeaderId
	LEFT JOIN Web.JobReceiveLines  JRL WITH (nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
	LEFT JOIN Web._JobOrderLines JOL WITH (nolock) ON JOL.JobOrderLineId = JRL.JobOrderLineId 
	LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JoL.JobOrderHeaderId	
	LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
	LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId  
	LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
	LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
	LEFT Join Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId=P.ProductId 
	LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=P.ProductId
	LEFT JOIN Web.ProductQualities PQ WITH (nolock)  ON FP.ProductQualityId = PQ.ProductQualityId
	LEFT JOIN Web.People PS WITH (nolock) ON PS.PersonID = L.JobWorkerId	
	LEFT JOIN Web.CostCenters CC WITH (nolock) ON CC.CostCenterId=JOH.CostCenterId
	
	LEFT JOIN 
	(
		SELECT l.LineTableId, Max(CASE WHEN l.ChargeId =31 THEN l.Rate ELSE 0 End) AS IncentiveRate ,
		Sum(CASE WHEN l.ChargeId =31 THEN l.Amount ELSE 0 End) AS Incentive ,
		Sum(CASE WHEN l.ChargeId =37 THEN l.Amount ELSE 0 End) AS Penalty
		FROM web.jobinvoicelinecharges l
		LEFT JOIN web.JobInvoiceHeaders h ON l.HeaderTableId = h.JobInvoiceHeaderId 
		WHERE 1=1
		AND ( @Site is null or h.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
		AND ( @Division is null or h.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
		AND ( @FromDate is null or @FromDate <= h.DocDate ) 
		AND ( @ToDate is null or @ToDate >= h.DocDate ) 
		AND ( @DocumentType is null or h.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ',')))
		AND ( @ProcessId is null or h.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
		AND ( @JobInvoiceHeaderId is null or h.JobInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobInvoiceHeaderId, ','))) 
		GROUP BY l.LineTableId 
	) JILC ON JILC.LineTableId=L.JobInvoiceLineid
	WHERE 1=1
	AND JOL.ProductId IS NOT NULL 
	AND (@JobworkerId is null OR L.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobworkerId, ',')))
	AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @Product is null or JOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
	AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @JobOrderHeaderId is null or JOL.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ',')))
	AND ( @ProductCategory is null or FP.ProductCategoryId IN (SELECT Items FROM [dbo].[Split] (@ProductCategory, ','))) 
	AND ( @ProductQuality is null or FP.ProductQualityId IN (SELECT Items FROM [dbo].[Split] (@ProductQuality, ','))) 
	Group By CC.CostCenterName, JOL.ProductId ,L.Rate, JILC.IncentiveRate
	End

GO


