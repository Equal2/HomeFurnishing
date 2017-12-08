IF OBJECT_ID ('[Web].[spGatePassPurchaseGoodsReturn]') IS NOT NULL
	DROP PROCEDURE [Web].[spGatePassPurchaseGoodsReturn]	
GO


Create PROCEDURE Web.spGatePassPurchaseGoodsReturn
@Id INT 
AS 
Begin

SELECT P.ProductName, GL.Specification, sum(L.Qty) AS Qty, Max(P.UnitId) AS UnitId  
FROM Web.PurchaseGoodsReturnLines L
LEFT JOIN web.PurchaseGoodsReceiptLines GL ON L.PurchaseGoodsReceiptLineId = GL.PurchaseGoodsReceiptLineId 
LEFT JOIN WEB.Products P ON GL.ProductId = P.ProductId 
WHERE L.PurchaseGoodsReturnHeaderId =@Id
GROUP BY P.ProductName, GL.Specification
End
GO