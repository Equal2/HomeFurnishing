IF object_id ('[Web].[ProcMaterialRequirement]') IS NOT NULL 
 DROP Procedure  [Web].[ProcMaterialRequirement]
GO 


CREATE procedure   [Web].[ProcMaterialRequirement]     
@AsOnDate VARCHAR(20) = NULL 
AS  
Begin  

--#Create Table For Add Product Group Rank
DECLARE @TempTable TABLE ( ItemGroupName NVARCHAR (100), Rank INT  )
insert into @TempTable(ItemGroupName, Rank)
SELECT 'Woolen Yarn', 1
UNION 
SELECT 'Silk', 2

DECLARE @DocumentCategoryId VARCHAR(Max)
DECLARE @LoginSite VARCHAR(20)
DECLARE @StartDate VARCHAR(20)

SET  @StartDate = '01/May/2015' 

IF @AsOnDate IS NULL  SET @AsOnDate = getdate ()
IF @DocumentCategoryId IS NULL  SET @DocumentCategoryId = 331
IF @LoginSite IS NULL  SET @LoginSite = 17


--Declare DocType & ContraDocTypes
DECLARE @PlanDocTypeId AS NVARCHAR (20)
DECLARE @FilterContraDocTypes AS NVARCHAR (20)

SELECT @FilterContraDocTypes = Max(H.filterContraDocTypes),
@PlanDocTypeId = Max(H.DocTypeId) 
FROM Web.MaterialPlanSettings H  WITH (Nolock)
LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = H.DocTypeId 
WHERE DT.DocumentCategoryId  = @DocumentCategoryId 
AND (@LoginSite IS NULL or H.SiteId  = @LoginSite)



DECLARE @T AS Web.TypeParameterForBOM
DECLARE @TS AS Web.TypeParameterForBOM


-- Insert Value For Balance Prod Order Qty
INSERT INTO @T  
SELECT VProdOrder.ProdOrderLineId, Max(VProdOrder.ProductId) AS ProductId,  IsNull(Sum(VProdOrder.Qty),0) AS Qty
From
(   	
		SELECT L.ProdOrderLineId, L.Qty , H.ProdOrderHeaderId, H.DivisionId, H.SiteId , H.DocTypeId, H.DocNo AS ProdOrderNo,  
		L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId , H.DocDate  
		FROM  Web.ProdOrderLines L WITH (Nolock)  
		LEFT JOIN Web.ProdOrderHeaders H WITH (Nolock) ON L.ProdOrderHeaderId = H.ProdOrderHeaderId  
		LEFT JOIN Web.Products P WITH (Nolock) ON L.ProductId  = P.ProductId 
		LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId 
		WHERE 1=1 
		AND H.DocDate > =@StartDate AND isnull(FP.IsSample,0) = 0
		AND ( @FilterContraDocTypes is null or H.DocTypeId IN (SELECT Items FROM  Web.[Split] (@FilterContraDocTypes, ',')))   
		AND ( @LoginSite is null or H.SiteId IN (SELECT Items FROM  Web.[Split] (@LoginSite, ',')))   		
		AND ( @AsOnDate is null or @AsOnDate >= H.DocDate )   		


		UNION ALL  
		SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL AS DivisionId, NULL AS SiteId, NULL AS DocTypeId, NULL AS ProdOrderNo, 
		NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL ProcessId,  NULL AS DocDate  
		FROM  Web.ProdOrderCancelLines L  WITH (Nolock) 
		LEFT JOIN Web.ProdOrderLines POL WITH (Nolock) ON POL.ProdOrderLineId = L.ProdOrderLineId    
		LEFT JOIN Web.ProdOrderHeaders H WITH (Nolock) ON POL.ProdOrderHeaderId = H.ProdOrderHeaderId  
		LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId  = POL.ProductId 
		LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId
		WHERE 1=1 AND H.DocDate > =@StartDate AND isnull(FP.IsSample,0) = 0
		AND ( @FilterContraDocTypes is null or H.DocTypeId IN (SELECT Items FROM  Web.[Split] (@FilterContraDocTypes, ','))) 
		AND ( @LoginSite is null or H.SiteId IN (SELECT Items FROM  Web.[Split] (@LoginSite, ',')))   
		AND ( @AsOnDate is null or @AsOnDate >= H.DocDate )   
		
		
		UNION ALL  
		SELECT L.ProdOrderLineId,  - L.Qty, NULL AS ProdOrderHeaderId, NULL AS DivisionId, NULL AS SiteId, NULL AS DocTypeId, NULL AS ProdOrderNo, 		
		NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL ProcessId,  NULL AS DocDate   
		FROM  Web.MaterialPlanForProdOrders L WITH (Nolock)
		LEFT JOIN Web.MaterialPlanHeaders H WITH (Nolock) ON l.MaterialPlanHeaderId = H.MaterialPlanHeaderId 
		LEFT JOIN Web.ProdOrderLines POL WITH (Nolock) ON POL.ProdOrderLineId = L.ProdOrderLineId    
		LEFT JOIN Web.ProdOrderHeaders POH WITH (Nolock) ON POL.ProdOrderHeaderId = POH.ProdOrderHeaderId  
		LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId  = POL.ProductId 
		LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId
		WHERE POH.DocDate > =@StartDate AND isnull(FP.IsSample,0) = 0
		AND H.DocTypeId = @PlanDocTypeId
		AND ( @FilterContraDocTypes is null or POH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@FilterContraDocTypes, ','))) 
		AND ( @LoginSite is null or POH.SiteId IN (SELECT Items FROM  Web.[Split] (@LoginSite, ',')))   
		AND ( @AsOnDate is null or @AsOnDate >= POH.DocDate )   
	  

) AS VProdOrder 
GROUP BY VProdOrder.ProdOrderLineId
HAVING IsNull(Sum(VProdOrder.Qty),0) > 0
		



