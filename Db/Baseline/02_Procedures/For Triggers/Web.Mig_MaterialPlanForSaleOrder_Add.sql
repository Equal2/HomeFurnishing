IF OBJECT_ID ('[Web].[Mig_MaterialPlanForSaleOrder_Add]') IS NOT NULL
	DROP PROCEDURE [Web].[Mig_MaterialPlanForSaleOrder_Add]	
GO


Create PROCEDURE [Web].[Mig_MaterialPlanForSaleOrder_Add]
	@Id INT 
AS 
BEGIN 

INSERT INTO dbo.MaterialPlanForDetail 
	(
DocId, 
Sr, 
Item, 
Qty, 
Unit, 
MeasurePerPcs, 
TotalMeasure, 
MeasureUnit, 
UID, 
ProdOrder, 
ProdOrderSr, 
Specification, 
MaterialPlan, 
MaterialPlanSr, 
BOM, 
OMSId
	)
SELECT 
MP.DocId,
(SELECT IsNull(Max(Sr),0) + 1 FROM MaterialPlanForDetail WHERE DocId = MP.DocId) As Sr,
I.Code AS item,
L.Qty AS Qty, 
I.Unit AS Unit, 
I.Measure AS MeasurePerPcs, 
I.Measure*L.Qty AS TotalMeasure, 
I.MeasureUnit AS MeasureUnit, 
NULL AS UID, 
NULL AS ProdOrder, 
NULL AS ProdOrderSr, 
NULL AS Specification, 
MPD.DocId AS MaterialPlan, 
MPD.Sr AS MaterialPlanSr, 
NULL AS BOM, 
'Material Plan For Sale Order-' + Convert(VARCHAR,L.MaterialPlanForSaleOrderId ) AS OMSId
FROM Web.MaterialPlanForSaleOrders L
LEFT JOIN Web.MaterialPlanHeaders H ON L.MaterialPlanHeaderId  = H.MaterialPlanHeaderId
LEFT JOIN dbo.MaterialPlan MP ON 'Material Plan-' + Convert(Varchar,H.MaterialPlanHeaderId) = MP.OMSId
LEFT JOIN dbo.SaleOrderDetail SOD ON 'Sale Order-' + Convert(Varchar,L.SaleOrderLineId) = SOD.OMSId
LEFT JOIN dbo.Item I On  'Product-' + Convert(Varchar,SOD.Item) = I.OMSId 
LEFT JOIN dbo.MaterialPlanDetail MPD ON 'Material Plan-' + Convert(Varchar,L.MaterialPlanLineId) = MPD.OMSId 
WHERE L.MaterialPlanForSaleOrderId   = @Id

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


--  To Update Sale Order In Prod Order Detail
UPDATE ProdOrderDetail SET 
ProdOrderDetail.SaleOrder = V.SaleOrder,
ProdOrderDetail.SaleOrderSr = V.SaleOrderSr    
FROM 
( 
SELECT SOD.DocId AS SaleOrder, SOD.Sr AS SaleOrderSr, POD.DocId AS ProdOrderDocId, POD.Sr AS ProdOrderDocIdSr
FROM Web.MaterialPlanForSaleOrders  L WITH (Nolock)
LEFT JOIN web.ProdOrderLines POL  WITH (Nolock) ON POL.MaterialPlanLineId = L.MaterialPlanLineId
LEFT JOIN ProdOrderDetail POD  WITH (Nolock) ON 'Prod Order-' + Convert(Varchar,POL.ProdOrderLineId ) = POD.OMSId
LEFT JOIN SaleOrderDetail  SOD  WITH (Nolock) ON SOD.OMSId = 'Sale Order-' + convert(NVARCHAR,L.SaleOrderLineId) 
WHERE  L.MaterialPlanForSaleOrderId =@Id 
) V
WHERE ProdOrderDetail.DocId = V.ProdOrderDocId AND ProdOrderDetail.Sr = V.ProdOrderDocIdSr

End
GO

