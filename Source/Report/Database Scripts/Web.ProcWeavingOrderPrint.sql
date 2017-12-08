IF object_id ('[Web].[ProcWeavingOrderPrint]') IS NOT NULL 
 DROP Procedure  [Web].[ProcWeavingOrderPrint]
GO 

CREATE Procedure  [Web].[ProcWeavingOrderPrint]
(@Id INT)  
As  
BEGIN  
SELECT H.SiteId, H.DivisionId, H.JobOrderHeaderId, DT.DocumentTypeShortName AS DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.CreatedBy, H.CreatedDate,  
H.Remark,  P.Name AS JobWorkerName,  PA.Address AS JobWorkerAddress,City.CityName AS JobWorkerCity, P.Phone AS JobWorkerContactNo,
PB.Name AS OrderIssueBy, CC.CostCenterName,  H.CreditDays, H.TermsAndConditions,    
JOHD.TimeIncentive, JOHD.TimePenalty,JOHD.SmallChunksPenaltyRate, JOHD.SmallChunksPenaltyCount,
L.JobOrderLineId, PD.ProductName, L.DueDate AS LineDueDate, L.Remark AS LineRemark,  
PG.ProductGroupName, VRA.SizeName, U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 
L.Rate, 0 AS LineIncentive, POH.DocNo AS ProdOrderNo,  COL.ColourName, PQ.Weight AS WoolLagat,
L.LossQty, L.NonCountedQty AS ClothQty,
FJOL.OrderQty,FJOL.CancelQty, FJOL.Qty, FJOL.Qty* VRA.SqYardPerPcs  AS BalanceArea,
( SELECT TOP 1 isnull(RegistrationNo,'')  FROM web.PersonRegistrations WHERE PersonId = H.JobWorkerId AND RegistrationType = 'PAN No' ) AS PanNo,
--CASE WHEN L.AmendmentCount > 0 THEN 'A ' ELSE '' END + CASE WHEN L.CancelCount >0  THEN 'C ' ELSE '' END + CASE WHEN L.RateAmdCount > 0 THEN 'R ' ELSE '' END AS CurrentStatus,  
''  AS CurrentStatus,  VPU.ProductUidList,
H.ModifiedBy +' ' + replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, 
H.ModifiedDate,   AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy,   AL.ApproveDate,
'ProcWeavingOrderPrint1 ' + convert(NVARCHAR,H.JobOrderHeaderId) AS SubReportProcList,  
'WeavingOrder_Print.rdl' AS ReportName ,  'Weaving Order Form' AS ReportTitle      
FROM  Web._JobOrderHeaders H  WITH (Nolock)
LEFT JOIN Web._JobOrderLines L WITH (Nolock) ON L.JobOrderHeaderId = H.JobOrderHeaderId 
LEFT JOIN 
 (
  SELECT * FROM web.FJobOrder( NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL, @Id) 
  ) FJOL ON FJOL.JobOrderLineId = L.JobOrderLineId 
  LEFT JOIN web.JobOrderHeaderDetail JOHD ON JOHD.JobOrderHeaderId = H.JobOrderHeaderId
  LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = H.DocTypeId   
  LEFT JOIN web.CostCenters CC ON CC.CostCenterId = H.CostCenterId
  LEFT JOIN Web.People P WITH (Nolock) ON P.PersonID = H.JobWorkerId       
  LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
  LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
  LEFT JOIN Web.People PB WITH (Nolock) ON PB.PersonID = H.OrderById  
  LEFT JOIN Web.Products PD WITH (Nolock) ON PD.ProductId = L.ProductId  
  LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId = PD.ProductGroupId 
  LEFT JOIN web.FinishedProduct FP ON FP.ProductId =L.ProductId
  LEFT JOIN web.Colours COL ON COL.ColourId = FP.ColourId 
  LEFT JOIN web.ProductQualities PQ ON PQ.ProductQualityId = FP.ProductQualityId 
  LEFT JOIN web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId
  LEFT JOIN WEb.ProdOrderHeaders POH ON POH.ProdOrderHeaderId = POL.ProdOrderHeaderId
  LEFT JOIN Web.Units U WITH (Nolock) ON U.UnitId = PD.UnitId      
  LEFT JOIN Web.ViewRugarea VRA WITH (Nolock) ON VRA.ProductId = L.ProductId    
  LEFT JOIN (      SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate        FROM  Web.ActivityLogs AL      WHERE AL.ActivityType =2      GROUP BY AL.DocTypeId, AL.DocId  ) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.JobOrderHeaderId  
  LEFT JOIN 
  (
  	SELECT TOP 1 PUH.ProductUIDHeaderId, Min( convert(INT,PU.ProductUidName)) AS MinProductUId, Max(convert(INT,PU.ProductUidName)) AS MaxProductUId,
 	CASE WHEN  Min( convert(INT,PU.ProductUidName)) = Max(convert(INT,PU.ProductUidName)) THEN convert(NVARCHAR,Max(convert(INT,PU.ProductUidName))) ELSE 
	Convert(NVARCHAR,Min( convert(INT,PU.ProductUidName))) +'-'+ convert(NVARCHAR,Max(convert(INT,PU.ProductUidName))) END AS ProductUidList
	FROM
	(
	SELECT * FROM web.ProductUidHeaders WHERE GenDocId = 7806
	) AS PUH
	LEFT JOIN web.ProductUids PU ON PU.ProductUidHeaderId = PUH.ProductUidHeaderId 
	GROUP BY PUH.ProductUIDHeaderId

  ) VPU ON 1=1
  --VPU.ProductUIDHeaderId = L.ProductUIDHeaderId
 WHERE H.JobOrderHeaderId	=  @Id   
 ORDER BY L.JobOrderLineId
 End
GO

