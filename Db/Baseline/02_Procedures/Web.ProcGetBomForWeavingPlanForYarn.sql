IF OBJECT_ID ('[Web].[ProcGetBomForWeavingPlanForYarn]') IS NOT NULL
	DROP PROCEDURE [Web].[ProcGetBomForWeavingPlanForYarn]	
GO


Create PROCEDURE [Web].[ProcGetBomForWeavingPlanForYarn]
(
	@T AS Web.TypeParameter READONLY,
	@MaterialPlanHeaderId INT  =NULL 
)
AS

DECLARE @FilterProductTypesConsumption NVARCHAR(255);

DECLARE @SpinningProcessId INT ;
SET @SpinningProcessId =31;

IF ( @MaterialPlanHeaderId IS NOT NULL AND @MaterialPlanHeaderId <>'' )
BEGIN 
SELECT @FilterProductTypesConsumption=MPS.filterProductTypesConsumption
FROM Web.MaterialPlanHeaders H 
LEFT JOIN web.MaterialPlanSettings MPS ON MPS.DocTypeId = H.DocTypeId AND MPS.SiteId  = H.SiteId AND MPS.DivisionId   = H.DivisionId
WHERE H.MaterialPlanHeaderId =@MaterialPlanHeaderId
END  


Declare @TmpTable as Table  
(  
	ProdOrderLineId INT,
	ProductId INT,
	Dimension1Id INT,
	Dimension2Id INT,
	ProcessId INT,
	Qty Float  
);

INSERT INTO @TmpTable (ProdOrderLineId,	ProductId ,	Dimension1Id ,Dimension2Id ,ProcessId ,Qty)
SELECT POL.ProdOrderLineId, POL.ProductId, NULL AS Dimension1Id, NULL AS Dimension2Id, POL.ProcessId, T.Qty AS  Qty    
FROM @T AS T 	
LEFT JOIN Web.ProdOrderLines AS POL ON T.Id = POL.ProdOrderLineId;


-- Fetch Data From BOM Detail of Balance Plan
declare @TempTablePlan TABLE (ProdOrderLineId INT , ProductDivisionId INT, ProductId INT , ReqQty FLOAT, UnPlannedPOReqQty FLOAT  )
insert into @TempTablePlan(ProdOrderLineId,ProductDivisionId,ProductId,ReqQty,UnPlannedPOReqQty)
SELECT  H.ProdOrderLineId AS ProdOrderLineId, H.ProductDivisionId, H.ProductId,
round(isnull(H.Qty,0),4)  AS ReqQty, 
0 AS UnPlannedPOReqQty
FROM 
(
SELECT * FROM [Web].[FGetBOM]  (@T)  
A
) H




-- Create Temp Table for Material Requred More Than MinimumOrderQty Qty
declare @TempTableProduct TABLE (ProductId int NOT NULL PRIMARY KEY, ReqQty FLOAT, MinimumOrderQty FLOAT  )
insert into @TempTableProduct(ProductId, ReqQty,MinimumOrderQty)
SELECT V1.ProductId, sum(isnull(V1.ReqQty,0)) AS ReqQty, Max(isnull(PSD.MinimumOrderQty,0)) AS MinimumOrderQty
FROM 
(
SELECT * FROM @TempTablePlan
) V1
LEFT JOIN web.ProductSiteDetails PSD WITH (Nolock) ON PSD.ProductId =  V1.ProductId
WHERE 1=1 
GROUP BY V1.ProductId
HAVING sum(isnull(V1.ReqQty,0)) >  Max(isnull(PSD.MinimumOrderQty,0))



SELECT H.ProdOrderLineId,H.ProductId,NULL AS Dimension1Id, NULL AS Dimension2Id, @SpinningProcessId AS ProcessId, H.ReqQty AS Qty
FROM @TempTablePlan H
LEFT JOIN @TempTableProduct PSD ON PSD.ProductId =  H.ProductId
LEFT JOIN web.Products P ON P.ProductId = H.ProductId
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId 
WHERE 1=1
AND isnull(PSD.ReqQty,0) > 0
AND (@FilterProductTypesConsumption IS NULL or PG.ProductTypeId IN (SELECT Items FROM web.Split(@FilterProductTypesConsumption,',')))
GO
