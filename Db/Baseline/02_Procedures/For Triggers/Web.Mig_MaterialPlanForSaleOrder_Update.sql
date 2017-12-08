IF OBJECT_ID ('[Web].[Mig_MaterialPlanForSaleOrder_Update]') IS NOT NULL
	DROP PROCEDURE [Web].[Mig_MaterialPlanForSaleOrder_Update]	
GO


Create PROCEDURE [Web].[Mig_MaterialPlanForSaleOrder_Update]
	@Id INT 
AS 
BEGIN 
UPDATE dbo.MaterialPlanForDetail
SET 
item=I.Code,
Qty = L.Qty, 
Unit = I.Unit , 
MeasurePerPcs = I.Measure , 
TotalMeasure = I.Measure*L.Qty , 
MeasureUnit = I.MeasureUnit , 
MaterialPlan = MPD.DocId  , 
MaterialPlanSr = MPD.Sr  
FROM Web.MaterialPlanForSaleOrders L
LEFT JOIN Web.MaterialPlanHeaders H ON L.MaterialPlanHeaderId  = H.MaterialPlanHeaderId
LEFT JOIN dbo.MaterialPlan MP ON 'Material Plan-' + Convert(Varchar,H.MaterialPlanHeaderId) = MP.OMSId
LEFT JOIN dbo.SaleOrderDetail SOD ON 'Sale Order-' + Convert(Varchar,L.SaleOrderLineId) = SOD.OMSId
LEFT JOIN dbo.Item I On  'Product-' + Convert(Varchar,SOD.Item) = I.OMSId 
LEFT JOIN dbo.MaterialPlanDetail MPD ON 'Material Plan-' + Convert(Varchar,L.MaterialPlanLineId) = MPD.OMSId 
WHERE MaterialPlanForDetail.OMSId = 'Material Plan For Sale Order-' + Convert(VARCHAR,@Id)
And L.MaterialPlanForSaleOrderId    = @Id



--  To Update Sale Order In Material Plan Detail
UPDATE  MaterialplanDetail SET 
MaterialplanDetail.SaleOrder = V.SaleOrder,
MaterialplanDetail.SaleOrderSr = V.SaleOrderSr    
FROM 
( 
SELECT SOD.DocId AS SaleOrder, SOD.Sr AS SaleOrderSr, MPD.DocId AS MaterialplanDocId, MPD.Sr AS MaterialplanDocSr
FROM Web.MaterialPlanForSaleOrders  L 
LEFT JOIN MaterialplanDetail MPD ON 'Material Plan-' + Convert(Varchar,L.MaterialPlanLineId) = MPD.OMSId
LEFT JOIN SaleOrderDetail  SOD ON SOD.OMSId = 'Sale Order-' + convert(NVARCHAR,L.SaleOrderLineId) 
WHERE  L.MaterialPlanForSaleOrderId =@Id 
) V
WHERE MaterialPlanDetail.DocId = V.MaterialplanDocId AND MaterialPlanDetail.Sr = V.MaterialplanDocSr

End
GO

