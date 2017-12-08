IF object_id ('[Web].[spRep_DyeingAnalysisReport]') IS NOT NULL 
DROP Procedure  [Web].[spRep_DyeingAnalysisReport]
GO 

CREATE PROCEDURE [Web].[spRep_DyeingAnalysisReport]  
@ReportType VARCHAR(Max) = 'Dyer Wise Summary',   
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,	
    @JobWorker VARCHAR(Max) = NULL,
    @ProductNature VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @PersonCustomGroup VARCHAR(Max) = NULL,
    @Product VARCHAR(Max) = NULL  
	
AS
BEGIN

DECLARE @DocumentType VARCHAR(20) 
DECLARE @Process VARCHAR(Max)

SET @DocumentType =103
SET @Process =10 


/*DECLARE @Site VARCHAR(20)
DECLARE @Division VARCHAR(20) 
DECLARE @FromDate VARCHAR(20) 
DECLARE @ToDate VARCHAR(20) 
DECLARE @DocumentType VARCHAR(20) 
DECLARE @Process VARCHAR(Max)
DECLARE @JobWorker VARCHAR(Max) 
DECLARE @ProductNature VARCHAR(Max) 
DECLARE @ProductType VARCHAR(Max) 
DECLARE @ProductGroup VARCHAR(Max) 
DECLARE @ProductCustomGroup VARCHAR(Max) 
DECLARE @PersonCustomGroup VARCHAR(Max) 
DECLARE @Product VARCHAR(Max) 
DECLARE @Dimension1 VARCHAR(Max) 
DECLARE @Dimension2 VARCHAR(Max)  
DECLARE @ProdOrderHeaderId VARCHAR(Max) 
DECLARE @JobOrderHeaderId VARCHAR(Max) 
DECLARE @ReportType VARCHAR(Max) 
   	
SET @Site =17
SET @Division =6
SET @FromDate ='01/Apr/2015'
SET @ToDate ='30/Apr/2015'
SET @DocumentType =103
SET @Process =10 
SET @JobWorker =841
SET @ProductNature = NULL 
SET @ProductType = NULL 
SET @ProductGroup = NULL 
SET @ProductCustomGroup = NULL 
SET @PersonCustomGroup = NULL 
SET @ReportType = 'Product Group Wise Summary' 
*/


DECLARE @DyeingCancel INT = 98	
DECLARE @DyeingIssue INT = 101
DECLARE @DyeingMaterialIssue INT = 102
DECLARE @DyeingReturn INT = 107
DECLARE @DyeingRateConversion INT = 105
DECLARE @DyeingConsumption INT = 99

	
-- Create Temp Table For Collect All Record
Declare @TempTable TABLE (Trans_Type INT , DocDate SMALLDATETIME, JobWorkerId INT, ProductId INT, SiteId INT , DivisionId INT, 
OpeningOrder FLOAT, OpeningUndyedQty FLOAT , OrderQty FLOAT, ReceiveQty FLOAT , LossQty FLOAT, UndyedIssQty FLOAT, UndyedConsumedQty FLOAT, UndyedRetQty FLOAT , RateConversionQty FLOAT  )

                            
Insert INTO @TempTable (Trans_Type, SiteId, DivisionId, DocDate, JobWorkerId, ProductId,  OpeningOrder,OpeningUndyedQty,OrderQty,ReceiveQty, LossQty, UndyedIssQty , UndyedConsumedQty , UndyedRetQty , RateConversionQty)
--- For Opening Order
Select 0 As Trans_Type, VOpen.SiteId, VOpen.DivisionId, convert(SMALLDATETIME,@FromDate)  AS  DocDate, VOpen.JobWorkerId, VOpen.ProductId, 
IsNull(Sum(VOpen.OpeningOrder),0) As OpeningOrder, IsNull(Sum(VOpen.UndyedQty),0) As OpeningUndyedQty, 
0 As OrderQty, 0 As OrderRecQty, 0 As LossQty, 0 As UndyedIssQty, 0 As UndyedConsumedQty, 0 As UndyedRetQty, 0 As RateConversionQty 
From 
(
SELECT H.SiteId, H.DivisionId,  H.JobWorkerId, H.OrderDate AS DocDate, H.ProductId, IsNull(Sum(H.BalanceQty),0) AS OpeningOrder, 0 AS UndyedQty                    
FROM Web.FJobOrderBalance(@FromDate, @Site,@Division, NULL, @FromDate, @DocumentType, @JobWorker,@Process,@Product, NULL ) AS H 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines Pil ON H.ProdOrderLineId = Pil.ProdOrderLineId
WHERE 1=1
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
GROUP BY H.SiteId, H.DivisionId,  H.JobWorkerId, H.OrderDate, H.ProductId

UNION ALL 

SELECT H.SiteId, H.DivisionId,  H.PersonId AS JobWorkerId, NULL AS DocDate, H.ProductId , 0 AS OpeningOrder, IsNull(sum(H.BalQty),0) AS UndyedQty 
FROM web.FStockProcessBalance (@FromDate,@Site,@Division,@JobWorker,NULL ) AS H
GROUP BY H.SiteId, H.DivisionId,  H.PersonId,  H.ProductId

 ) As VOpen
