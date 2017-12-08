IF OBJECT_ID ('[Web].[Mig_JobOrderLine_Add]') IS NOT NULL
	DROP PROCEDURE [Web].[Mig_JobOrderLine_Add]	
GO


Create PROCEDURE [Web].[Mig_JobOrderLine_Add]
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

SELECT TOP 1 Jo.DocId AS DocId,
(SELECT IsNull(Max(Sr),0) + 1 FROM JobOrderDetail WHERE DocId = Jo.DocId) As Sr,
IsNull(Ys.Code,I.Code) AS Item,
L.Qty AS Qty, 
I.Unit AS Unit, 
I.Measure AS MeasurePerPcs, 
L.Qty * I.Measure AS TotalMeasure, 
I.MeasureUnit AS MeasureUnit, 
NULL AS ProdPlan,
L.LossQty AS Loss, 
L.Rate AS Rate, 
--Incentive 
NULL AS Incentive,
NULL AS ReceiveQty, 
NULL AS ReceiveMeasure, 
NULL AS UID, 
Pod.DocId AS ProdOrder , 
L.Amount AS Amount, 
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
L.Specification AS Specification, 
--IncentivePer, 
NULL AS IncentivePer,
Quality = Q.Code, 
Jo.DocId AS JobOrder,
(SELECT IsNull(Max(Sr),0) + 1 FROM JobOrderDetail WHERE DocId = Jo.DocId) As JobOrderSr,
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
L.LotNo AS LotNo,
NULL AS Dimension1, 
NULL AS Dimension2, 
NULL AS T_Nature, 
NULL AS DyedOnItem, 
--IncentiveRate
NULL AS IncentiveRate,
'Job Order-' + Convert(VARCHAR,L.JobOrderLineId) AS OMSId,
NULL AS Sub_Total
FROM Web.JobOrderLines  L
LEFT JOIN Web.JobOrderHeaders H ON L.JobOrderHeaderId = H.JobOrderHeaderId
LEFT JOIN Web.Dimension2 D2 ON L.Dimension2Id = D2.Dimension2Id
LEFT JOIN Web.FinishedProduct Fp ON L.ProductId = Fp.ProductId
LEFT JOIN JobOrder Jo ON 'Job Order-' + Convert(Varchar,H.JobOrderHeaderId) = Jo.OMSId
LEFT JOIN Item I On  'Product-' + Convert(Varchar,L.ProductId) = I.OMSId 
LEFT JOIN RUG_SHADE S  ON 'Dimension1-' +  Convert(Varchar,L.Dimension1Id) = S.OMSId 
LEFT JOIN RUG_YarnSKU Ys ON I.Code = Ys.Yarn AND S.Code = Ys.SHADE
LEFT JOIN ProdOrderDetail Pod ON 'Prod Order-'  + Convert(Varchar,L.ProdOrderLineId) = Pod.OMSId 
LEFT JOIN Process P ON 'Process-' + Convert(Varchar,L.FromProcessId) = P.OMSId 
LEFT JOIN RUG_Design D ON D2.Dimension2Name = D.Description
LEFT JOIN RUG_Quality Q ON 'Quality-' + Convert(Varchar,Fp.ProductQualityId) = Q.OMSId
LEFT JOIN Item_UID Iu ON 'Product Uid-' + Convert(Varchar,L.ProductUidId) = Iu.OMSId
WHERE L.JobOrderLineId = @Id


