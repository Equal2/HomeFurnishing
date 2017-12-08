


IF OBJECT_ID ('Web.ViewJobOrderBalance') IS NOT NULL
	DROP VIEW Web.ViewJobOrderBalance
GO

CREATE VIEW [Web].[ViewJobOrderBalance] AS 
SELECT VJobOrder.JobOrderLineId, IsNull(Sum(VJobOrder.Qty),0) AS BalanceQty, IsNull(Sum(VJobOrder.Rate),0) AS Rate,  IsNull(Sum(VJobOrder.Qty),0) * IsNull(Sum(VJobOrder.Rate),0) AS BalanceAmount, Max(VJobOrder.JobOrderHeaderId) AS JobOrderHeaderId,  Max(VJobOrder.JobOrderNo) AS JobOrderNo,  Max(VJobOrder.ProductId) AS ProductId, Max(VJobOrder.Dimension1Id) Dimension1Id, Max(VJobOrder.Dimension2Id) Dimension2Id, 
Max(VJobOrder.JobWorkerId) AS JobWorkerId,  Max(VJobOrder.DocDate) AS OrderDate  
FROM  (  
	SELECT L.JobOrderLineId, H.SiteId, H.DivisionId, L.Qty , L.Rate , H.JobOrderHeaderId, H.DocNo AS JobOrderNo,  L.ProductId, L.Dimension1Id, L.Dimension2Id, H.JobWorkerId, H.DocDate  
	FROM  Web.JobOrderLines L   
	LEFT JOIN Web.JobOrderHeaders H ON L.JobOrderHeaderId = H.JobOrderHeaderId  
	UNION ALL  
	SELECT L.JobOrderLineId, NULL SiteId, NULL AS DivisionId, - L.Qty, 0 AS Rate, NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL AS JobWorkerId,  NULL AS DocDate  
	FROM  Web.JobOrderCancelLines L   
	UNION ALL  
	SELECT L.JobOrderLineId, NULL SiteId, NULL AS DivisionId, L.Qty, 0 AS Rate , NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL AS JobWorkerId,  NULL AS DocDate  
	FROM  Web.JobOrderQtyAmendmentLines L   
	UNION ALL  
	SELECT L.JobOrderLineId, NULL SiteId, NULL AS DivisionId, 0 AS Qty, L.Rate , NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL AS JobWorkerId,  NULL AS DocDate  
	FROM  Web.JobOrderRateAmendmentLines L   
	UNION ALL  
	SELECT L.JobOrderLineId,  NULL SiteId, NULL AS DivisionId, - (L.Qty+L.LossQty), 0 AS Rate ,NULL AS JobOrderHeaderId, NULL AS JobOrderNo, NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL AS JobWorkerId,  NULL AS DocDate   
	FROM  Web.JobReceiveLines L   
	) AS VJobOrder  
GROUP BY VJobOrder.JobOrderLineId
GO