-- Insert Value For Balance Sale Order Qty

INSERT INTO @TS  
SELECT L.SaleOrderLineId, L.ProductId, L.BalanceQty AS Qty 
FROM ( SELECT * FROM Web.ViewSaleOrderBalanceForPlanning WITH (Nolock) ) L
LEFT JOIN web.SaleOrderHeaders H ON H.SaleOrderHeaderId = L.SaleOrderHeaderId
LEFT JOIN Web.Products P WITH (Nolock) ON L.ProductId  = P.ProductId
LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId
WHERE H.DocDate > =@StartDate
AND isnull(FP.IsSample,0) = 0

--SELECT * FROM @T





-- Fetch Data From BOM Detail of Balance Plan
declare @TempTablePlan TABLE ( ProductDivisionId INT, ProductId INT , ReqQty FLOAT, UnPlannedPOReqQty FLOAT  )



insert into @TempTablePlan(ProductDivisionId,ProductId,ReqQty,UnPlannedPOReqQty)
SELECT  H.ProductDivisionId, H.ProductId,
round(isnull(H.Qty,0),4)  AS ReqQty, 
0 AS UnPlannedPOReqQty
FROM 
(
SELECT * FROM [Web].[FGetBOMForProductForMaterialRequirement]  (@T)  
A
) H

UNION ALL 

SELECT  6 AS ProductDivisionId, H.ProductId,
0 AS ReqQty, H.Qty AS UnPlannedPOReqQty
FROM 
(
SELECT * FROM [Web].[FGetBOMForProductForMaterialRequirement]  (@TS)  B
) H




-- Create Temp Table for Material Requred More Than MinimumOrderQty Qty
declare @TempTableProduct TABLE (ProductId int NOT NULL PRIMARY KEY, ReqQty FLOAT, MinimumOrderQty FLOAT  )

insert into @TempTableProduct(ProductId, ReqQty,MinimumOrderQty)

SELECT V1.ProductId, sum(isnull(V1.ReqQty,0)) AS ReqQty, Max(isnull(PSD.MinimumOrderQty,0)) AS MinimumOrderQty
FROM 
(
SELECT * FROM @TempTablePlan
) V1
LEFT JOIN web.ProductSiteDetails PSD WITH (Nolock) ON PSD.SiteId = @LoginSite AND PSD.ProductId =  V1.ProductId
WHERE 1=1 
GROUP BY V1.ProductId
HAVING sum(isnull(V1.ReqQty,0)) >  Max(isnull(PSD.MinimumOrderQty,0))



-- Main Qry
SELECT VMain.*,  
--isnull(VS.StockQty,0) AS StockQty , 
0 AS StockQty ,
'Material Requirement' AS ReportTitle,  'MaterialRequirement'  AS ReportName, 'ProcMaterialRequirement1' AS SubReportProcList,
( SELECT TOP 1 Notes  FROM Web.ReportHeaders WITH (Nolock) WHERE SqlProc = 'ProcMaterialRequirement' ) AS ReportNotes
FROM
(

SELECT @LoginSite AS SiteId, V1.ProductDivisionId, V1.ProductId, 
Max(D.DivisionName) AS ProductDivisionName,
Max(P.ProductName) AS ProductName, Max(U.UnitName) AS Unit, 
Max(U.DecimalPlaces) AS DecimalPlaces, Max(PG.ProductGroupName) AS ProductGroup,
Max(isnull(TIG.Rank,100)) AS ProductGroupRank, sum(isnull(V1.ReqQty,0)) AS ReqQty,
sum(V1.UnPlannedPOReqQty) AS UnPlannedPOReqQty, 
( SELECT TOP 1 SiteName  FROM Web.Sites WHERE SiteId = @LoginSite )  AS SiteName, 
'Consolidated' AS DivisionName
FROM 
(
SELECT H.* 
FROM @TempTablePlan H
LEFT JOIN @TempTableProduct PSD ON PSD.ProductId =  H.ProductId
WHERE isnull(PSD.ReqQty,0) > 0 
) V1
LEFT JOIN web.Divisions D WITH (Nolock) ON D.DivisionId = V1.ProductDivisionId
LEFT JOIN web.Products P WITH (nolock) ON P.ProductId = V1.ProductId
LEFT JOIN web.Units U WITH (nolock) ON U.UnitId = P.UnitId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = P.ProductGroupId 
LEFT JOIN @TempTable TIG ON TIG.ItemGroupName = PG.ProductGroupName
GROUP BY V1.ProductDivisionId, V1.ProductId 


) VMain
WHERE 1=1 


END
GO


