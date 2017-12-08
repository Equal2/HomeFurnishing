IF OBJECT_ID ('Web.ViewPurchaseOrderBalance') IS NOT NULL
	DROP VIEW Web.ViewPurchaseOrderBalance
GO

CREATE VIEW [Web].[ViewPurchaseOrderBalance] AS 
SELECT VPurchaseOrder.PurchaseOrderLineId, Max(PurchaseIndentLineId) AS PurchaseIndentLineId, IsNull(Sum(VPurchaseOrder.Qty),0) AS BalanceQty, IsNull(Sum(VPurchaseOrder.Rate),0) AS Rate,  IsNull(Sum(VPurchaseOrder.Qty),0) * IsNull(Sum(VPurchaseOrder.Rate),0) AS BalanceAmount, Max(VPurchaseOrder.PurchaseOrderHeaderId) AS PurchaseOrderHeaderId,  
Max(VPurchaseOrder.PurchaseOrderNo) AS PurchaseOrderNo,  Max(VPurchaseOrder.ProductId) AS ProductId, Max(VPurchaseOrder.SupplierId) AS SupplierId,  Max(VPurchaseOrder.DocDate) AS OrderDate, Max(VPurchaseOrder.DueDate) AS DueDate, Max(VPurchaseOrder.docTypeId)  DocTypeId,
Max(VPurchaseOrder.DivisionId)  DivisionId, Max(VPurchaseOrder.SiteId)  SiteId
FROM  (  
SELECT L.PurchaseOrderLineId, L.PurchaseIndentLineId,  L.Qty , L.Rate , H.PurchaseOrderHeaderId, H.DocTypeId, Dt.DocumentTypeShortName, H.DocNo AS PurchaseOrderNo,  L.ProductId, H.SupplierId AS SupplierId, H.DocDate, H.DueDate, H.DivisionId, H.SiteId
FROM  Web.PurchaseOrderLines L   
LEFT JOIN Web.PurchaseOrderHeaders H ON L.PurchaseOrderHeaderId = H.PurchaseOrderHeaderId
LEFT JOIN Web.DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId  
UNION ALL  
SELECT L.PurchaseOrderLineId, NULL AS PurchaseIndentLineId, - L.Qty, 0 AS Rate, NULL AS PurchaseOrderHeaderId, NULL DocTypeId, NULL DocumentTypeShortName, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  NULL AS DocDate  , NULL AS DueDate, NULL DivisionId, NULL SiteId
FROM  Web.PurchaseOrderCancelLines L   
UNION ALL  
SELECT L.PurchaseOrderLineId, NULL AS PurchaseIndentLineId, L.Qty, 0 AS Rate , NULL AS PurchaseOrderHeaderId, NULL DocTypeId, NULL DocumentTypeShortName, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  NULL AS DocDate  , NULL AS DueDate, NULL DivisionId, NULL SiteId
FROM  Web.PurchaseOrderQtyAmendmentLines L   
UNION ALL  
SELECT L.PurchaseOrderLineId, NULL AS PurchaseIndentLineId, 0 AS Qty, L.Rate , NULL AS PurchaseOrderHeaderId, NULL DocTypeId, NULL DocumentTypeShortName, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  NULL AS DocDate  ,  NULL AS DueDate, NULL DivisionId, NULL SiteId
FROM  Web.PurchaseOrderRateAmendmentLines L   
UNION ALL  
SELECT L.PurchaseOrderLineId, NULL AS PurchaseIndentLineId, - L.Qty, 0 AS Rate ,NULL AS PurchaseOrderHeaderId, NULL DocTypeId, NULL DocumentTypeShortName, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  NULL AS DocDate   ,  NULL AS DueDate, NULL DivisionId, NULL SiteId
FROM  Web.PurchaseGoodsReceiptLines L   
UNION All
SELECT L.PurchaseOrderLineId, NULL AS PurchaseIndentLineId,  RL.Qty, 0 AS Rate ,NULL AS PurchaseOrderHeaderId, NULL DocTypeId, NULL DocumentTypeShortName, NULL AS PurchaseOrderNo, NULL AS ProductId, NULL AS SupplierId,  NULL AS DocDate   ,  NULL AS DueDate, NULL DivisionId, NULL SiteId
FROM  Web.PurchaseGoodsReturnLines RL 
LEFT Join Web.PurchaseGoodsReceiptLines L   ON L.PurchaseGoodsReceiptLineId = rl.PurchaseGoodsReceiptLineId 

) AS VPurchaseOrder  GROUP BY VPurchaseOrder.PurchaseOrderLineId
GO





