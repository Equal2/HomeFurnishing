IF OBJECT_ID ('Web.ViewPurchaseIndentBalance') IS NOT NULL
	DROP VIEW Web.ViewPurchaseIndentBalance
GO

CREATE VIEW [Web].[ViewPurchaseIndentBalance] AS 
SELECT VPurchaseIndent.PurchaseIndentLineId, IsNull(Sum(VPurchaseIndent.Qty),0) AS BalanceQty,  
Max(VPurchaseIndent.PurchaseIndentHeaderId) AS PurchaseIndentHeaderId,  
Max(VPurchaseIndent.PurchaseIndentNo) AS PurchaseIndentNo,  
Max(VPurchaseIndent.ProductId) AS ProductId, Max(VPurchaseIndent.DocDate) AS IndentDate, 
Max(VPurchaseIndent.DocTypeId) AS DocTypeId, Max(VPurchaseIndent.DivisionId) AS DivisionId,
Max(VPurchaseIndent.SiteId) AS SiteId
FROM  (  
	SELECT L.PurchaseIndentLineId, L.Qty , H.PurchaseIndentHeaderId, H.DocNo AS PurchaseIndentNo,  
	L.ProductId, H.DocDate, H.DocTypeId, H.DivisionId, H.SiteId    
	FROM  Web.PurchaseIndentLines L   
	LEFT JOIN Web.PurchaseIndentHeaders H ON L.PurchaseIndentHeaderId = H.PurchaseIndentHeaderId  
	UNION ALL  
	SELECT L.PurchaseIndentLineId, - L.Qty, NULL AS PurchaseIndentHeaderId, NULL AS PurchaseIndentNo, NULL AS ProductId,  NULL AS DocDate , NULL DocTypeId, NULL DivisionId, NULL SiteId
	FROM  Web.PurchaseIndentCancelLines L   
	UNION ALL  
	SELECT L.PurchaseIndentLineId,  - L.Qty, NULL AS PurchaseIndentHeaderId, NULL AS PurchaseIndentNo, NULL AS ProductId,  NULL AS DocDate, NULL DocTypeId   , NULL DivisionId, NULL SiteId
	FROM  Web.PurchaseOrderLines L   
	UNION ALL 

	SELECT POL.PurchaseIndentLineId,   sum(POL.Qty) AS Qty, NULL AS PurchaseIndentHeaderId, NULL AS PurchaseIndentNo, NULL AS ProductId,  NULL AS DocDate, NULL DocTypeId   , NULL DivisionId, NULL SiteId
	FROM web.PurchaseOrderCancelLines POCL
	LEFT JOIN web.PurchaseOrderLines POL ON POCL.PurchaseOrderLineId = POL.PurchaseOrderLineId 
	WHERE POL.PurchaseIndentLineId IS NOT NULL 
	GROUP BY POL.PurchaseIndentLineId 
	
	) AS VPurchaseIndent  
GROUP BY VPurchaseIndent.PurchaseIndentLineId
HAVING IsNull(Sum(VPurchaseIndent.Qty),0) > 0
GO

