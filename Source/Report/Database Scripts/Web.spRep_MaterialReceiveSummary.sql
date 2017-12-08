USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialReceiveSummary]    Script Date: 31/Aug/2015 1:00:29 PM ******/
DROP PROCEDURE [Web].[spRep_MaterialReceiveSummary]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialReceiveSummary]    Script Date: 31/Aug/2015 1:00:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_MaterialReceiveSummary]
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
	  	 @ReportType VARCHAR(Max) = 'Party Wise Summary'
AS
BEGIN

SELECT
    	CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Party Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
				WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,11)
				WHEN @ReportType ='ReceiveNo Wise Summary' THEN DT.DocumentTypeShortName+'-'+H.DocNo				
				ELSE P.ProductName
			END AS GroupOnValue,
	
	         CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Party Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN STUFF(CONVERT(CHAR(11), H.DocDate, 100), 5,5, '-')
			    WHEN @ReportType ='Date Wise Summary' THEN convert(NVARCHAR,H.DocDate,103)
			    WHEN @ReportType ='ReceiveNo Wise Summary' THEN DT.DocumentTypeShortName+'-'+H.DocNo
				ELSE P.ProductName
			END AS GroupOnText,
        	CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
				WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'
				WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Nature'
				WHEN @ReportType ='Party Wise Summary' THEN 'Party'
				WHEN @ReportType ='Month Wise Summary' THEN 'Month'
				WHEN @ReportType ='Date Wise Summary' THEN 'Date'
				WHEN @ReportType ='ReceiveNo Wise Summary' THEN 'Receive No'
				ELSE 'Product'
			END AS GroupTitle,
	  'Mix'  AS UnitName,
	 IsNull(L.Qty_Rec,0) AS StockQty,IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
	 H.SiteId AS SiteId, H.DivisionId AS DivisionId,
	 
     S.SiteName,D.DivisionName,Pg.ProductGroupName AS ProductGroupName, 
	'MaterialReceiveSummary.rdl' AS ReportName,'Material Receive Summary' AS ReportTitle, NULL AS SubReportProcList
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
	WHERE 1=1
	AND ( @ProcessId is null or L.ProcessId IN (SELECT Items FROM [dbo].[Split] (@ProcessId, ','))) 
	AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
	AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @PersonCustomGroup IS NULL OR H.PersonId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 	
    AND IsNull(L.Qty_Rec,0)>0
	ORDER BY GroupOnValue
END
GO


