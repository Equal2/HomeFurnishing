IF OBJECT_ID ('[Web].[Mig_JobOrderLine_Update]') IS NOT NULL
	DROP PROCEDURE [Web].[Mig_JobOrderLine_Update]	
GO


Create PROCEDURE [Web].[Mig_JobOrderLine_Update]
	@Id INT 
AS 
BEGIN 



UPDATE JobOrderDetail
SET  Item = IsNull(Ys.Code,I.Code),
Qty = L.Qty, 
Unit = I.Unit, 
MeasurePerPcs = I.Measure, 
TotalMeasure = L.Qty * I.Measure, 
MeasureUnit = I.MeasureUnit, 
Loss = L.LossQty, 
Rate = L.Rate, 
--Incentive 
ProdOrder = Pod.DocId, 
Amount = L.Amount, 
--Bom 
--PerimeterPerPcs
--TotalPerimeter
FromProcess = P.NCat, 
--Perimeter
--JobWorkerRateGroup, 
Remark = L.Remark, 
Design = D.Code, 
--Colour
Specification = L.Specification, 
--IncentivePer, 
Quality = Q.Code, 
Item_UID = Iu.Code, 
--TransactionNature
--ProcessOnPerimeter
DocQty = L.Qty, 
TotalDocMeasure = L.Qty * I.Measure, 
--TotalDocPerimeter
LotNo = L.LotNo
--IncentiveRate
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
WHERE JobOrderDetail.OMSId = 'Job Order-' + Convert(VARCHAR,@Id)
And L.JobOrderLineId = @Id


IF (SELECT StockId FROM Web.JobOrderLines WHERE JobOrderLineId = @Id) IS NOT NULL
BEGIN
	UPDATE Stock
	SET V_Type = C.V_Type, 
	V_Date = C.V_Date, 
	Div_Code = C.Div_Code, 
	Site_Code = C.Site_Code, 
	SubCode = C.JobWorker , 
	Item = Cd.Item, 
	Godown = C.Godown, 
	Qty_Iss = Cd.Qty , 
	Qty_Rec = 0 , 
	Unit = Cd.Unit ,
	MeasurePerPcs = Cd.MeasurePerPcs , 
	Measure_Iss = Cd.TotalDocMeasure , 
	Measure_Rec = 0 ,
	MeasureUnit = Cd.MeasureUnit ,
	Process = Cd.FromProcess ,
	RecId = C.ManualRefNo  
	FROM Web.JobOrderLines L
	LEFT JOIN JobOrderDetail  Cd ON 'Job Order-' + Convert(NVARCHAR,L.JobOrderLineId) = Cd.OMSId
	LEFT JOIN JobOrder C ON Cd.DocId = C.DocID
	WHERE Stock.OMSId = 'Job Order-' + Convert(VARCHAR,@Id)
	And L.JobOrderLineId = @Id
END 


IF (SELECT StockProcessId FROM Web.JobOrderLines WHERE JobOrderLineId = @Id) IS NOT NULL
BEGIN
	UPDATE StockProcess
	SET V_Type = C.V_Type, 
	V_Date = C.V_Date, 
	Div_Code = C.Div_Code, 
	Site_Code = C.Site_Code, 
	SubCode = C.JobWorker , 
	Item = Cd.Item, 
	Godown = C.Godown, 
	Qty_Iss = 0, 
	Qty_Rec = Cd.Qty , 
	Unit = Cd.Unit ,
	MeasurePerPcs = Cd.MeasurePerPcs , 
	Measure_Iss = 0, 
	Measure_Rec = Cd.TotalDocMeasure  ,
	MeasureUnit = Cd.MeasureUnit ,
	Process = Cd.FromProcess ,
	RecId = C.ManualRefNo  
	FROM Web.JobOrderLines L
	LEFT JOIN JobOrderDetail  Cd ON 'Job Order-' + Convert(NVARCHAR,L.JobOrderLineId) = Cd.OMSId
	LEFT JOIN JobOrder C ON Cd.DocId = C.DocID
	WHERE StockProcess.OMSId = 'Job Order-' + Convert(VARCHAR,@Id)
	And L.JobOrderLineId = @Id