Group By VOpen.SiteId, VOpen.DivisionId, VOpen.JobWorkerId, VOpen.DocDate, VOpen.ProductId

--- For Current Period
UNION ALL 

Select 1 As Trans_Type, VCurrent.SiteId, VCurrent.DivisionId, VCurrent.DocDate, VCurrent.JobWorkerId,  VCurrent.ProductId, 
0 As OpeningOrder, 0 As OpeningUndyedQty,
IsNull(Sum(VCurrent.OrderQty),0) As OrderQty, IsNull(Sum(VCurrent.RecQty),0) As OrderRecQty,
IsNull(Sum(VCurrent.LossQty),0) As LossQty, 
IsNull(Sum(VCurrent.UndyedIssQty),0) As UndyedIssQty, 
IsNull(Sum(VCurrent.UndyedConsumedQty),0) As UndyedConsumedQty, 
IsNull(Sum(VCurrent.UndyedRetQty),0) As UndyedRetQty, 
IsNull(Sum(VCurrent.RateConversionQty),0) As RateConversionQty 
From 
(

--- For Order
SELECT H.SiteId, H.DivisionId, H.JobWorkerId, H.DocDate, H.ProductId , 
IsNull(H.Qty,0) As OrderQty, 0 AS RecQty, 0 AS LossQty, 0 AS UndyedIssQty, 0 AS UndyedConsumedQty, 0 AS UndyedRetQty, 0 As RateConversionQty    
FROM Web.FJobOrder(@ToDate, @Site,@Division, @FromDate, @ToDate, @DocumentType, @JobWorker,@Process,@Product, NULL ) AS H
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines Pil ON H.ProdOrderLineId = Pil.ProdOrderLineId
WHERE 1=1
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 


--- For Job Order Cancel

UNION ALL 
SELECT H.SiteId, H.DivisionId, JOH.JobWorkerId AS JobWorkerId, H.DocDate, JOL.ProductId , 
- L.Qty As OrderQty, 0 AS RecQty, 0 AS LossQty, 0  AS UndyedIssQty, 0 AS UndyedConsumedQty, 0 AS UndyedRetQty, 0 As RateConversionQty    
FROM web.JobOrderCancelHeaders  H
LEFT JOIN web.JobOrderCancelLines  L ON L.JobOrderCancelHeaderId  = H.JobOrderCancelHeaderId 
LEFT JOIN Web.JobOrderLines JOL ON JOL.JobOrderLineId = L.JobOrderLineId 
LEFT JOIN web.JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId 
WHERE 1=1 AND H.DocTypeId IN (@DyeingCancel)
AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @JobWorker IS NULL OR JOH.JobWorkerId  IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
--AND ( @Process IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)   


--- For Receive 
UNION ALL 


SELECT H.SiteId, H.DivisionId, H.JobWorkerId, H.DocDate, H.ProductId , 
0 As OrderQty, H.ReceiveQty AS RecQty, H.LossQty AS LossQty, 0 AS UndyedIssQty, 0 AS UndyedConsumedQty, 0 AS UndyedRetQty, 0 As RateConversionQty    
FROM Web.FJobReceive(@ToDate, @Site,@Division, @FromDate, @ToDate, NULL , @Process, @JobWorker, NULL ) AS H 
LEFT JOIN web.JobReceiveLines L ON L.JobReceiveLineId = H.JobReceiveLineId 
LEFT JOIN web.JobOrderLines JOL ON JOL.JobOrderLineId = L.JobOrderLineId 
LEFT JOIN web.JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.ProdOrderLines Pil ON JOL.ProdOrderLineId = Pil.ProdOrderLineId
WHERE 1=1 
AND ( @DocumentType IS NULL OR JOH.DocTypeId  IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 

AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 



--- For Undyed Issue

UNION ALL 
SELECT H.SiteId, H.DivisionId, H.PersonId AS JobWorkerId, H.DocDate, L.ProductId , 
0 As OrderQty, 0 AS RecQty, 0 AS LossQty, IsNull(L.Qty_Rec,0)  AS UndyedIssQty, 0 AS UndyedConsumedQty, IsNull(L.Qty_Iss,0) AS UndyedRetQty, 0 As RateConversionQty    
FROM web.StockHeaders H
LEFT JOIN web.StockProcesses L ON L.StockHeaderId = H.StockHeaderId 
WHERE 1=1 AND H.DocTypeId IN (@DyeingIssue,@DyeingMaterialIssue,@DyeingReturn)
AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @JobWorker IS NULL OR H.PersonId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
--AND ( @Process IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)   


--- For Undyed ConsumedQty

UNION ALL 
SELECT H.SiteId, H.DivisionId, H.PersonId AS JobWorkerId, H.DocDate, L.ProductId , 
0 As OrderQty, 0 AS RecQty, 0 AS LossQty, 0  AS UndyedIssQty, IsNull(L.Qty_Iss,0) AS UndyedConsumedQty, 0 AS UndyedRetQty, 0 As RateConversionQty    
FROM web.StockHeaders H
LEFT JOIN web.StockProcesses L ON L.StockHeaderId = H.StockHeaderId 
WHERE 1=1 AND H.DocTypeId Not IN (@DyeingReturn,@DyeingRateConversion,@DyeingConsumption)
AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @JobWorker IS NULL OR H.PersonId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
--AND ( @Process IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)   


--- For Rate Conversion

UNION ALL 
SELECT H.SiteId, H.DivisionId, H.PersonId AS JobWorkerId, H.DocDate, L.ProductId , 
0 As OrderQty, 0 AS RecQty, 0 AS LossQty, 0  AS UndyedIssQty, 0 AS UndyedConsumedQty, 0 AS UndyedRetQty, IsNull(L.Qty_Iss,0) - IsNull(L.Qty_Rec,0) As RateConversionQty    
FROM web.StockHeaders H
LEFT JOIN web.StockProcesses L ON L.StockHeaderId = H.StockHeaderId 
WHERE 1=1 AND H.DocTypeId IN (@DyeingRateConversion)
AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @JobWorker IS NULL OR H.PersonId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
--AND ( @Process IS NULL OR H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)   



) As VCurrent 
Group By VCurrent.SiteId, VCurrent.DivisionId, VCurrent.JobWorkerId, VCurrent.DocDate, VCurrent.ProductId 





IF ( @ReportType = 'Dyer Wise Summary' )
BEGIN     
	Select Null AS Trans_Type, VMain.SiteId, VMain.DivisionId, Max(JW.Name) As JobWorkerName, 
	Max(JW.Name) AS GroupOnValue, Max(JW.Name) AS GroupOnText,'Dyer' AS GroupOnHead,
	IsNull(Sum(VMain.OpeningOrder),0) As OpeningOrder, 
	IsNull(Sum(VMain.OpeningUndyedQty),0) As OpeningUndyedQty, 
	IsNull(Sum(VMain.OrderQty),0) As OrderQty, 
	IsNull(Sum(VMain.ReceiveQty),0) As ReceiveQty,
	IsNull(Sum(VMain.LossQty),0) As LossQty, 
	IsNull(Sum(VMain.OpeningOrder),0) + IsNull(Sum(VMain.OrderQty),0) - IsNull(Sum(VMain.ReceiveQty),0) - IsNull(Sum(VMain.LossQty),0) AS BalIndentQty,
	IsNull(Sum(VMain.UndyedIssQty),0) As UndyedIssQty, 
	IsNull(Sum(VMain.UndyedConsumedQty),0) As UndyedConsumedQty, 
	 IsNull(Sum(VMain.UndyedRetQty),0) As UndyedRetQty, 
	IsNull(Sum(VMain.RateConversionQty),0) As RateConversionQty,
	IsNull(Sum(VMain.OpeningUndyedQty),0) + IsNull(Sum(VMain.UndyedIssQty),0) - IsNull(Sum(VMain.UndyedConsumedQty),0)-  IsNull(Sum(VMain.UndyedRetQty),0) - IsNull(Sum(VMain.RateConversionQty),0) AS balUndyedQty,
	Case When IsNull(Sum(VMain.LossQty),0) <> 0 AND (IsNull(Sum(VMain.OpeningOrder),0)+IsNull(Sum(VMain.OrderQty),0)) <> 0  then IsNull(Sum(VMain.LossQty),0) * 100/ (IsNull(Sum(VMain.OpeningOrder),0)+IsNull(Sum(VMain.OrderQty),0)) Else 0 End AS LossPer,
   'DyeingAnalysisReport_JobWorkerWise.rdl' AS ReportName, 'Dyeing Analysis Report' AS ReportTitle 
	FROM ( SELECT * FROM @TempTable ) VMain 
	LEFT JOIN web._People  JW WITH (Nolock) ON VMain.JobWorkerId = JW.PersonId 
	LEFT JOIN web.Products P WITH (Nolock) On VMain.ProductId = P.ProductId
	Where 1=1 
	Group By VMain.SiteId, VMain.DivisionId,  VMain.JobWorkerId
END 
      
ELSE
      
BEGIN    
	Select NULL AS Trans_Type, VMain.SiteId, VMain.DivisionId, Max(JW.Name) As JobWorkerName, 
	CASE WHEN @ReportType = 'Month Wise Summary' THEN Substring(Convert(nvarchar,VMain.DocDate,11),0,6) ELSE Convert(nvarchar,VMain.DocDate,103) END AS GroupOnValue, 	
	CASE WHEN @ReportType = 'Month Wise Summary' THEN CONVERT(varchar(3), VMain.DocDate )+'/'+right(CONVERT(varchar(11), VMain.DocDate ),2) ELSE Convert(nvarchar,VMain.DocDate,103) END AS GroupOnText, 
	CASE WHEN @ReportType = 'Month Wise Summary' THEN 'Month' ELSE 'Date' END AS GroupOnHead, 	
	IsNull(Sum(VMain.OpeningOrder),0) As OpeningOrder, 
	IsNull(Sum(VMain.OpeningUndyedQty),0) As OpeningUndyedQty, 
	IsNull(Sum(VMain.OrderQty),0) As OrderQty, 
	IsNull(Sum(VMain.ReceiveQty),0) As ReceiveQty,
	IsNull(Sum(VMain.LossQty),0) As LossQty, 
	IsNull(Sum(VMain.OpeningOrder),0) + IsNull(Sum(VMain.OrderQty),0) - IsNull(Sum(VMain.ReceiveQty),0) - IsNull(Sum(VMain.LossQty),0) AS BalIndentQty,
	IsNull(Sum(VMain.UndyedIssQty),0) As UndyedIssQty, 
	IsNull(Sum(VMain.UndyedConsumedQty),0) As UndyedConsumedQty, 
	 IsNull(Sum(VMain.UndyedRetQty),0) As UndyedRetQty, 
	IsNull(Sum(VMain.RateConversionQty),0) As RateConversionQty,
	IsNull(Sum(VMain.OpeningUndyedQty),0) + IsNull(Sum(VMain.UndyedIssQty),0) - IsNull(Sum(VMain.UndyedConsumedQty),0)-  IsNull(Sum(VMain.UndyedRetQty),0) - IsNull(Sum(VMain.RateConversionQty),0) AS balUndyedQty,
	Case When IsNull(Sum(VMain.LossQty),0) <> 0 AND (IsNull(Sum(VMain.OpeningOrder),0)+IsNull(Sum(VMain.OrderQty),0)) <> 0  then IsNull(Sum(VMain.LossQty),0) * 100/ (IsNull(Sum(VMain.OpeningOrder),0)+IsNull(Sum(VMain.OrderQty),0)) Else 0 End AS LossPer,
	'DyeingAnalysisReport_MonthWise.rdl' AS ReportName, 'Dyeing Analysis Report' AS ReportTitle 
	FROM ( SELECT * FROM @TempTable ) VMain 
	LEFT JOIN web._People  JW WITH (Nolock) ON VMain.JobWorkerId = JW.PersonId 
	LEFT JOIN web.Products P WITH (Nolock) On VMain.ProductId = P.ProductId
	Where 1=1 
	Group By VMain.SiteId, VMain.DivisionId, VMain.JobWorkerId, 
	CASE WHEN @ReportType = 'Month Wise Summary' THEN Substring(Convert(nvarchar,VMain.DocDate,11),0,6) ELSE Convert(nvarchar,VMain.DocDate,103) END,
	CASE WHEN @ReportType = 'Month Wise Summary' THEN CONVERT(varchar(3), VMain.DocDate )+'/'+right(CONVERT(varchar(11), VMain.DocDate ),2) ELSE Convert(nvarchar,VMain.DocDate,103) END
END           

End
GO


