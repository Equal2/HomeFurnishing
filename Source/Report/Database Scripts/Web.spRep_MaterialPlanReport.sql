USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialPlanReport]    Script Date: 13/Jul/2015 2:27:30 PM ******/
IF OBJECT_ID ('[Web].[spRep_MaterialPlanReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_MaterialPlanReport]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialPlanReport]    Script Date: 13/Jul/2015 2:27:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure   [Web].[spRep_MaterialPlanReport]
 @Site VARCHAR(20) = NULL,
 @Division VARCHAR(20) = NULL,
 @DocumentType VARCHAR(Max) = NULL,
 @MaterialPlanHeaderId VARCHAR(Max) = NULL,
 @ProductType VARCHAR(Max) = NULL, 
 @ProductGroup VARCHAR(Max) = NULL,
 @ProductNature VARCHAR(Max) = NULL,
 @Product VARCHAR(Max) = NULL,
 @ProductCustomGroup VARCHAR(Max) = NULL,
 @Dimension1  VARCHAR(Max) = NULL,
 @Dimension2 VARCHAR(Max) = NULL,
 @ProcessId VARCHAR(Max)=NULL,
 @FromDate VARCHAR(20) = NULL,
 @ToDate VARCHAR(20) = NULL
As
BEGIN
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 
SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM web._MaterialPlanHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ',')))
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate )  
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND (  @MaterialPlanHeaderId is null or H.MaterialPlanHeaderId IN (SELECT Items FROM [dbo].[Split] ( @MaterialPlanHeaderId, ','))) 
) H 
LEFT JOIN  web._MaterialPlanLines L ON L.MaterialPlanHeaderId = H.MaterialPlanHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON PG.ProductTypeId = Pt.ProductTypeId 
WHERE 1=1 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND (  @MaterialPlanHeaderId is null or H.MaterialPlanHeaderId IN (SELECT Items FROM [dbo].[Split] ( @MaterialPlanHeaderId, ','))) 
AND L.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt =  Count(*)
FROM  
( 
SELECT * FROM web._MaterialPlanHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ',')))
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate )  
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND (  @MaterialPlanHeaderId is null or H.MaterialPlanHeaderId IN (SELECT Items FROM [dbo].[Split] ( @MaterialPlanHeaderId, ','))) 
) H 
LEFT JOIN  web._MaterialPlanLines L ON L.MaterialPlanHeaderId = H.MaterialPlanHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON PG.ProductTypeId = Pt.ProductTypeId 
WHERE 1=1  
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND (  @MaterialPlanHeaderId is null or H.MaterialPlanHeaderId IN (SELECT Items FROM [dbo].[Split] ( @MaterialPlanHeaderId, ','))) 
AND L.Dimension2Id IS NOT NULL



SELECT 
H.MaterialPlanHeaderId,L.MaterialPlanLineId,H.DocDate as PlanDate,DT.DocumentTypeShortName +'-'+ H.DocNo AS PlanNo,H.DueDate as DueDate
,WP.ProcessName,PD.ProductName,L.Dimension1Id,L.Dimension2Id,L.RequiredQty,
U.UnitName,L.ProdPlanQty,L.PurchPlanQty, isnull(U.DecimalPlaces,0) AS DecimalPlaces
,H.Siteid,H.DivisionId,SI.SiteName,DI.DivisionName,PT.ProductTypeName , NULL AS SubReportProcList,L.Remark,L.SpeciFication
,CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 
 CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'MaterialPlanReport_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'MaterialPlanReport_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'MaterialPlanReport_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'MaterialPlanReport_ProductWiseDetail.rdl'
	END AS ReportName ,'Material Plan Report' AS ReportTitle
FROM  
( 
--select top 2 * from web.MaterialPlanHeaders
--select top 2 * from web.MaterialPlanLines
SELECT * FROM web._MaterialPlanHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @MaterialPlanHeaderId is null or H.MaterialPlanHeaderId IN (SELECT Items FROM [dbo].[Split] (@MaterialPlanHeaderId, ','))) 
) H 
LEFT JOIN  web._MaterialPlanLines L WITH (nolock)  ON H.MaterialPlanHeaderId=L.MaterialPlanHeaderId 
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = PD.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON PG.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock)  ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock)  ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = PD.UnitId  
LEFT JOIN  Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = L.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = L.Dimension2Id 
LEFT JOIN   Web.Processes WP WITH (nolock) ON WP.ProcessId=L.ProcessId
LEFT JOIN  web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN  web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
WHERE 1=1 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or PD.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR PD.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @MaterialPlanHeaderId is null or H.MaterialPlanHeaderId IN (SELECT Items FROM [dbo].[Split] (@MaterialPlanHeaderId, ',')))  
AND ( @ProcessId is null or L.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',')))  
ORDER BY L.MaterialPlanLineId
End
GO


