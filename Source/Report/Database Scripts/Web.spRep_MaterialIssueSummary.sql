USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialIssueSummary]    Script Date: 31/Aug/2015 1:12:30 PM ******/

IF object_id ('[Web].[spRep_MaterialIssueSummary]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_MaterialIssueSummary]
GO
/****** Object:  StoredProcedure [Web].[spRep_MaterialIssueSummary]    Script Date: 31/Aug/2015 1:12:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_MaterialIssueSummary]
         @Site VARCHAR(20) = NULL,
	     @Division VARCHAR(20) = NULL,
	     @FromDate VARCHAR(20) = NULL,
	 	 @ToDate VARCHAR(20) = NULL,
	     @DocumentType VARCHAR(20) = NULL,
	     @Person VARCHAR(Max) = NULL,    
	     @ProductNature VARCHAR(Max) = NULL,
	     @ProductType VARCHAR(Max) = NULL,
	     @ProductGroup VARCHAR(Max) = NULL,
	     @ProductCustomGroup VARCHAR(Max) = NULL,
	     @PersonCustomGroup VARCHAR(Max) = NULL,
	     @Product VARCHAR(Max) = NULL,
	     @StockHeaderId VARCHAR(Max) = NULL,
	     @ProcessId VARCHAR(Max)=NULL,
	  	 @ReportType VARCHAR(Max) = 'Date Wise Summary'
AS
BEGIN

SELECT
    	Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Party Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
				WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,11)
				WHEN @ReportType ='IssueNo Wise Summary' THEN DT.DocumentTypeShortName+'-'+H.DocNo				
				ELSE P.ProductName
			END) AS GroupOnValue,
	
	         Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Party Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN STUFF(CONVERT(CHAR(11), H.DocDate, 100), 5,5, '-')
			    WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,103)
			    WHEN @ReportType ='IssueNo Wise Summary' THEN DT.DocumentTypeShortName+'-'+H.DocNo
				ELSE P.ProductName
			END) AS GroupOnText,
        	Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
				WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'
				WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Nature'
				WHEN @ReportType ='Party Wise Summary' THEN 'Party'
				WHEN @ReportType ='Month Wise Summary' THEN 'Month'
				WHEN @ReportType ='Date Wise Summary' THEN 'Date'
				WHEN @ReportType ='IssueNo Wise Summary' THEN 'Issue No'
				ELSE 'Product'
			END) AS GroupTitle,
	 CASE WHEN Min(U.UnitName) =  Max(U.UnitName) THEN Max(U.UnitName) ELSE 'Mix' END AS UnitName,
	 IsNull(Sum(L.Qty_Iss),0) AS StockQty,IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces,
	 Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
     S.SiteName,D.DivisionName, 
	'MaterialIssueSummary.rdl' AS ReportName,'Material Issue Summary' AS ReportTitle, NULL AS SubReportProcList
	 FROM 
	(
	SELECT * FROM Web.StockHeaders H WHERE 1=1
	AND (@Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ',')))
	AND (@Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ',')))
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
	AND ( @Person is null or H.PersonId IN (SELECT Items FROM [dbo].[Split] (@Person, ','))) 	
	AND ( @StockHeaderId is null or H.StockHeaderId IN (SELECT Items FROM [dbo].[Split] (@StockHeaderId, ','))) 
	) H
	LEFT JOIN Web.Stocks L WITH (Nolock) ON L.StockHeaderId=H.StockHeaderId
	LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
	LEFT JOIN Web.Sites S WITH (nolock) ON H.SiteId = S.SiteId
	LEFT JOIN Web.Divisions  D WITH (nolock) ON H.DivisionId = D.DivisionId 
	LEFT JOIN Web.People PS WITH (nolock) ON PS.PersonID = H.PersonId
	LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId
	LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
	LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId	
	LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = P.UnitId
	WHERE 1=1 AND isnull(L.Qty_Iss,0) <> 0 
	AND ( @ProcessId is null or L.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
	AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
	AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @PersonCustomGroup IS NULL OR H.PersonId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 	
	GROUP BY CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Party Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
				WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,11)
				WHEN @ReportType ='IssueNo Wise Summary' THEN DT.DocumentTypeShortName+'-'+H.DocNo
				ELSE P.ProductName END,S.SiteName,D.DivisionName
	ORDER BY GroupOnValue
END
GO


