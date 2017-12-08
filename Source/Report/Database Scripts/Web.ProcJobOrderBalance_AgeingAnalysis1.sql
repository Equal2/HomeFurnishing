USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[ProcJobOrderBalance_AgeingAnalysis1]    Script Date: 07/Aug/2015 12:04:46 PM ******/


IF OBJECT_ID ('[Web].[ProcJobOrderBalance_AgeingAnalysis1]') IS NOT NULL
	DROP PROCEDURE [Web].[ProcJobOrderBalance_AgeingAnalysis1]	
GO
/****** Object:  StoredProcedure [Web].[ProcJobOrderBalance_AgeingAnalysis1]    Script Date: 07/Aug/2015 12:04:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[ProcJobOrderBalance_AgeingAnalysis1]
 @StatusOnDate VARCHAR(20)=NULL ,
 @Division VARCHAR(20)=NULL,
 @FromDate VARCHAR(20)=NULL,
 @ToDate VARCHAR(20)=NULL,
 @DocumentType VARCHAR(max)=NULL,
 @JobWorker VARCHAR(max)=NULL,
 @Process VARCHAR(max)=NULL,
 @Product VARCHAR(Max)=NULL,
 @JobOrderHeaderId VARCHAR(max)=NULL
AS
BEGIN

DECLARE @Site VARCHAR(20)
SET @Site='17'
DECLARE @SiteCnt AS INT
DECLARE @DivisionCnt AS Int
SELECT @SiteCnt=count(DISTINCT SiteId),@DivisionCnt=count(DISTINCT DivisionId) FROM web.FjobOrderBalance(@StatusOnDate,@Site,@Division,@FromDate,@ToDate,@DocumentType,@JobWorker,@Process,@Product,@JobOrderHeaderId) H
SELECT 
(case when @DivisionCnt>1 THEN '0' else H1.DivisionId end) as DivisionId,
max(DivisionName) AS DivisionName,H1.Name AS JobWorkerName,sum(H1.BalanceQty) AS BalanceQty,H1.Ageing,max(H1.AgeingSr) AS AgeingSr,
'Ageing Analysis: Job Order Balance' AS ReportTitle,
 'JobOrderBalance_AgeingAnakysis.rdl'  AS ReportName,Null AS SubReportProcList,@Site AS SiteId
 FROM
(
select H.DivisionId,PN.Name,H.BalanceQty,D.DivisionName,
 Case         
  WHEN datediff(day, H.OrderDate,getdate() ) <= 15 THEN '0 - 15'          
  WHEN datediff(day, H.OrderDate,getdate()) > 15 And datediff(day, H.OrderDate,getdate()) <= 30 THEN '16 - 30'         
  WHEN datediff(day, H.OrderDate,getdate()) > 30 And datediff(day, H.OrderDate,getdate()) <= 45 THEN '31 - 45'         
  WHEN datediff(day, H.OrderDate,getdate()) > 45 And datediff(day, H.OrderDate,getdate()) <= 60 THEN '46 - 60'                 
  WHEN datediff(day, H.OrderDate,getdate()) > 60 THEN '> 60'      	
  ELSE 'N/A'      
  END AS Ageing  , 
  Case         
  WHEN datediff(day, H.OrderDate,getdate() ) <= 15 THEN 1          
  WHEN datediff(day, H.OrderDate,getdate()) > 15 And datediff(day, H.OrderDate,getdate()) <= 30 THEN 2       
  WHEN datediff(day, H.OrderDate,getdate()) > 30 And datediff(day, H.OrderDate,getdate()) <= 45 THEN 3        
  WHEN datediff(day, H.OrderDate,getdate()) > 45 And datediff(day, H.OrderDate,getdate()) <= 60 THEN 4                
  WHEN datediff(day, H.OrderDate,getdate()) > 60 THEN 5     	    
  END AS AgeingSr  
 FROM web.FjobOrderBalance(@StatusOnDate,@Site,@Division,@FromDate,@ToDate,@DocumentType,@JobWorker,@Process,@Product,@JobOrderHeaderId) H
 LEFT JOIN Web.People PN WITH (Nolock) ON PN.PersonID=H.JobWorkerId
 LEFT JOIN Web.Divisions D WITH  (Nolock) ON D.DivisionId=H.DivisionId
 ) AS H1
 GROUP BY H1.DivisionId,H1.Name,H1.Ageing
 end
GO


