USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobReceiveBalance]    Script Date: 04/Aug/2015 12:18:37 PM ******/

IF OBJECT_ID ('[Web].[spRep_JobReceiveBalance]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobReceiveBalance]	
GO
/****** Object:  StoredProcedure [Web].[spRep_JobReceiveBalance]    Script Date: 04/Aug/2015 12:18:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_JobReceiveBalance]
      @StatusOnDate VARCHAR(20) = NULL,
      @Site VARCHAR(20) = NULL,
      @Division VARCHAR(20) = NULL,
   	  @FromDate VARCHAR(20) = NULL,
   	  @ToDate VARCHAR(20) = NULL,
   	  @DocumentType VARCHAR(20) = NULL,
      @JobWorker VARCHAR(Max) = NULL,
      @Process VARCHAR(Max) = NULL,
      @Product VARCHAR(Max) = NULL,
      @ProductNature VARCHAR(Max) = NULL,
      @ProductType VARCHAR(Max) = NULL,
      @ProductGroup VARCHAR(Max) = NULL,
      @ProductCustomGroup VARCHAR(Max) = NULL,
      @PersonCustomGroup VARCHAR(Max) = NULL,
      @JobreceiveHeaderId VARCHAR(Max) = NULL,
      @Dimension1 VARCHAR(Max) = NULL,
      @Dimension2 VARCHAR(Max) = NULL
   AS
   BEGIN 
      
      DECLARE @Dimension1IdCnt INT 
      DECLARE @Dimension2IdCnt INT 
      
     SELECT @Dimension1IdCnt = Count(*)
      FROM Web.FJobReceiveBalance(@StatusOnDate,@Site,@Division,@FromDate,@ToDate,@DocumentType,@JobWorker,@Process,@Product,@JobreceiveHeaderId) AS H
      LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
	  LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
      LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId	
      WHERE 1=1	
	  AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	  AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	  AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
      AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
      AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
	  AND H.Dimension1Id IS NOT NULL
	  
	  SELECT @Dimension2IdCnt = Count(*)
      FROM Web.FJobReceiveBalance(@StatusOnDate,@Site,@Division,@FromDate,@ToDate,@DocumentType,@JobWorker,@Process,@Product,@JobreceiveHeaderId) AS H
      LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
	  LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
      LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId	
      WHERE 1=1	
	  AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	  AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	  AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
      AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
      AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
	  AND H.Dimension2Id IS NOT NULL 
	  
	  SELECT H.JobReceiveNo,ReceiveDate, Pp.Name AS JobWorkerName, P.ProductName, H.Specification,
	  Pt.ProductTypeName,
	  CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
     CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
     CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
     CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name,H.BalQty,	 
     U.UnitName AS UnitName,
     IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
     H.SiteId AS SiteId, H.DivisionId AS DivisionId,
     S.SiteName AS SiteName, D.DivisionName AS DivisionName, 
     CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'JobReceiveBalanceWithDoubleDimension.rdl'
	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'JobReceiveBalanceWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'JobReceiveBalanceWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'JobReceiveBalance.rdl'
     END AS ReportName,PS.ProcessName AS ProcessName,
     H.DocumentTypeName +' Balance' AS ReportTitle, NULL AS SubReportProcList
     
	    FROM Web.FJobReceiveBalance(@StatusOnDate,@Site,@Division,@FromDate,@ToDate,@DocumentType,@JobWorker,@Process,@Product,@JobreceiveHeaderId) AS H
	    LEFT JOIN web.Processes PS ON PS.ProcessId=H.ProcessId
	    LEFT JOIN Web.Products P ON H.ProductId = P.ProductId	  
		LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
		LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
		LEFT JOIN Web.Dimension1Types Dt1 ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
		LEFT JOIN Web.Dimension2Types Dt2 ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
		LEFT JOIN Web.Units U ON P.UnitId = U.UnitId
		LEFT JOIN Web._People Pp ON H.JobWorkerId = Pp.PersonID 
		LEFT JOIN Web.Sites S ON H.SiteId = S.SiteId
		LEFT JOIN Web.Divisions D ON H.DivisionId = D.DivisionId
		LEFT JOIN Web.Dimension1 D1 ON H.Dimension1Id = D1.Dimension1Id
		LEFT JOIN Web.Dimension2 D2 ON H.Dimension2Id = D2.Dimension2Id
		WHERE 1=1 
		AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
        AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
        AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
        AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
        AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
        AND ( @Dimension1 is null or H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
        AND ( @Dimension2 is null or H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ',')))
        ORDER BY H.ReceiveDate
   END
GO


