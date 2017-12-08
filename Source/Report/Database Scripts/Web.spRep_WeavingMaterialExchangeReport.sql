USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingMaterialExchangeReport]    Script Date: 02/Sep/2015 1:19:38 PM ******/

IF object_id ('[Web].[spRep_WeavingMaterialExchangeReport]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_WeavingMaterialExchangeReport]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingMaterialExchangeReport]    Script Date: 02/Sep/2015 1:19:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE  [Web].[spRep_WeavingMaterialExchangeReport]
			         @Site VARCHAR(20) = NULL,
				     @Division VARCHAR(20) = NULL,
				     @FromDate VARCHAR(20) = NULL,
				 	 @ToDate VARCHAR(20) = NULL,
				     @DocumentType VARCHAR(20) = NULL,
				     @Person VARCHAR(Max) = NULL  ,
				     @ProductNature VARCHAR(Max) = NULL,
				     @ProductType VARCHAR(Max) = NULL,
				     @ProductGroup VARCHAR(Max) = NULL,
				     @ProductCustomGroup VARCHAR(Max) = NULL,
				     @PersonCustomGroup VARCHAR(Max) = NULL,
				     @Product VARCHAR(Max) = NULL,
				     @StockHeaderId VARCHAR(Max) = NULL				    
			AS
			BEGIN
			select 
			     H.SiteId  ,H.DivisionId,
				 DT.DocumentTypeShortName+'-'+H.DocNo  as IssueNo,
				 CC.CostCenterName as PurjaNo,
				 PS.Name,
				 Pg.ProductGroupName AS ProductGroupName,
				 sum(IsNull(L.Qty,0)) AS StockQty,
				 max(IsNull(U.DecimalPlaces,0)) AS DecimalPlaces,
				 'WeavingMaterialExchangeReport.rdl' AS ReportName,'Material Exchange  For Weaving Report' AS ReportTitle, NULL AS SubReportProcList,
				H.DocDate as Issuedate,
			   	L.DOCNature AS exchange
				  from 
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
				 LEFT JOIN Web.StockLines L WITH (Nolock) ON L.StockHeaderId=H.StockHeaderId
				LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
				LEFT JOIN Web.CostCenters CC with (nolock) on CC.CostCenterId=L.CostCenterId
				LEFT JOIN Web.Sites S WITH (nolock) ON H.SiteId = S.SiteId
				LEFT JOIN Web.Divisions  D WITH (nolock) ON H.DivisionId = D.DivisionId 
				LEFT JOIN Web.People PS WITH (nolock) ON PS.PersonID = H.PersonId
				LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId
				LEFT JOIN Web.ProductGroups Pg ON P.ProductGroupId = Pg.ProductGroupId
				LEFT JOIN Web.ProductTypes Pt ON Pg.ProductTypeId = Pt.ProductTypeId	
				LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = P.UnitId
				WHERE 1=1			 
				AND ( @Product is null or L.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
				AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
				AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
				AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
				AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
				AND ( @PersonCustomGroup IS NULL OR H.PersonId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 	
			     AND IsNull(L.Qty,0)>0 AND L.DOCNature IS NOT NULL
			    GROUP BY H.SiteId,H.DivisionId,DT.DocumentTypeShortName+'-'+H.DocNo,CC.CostCenterName,PS.Name,Pg.ProductGroupName,H.DocDate,L.DOCNature
			    ORDER BY  H.DocDate
			End
GO


