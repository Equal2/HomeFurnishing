USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiveSummary]    Script Date: 04/Aug/2015 12:12:08 PM ******/

IF OBJECT_ID ('[Web].[spRep_PurchaseGoodsReceiveSummary]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_PurchaseGoodsReceiveSummary]	
GO
/****** Object:  StoredProcedure [Web].[spRep_PurchaseGoodsReceiveSummary]    Script Date: 04/Aug/2015 12:12:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_PurchaseGoodsReceiveSummary]
	   	--@DateFilterOn VARCHAR(20) =Null,
	    @Site VARCHAR(20) = NULL,
	    @Division VARCHAR(20) = NULL,
		@FromDate VARCHAR(20) = NULL,
		@ToDate VARCHAR(20) = NULL,
		@DocumentType VARCHAR(20) = NULL,	
	    @Supplier VARCHAR(Max) = NULL,     
	    @ProductNature VARCHAR(Max) = NULL,
	    @ProductType VARCHAR(Max) = NULL,
	    @ProductGroup VARCHAR(Max) = NULL,
	    @ProductCustomGroup VARCHAR(Max) = NULL,
	    @PersonCustomGroup VARCHAR(Max) = NULL,
	    @Product VARCHAR(Max) = NULL,    
	    @PurchaseIndentHeaderId VARCHAR(Max) = NULL,  
	   	@PurchaseGoodReceiptHeaderId VARCHAR(Max) = NULL,
		@ReportType VARCHAR(Max) = 'Product Wise Summary'
	as
	BEGIN
	SELECT Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Supplier Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
				ELSE P.ProductName
			END) AS GroupOnValue,
	
	         Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Supplier Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN STUFF(CONVERT(CHAR(11), H.DocDate, 100), 5,5, '-')
				ELSE P.ProductName
			END) AS GroupOnText,
			
	Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product'
				WHEN @ReportType ='Product Group Wise Summary' THEN 'Product Group'
				WHEN @ReportType ='Product Type Wise Summary' THEN 'Product Nature'
				WHEN @ReportType ='Supplier Wise Summary' THEN 'Supplier'
				WHEN @ReportType ='Month Wise Summary' THEN 'Month'
				ELSE 'Product'
			END) AS GroupTitle,
	CASE WHEN Min(U.UnitName) =  Max(U.UnitName) THEN Max(U.UnitName) ELSE 'Mix' END AS UnitName,
	IsNull(Sum(L.Qty),0) AS ReceiveQty,
	IsNull(Max(U.DecimalPlaces),0) AS DecimalPlaces,
	Max(H.SiteId) AS SiteId, Max(H.DivisionId) AS DivisionId,
	S.SiteName,D.DivisionName, 
	'PurchaseGoodsReceiveSummary.rdl' AS ReportName, 'Purchase Goods Receive Summary' AS ReportTitle, NULL AS SubReportProcList
	
	FROM
	(
	SELECT * FROM Web._PurchaseGoodsReceiptHeaders H WHERE 1=1
	AND (@Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ',')))
	AND (@Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ',')))
	AND ( @FromDate is null or @FromDate <= H.DocDate ) 
	AND ( @ToDate is null or @ToDate >= H.DocDate ) 
	AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
	AND ( @Supplier is null or H.SupplierId IN (SELECT Items FROM [dbo].[Split] (@Supplier, ','))) 
	AND ( @PurchaseGoodReceiptHeaderId is null or H.PurchaseGoodsReceiptHeaderId IN (SELECT Items FROM [dbo].[Split] (@PurchaseGoodReceiptHeaderId, ','))) 
	--AND ( @DateFilterOn is null or @DateFilterOn =  Convert(DATE,H.CreatedDate))  
	)  H
	LEFT JOIN Web.PurchaseGoodsReceiptLines L WITH (nolock) ON L.PurchaseGoodsReceiptHeaderId=H.PurchaseGoodsReceiptHeaderId
	LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
	LEFT JOIN  Web.Sites S WITH (nolock) ON H.SiteId = S.SiteId
	LEFT JOIN   Web.Divisions  D WITH (nolock) ON H.DivisionId = D.DivisionId 
	LEFT JOIN   Web.People PS WITH (nolock) ON PS.PersonID = H.SupplierId
	LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId
	LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
	LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId
	LEFT JOIN  Web.Units U WITH (nolock) ON U.UnitId = P.UnitId
	LEFT JOIN Web.PurchaseIndentLines Pil ON L.PurchaseIndentLineId = Pil.PurchaseIndentLineId
	WHERE 1=1  
	AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
	AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
	AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
	AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
	AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
	AND ( @PersonCustomGroup IS NULL OR H.SupplierId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
	GROUP BY CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName
				WHEN @ReportType ='Product Group Wise Summary' THEN Pg.ProductGroupName 
				WHEN @ReportType ='Product Type Wise Summary' THEN Pt.ProductTypeName
				WHEN @ReportType ='Supplier Wise Summary' THEN PS.Name
				WHEN @ReportType ='Month Wise Summary' THEN Substring(convert(NVARCHAR,H.DocDate,11),0,6)
				ELSE P.ProductName END,S.SiteName,D.DivisionName
	ORDER BY GroupOnValue
	end
GO