IF (SELECT StockId FROM Web.JobOrderLines WHERE JobOrderLineId = @Id) IS NOT NULL
BEGIN
	INSERT INTO dbo.Stock (DocID, 
	Sr, 
	V_Type, 
	V_Prefix, 
	V_Date, 
	V_No, 
	Div_Code, 
	Site_Code, 
	SubCode, 
	Item, 
	Godown, 
	Qty_Iss, 
	Qty_Rec, 
	Unit,
	MeasurePerPcs, 
	Measure_Iss, 
	Measure_Rec,
	MeasureUnit,
	Process, 
	RecId, 
	OMSId)
	
	SELECT Cd.DocId AS DocId, 
	Cd.Sr AS Sr,
	C.V_Type, 
	C.V_Prefix, 
	C.V_Date, 
	C.V_No, 
	C.Div_Code, 
	C.Site_Code, 
	C.JobWorker AS SubCode, 
	Cd.Item, 
	C.Godown, 
	Cd.Qty AS Qty_Iss, 
	0 AS Qty_Rec, 
	Cd.Unit AS Unit,
	Cd.MeasurePerPcs AS  MeasurePerPcs, 
	Cd.TotalDocMeasure AS  Measure_Iss, 
	0 AS Measure_Rec,
	Cd.MeasureUnit AS  MeasureUnit,
	Cd.FromProcess AS Process,
	C.ManualRefNo AS RecId, 
	Cd.OMSId AS OMSId
	FROM Web.JobOrderLines L
	LEFT JOIN JobOrderDetail  Cd ON 'Job Order-' + Convert(NVARCHAR,L.JobOrderLineId) = Cd.OMSId
	LEFT JOIN JobOrder C ON Cd.DocId = C.DocID
	WHERE L.JobOrderLineId = @Id
END 





IF (SELECT StockProcessId FROM Web.JobOrderLines WHERE JobOrderLineId = @Id) IS NOT NULL
BEGIN
	INSERT INTO dbo.StockProcess (DocID, 
	Sr, 
	V_Type, 
	V_Prefix, 
	V_Date, 
	V_No, 
	Div_Code, 
	Site_Code, 
	SubCode, 
	Item, 
	Godown, 
	Qty_Iss, 
	Qty_Rec, 
	Unit,
	MeasurePerPcs, 
	Measure_Iss, 
	Measure_Rec,
	MeasureUnit,
	Process, 
	RecId, 
	OMSId)
	
	SELECT Cd.DocId AS DocId, 
	Cd.Sr AS Sr,
	C.V_Type, 
	C.V_Prefix, 
	C.V_Date, 
	C.V_No, 
	C.Div_Code, 
	C.Site_Code, 
	C.JobWorker AS SubCode, 
	Cd.Item, 
	C.Godown, 
	0 AS Qty_Iss, 
	Cd.Qty AS Qty_Rec, 
	Cd.Unit AS Unit,
	Cd.MeasurePerPcs AS  MeasurePerPcs, 
	0 AS  Measure_Iss, 
	Cd.TotalDocMeasure AS Measure_Rec,
	Cd.MeasureUnit AS  MeasureUnit,
	Cd.FromProcess AS Process,
	C.ManualRefNo AS RecId, 
	Cd.OMSId AS OMSId
	FROM Web.JobOrderLines L
	LEFT JOIN JobOrderDetail  Cd ON 'Job Order-' + Convert(NVARCHAR,L.JobOrderLineId) = Cd.OMSId
	LEFT JOIN JobOrder C ON Cd.DocId = C.DocID
	WHERE L.JobOrderLineId = @Id
END 



DECLARE @DocumentTypeShortName NVARCHAR(5)

SELECT @DocumentTypeShortName = D.DocumentTypeShortName
FROM Web.JobOrderLines L
LEFT JOIN Web.JobOrderHeaders H ON L.JobOrderHeaderId = H.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
WHERE L.JobOrderLineId = @Id


