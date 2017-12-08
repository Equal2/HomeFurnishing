USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProdOrderCancelReport]    Script Date: 7/7/2015 12:22:39 PM ******/
IF OBJECT_ID ('[Web].[spRep_ProdOrderCancelReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_ProdOrderCancelReport]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProdOrderCancelReport]    Script Date: 7/7/2015 12:22:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create  procedure   [Web].[spRep_ProdOrderCancelReport]
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@FilterDateOn VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	 
    @ProductType VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,   
    @ProductNature VARCHAR(Max) = NULL, 
    @Product VARCHAR(Max) = NULL,      
    @ProductCustomGroup VARCHAR(Max) = NULL,    
    @PersonCustomGroup VARCHAR(Max) = NULL,    
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,        
	@ProdOrderHeaderId VARCHAR(Max) = NULL,
	@ProdOrderCancelHeaderId VARCHAR(Max) = NULL,
	@ProcessId varchar(MAx)	=NULL	   
AS
BEGIN
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 



SELECT @Dimension1IdCnt =  Count(*)
FROM  
( 
SELECT * FROM Web.ProdOrderCancelHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ',')))
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProdOrderCancelHeaderId is null or H.ProdOrderCancelHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderCancelHeaderId, ','))) ) H 
LEFT JOIN  Web.ProdOrderCancelLines L ON L.ProdOrderCancelHeaderId = H.ProdOrderCancelHeaderId
LEFT JOIN  web._ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId
LEFT JOIN  Web.__ProdOrderHeaders POH ON POH.ProdOrderHeaderId= POL.ProdOrderHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = POL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
WHERE 1=1 AND P.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ',')))
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or POL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or POL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ',')))
AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Prod Order Date' THEN H.DocDate ELSE H.DocDate END ) 
AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Prod Order Date' THEN H.DocDate ELSE H.DocDate END ) 
AND POL.Dimension1Id IS NOT NULL



SELECT @Dimension2IdCnt =  Count(*)
FROM  
( 
SELECT * FROM Web.ProdOrderCancelHeaders H WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ',')))
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProdOrderCancelHeaderId is null or H.ProdOrderCancelHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderCancelHeaderId, ','))) ) H 
LEFT JOIN  Web.ProdOrderCancelLines L ON L.ProdOrderCancelHeaderId = H.ProdOrderCancelHeaderId
LEFT JOIN  web._ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId
LEFT JOIN  Web.__ProdOrderHeaders POH ON POH.ProdOrderHeaderId= POL.ProdOrderHeaderId
LEFT JOIN [Web].Products P ON P.ProductId = POL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
WHERE 1=1 AND P.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ',')))
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 

AND ( @Dimension1 is null or POL.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or POL.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ',')))
AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
AND ( @FromDate is null or @FromDate <= CASE WHEN @FilterDateOn = 'Prod Order Date' THEN H.DocDate ELSE H.DocDate END ) 
AND ( @ToDate is null or @ToDate >= CASE WHEN @FilterDateOn = 'Prod Order Date' THEN H.DocDate ELSE H.DocDate END ) 
AND POL.Dimension2Id IS NOT NULL



SELECT  H.SiteId, H.DivisionId, H.ProdOrderCancelHeaderId, DT.DocumentTypeName, H.DocDate,DT.DocumentTypeShortName + '-' + H.DocNo AS DocNo,
 DTPI.DocumentTypeShortName + '-' + POH.DocNo AS OrderNo,Pt.ProductTypeName,   L.ProdOrderCancelLineId,  L.ProdOrderLineId, P.ProductName, L.Qty,
 U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,H.CreatedBy, H.CreatedDate, H.ModifiedDate,SI.SiteName, DI.DivisionName,
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	 
	CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'ProdOrderCancel_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'ProdOrderCancel_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'ProdOrderCancel_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'ProdOrderCancel_ProductWiseDetail.rdl'
	END AS ReportName,'Prod Order Cancel Report' AS ReportTitle,POL.Remark,POL.Specification,PS.ProcessName
FROM  
( 
SELECT * FROM Web.ProdOrderCancelHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @ProdOrderCancelHeaderId is null or H.ProdOrderCancelHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderCancelHeaderId, ','))) 
) H 
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.ProdOrderCancelLines L WITH (nolock) ON L.ProdOrderCancelHeaderId = H.ProdOrderCancelHeaderId
LEFT JOIN web._ProdOrderLines POl WITH (nolock) ON L.ProdOrderLineId = POL.ProdOrderLineId 
LEFT JOIN web.__ProdOrderHeaders POH WITH (nolock) ON POH.ProdOrderHeaderId = POl.ProdOrderHeaderId
LEFT JOIN [Web].Products P WITH (nolock) ON P.ProductId = POl.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock)  ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock)  ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId   
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = POl.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = POl.Dimension2Id 
LEFT JOIN web.Sites SI WITH (nolock) ON SI.SiteId = H.SiteId
LEFT JOIN web.Divisions DI WITH (nolock) ON DI.DivisionId  = H.DivisionId
LEFT JOIN [Web].DocumentTypes DTPI WITH (nolock) ON DTPI.DocumentTypeId = POH.DocTypeId
left JOIN Web.Processes PS WITH (nolock) ON PS.ProcessId = POl.ProcessId
WHERE 1=1 AND POL.ProductId IS NOT NULL 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @Product is null or POl.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 is null or POl.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or POl.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProdOrderHeaderId is null or POl.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
And (@FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate) 
AND ( @ProcessId is null or POL.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
ORDER BY H.ProdOrderCancelHeaderId,L.ProdOrderCancelLineId,POl.ProdOrderLineId
End


GO


