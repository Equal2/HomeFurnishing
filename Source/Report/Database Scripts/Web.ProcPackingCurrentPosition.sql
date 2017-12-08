

IF OBJECT_ID ('[Web].[ProcPackingCurrentPosition]') IS NOT NULL
	DROP PROCEDURE [Web].[ProcPackingCurrentPosition]	
GO



CREATE procedure  [Web].[ProcPackingCurrentPosition]     
@JobWorker VARCHAR(Max) = NULL,
@TransactionStatusType VARCHAR(20) = NULL, 
@SaleInvoiceHeaderId VARCHAR(20) = NULL,
@PackingHeaderId VARCHAR(20) = NULL,
@ReportType VARCHAR(20) = 'Without Sale Order'
AS  
Begin    

IF ( @ReportType = 'With Sale Order' )
BEGIN 

SELECT   Max(VM.SiteId) AS SiteId, Max(VM.Status) AS Status, Max(VM.DivisionName) AS PackingDivisionName, Max(VM.PackingNo) AS PackingNo, Max(VM.BuyerName) AS BuyerName,  
Max(VM.ProductInvoiceGroupName) AS ProductInvoiceGroupName ,  Max(VM.JobWorkerName) AS  JobWorkerName, Max(VM.SaleOrderNo) AS  SaleOrderNo, 
(SELECT Web.[FGetListofPackingBaleNo] (VM.PackingHeaderId,VM.ProductInvoiceGroupId, VM.SaleOrderHeaderId )) AS BaleNo,

count( DISTINCT VM.BaleNo) AS TotalBale, Sum(VM.Qty) AS Qty , Sum(VM.Area) AS Area,Sum(VM.Weight) AS Weight,
NULL AS SubReportProcList,'PackingCurrentPosition_WithSaleOrder.rdl' AS ReportName,   'Packing Current Position' AS ReportTitle
FROM
(
SELECT H.Status, H.SiteId, H.DivisionId, H.PackingHeaderId,  D.DivisionName , H.DocNo AS PackingNo, B.Code  AS BuyerName,  PIG.ProductInvoiceGroupName, FPD.ProductInvoiceGroupId, H.JobWorkerId, JW.Name AS JobWorkerName ,  
SOH.SaleOrderHeaderId,  SOH.DocNo AS SaleOrderNo, L.BaleNo, L.Qty , L.Qty*UC.StandardSizeArea AS Area, L.Qty*UC.StandardSizeArea*PIG.Weight AS Weight
FROM Web.PackingLines L WITH (Nolock) 
LEFT JOIN web.PackingHeaders H WITH (Nolock) ON H.PackingHeaderId = L.PackingHeaderId 
LEFT JOIN web.Divisions D WITH (Nolock) ON D.DivisionId = H.DivisionId 
LEFT JOIN web.People B WITH (Nolock) ON B.PersonID = H.BuyerId  
LEFT JOIN web.People JW WITH (Nolock) ON JW.PersonID = H.JobWorkerId 
LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.FinishedProduct  FPD WITH (Nolock) ON FPD.ProductId = L.ProductId 
LEFT JOIN Web.ProductInvoiceGroups PIG WITH (Nolock) ON PIG.ProductInvoiceGroupId = FPD.ProductInvoiceGroupId 
LEFT JOIN Web.ViewRugSize UC WITH (Nolock) ON UC.ProductId = L.ProductId  
LEFT JOIN web.SaleDispatchLines SDL WITH (Nolock) ON SDL.PackingLineId = L.PackingLineId 
LEFT JOIN web.SaleInvoiceLines SIL WITH (Nolock) ON SIL.SaleDispatchLineId = SDL.SaleDispatchLineId 
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId  = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOH.SaleOrderHeaderId  = SOL.SaleOrderHeaderId
WHERE L.CreatedDate  IS NOT NULL  
AND H.Status <> 5
--AND H.PackingHeaderId = 3051
AND ( @TransactionStatusType is null or H.Status IN (SELECT Items FROM [dbo].[SplitStatusType] (@TransactionStatusType)))  
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ',')))  
AND ( @SaleInvoiceHeaderId is null or SIL.SaleInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@SaleInvoiceHeaderId, ',')))  
) VM
GROUP BY VM.PackingHeaderId, VM.ProductInvoiceGroupId, VM.SaleOrderHeaderId

END 

ELSE

BEGIN

SELECT   Max(VM.SiteId) AS SiteId, Max(VM.Status) AS Status, Max(VM.DivisionName) AS PackingDivisionName, Max(VM.PackingNo) AS PackingNo, Max(VM.BuyerName) AS BuyerName,  
Max(VM.ProductInvoiceGroupName) AS ProductInvoiceGroupName ,  Max(VM.JobWorkerName) AS  JobWorkerName, '' AS  SaleOrderNo, 
(SELECT Web.[FGetListofPackingBaleNo] (VM.PackingHeaderId,VM.ProductInvoiceGroupId, NULL  )) AS BaleNo,
count( DISTINCT VM.BaleNo) AS TotalBale, Sum(VM.Qty) AS Qty , Sum(VM.Area) AS Area,Sum(VM.Weight) AS Weight,
NULL AS SubReportProcList,'PackingCurrentPosition.rdl' AS ReportName,   'Packing Current Position' AS ReportTitle
FROM
(
SELECT H.Status, H.SiteId, H.DivisionId, H.PackingHeaderId,  D.DivisionName , H.DocNo AS PackingNo, B.Code  AS BuyerName,  PIG.ProductInvoiceGroupName, FPD.ProductInvoiceGroupId, H.JobWorkerId, JW.Name AS JobWorkerName ,  
L.BaleNo, L.Qty , L.Qty*UC.StandardSizeArea AS Area, L.Qty*UC.StandardSizeArea*PIG.Weight AS Weight
FROM Web.PackingLines L WITH (Nolock) 
LEFT JOIN web.PackingHeaders H WITH (Nolock) ON H.PackingHeaderId = L.PackingHeaderId 
LEFT JOIN web.Divisions D WITH (Nolock) ON D.DivisionId = H.DivisionId 
LEFT JOIN web.People B WITH (Nolock) ON B.PersonID = H.BuyerId  
LEFT JOIN web.People JW WITH (Nolock) ON JW.PersonID = H.JobWorkerId 
LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.FinishedProduct  FPD WITH (Nolock) ON FPD.ProductId = L.ProductId 
LEFT JOIN Web.ProductInvoiceGroups PIG WITH (Nolock) ON PIG.ProductInvoiceGroupId = FPD.ProductInvoiceGroupId 
LEFT JOIN Web.ViewRugSize UC WITH (Nolock) ON UC.ProductId = L.ProductId  
LEFT JOIN web.SaleDispatchLines SDL WITH (Nolock) ON SDL.PackingLineId = L.PackingLineId 
LEFT JOIN web.SaleInvoiceLines SIL WITH (Nolock) ON SIL.SaleDispatchLineId = SDL.SaleDispatchLineId 
WHERE L.CreatedDate  IS NOT NULL  
AND H.Status <> 5
--AND H.PackingHeaderId = 3051
AND ( @TransactionStatusType is null or H.Status IN (SELECT Items FROM [dbo].[SplitStatusType] (@TransactionStatusType)))  
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ',')))  
AND ( @SaleInvoiceHeaderId is null or SIL.SaleInvoiceHeaderId IN (SELECT Items FROM [dbo].[Split] (@SaleInvoiceHeaderId, ',')))  
) VM
GROUP BY VM.PackingHeaderId, VM.ProductInvoiceGroupId

END 


End
GO