IF OBJECT_ID ('[Web].[Mig_JobOrderCancelLine_Add]') IS NOT NULL
	DROP PROCEDURE [Web].[Mig_JobOrderCancelLine_Add]	
GO


Create PROCEDURE [Web].[Mig_JobOrderCancelLine_Add]
	@Id INT 
AS 
BEGIN 
INSERT INTO dbo.JobOrderDetail (
DocId, 
Sr, 
Item, 
Qty, 
Unit, 
MeasurePerPcs, 
TotalMeasure, 
MeasureUnit, 
ProdPlan, 
Loss, 
Rate, 
Incentive, 
ReceiveQty, 
ReceiveMeasure, 
UID, 
ProdOrder, 
Amount, 
BOM, 
CancelQty, 
CancelMeasure, 
LossPer, 
ReceiveLoss, 
ReceiveLossMeasure, 
JobMaterialPer, 
JobMaterialQty, 
InvoiceQty, 
InvoiceMeasure, 
InvoiceAmount, 
PerimeterPerPcs, 
TotalPerimeter, 
FromProcess, 
CurrStock, 
Perimeter, 
JobWorkerRateGroup, 
Remark, 
Design, 
Colour, 
Specification, 
IncentivePer, 
Quality, 
JobOrder, 
JobOrderSr, 
Item_UID, 
TransactionNature, 
ProcessOnPerimeter, 
ProdPlanSr, 
ProdOrderSr, 
DocQty, 
TotalDocMeasure, 
TotalDocPerimeter, 
V_Nature, 
LotNo, 
Dimension1, 
Dimension2, 
T_Nature, 
DyedOnItem, 
IncentiveRate, 
OMSId, 
Sub_Total)

SELECT Jo.DocId AS DocId,
(SELECT IsNull(Max(Sr),0) + 1 FROM JobOrderDetail WHERE DocId = Jo.DocId) As Sr,
IsNull(Ys.Code,I.Code) AS Item,
-L.Qty AS Qty, 
I.Unit AS Unit, 
I.Measure AS MeasurePerPcs, 
-L.Qty * I.Measure AS TotalMeasure, 
I.MeasureUnit AS MeasureUnit, 
NULL AS ProdPlan,
NULL AS Loss, 
Jol.Rate AS Rate, 
--Incentive 
NULL AS Incentive,
NULL AS ReceiveQty, 
NULL AS ReceiveMeasure, 
NULL AS UID, 
Pod.DocId AS ProdOrder , 
-L.Qty * I.Measure * Jol.Rate AS Amount, 
--Bom 
NULL AS Bom,
NULL AS CancelQty, 
NULL AS CancelMeasure, 
NULL AS LossPer, 
NULL AS ReceiveLoss, 
NULL AS ReceiveLossMeasure, 
NULL AS JobMaterialPer, 
NULL AS JobMaterialQty, 
NULL AS InvoiceQty, 
NULL AS InvoiceMeasure, 
NULL AS InvoiceAmount, 
--PerimeterPerPcs
NULL AS PerimeterPerPcs,
--TotalPerimeter
NULL AS TotalPerimeter,
P.NCat AS FromProcess, 
NULL AS CurrStock, 
--Perimeter
NULL AS Perimeter,
--JobWorkerRateGroup, 
NULL AS JobWorkerRateGroup,
L.Remark AS Remark, 
D.Code AS Design, 
--Colour
NULL AS Colour,
Jol.Specification AS Specification, 
--IncentivePer, 
NULL AS IncentivePer,
Quality = Q.Code, 
--Jo.DocId AS JobOrder,
--(SELECT IsNull(Max(Sr),0) + 1 FROM JobOrderDetail WHERE DocId = Jo.DocId) As JobOrderSr,
DJO.DocId AS JobOrder,
DJO.Sr  AS JobOrderSr,
Item_UID = Iu.Code, 
--TransactionNature
NULL AS TransactionNature,
--ProcessOnPerimeter
NULL AS ProcessOnPerimeter,
NULL AS ProdPlanSr, 
Pod.Sr ProdOrderSr, 
DocQty = L.Qty, 
TotalDocMeasure = L.Qty * I.Measure, 
--TotalDocPerimeter
NULL AS TotalDocPerimeter,
NULL AS V_Nature, 
Jol.LotNo AS LotNo,
NULL AS Dimension1, 
NULL AS Dimension2, 
NULL AS T_Nature, 
NULL AS DyedOnItem, 
--IncentiveRate
NULL AS IncentiveRate,
'Job Order Cancel-' + Convert(VARCHAR,L.JobOrderLineId) AS OMSId,
NULL AS Sub_Total
FROM Web.JobOrderCancelLines  L
LEFT JOIN Web.JobOrderCancelHeaders H ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId
LEFT JOIN Web.JobOrderLines Jol ON L.JobOrderLineId = Jol.JobOrderLineId
LEFT JOIN Web.Dimension2 D2 ON Jol.Dimension2Id = D2.Dimension2Id
LEFT JOIN Web.FinishedProduct Fp ON Jol.ProductId = Fp.ProductId
LEFT JOIN JobOrder Jo ON 'Job Order Cancel-' + Convert(Varchar,H.JobOrderCancelHeaderId) = Jo.OMSId
LEFT JOIN JobOrderDetail DJO ON 'Job Order-' + Convert(Varchar,L.JobOrderLineId) = DJO.OMSId
LEFT JOIN Item I On  'Product-' + Convert(Varchar,Jol.ProductId) = I.OMSId 
LEFT JOIN RUG_SHADE S  ON 'Dimension1-' +  Convert(Varchar,Jol.Dimension1Id) = S.OMSId 
LEFT JOIN RUG_YarnSKU Ys ON I.Code = Ys.Yarn AND S.Code = Ys.SHADE
LEFT JOIN ProdOrderDetail Pod ON 'Prod Order-'  + Convert(Varchar,Jol.ProdOrderLineId) = Pod.OMSId 
LEFT JOIN Process P ON 'Process-' + Convert(Varchar,Jol.FromProcessId) = P.OMSId 
LEFT JOIN RUG_Design D ON D2.Dimension2Name = D.Description
LEFT JOIN RUG_Quality Q ON 'Quality-' + Convert(Varchar,Fp.ProductQualityId) = Q.OMSId
LEFT JOIN Item_UID Iu ON 'Product Uid-' + Convert(Varchar,Jol.ProductUidId) = Iu.OMSId
WHERE L.JobOrderCancelLineId  = @Id
End
GO










