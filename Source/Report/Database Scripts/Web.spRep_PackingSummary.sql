USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_PackingSummary]    Script Date: 14/Jul/2015 4:19:20 PM ******/

IF object_id ('[Web].[spRep_PackingSummary]') IS NOT NULL 
DROP Procedure  [Web].[spRep_PackingSummary]
GO 
/****** Object:  StoredProcedure [Web].[spRep_PackingSummary]    Script Date: 14/Jul/2015 4:19:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create procedure   [Web].[spRep_PackingSummary]
   @ReportType VARCHAR(Max) ='Product Category Wise Summary',
   @FromDate VARCHAR(20) = NULL,
   @ToDate VARCHAR(20) = NULL,
   @Site VARCHAR(20) = NULL,
   @Division VARCHAR(20) = NULL,
   @PackingHeaderid varchar(200)=Null,
   @DocumentType VARCHAR(20) = NULL,	
   @JobWorker VARCHAR(Max) = NULL,
   @DateFilterOn VARCHAR(20) = NULL, 
   @Product VARCHAR(Max) = NULL, 
   @ProductType VARCHAR(Max) = NULL,	
   @ProductNature VARCHAR(Max) = NULL,
   @ProductGroup VARCHAR(Max) = NULL,
   @ProductCustomGroup VARCHAR(Max) = NULL,
   @ProductCollection VARCHAR(Max) = NULL,
   @ProductQuality VARCHAR(Max) = NULL,
   @ProductStyle VARCHAR(Max) = NULL,
   @ProductInvoiceGroup VARCHAR(Max) = NULL,
   @ProductCategory VARCHAR(Max) = NULL,
   @ProductDesign	VARCHAR(Max) = NULL
AS
BEGIN
SELECT  PT.ProductTypeName,
		Max(CASE WHEN @ReportType ='Product Category Wise Summary' THEN PC.ProductCategoryName
			WHEN @ReportType ='Product Size Wise Summary' THEN  Replace(Str(VRS.StandardSizeArea,15,4), ' ','0')  		   
			WHEN @ReportType ='JobWorker Wise Summary' THEN PS.Name
		   	WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE PC.ProductCategoryName
		END) AS GroupOnValue,	
		Max(CASE WHEN @ReportType ='Product Category Wise Summary' THEN PC.ProductCategoryName
			WHEN @ReportType ='Product Size Wise Summary' THEN VRS.StandardSizeName 		   
			WHEN @ReportType ='JobWorker Wise Summary' THEN PS.Name
		   	WHEN @ReportType ='Month Wise Summary' THEN STUFF(CONVERT(CHAR(11), H.DocDate, 100), 5,5, '-') --Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE PC.ProductCategoryName
		END) AS GroupOnText,				
		Max(CASE WHEN @ReportType ='Product Category Wise Summary' THEN 'Product Category'
			WHEN @ReportType ='Product Size Wise Summary' THEN 'Product Size'
			WHEN @ReportType ='JobWorker Wise Summary' THEN 'JobWorker'			 
			WHEN @ReportType ='Month Wise Summary' THEN 'Month'
			ELSE 'Product Category'
		END) AS GroupTitle,				
		CASE WHEN Min(U.UnitName) =  Max(U.UnitName) THEN Max(U.UnitName) ELSE 'Mix' END AS UnitName,
		IsNull(Sum(L.Qty),0) AS Qty,Sum(L.Qty*VRS.StandardSizeArea) AS  Area,
		/*IsNull(Sum(VRS.StandardSizeArea),0) AS  TestArea,*/
		IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces,		
		Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
Max(S.SiteName) AS SiteName, Max(D.DivisionName) AS DivisionName,
'PackingSummary.rdl' AS ReportName, 'Packing Summary' AS ReportTitle, NULL AS SubReportProcList
 FROM (
SELECT * from Web._PackingHeaders H where 1=1 
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
--AND ( @FromDate is null or @FromDate <= H.DocDate ) 
--AND ( @ToDate is null or @ToDate >= H.DocDate ) 

AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ','))) 
AND ( @PackingHeaderid is null or H.PackingHeaderId IN (SELECT Items FROM [dbo].[Split] (@PackingHeaderid, ','))) 
--AND ( @DateFilterOn is null or @DateFilterOn =  Convert(DATE,H.CreatedDate)  ) 
) H
LEFT JOIN  Web._PackingLines L WITH (nolock) ON L.PackingHeaderId = H.PackingHeaderId 
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN  Web.Sites S WITH (nolock) ON H.SiteId = S.SiteId
LEFT JOIN   Web.Divisions  D WITH (nolock) ON H.DivisionId = D.DivisionId 
LEFT JOIN   Web.People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId 
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId
LEFT JOIN  Web.Units U WITH (nolock) ON U.UnitId = P.UnitId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId 
LEFT JOIN Web.FinishedProduct  FPD WITH (nolock) ON FPD.ProductId = L.ProductId 
left join web.ProductCategories PC WITH (nolock) On PC.ProductCategoryId=FPD.ProductCategoryId
left JOIN  Web.viewrugsize VRS WITH (nolock) on VRS.ProductId=L.ProductId
WHERE 1=1 AND VRS.StandardSizeArea IS NOT NULL 

AND ( @FromDate is null or @FromDate <= ( CASE WHEN @DateFilterOn = '2' THEN L.CreatedDate ELSE  H.DocDate END ) )   
AND ( @ToDate is null or @ToDate >= ( CASE WHEN @DateFilterOn = '2' THEN dateadd (Day,-1, L.CreatedDate) ELSE  H.DocDate END ) )    

AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM [dbo].[Split] (@ProductCollection, ','))) 
AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM [dbo].[Split] (@ProductQuality, ','))) 
AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM [dbo].[Split] (@ProductStyle, ','))) 
AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM [dbo].[Split] (@ProductDesign, ','))) 
AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductInvoiceGroup, ','))) 
AND ( @ProductCategory is null or FPD.ProductCategoryId IN (SELECT Items FROM [dbo].[Split] (@ProductCategory, ',')))
GROUP BY
PT.ProductTypeName,
CASE WHEN @ReportType ='Product Category Wise Summary' THEN PC.ProductCategoryName
			WHEN @ReportType ='Product Size Wise Summary' THEN VRS.StandardSizeName 		   
			WHEN @ReportType ='JobWorker Wise Summary' THEN PS.Name
		   	WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
			ELSE PC.ProductCategoryName END
			ORDER BY GroupOnValue 
END
GO

