USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingOrderReport]    Script Date: 21/Aug/2015 4:29:58 PM ******/
IF OBJECT_ID ('[Web].[spRep_WeavingOrderReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_WeavingOrderReport]	
GO
/****** Object:  StoredProcedure [Web].[spRep_WeavingOrderReport]    Script Date: 21/Aug/2015 4:29:58 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[spRep_WeavingOrderReport]
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
	@JoborderHeaderId VARCHAR(Max) = NULL	   
as
begin
select WeavMain.SiteId,WeavMain.DivisionId,WeavMain.OrderNO,
WeavMain.Date,WeavMain.duedate,WeavMain.PurjaNo,WeavMain.JobWorkerName,
WeavMain.Design,WeavMain.Qty,WeavMain.DealQty,WeavMain.Rate,WeavMain.UnitName,WeavMain.DecimalPlaces,WeavMain.ProcessName,
WeavMain.ReportName,WeavMain.ReportTitle,WeavMain.Incentive as Incentive,WeavMain.Issuedby,WeavMain.SubReportProcList
  from 
(
SELECT max(H.SiteId) as SiteId, max(H.DivisionId) as DivisionId,
max(DT.DocumentTypeShortName + '-' + H.DocNo) AS OrderNO,
max(H.DocDate) as Date,max(H.DueDate) as duedate,max(CC.CostCenterName) as PurjaNo,
max(PS.Name) AS JobWorkerName,
PG.ProductGroupName as  Design,
sum(L.Qty) as Qty, sum(L.DealQty) AS DealQty,
L.Rate,max(U.UnitName) as UnitName, isnull(max(U.DecimalPlaces),0) AS DecimalPlaces, 
max(PR.ProcessName) as ProcessName,'WeavingOrderReport.rdl' AS ReportName, 
max(PR.ProcessName)+ ' Order Report' AS ReportTitle,
JOLC.Rate as Incentive,
max(Orderby.Name) as Issuedby,NULL AS SubReportProcList
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
LEFT JOIN web.CostCenters CC with (nolock) on CC.CostCenterId=H.CostCenterId
LEFT JOIN [Web]._People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId 
LEFT JOIN [Web]._People Orderby WITH (nolock) ON Orderby.PersonID = H.OrderById 
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN   Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId  
LEFT JOIN Web.Processes PR WITH (nolock) ON PR.ProcessId = H.ProcessId
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
left join (select Rate,LineTableId from  web.joborderlinecharges where ChargeId='31') as JOLC on  JOLC.LineTableId=L.JobOrderLineId
WHERE 1=1  AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
group by PG.ProductGroupName,L.Rate,jolc.rate
) as WeavMain
order by WeavMain.OrderNO,WeavMain.Date
end

GO