IF (@DocumentTypeShortName = 'WVORD' OR @DocumentTypeShortName = 'WFORD' )
BEGIN
	INSERT INTO dbo.StockVirtual (DocID, Sr, V_Type, V_Prefix, V_Date, V_No, Div_Code, 
	Site_Code, SubCode, Item,
	Qty_Iss, Qty_Rec, Unit,
	MeasurePerPcs, Measure_Iss, Measure_Rec, MeasureUnit, Rate, Amount, OMSId)
	SELECT H.DocID, 
	(SELECT IsNull(Max(S.Sr),0) + 1 FROM dbo.StockVirtual S WHERE S.DocId = H.DocID) AS Sr
	, H.V_Type, H.V_Prefix, H.V_Date, H.V_No, H.Div_Code, H.Site_Code,
	H.JobWorker, L.Item, L.Qty, 0, L.Unit, L.MeasurePerPcs, L.TotalMeasure, 0, L.MeasureUnit,
	L.Rate, L.Amount, 'Job Order-' + Convert(VARCHAR,Pol.JobOrderLineId) AS OMSId
	FROM Web.JobOrderLines Pol 
	LEFT JOIN JobOrderDetail L ON 'Job Order-' + Convert(NVARCHAR,Pol.JobOrderLineId) = L.OMSId
	LEFT JOIN JobOrder H ON L.DocId = H.DocID
	WHERE Pol.JobOrderLineId = @Id
END

DECLARE @ProductUidId NVARCHAR(20)


IF (SELECT L.ProductUidId FROM Web.JobOrderLines L WHERE L.JobOrderLineId = @Id) IS NOT NULL
BEGIN
	UPDATE dbo.Item_UID
	SET IsInStock = 1,
		RecDocId = V1.RecDocId,
		RecSr = V1.RecSr,
		LastTransactionV_Type = V1.LastTransactionV_Type,
		LastTransactionV_Date = V1.LastTransactionV_Date,
		LastTransactionSubCode = V1.LastTransactionSubCode,
		CurrenctGodown = V1.CurrenctGodown,
		CurrenctProcess = V1.CurrenctProcess,
		Status = V1.Status,
		LastTransactionDocNo = V1.LastTransactionDocNo, 
		LastTransactionDocId = V1.LastTransactionDocId
	FROM (
		SELECT L.Item_UID, L.DocId AS RecDocId, L.Sr AS RecSr, H.V_Type AS LastTransactionV_Type, H.V_Date AS LastTransactionV_Date, 
		H.JobWorker AS LastTransactionSubCode, NULL AS  CurrenctGodown,
		H.Process AS CurrenctProcess, 'Issue' AS Status, H.ManualRefNo AS LastTransactionDocNo, L.DocId AS LastTransactionDocId
		FROM Web.JobOrderLines Pol 
		LEFT JOIN JobOrderDetail L ON 'Job Order-' + Convert(NVARCHAR,Pol.JobOrderLineId) = L.OMSId
		LEFT JOIN JobOrder H ON L.DocId = H.DocID
		WHERE Pol.JobOrderLineId = @Id
	) AS V1
	WHERE Item_UID.Code = V1.Item_Uid 
	
	
	INSERT INTO dbo.JobIssRecUID (DocID, TSr, Sr, Process, Item, Item_UID, UID, RollNo, Item_ManualUID, JobRecDocID, ISSREC, Godown, 
	Site_Code, V_Date, V_Type, SubCode, Div_Code, RecId, Dimension1, Dimension2, EntryDate, Remark, OMSId)
	SELECT L.DocId, L.Sr AS TSr, L.Sr, H.Process AS Process, L.Item, L.Item_UID, NULL AS Uid, NULL AS RollNo, NULL AS Item_ManualUID,
	NULL AS JobRecDocID, 'I' AS ISSREC, H.Godown AS  Godown, 
	H.Site_Code, H.V_Date, H.V_Type, H.JobWorker AS SubCode, H.Div_Code, H.ManualRefNo AS RecId, 
	NULL AS Dimension1, NULL AS Dimension2, NULL AS EntryDate, IsNull(H.Remarks,'') + '.' + IsNull(L.Remark,'')  AS Remark, 
	'Job Order-' + COnvert(NVARCHAR,@Id) AS OMSId
	FROM Web.JobOrderLines Pol 
	LEFT JOIN JobOrderDetail L ON 'Job Order-' + Convert(NVARCHAR,Pol.JobOrderLineId) = L.OMSId
	LEFT JOIN JobOrder H ON L.DocId = H.DocID
	WHERE Pol.JobOrderLineId = @Id
END 
END
GO

