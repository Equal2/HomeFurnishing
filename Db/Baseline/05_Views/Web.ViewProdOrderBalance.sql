IF OBJECT_ID ('Web.ViewProdOrderBalance') IS NOT NULL
	DROP VIEW Web.ViewProdOrderBalance
GO

CREATE VIEW [Web].[ViewProdOrderBalance] AS 
SELECT VProdOrder.ProdOrderLineId, IsNull(Sum(VProdOrder.Qty),0) AS BalanceQty, 
Max(VProdOrder.ProdOrderHeaderId) AS ProdOrderHeaderId,  Max(VProdOrder.ProdOrderNo) AS ProdOrderNo,  Max(VProdOrder.DocTypeId) AS DocTypeId,Max(VProdOrder.SiteId) AS SiteId,Max(VProdOrder.DivisionId) AS DivisionId,
Max(VProdOrder.ProductId) AS ProductId, Max(VProdOrder.DocDate) AS IndentDate  , Max(VProdOrder.Dimension1Id) AS Dimension1Id , Max(VProdOrder.Dimension2Id) AS Dimension2Id 
FROM  
(  
SELECT L.ProdOrderLineId, L.Qty , H.ProdOrderHeaderId, H.DocNo AS ProdOrderNo,  L.ProductId, H.DocDate  ,H.SiteId,H.DocTypeId,H.DivisionId , L.Dimension1Id, L.Dimension2Id
FROM  Web.ProdOrderLines L   
LEFT JOIN Web.ProdOrderHeaders H ON L.ProdOrderHeaderId = H.ProdOrderHeaderId  
--WHERE H.DocDate >='01/May/2015'
UNION ALL  
SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProdOrderNo, NULL AS ProductId,  NULL AS DocDate  ,  NULL AS SiteId,  NULL AS DocTypeId,  NULL AS DivisionId,  NULL AS Dimension1Id,  NULL AS Dimension2Id
FROM  Web.ProdOrderCancelLines L   
LEFT JOIN Web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId    
LEFT JOIN Web.ProdOrderHeaders POH ON POL.ProdOrderHeaderId = POH.ProdOrderHeaderId  
--WHERE POH.DocDate >='01/May/2015'

UNION ALL  
SELECT L.ProdOrderLineId,  - L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProdOrderNo, NULL AS ProductId,  NULL AS DocDate   ,  NULL AS SiteId,  NULL AS DocTypeId,  NULL AS DivisionId,  NULL AS Dimension1Id,  NULL AS Dimension2Id
FROM  Web.JobOrderLines L   
LEFT JOIN Web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId    
LEFT JOIN Web.ProdOrderHeaders POH ON POL.ProdOrderHeaderId = POH.ProdOrderHeaderId  
--WHERE POH.DocDate >='01/May/2015'


UNION ALL 
SELECT JOL.ProdOrderLineId,  L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProdOrderNo, NULL AS ProductId,  NULL AS DocDate   ,  NULL AS SiteId,  NULL AS DocTypeId,  NULL AS DivisionId,  NULL AS Dimension1Id,  NULL AS Dimension2Id
FROM  Web._JobOrderCancelLines L  WITH (NoLock)
LEFT JOIN web._JobOrderLines JOL WITH (NoLock) ON JOL.JobOrderLineId = L.JobOrderLineId 
LEFT JOIN Web._JobOrderCancelHeaders H WITH (NoLock) ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId
LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId =JOL.ProdOrderLineId
LEFT JOIN Web.ProdOrderHeaders POH ON POL.ProdOrderHeaderId = POH.ProdOrderHeaderId  
--WHERE POH.DocDate >='01/May/2015'
	
UNION ALL  
SELECT JOL.ProdOrderLineId,  -L.Qty, NULL AS ProdOrderHeaderId, NULL AS ProdOrderNo, NULL AS ProductId,  NULL AS DocDate   ,  NULL AS SiteId,  NULL AS DocTypeId,  NULL AS DivisionId,  NULL AS Dimension1Id,  NULL AS Dimension2Id
FROM  Web._JobOrderQtyAmendmentLines L  WITH (NoLock) 
LEFT JOIN web._JobOrderLines JOL WITH (NoLock) ON JOL.JobOrderLineId = L.JobOrderLineId 
LEFT JOIN Web._JobOrderAmendmentHeaders H WITH (NoLock) ON L.JobOrderAmendmentHeaderId = H.JobOrderAmendmentHeaderId
LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId =JOL.ProdOrderLineId
LEFT JOIN web.ProdOrderHeaders POH ON POH.ProdOrderHeaderId = POL.ProdOrderHeaderId 
--WHERE POH.DocDate >='01/May/2015'

	
) AS VProdOrder  
GROUP BY VProdOrder.ProdOrderLineId
HAVING IsNull(Sum(VProdOrder.Qty),0) > 0
GO