END 


DECLARE @DocumentTypeShortName NVARCHAR(5)

SELECT @DocumentTypeShortName = D.DocumentTypeShortName
FROM Web.JobOrderLines L
LEFT JOIN Web.JobOrderHeaders H ON L.JobOrderHeaderId = H.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
WHERE L.JobOrderLineId = @Id


IF (@DocumentTypeShortName = 'WVORD' OR @DocumentTypeShortName = 'WFORD' )
BEGIN
	UPDATE dbo.StockVirtual
	SET V_Type = C.V_Type, 
	V_Date = C.V_Date, 
	Div_Code = C.Div_Code, 
	Site_Code = C.Site_Code, 
	SubCode = C.JobWorker , 
	Item = Cd.Item, 
	Godown = C.Godown, 
	Qty_Iss = Cd.Qty, 
	Qty_Rec = 0 , 
	Unit = Cd.Unit ,
	MeasurePerPcs = Cd.MeasurePerPcs , 
	Measure_Iss = Cd.TotalDocMeasure  , 
	Measure_Rec = 0 ,
	MeasureUnit = Cd.MeasureUnit ,
	Process = Cd.FromProcess ,
	RecId = C.ManualRefNo  
	FROM Web.JobOrderLines L
	LEFT JOIN JobOrderDetail  Cd ON 'Job Order-' + Convert(NVARCHAR,L.JobOrderLineId) = Cd.OMSId
	LEFT JOIN JobOrder C ON Cd.DocId = C.DocID
	WHERE dbo.StockVirtual.OMSId = 'Job Order-' + Convert(VARCHAR,@Id)
	And L.JobOrderLineId = @Id
END


IF (SELECT L.ProductUidId FROM Web.JobOrderLines L WHERE L.JobOrderLineId = @Id) IS NOT NULL
BEGIN

DECLARE @Item_Uid NVARCHAR(20)
DECLARE @DocId NVARCHAR(21)
DECLARE @Sr INT 

SELECT @DocId = DocId, @Sr = Sr, @Item_Uid = L.Item_Uid  FROM JobOrderDetail L WHERE OMSId   = 'Job Order-' + Convert(VARCHAR,@Id)



DELETE FROM JobIssRecUID WHERE DocID = @DocId AND TSr = @Sr


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


UPDATE Item_UID  
SET Item_UID.LastTransactionDocId = VUid.DocId,   
Item_UID.LastTransactionV_Type = VUid.V_Type,   
Item_UID.LastTransactionV_Date = VUid.V_Date,   
Item_UID.LastTransactionSubCode = VUid.SubCode,   
Item_UID.CurrenctGodown = VUid.Godown,   
Item_UID.LastTransactionDocNo = VUid.RecId,   
Item_Uid.CurrenctProcess = VUid.Process,  
Item_Uid.Status = VUid.Status  
FROM (SELECT TOP 1 L.Item_UID, L.DocId, L.V_Type, L.V_Date, L.SubCode, L.Process, L.RecId,  
   Case When L.IssRec = 'R' Then L.Godown Else Null End As Godown,  
   Case When L.IssRec = 'R' Then 'Receive' Else 'Issue' End As Status  
    FROM JobIssRecUID L With (NoLock)  
    LEFT JOIN Process P With (NoLock) ON L.Process = P.NCat  
    WHERE L.Item_UID = @Item_Uid
    ORDER BY V_Date DESC, P.Sr DESC, L.ISSREC DESC  
) AS VUid Where Item_UID.Code = VUid.Item_UID  
And Item_Uid.Code =  @Item_Uid


END                
                    
END
GO

