IF OBJECT_ID ('[Web].[ProcSaleOrderBalanceForPlanning]') IS NOT NULL
	DROP PROCEDURE [Web].[ProcSaleOrderBalanceForPlanning]	
GO


Create procedure  [Web].[ProcSaleOrderBalanceForPlanning]       
@PlanningDocumentType VARCHAR(20) = NULL,	      
@Site VARCHAR(20) = NULL,      
@Division VARCHAR(20) = NULL  
as  Begin  

DECLARE @StrfilterContraDocTypes NVARCHAR (Max) 
DECLARE @CondStr NVARCHAR (Max) 

SET @StrfilterContraDocTypes = 
( SELECT filterContraDocTypes  FROM Web.MaterialPlanSettings WHERE DocTypeId = @PlanningDocumentType AND SiteId  = @Site AND DivisionId = @Division )

IF ( @StrfilterContraDocTypes <> '' )
BEGIN 
SELECT VSaleOrder.SaleOrderLineId, IsNull(Sum(VSaleOrder.Qty),0) AS BalanceQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0) AS BalanceAmount, 
Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, Max(VSaleOrder.SaleOrderNo) AS SaleOrderNo,  Max(VSaleOrder.ProductId) AS ProductId,  Max(VSaleOrder.Specification) AS Specification, Max(VSaleOrder.BuyerId) AS BuyerId, Max(VSaleOrder.DocDate) AS OrderDate,
Max(P.ProductName) ProductName, Max(U.UnitName) UnitName, Max(P.ProductGroupId) ProductGroupId  
FROM 
( 
SELECT L.SaleOrderLineId, L.Qty , L.Rate , H.SaleOrderHeaderId, H.DocNo AS SaleOrderNo, L.ProductId, L.Specification, H.SaleToBuyerId AS BuyerId, H.DocDate 
FROM web.SaleOrderLines L  
LEFT JOIN web.SaleOrderHeaders H ON L.SaleOrderHeaderId = H.SaleOrderHeaderId 
WHERE  1=1 
AND H.DocDate >='01/May/2015' 
AND H.Status <> 5 
UNION ALL 
SELECT L.SaleOrderLineId, - L.Qty, 0 AS Rate, NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderCancelLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE  1=1 
AND SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 

UNION 
ALL SELECT L.SaleOrderLineId, L.Qty, 0 AS Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderQtyAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE 1=1 
AND SOH.DocDate >='01/May/2015'
AND SOH.Status <> 5 

UNION ALL 
SELECT L.SaleOrderLineId, 0 AS Qty, L.Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderRateAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE 1=1
AND  SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 

UNION ALL 
SELECT l.SaleOrderLineId,  - L.Qty, 0 AS Rate ,SOL.SaleOrderHeaderId  AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate  
FROM web.MaterialPlanForSaleOrders  L  
LEFT JOIN web.MaterialPlanHeaders H ON H.MaterialPlanHeaderId  = L.MaterialPlanHeaderId 
LEFT JOIN Web.SaleOrderLines SOL ON SOL.SaleOrderLineId  = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE H.DocTypeId = @PlanningDocumentType
AND SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 

) AS VSaleOrder 
LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = VSaleOrder.SaleOrderHeaderId
LEFT JOIN web.Products P ON VSaleOrder.ProductId = P.ProductId 
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId 
--WHERE SOH.Status <> 5 --# Where Status <> 'Closed'
--AND  SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')) 
GROUP BY VSaleOrder.SaleOrderLineId
HAVING IsNull(Sum(VSaleOrder.Qty),0) > 0 
AND max(SOH.Status) <> 5 --# Where Status <> 'Closed'
AND max(P.DivisionId) = @Division
AND max(SOH.DocTypeId) IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')) 

END 


ELSE --In case Contra doc types are defined



BEGIN
SELECT VSaleOrder.SaleOrderLineId, IsNull(Sum(VSaleOrder.Qty),0) AS BalanceQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0) AS BalanceAmount, 
Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, Max(VSaleOrder.SaleOrderNo) AS SaleOrderNo,  Max(VSaleOrder.ProductId) AS ProductId,  Max(VSaleOrder.Specification) AS Specification, Max(VSaleOrder.ProductId) AS ProductId, Max(VSaleOrder.BuyerId) AS BuyerId, Max(VSaleOrder.DocDate) AS OrderDate,
Max(P.ProductName) ProductName, Max(U.UnitName) UnitName, Max(P.ProductGroupId) ProductGroupId 
FROM 
( 
SELECT L.SaleOrderLineId, L.Qty , L.Rate , H.SaleOrderHeaderId, H.DocNo AS SaleOrderNo, L.ProductId, L.Specification , H.SaleToBuyerId AS BuyerId, H.DocDate 
FROM web.SaleOrderLines L  
LEFT JOIN web.SaleOrderHeaders H ON L.SaleOrderHeaderId = H.SaleOrderHeaderId 
WHERE  1=1  
AND H.DocDate >='01/May/2015' 
AND H.Status <> 5 
UNION ALL 
SELECT L.SaleOrderLineId, - L.Qty, 0 AS Rate, NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderCancelLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE  1=1  
AND SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 

UNION 
ALL SELECT L.SaleOrderLineId, L.Qty, 0 AS Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderQtyAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE 1=1 
AND SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 

UNION ALL 
SELECT L.SaleOrderLineId, 0 AS Qty, L.Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderRateAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE 1=1
AND SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 

UNION ALL 
SELECT l.SaleOrderLineId,  - L.Qty, 0 AS Rate ,SOL.SaleOrderHeaderId AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL Specification, NULL AS BuyerId, NULL AS DocDate  
FROM web.MaterialPlanForSaleOrders  L  
LEFT JOIN web.MaterialPlanHeaders H ON H.MaterialPlanHeaderId  = L.MaterialPlanHeaderId 
LEFT JOIN Web.SaleOrderLines SOL ON SOL.SaleOrderLineId  = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
WHERE H.DocTypeId = @PlanningDocumentType
AND SOH.DocDate >='01/May/2015' 
AND SOH.Status <> 5 
) AS VSaleOrder 
LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = VSaleOrder.SaleOrderHeaderId
LEFT JOIN web.Products P ON VSaleOrder.ProductId = P.ProductId 
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId 
--WHERE SOH.Status <> 5  
GROUP BY VSaleOrder.SaleOrderLineId
HAVING IsNull(Sum(VSaleOrder.Qty),0) > 0 
AND max(SOH.Status) <> 5 --# Where Status <> 'Closed'
AND max(P.DivisionId) = @Division
AND max(SOH.DocTypeId) IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')) 
END  
End
GO

