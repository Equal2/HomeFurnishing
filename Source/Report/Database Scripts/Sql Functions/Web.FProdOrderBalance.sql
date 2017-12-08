IF object_id ('[Web].[FProdOrderBalance]') IS NOT NULL 
DROP FUNCTION [Web].[FProdOrderBalance]
GO 

CREATE FUNCTION Web.FProdOrderBalance(
	@StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
	@Process VARCHAR(20) = NULL,
    @Product VARCHAR(Max) = NULL,  
	@ProdOrderHeaderId VARCHAR(Max) = NULL
)
RETURNS TABLE
AS 

RETURN

SELECT Max(VProdOrder.ProdOrderHeaderId) AS ProdOrderHeaderId,  
Max(VProdOrder.DocTypeId)  DocTypeId, Max(VProdOrder.ProcessId) AS ProcessId, 
Max(VProdOrder.DivisionId)  DivisionId, Max(VProdOrder.SiteId)  SiteId, 
Max(VProdOrder.DocDate) AS OrderDate, Max(VProdOrder.ProdOrderNo) AS OrderNo,  
VProdOrder.ProdOrderLineId, Max(VProdOrder.ProductId) AS ProductId, Max(VProdOrder.Dimension1Id) AS Dimension1Id, 
Max(VProdOrder.Dimension2Id) AS Dimension2Id, 
Max(VProdOrder.Specification) AS Specification, 
IsNull(Sum(VProdOrder.Qty),0) AS BalanceQty
FROM  (  
	SELECT L.ProdOrderLineId, L.Qty ,  H.ProdOrderHeaderId, H.DocTypeId, L.ProcessId,  Dt.DocumentTypeShortName, Dt.DocumentTypeShortName + '-' +  H.DocNo AS ProdOrderNo,  L.ProductId, L.Dimension1Id, L.Specification, L.Dimension2Id, H.DocDate, H.DivisionId, H.SiteId  
	FROM (SELECT H.*
		FROM Web._ProdOrderHeaders H WITH (NoLock)
		WHERE 1=1    
		AND ( @ProdOrderHeaderId IS NULL OR H.ProdOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@ProdOrderHeaderId, ',')))     
		AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
		AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)     
	    AND ( @DocumentType IS NULL OR H.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
		AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
		AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))       
	) AS H
	LEFT JOIN Web._ProdOrderLines L WITH (NoLock) ON L.ProdOrderHeaderId = H.ProdOrderHeaderId
	LEFT JOIN Web.DocumentTypes Dt WITH (NoLock) ON H.DocTypeId = Dt.DocumentTypeId 
	WHERE 1=1
	AND ( @Process IS NULL OR L.ProcessId IN (SELECT Items FROM  Web.[Split] (@Process, ',')))    
	
	
	UNION ALL  
	SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL DocTypeId,  NULL AS ProcessId,  NULL DocumentTypeShortName, NULL AS ProdOrderNo, NULL AS ProductId, NULL AS Dimension1Id, NULL AS Dimension2Id, NULL AS Specification,  NULL AS DocDate, NULL AS DivisionId, NULL AS SiteId  
	FROM  Web._ProdOrderCancelLines L  WITH (NoLock)
	LEFT JOIN Web._ProdOrderCancelHeaders H WITH (NoLock) ON L.ProdOrderCancelHeaderId = H.ProdOrderCancelHeaderId
	LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId
	LEFT JOIN web.ProdOrderHeaders POH ON POH.ProdOrderHeaderId = POL.ProdOrderHeaderId 
	WHERE 1=1 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)     
	AND ( @ProdOrderHeaderId IS NULL OR POH.ProdOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@ProdOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR POH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR POH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR POH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
	AND ( @Process IS NULL OR POL.ProcessId IN (SELECT Items FROM  Web.[Split] (@Process, ',')))   	
  
	UNION ALL	
	SELECT JOL.ProdOrderLineId,  -JOL.Qty, NULL AS ProdOrderHeaderId, NULL DocTypeId, NULL AS ProcessId, NULL DocumentTypeShortName, NULL AS ProdOrderNo, NULL AS ProductId, NULL AS Dimension1Id, NULL AS Dimension2Id, NULL AS Specification,   NULL AS DocDate  , NULL AS DivisionId, NULL AS SiteId   
	FROM  Web._JobOrderLines JOL WITH (NoLock)
	LEFT JOIN Web._JobOrderHeaders JOH WITH (NoLock) ON JOH.JobOrderHeaderId  = JOL.JobOrderHeaderId 
	LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId =JOL.ProdOrderLineId
	LEFT JOIN web.ProdOrderHeaders POH ON POH.ProdOrderHeaderId = POL.ProdOrderHeaderId 
	WHERE 1=1 AND JOL.ProdOrderLineId IS NOT NULL 
	AND ( @StatusOnDate IS NULL OR JOH.DocDate <= @StatusOnDate)   
	AND ( @ProdOrderHeaderId IS NULL OR POH.ProdOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@ProdOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR POH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR POH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR POH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
	AND ( @Process IS NULL OR POL.ProcessId IN (SELECT Items FROM  Web.[Split] (@Process, ',')))   	
	
	UNION ALL  
	SELECT JOL.ProdOrderLineId,  L.Qty, NULL AS ProdOrderHeaderId, NULL DocTypeId, NULL AS ProcessId, NULL DocumentTypeShortName, NULL AS ProdOrderNo, NULL AS ProductId, NULL AS Dimension1Id, NULL AS Dimension2Id, NULL AS Specification,   NULL AS DocDate  , NULL AS DivisionId, NULL AS SiteId   
	FROM  Web._JobOrderCancelLines L  WITH (NoLock)
	LEFT JOIN web._JobOrderLines JOL WITH (NoLock) ON JOL.JobOrderLineId = L.JobOrderLineId 
	LEFT JOIN Web._JobOrderCancelHeaders H WITH (NoLock) ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId
	LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId =JOL.ProdOrderLineId
	LEFT JOIN web.ProdOrderHeaders POH ON POH.ProdOrderHeaderId = POL.ProdOrderHeaderId 
	WHERE 1=1 AND JOL.ProdOrderLineId IS NOT NULL 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)   
	AND ( @ProdOrderHeaderId IS NULL OR POH.ProdOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@ProdOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR POH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR POH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR POH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
	AND ( @Process IS NULL OR POL.ProcessId IN (SELECT Items FROM  Web.[Split] (@Process, ',')))   	
	
	UNION ALL  
	SELECT JOL.ProdOrderLineId,  -JOL.Qty, NULL AS ProdOrderHeaderId, NULL DocTypeId, NULL AS ProcessId, NULL DocumentTypeShortName, NULL AS ProdOrderNo, NULL AS ProductId, NULL AS Dimension1Id, NULL AS Dimension2Id, NULL AS Specification,   NULL AS DocDate  , NULL AS DivisionId, NULL AS SiteId   
	FROM  Web._JobOrderQtyAmendmentLines L  WITH (NoLock) 
	LEFT JOIN web._JobOrderLines JOL WITH (NoLock) ON JOL.JobOrderLineId = L.JobOrderLineId 
	LEFT JOIN Web._JobOrderAmendmentHeaders H WITH (NoLock) ON L.JobOrderAmendmentHeaderId = H.JobOrderAmendmentHeaderId
	LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId =JOL.ProdOrderLineId
	LEFT JOIN web.ProdOrderHeaders POH ON POH.ProdOrderHeaderId = POL.ProdOrderHeaderId 
	WHERE 1=1 AND JOL.ProdOrderLineId IS NOT NULL 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)   
	AND ( @ProdOrderHeaderId IS NULL OR POH.ProdOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@ProdOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR POH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR POH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR POH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
	AND ( @Process IS NULL OR POL.ProcessId IN (SELECT Items FROM  Web.[Split] (@Process, ',')))   	
	
) AS VProdOrder  
WHERE 1=1
AND VProdOrder.ProdOrderLineId IS NOT NULL
AND ( @Product IS NULL OR ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ',')))
GROUP BY VProdOrder.ProdOrderLineId
HAVING IsNull(Sum(VProdOrder.Qty),0) > 0
GO

