IF OBJECT_ID ('[Web].[sp_ListProdOrderBalanceForMPlan]') IS NOT NULL
	DROP PROCEDURE [Web].[sp_ListProdOrderBalanceForMPlan]	
GO


Create PROCEDURE Web.sp_ListProdOrderBalanceForMPlan 
	 @MaterialPlanHeaderId INT AS 
BEGIN 


--#Create variables of Material Plan Header
DECLARE @StartDate NVARCHAR(20) ;
DECLARE @PlanDocTypeId INT;
DECLARE @DivisionId INT;
DECLARE @SiteId INT;

SET @StartDate = '01/May/2015'

 
SELECT @PlanDocTypeId=H.DocTypeId, @DivisionId=H.DivisionId,@SiteId= H.SiteId  
FROM Web.MaterialPlanHeaders H WHERE H.MaterialPlanHeaderId =@MaterialPlanHeaderId


--#Create variables of Material Plan Settings

DECLARE @FilterProductTypes NVARCHAR(255);
DECLARE @FilterContraDocTypes NVARCHAR(255);
DECLARE @filterContraSites NVARCHAR(255);
DECLARE @filterContraDivisions NVARCHAR(255);

SELECT @FilterProductTypes = H.filterProductTypes,
@FilterContraDocTypes = H.filterContraDocTypes,
@filterContraSites = H.filterContraSites ,
@filterContraDivisions = H.filterContraDivisions   
FROM Web.MaterialPlanSettings H 
WHERE H.DocTypeId = @PlanDocTypeId 
AND H.DivisionId =@DivisionId 
AND H.SiteId = @SiteId

IF ( @filterContraSites = NULL) SET  @filterContraSites = @SiteId
IF ( @filterContraDivisions = NULL) SET  @filterContraDivisions = @DivisionId


--#Main Query
SELECT VProdOrder.ProdOrderLineId, VProdOrder.BalanceQty, VProdOrder.ProdOrderHeaderId, VProdOrder.DivisionId, VProdOrder.SiteId, VProdOrder.DocTypeId, VProdOrder.ProdOrderNo,  VProdOrder.ProductId,  
VProdOrder.Dimension1Id,VProdOrder.Dimension2Id, VProdOrder.ProcessId, VProdOrder.ProdOrderDate,
P.ProductName, D1.Dimension1Name, D2.Dimension2Name, U.UnitName, pro.ProcessName, P.ProductGroupId,dt.DocumentTypeName DocTypeName,
(SELECT Convert(BIT,(CASE WHEN Count(*) > 0 THEN 1 ELSE 0 END)) FROM Web.BomDetails WHERE BaseProductId = VProdOrder.ProductId) AS IsBomExist
FROM  (  
SELECT VProdOrder.ProdOrderLineId, IsNull(Sum(VProdOrder.Qty),0) AS BalanceQty, Max(VProdOrder.ProdOrderHeaderId) AS ProdOrderHeaderId, Max(VProdOrder.DivisionID) AS DivisionId, Max(VProdOrder.SiteId) AS SiteId, Max(VProdOrder.DocTypeId) DocTypeId,  Max(VProdOrder.ProdOrderNo) AS ProdOrderNo,  Max(VProdOrder.ProductId) AS ProductId,  
Max(VProdOrder.Dimension1Id) AS Dimension1Id,Max(VProdOrder.Dimension2Id) AS Dimension2Id, Max(VProdOrder.ProcessId) AS ProcessId, Max(VProdOrder.DocDate) AS ProdOrderDate

From
(   	SELECT L.ProdOrderLineId, L.Qty , H.ProdOrderHeaderId, H.DivisionId, H.SiteId , H.DocTypeId, H.DocNo AS ProdOrderNo,  
		L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId , H.DocDate  
		FROM  Web.ProdOrderLines L   
		LEFT JOIN Web.ProdOrderHeaders H ON L.ProdOrderHeaderId = H.ProdOrderHeaderId 
		LEFT JOIN Web.Products P WITH (Nolock) ON L.ProductId  = P.ProductId 
		LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId 
		WHERE 1=1 AND H.DocDate > =@StartDate AND isnull(FP.IsSample,0) = 0		
		UNION ALL  
		SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL AS DivisionId, NULL AS SiteId, NULL AS DocTypeId, NULL AS ProdOrderNo, 
		NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL ProcessId,  NULL AS DocDate  
		FROM  Web.ProdOrderCancelLines L   
		LEFT JOIN Web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId    
		LEFT JOIN Web.ProdOrderHeaders H ON POL.ProdOrderHeaderId = H.ProdOrderHeaderId  
		LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId  = POL.ProductId 
		LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId
		WHERE 1=1 AND H.DocDate > =@StartDate AND isnull(FP.IsSample,0) = 0
				
		UNION ALL  
		SELECT L.ProdOrderLineId,  - L.Qty, NULL AS ProdOrderHeaderId, NULL AS DivisionId, NULL AS SiteId, NULL AS DocTypeId, NULL AS ProdOrderNo, 		
		NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL ProcessId,  NULL AS DocDate   
		FROM  Web.MaterialPlanForProdOrders L 
		LEFT JOIN Web.MaterialPlanHeaders H ON l.MaterialPlanHeaderId = H.MaterialPlanHeaderId 
		LEFT JOIN Web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId    
		LEFT JOIN Web.ProdOrderHeaders POH ON POL.ProdOrderHeaderId = POH.ProdOrderHeaderId  
		LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId  = POL.ProductId 
		LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId
		WHERE POH.DocDate > =@StartDate AND isnull(FP.IsSample,0) = 0
		AND H.DocTypeId = @PlanDocTypeId
		
) AS VProdOrder GROUP BY VProdOrder.ProdOrderLineId
		) AS VProdOrder  
LEFT JOIN Web.DocumentTypes dt ON VProdOrder.DocTypeId = dt.DocumentTypeId 		
LEFT JOIN web.Products p ON VProdOrder.ProductId = P.ProductId 
LEFT JOIN Web.ProductGroups pg ON p.ProductGroupId = pg.ProductGroupId 
LEFT JOIN web.Dimension1  d1 ON VProdOrder.Dimension1Id = D1.Dimension1Id 
LEFT JOIN web.Dimension2  d2 ON VProdOrder.Dimension2Id = D2.Dimension2Id 
LEFT JOIN web.Processes pro   ON VProdOrder.ProcessId = pro.ProcessId 
LEFT JOIN web.units u ON p.UnitId = u.UnitId 
WHERE 1=1--p.DivisionId =@DivisionId
AND (@FilterProductTypes IS NULL or pg.ProductTypeId IN (SELECT Items FROM web.Split(@FilterProductTypes,',')))
AND (@FilterContraDocTypes IS NULL OR VProdOrder.DocTypeId IN (SELECT Items FROM web.Split(@FilterContraDocTypes,',')))
AND (@filterContraSites IS NULL or VProdOrder.SiteId IN (SELECT Items FROM web.Split(@filterContraSites,',')))
AND (@filterContraDivisions IS NULL or VProdOrder.DivisionId IN (SELECT Items FROM web.Split(@filterContraDivisions,',')))
AND vProdOrder.BalanceQty>0
End
GO

