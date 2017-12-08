IF object_id ('[Web].[spRep_JobOrderStatus]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_JobOrderStatus]
GO 

CREATE PROCEDURE [Web].[spRep_JobOrderStatus]
	@StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
    @JobWorker VARCHAR(Max) = NULL,
    @Process VARCHAR(Max) = NULL,
    @ProductNature VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @PersonCustomGroup VARCHAR(Max) = NULL,
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,  
    @Product VARCHAR(Max) = NULL,  
    @ProdOrderHeaderId VARCHAR(Max) = NULL,  
	@JobOrderHeaderId VARCHAR(Max) = NULL
AS 
BEGIN 


--- *************** Check Conditions For ReportName *********************** ----
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 
DECLARE @LossQtySum INT 
DECLARE @ReportName NVARCHAR(100) 




SELECT @Dimension1IdCnt = Count(*)
FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines POL ON H.ProdOrderLineId = POL.ProdOrderLineId
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND H.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt = Count(*)
FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines POL ON H.ProdOrderLineId = POL.ProdOrderLineId
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND H.Dimension2Id IS NOT NULL


SELECT @LossQtySum =  
( SELECT IsNull(sum(VGoodsReceipt.LossQty),0) AS LossQty
FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
LEFT JOIN 
(
	SELECT L.JobOrderLineId, Sum(L.Qty) AS Qty, Sum(L.ReceiveQty) AS ReceiveQty,Sum(L.LossQty) AS LossQty, 
	Sum(L.Amount) AS Amount, Sum(L.ReceiveAmount) AS ReceiveAmount, Sum(L.LossAmount) AS LossAmount, 
	Max(L.DocDate) AS LastReceiptDate
	FROM Web.FJobReceive(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process, @JobWorker, DEFAULT) AS L 
	GROUP BY L.JobOrderLineId
) AS VGoodsReceipt ON H.JobOrderLineId = VGoodsReceipt.JobOrderLineId
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdorderLines POL ON H.ProdOrderLineId = POL.ProdOrderLineId 
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND ( @Dimension1 IS NULL OR H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 IS NULL OR H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
)


SET @ReportName =
(
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderStatusWithDoubleDimension.rdl'
	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderStatusWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobOrderStatusWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobOrderStatus.rdl'
END 
)

IF (@LossQtySum > 0) SET @ReportName = @ReportName + '_WithLossQty'










--- *************** Main Qry *********************** ----

SELECT H.DocDate AS OrderDate, H.DocNo AS OrderNo, PR.ProcessName, PR.ProcessSr,
CASE WHEN H.OrderQty + H.AmendmentQty - H.CancelQty - IsNull(VGoodsReceipt.ReceiveQty,0) > 0 
	 THEN DateDiff(Day,H.DueDate,GetDate())
	 ELSE DateDiff(Day,H.DueDate,IsNull(VGoodsReceipt.LastReceiptDate,H.DueDate)) END AS OverDueDays,
Pp.Name AS JobWorkerName, P.ProductName, U.UnitName, H.Specification, 
Pt.ProductTypeName,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
H.OrderQty, H.AmendmentQty, H.CancelQty, IsNull(VGoodsReceipt.ReceiveQty,0) AS ReceiveQty, IsNull(VGoodsReceipt.LossQty,0) AS LossQty,
H.OrderQty + H.AmendmentQty - H.CancelQty - IsNull(VGoodsReceipt.ReceiveQty,0) - IsNull(VGoodsReceipt.LossQty,0)   AS BalanceQty,
H.OrderAmount, H.AmendmentAmount, H.CancelAmount, IsNull(VGoodsReceipt.ReceiveAmount,0) AS  ReceiveAmount,IsNull(VGoodsReceipt.LossAmount,0) AS  LossAmount,
H.OrderAmount + H.AmendmentAmount - H.CancelAmount - IsNull(VGoodsReceipt.ReceiveAmount,0) - IsNull(VGoodsReceipt.LossAmount,0) AS BalanceAmount,
IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
H.SiteId AS SiteId, H.DivisionId AS DivisionId,
S.SiteName AS SiteName, D.DivisionName AS DivisionName, 
@ReportName + '.rdl' ReportName, 'Job Order Status' AS ReportTitle, NULL AS SubReportProcList
FROM Web.FJobOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, @JobOrderHeaderId) AS H 
LEFT JOIN 
(
	SELECT L.JobOrderLineId, Sum(L.ReceiveQty) AS ReceiveQty,Sum(L.LossQty) AS LossQty, 
	Sum(L.ReceiveAmount) AS ReceiveAmount, Sum(L.LossAmount) AS LossAmount, 
	Max(L.DocDate) AS LastReceiptDate
	FROM Web.FJobReceive(@StatusOnDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @Process, @JobWorker, DEFAULT) AS L 
	GROUP BY L.JobOrderLineId
) AS VGoodsReceipt ON H.JobOrderLineId = VGoodsReceipt.JobOrderLineId
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1 D1 ON H.Dimension1Id = D1.Dimension1Id
LEFT JOIN Web.Dimension2 D2 ON H.Dimension2Id = D2.Dimension2Id
LEFT JOIN Web.Dimension1Types Dt1 ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonID 
LEFT JOIN Web.ProdorderLines POL ON H.ProdOrderLineId = POL.ProdOrderLineId 
LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
LEFT JOIN web.Processes PR ON PR.ProcessId = H.ProcessId
WHERE 1=1
AND ( @ProdOrderHeaderId IS NULL OR POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
AND ( @Dimension1 IS NULL OR H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 IS NULL OR H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
ORDER BY H.DocDate
End
GO

