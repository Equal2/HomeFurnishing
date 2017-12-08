IF object_id ('[Web].[ProcSaleOrderBalanceForPlanningRep]') IS NOT NULL 
 DROP Procedure  [Web].[ProcSaleOrderBalanceForPlanningRep]
GO 

CREATE procedure   [Web].[ProcSaleOrderBalanceForPlanningRep]       
@PlanningDocumentType VARCHAR(20) = NULL,	      
@LoginSite VARCHAR(20) = NULL,      
@LoginDivision VARCHAR(20) = NULL,  
@DocumentCategoryId VARCHAR(Max) = NULL,     
@AsOnDate VARCHAR(20) = NULL,                   
@Product VARCHAR(Max) = NULL,     
@ReportType VARCHAR(Max) = NULL,
@ProductCustomGroup VARCHAR(Max) = NULL,
@SaleOrderHeaderId VARCHAR(Max) = NULL 	

as  Begin  

DECLARE @StrfilterContraDocTypes NVARCHAR (Max) 
DECLARE @CondStr NVARCHAR (Max) 

DECLARE @Division VARCHAR(20) = NULL
SET   @Division = @LoginDivision

IF ( @PlanningDocumentType IS NOT NULL AND @PlanningDocumentType <>'' )
BEGIN 
SET @StrfilterContraDocTypes = 
( SELECT filterContraDocTypes  FROM Web.MaterialPlanSettings WHERE DocTypeId = @PlanningDocumentType AND SiteId  = @LoginSite AND DivisionId = @Division )
END 
ELSE
BEGIN  
SELECT @StrfilterContraDocTypes = Max(H.filterContraDocTypes),
@PlanningDocumentType =  Max(H.DocTypeId)  
FROM Web.MaterialPlanSettings H 
LEFT JOIN web.DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId 
WHERE DT.DocumentCategoryId  = @DocumentCategoryId 
AND H.SiteId = @LoginSite
AND (@Division IS NULL or H.DivisionId = @Division)

END 




IF ( @StrfilterContraDocTypes <> '' AND @StrfilterContraDocTypes IS NOT NULL )
BEGIN 
SELECT @LoginSite AS SiteId, @LoginDivision AS DivisionId, VSaleOrder.SaleOrderLineId, IsNull(Sum(VSaleOrder.Qty),0) AS BalanceQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0) AS BalanceAmount, 
Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, Max(VSaleOrder.SaleOrderNo) AS SaleOrderNo,  Max(VSaleOrder.ProductId) AS ProductId, Max(VSaleOrder.BuyerId) AS BuyerId, Max(VSaleOrder.DocDate) AS OrderDate,
Max(SOH.DueDate) AS DueDate, datediff(Day,Max(SOH.DocDate),( CASE WHEN @AsOnDate IS NULL THEN getdate () ELSE @AsOnDate END ) )  AS Ageing,max(SOH.Remark) AS Remark,
Max(P.ProductName) ProductName, Max(U.UnitName) UnitName, Max(U.DecimalPlaces) AS DecimalPlaces, Max(P.ProductGroupId) ProductGroupId,
IsNull(Sum(VSaleOrder.Qty),0)*(SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard] (Max(UC.StandardSizeArea)) ) AS DeliveryQty,
CASE WHEN @ReportType = '2' THEN 'PendingListofSaleOrderPlan_ProductWiseDetail' ELSE 'PendingListofSaleOrderPlan_Detail' END AS ReportName,
'Pending List of Sale Order Plan' AS ReportTitle  
FROM 
( 
SELECT L.SaleOrderLineId, L.Qty , L.Rate , SOH.SaleOrderHeaderId, SOH.DocNo AS SaleOrderNo, L.ProductId, SOH.SaleToBuyerId AS BuyerId, SOH.DocDate 
FROM web.SaleOrderLines L  WITH (Nolock) 
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON L.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON L.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR L.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION ALL 
SELECT L.SaleOrderLineId, - L.Qty, 0 AS Rate, NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderCancelLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION 
ALL SELECT L.SaleOrderLineId, L.Qty, 0 AS Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderQtyAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION ALL 
SELECT L.SaleOrderLineId, 0 AS Qty, L.Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderRateAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))