ALTER PROCEDURE [Web].[Mig_JobOrderCancelLine_Update]
	@Id INT 
AS 
BEGIN 

UPDATE JobOrderDetail
SET  Item = IsNull(Ys.Code,I.Code),
Qty = - L.Qty, 
Unit = I.Unit, 
MeasurePerPcs = I.Measure, 
TotalMeasure = - L.Qty * I.Measure, 
MeasureUnit = I.MeasureUnit, 
Rate = Jol.Rate, 
--Incentive 
ProdOrder = Pod.DocId, 
Amount = - L.Qty * I.Measure * Jol.Rate , 
--Bom 
--PerimeterPerPcs
--TotalPerimeter
FromProcess = P.NCat, 
JobOrder = Jod.DocId,
JobOrderSr = Jod.Sr, 
--Perimeter
--JobWorkerRateGroup, 
Remark = L.Remark, 
Design = D.Code, 
--Colour
Specification = Jol.Specification, 
--IncentivePer, 
Quality = Q.Code, 
Item_UID = Iu.Code, 
--TransactionNature
--ProcessOnPerimeter
DocQty = L.Qty, 
TotalDocMeasure = L.Qty * I.Measure, 
--TotalDocPerimeter
LotNo = Jol.LotNo
--IncentiveRate
FROM Web.JobOrderCancelLines  L
LEFT JOIN Web.JobOrderCancelHeaders H ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId
LEFT JOIN Web.JobOrderLines Jol ON L.JobOrderLineId = Jol.JobOrderLineId
LEFT JOIN Web.Dimension2 D2 ON Jol.Dimension2Id = D2.Dimension2Id
LEFT JOIN Web.FinishedProduct Fp ON Jol.ProductId = Fp.ProductId
LEFT JOIN JobOrder Jo ON 'Job Order Cancel-' + Convert(Varchar,H.JobOrderCancelHeaderId) = Jo.OMSId
LEFT JOIN (SELECT * FROM JobOrderDetail) Jod ON 'Job Order-' + Convert(Varchar,Jol.JobOrderLineId) = Jod.OMSId
LEFT JOIN Item I On  'Product-' + Convert(Varchar,Jol.ProductId) = I.OMSId 
LEFT JOIN RUG_SHADE S  ON 'Dimension1-' +  Convert(Varchar,Jol.Dimension1Id) = S.OMSId 
LEFT JOIN RUG_YarnSKU Ys ON I.Code = Ys.Yarn AND S.Code = Ys.SHADE
LEFT JOIN ProdOrderDetail Pod ON 'Prod Order-'  + Convert(Varchar,Jol.ProdOrderLineId) = Pod.OMSId 
LEFT JOIN Process P ON 'Process-' + Convert(Varchar,Jol.FromProcessId) = P.OMSId 
LEFT JOIN RUG_Design D ON D2.Dimension2Name = D.Description
LEFT JOIN RUG_Quality Q ON 'Quality-' + Convert(Varchar,Fp.ProductQualityId) = Q.OMSId
LEFT JOIN Item_UID Iu ON 'Product Uid-' + Convert(Varchar,Jol.ProductUidId) = Iu.OMSId
WHERE JobOrderDetail.OMSId = 'Job Order-' + Convert(VARCHAR,@Id)
And L.JobOrderCancelLineId = @Id
End
GO


