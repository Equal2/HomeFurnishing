USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProductionOrderReport]    Script Date: 7/7/2015 12:16:25 PM ******/
IF OBJECT_ID ('[Web].[spRep_ProductionOrderReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_ProductionOrderReport]	
GO


/****** Object:  StoredProcedure [Web].[spRep_ProductionOrderReport]    Script Date: 7/7/2015 12:16:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create procedure   [Web].[spRep_ProductionOrderReport]    
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,   
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,
	@ProdOrderHeaderId VARCHAR(Max) = NULL, 
	@ProcessId  VARCHAR(Max) = NULL 
AS
Begin



DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 

SELECT @Dimension1IdCnt =  Count(*)
FROM  
(
SELECT * FROM Web.ProdOrderHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ',')))
AND ( @ProdOrderHeaderId is null or H.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
)  H
LEFT JOIN Web.ProdOrderLines L WITH (nolock) ON L.ProdOrderHeaderId = H.ProdOrderHeaderId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or L.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProcessId IS NULL OR L.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
AND L.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt =  Count(*)
FROM  
(
SELECT * FROM Web.ProdOrderHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProdOrderHeaderId is null or H.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
)  H

LEFT JOIN Web.ProdOrderLines L WITH (nolock) ON L.ProdOrderHeaderId = H.ProdOrderHeaderId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or L.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @ProcessId IS NULL OR L.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
AND L.Dimension2Id IS NOT NULL




DECLARE @ReportName NVARCHAR(100) 
SET @ReportName =
(
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'ProductionOrderReport_DoubleDimension'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'ProductionOrderReport_SingleDimension'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'ProductionOrderReport_SingleDimension'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'ProductionOrderReport_ProductWiseDetail'
	 END 	 	 
)


SELECT  H.SiteId,H.DivisionId,H.ProdOrderHeaderId, DT.DocumentTypeName,H.DocDate AS DocDate,H.DueDate
,DT.DocumentTypeShortName + '-' + H.DocNo AS PlanNo,Pt.ProductTypeName,L.ProdOrderLineId,P.ProductName, 
L.Qty,WPS.ProcessName,L.Specification,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,SI.SiteName,
 DI.DivisionName,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name,
   @ReportName +'.rdl' AS ReportName, 
DT.DocumentTypeName+' '+'Report' AS ReportTitle , NULL AS SubReportProcList 
FROM  
(
SELECT * FROM Web.ProdOrderHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null OR @ToDate>= H.DocDate ) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProdOrderHeaderId is null or H.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
)  H
LEFT JOIN   Web.ProdOrderLines L WITH (nolock) ON L.ProdOrderHeaderId=H.ProdOrderHeaderId
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId
LEFT JOIN Web.products P WITH (nolock) ON L.ProductId=P.ProductId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = L.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = L.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN Web.Processes WPS WITH (nolock) ON L.ProcessId=WPS.ProcessId
WHERE 1=1 AND L.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or L.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or L.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or L.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND (@ProcessId IS NULL OR WPS.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ',') ))
 ORDER BY H.DocDate,H.DocNo,L.ProdOrderLineId
End

GO