UNION ALL 
SELECT l.SaleOrderLineId,  - L.Qty, 0 AS Rate ,SOL.SaleOrderHeaderId  AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate  
FROM web.MaterialPlanForSaleOrders  L  
LEFT JOIN web.MaterialPlanHeaders H ON H.MaterialPlanHeaderId  = L.MaterialPlanHeaderId 
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'   AND SOH.Status <> 5 
AND H.DocTypeId = @PlanningDocumentType
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


) AS VSaleOrder 
LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = VSaleOrder.SaleOrderHeaderId
LEFT JOIN web.Products P ON VSaleOrder.ProductId = P.ProductId 
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId 
LEFT JOIN Web.ViewRugSize UC ON UC.ProductId = P.ProductId  
LEFT JOIN Web.Sizes PS ON PS.SizeId = UC.StandardSizeID   
GROUP BY VSaleOrder.SaleOrderLineId
HAVING IsNull(Sum(VSaleOrder.Qty),0) > 0 
END 

ELSE 

BEGIN
SELECT @LoginSite AS SiteId, @LoginDivision AS DivisionId, VSaleOrder.SaleOrderLineId, IsNull(Sum(VSaleOrder.Qty),0) AS BalanceQty, IsNull(Sum(VSaleOrder.Rate),0) AS Rate, IsNull(Sum(VSaleOrder.Qty),0) * IsNull(Sum(VSaleOrder.Rate),0) AS BalanceAmount, 
Max(VSaleOrder.SaleOrderHeaderId) AS SaleOrderHeaderId, Max(VSaleOrder.SaleOrderNo) AS SaleOrderNo,  Max(VSaleOrder.ProductId) AS ProductId, Max(VSaleOrder.BuyerId) AS BuyerId, Max(VSaleOrder.DocDate) AS OrderDate,
Max(P.ProductName) ProductName, Max(U.UnitName) UnitName, Max(U.DecimalPlaces) AS DecimalPlaces, Max(P.ProductGroupId) ProductGroupId, max(SOH.Remark) AS Remark,
Max(SOH.DueDate) AS DueDate, datediff(Day,Max(SOH.DocDate),( CASE WHEN @AsOnDate IS NULL THEN getdate () ELSE @AsOnDate END ) )  AS Ageing,
IsNull(Sum(VSaleOrder.Qty),0)*(SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard] (Max(UC.StandardSizeArea)) ) AS DeliveryQty,
CASE WHEN @ReportType = '2' THEN 'PendingListofSaleOrderPlan_ProductWiseDetail' ELSE 'PendingListofSaleOrderPlan_Detail' END AS ReportName,
'To Do : Sale Order Plan' AS ReportTitle
FROM 
( 
SELECT L.SaleOrderLineId, L.Qty , L.Rate , SOH.SaleOrderHeaderId, SOH.DocNo AS SaleOrderNo, L.ProductId, SOH.SaleToBuyerId AS BuyerId, SOH.DocDate 
FROM web.SaleOrderLines L  WITH (Nolock) 
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON L.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON L.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR L.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION ALL 
SELECT L.SaleOrderLineId, - L.Qty, 0 AS Rate, NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderCancelLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION 
ALL SELECT L.SaleOrderLineId, L.Qty, 0 AS Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderQtyAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION ALL 
SELECT L.SaleOrderLineId, 0 AS Qty, L.Rate , NULL AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate 
FROM web.SaleOrderRateAmendmentLines L  
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  AND SOH.Status <> 5 
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


UNION ALL 
SELECT l.SaleOrderLineId,  - L.Qty, 0 AS Rate ,SOL.SaleOrderHeaderId  AS SaleOrderHeaderId, NULL AS SaleOrderNo, NULL AS ProductId, NULL AS BuyerId, NULL AS DocDate  
FROM web.MaterialPlanForSaleOrders  L  
LEFT JOIN web.MaterialPlanHeaders H ON H.MaterialPlanHeaderId  = L.MaterialPlanHeaderId 
LEFT JOIN web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH WITH (Nolock) ON SOL.SaleOrderHeaderId = SOH.SaleOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON SOL.ProductId = P.ProductId 
WHERE SOH.Status <> 5 AND SOH.DocDate >='01/May/2015'  
AND H.DocTypeId = @PlanningDocumentType
AND (@Division IS NULL or P.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
AND (@StrfilterContraDocTypes IS NULL or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@StrfilterContraDocTypes, ',')))
AND (@SaleOrderHeaderId IS NULL or SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
AND ( @Product is null or SOL.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR SOL.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 


) AS VSaleOrder 
LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = VSaleOrder.SaleOrderHeaderId
LEFT JOIN web.Products P ON VSaleOrder.ProductId = P.ProductId 
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId 
LEFT JOIN Web.ViewRugSize UC ON UC.ProductId = P.ProductId  
LEFT JOIN Web.Sizes PS ON PS.SizeId = UC.StandardSizeID   
GROUP BY VSaleOrder.SaleOrderLineId
HAVING IsNull(Sum(VSaleOrder.Qty),0) > 0 
END  

End
GO
