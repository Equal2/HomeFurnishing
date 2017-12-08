IF OBJECT_ID ('[Web].[ProcGetBomForFinishedProductPlan]') IS NOT NULL
	DROP PROCEDURE [Web].[ProcGetBomForFinishedProductPlan]	
GO


Create PROCEDURE [Web].[ProcGetBomForFinishedProductPlan]
(
	@T AS Web.TypeParameter READONLY,
	@MaterialPlanHeaderId INT  =NULL 
)
AS


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


WITH CTBom AS  
(    
	SELECT row_number() OVER (PARTITION BY B.BaseProductId ORDER BY B.BaseProductId, B.ProductId) AS Sr, 
	T.ProdOrderLineId, B.BaseProductId, B.ProductId, (B.Qty/B.BatchQty) * T.Qty AS ToPQty , 
	B.ProcessId, B.Dimension1Id, B.Dimension2Id, 1 AS LEVEL  
	FROM @TmpTable  T  
	INNER JOIN Web.BomDetails B On B.BaseProductId = T.ProductId
	UNION ALL      
	SELECT row_number() OVER (PARTITION BY B.BaseProductId ORDER BY B.BaseProductId, B.ProductId) AS Sr,
	CTBom.ProdOrderLineId, B.BaseProductId, B.ProductId, (B.Qty/B.BatchQty)  * CTBom.ToPQty AS ToPQty ,  
	B.ProcessId,  B.Dimension1Id, B.Dimension2Id, LEVEL + 1  
	FROM Web.BomDetails B     
	--INNER JOIN Web.Processes on b.ProcessId = Web.Processes.ProcessId
	INNER JOIN CTBom on b.BaseProductId = CTBom.ProductId    
)      

SELECT CTBom.ProdOrderLineId, CTBom.ProductId, CTBom.Dimension1Id, CtBom.Dimension2Id, CTBom.ProcessId,Sum(CTBom.ToPQty) AS Qty  
FROM CTBom     
LEFT JOIN Web.Products p ON ctbom.ProductId = p.ProductId 
LEFT JOIN web.ProductGroups g ON p.ProductGroupId = g.ProductGroupId 
LEFT JOIN web.ProductTypes pt ON g.ProductTypeId = pt.ProductTypeId 
WHERE pt.ProductNatureId =2 --Where ProductNature = 'Finished Material'   CTBom.Level = (SELECT Max(Level) FROM CTBom)
GROUP BY CTBom.ProdOrderLineId, CTBom.ProductId, CTBom.Dimension1Id, CtBom.Dimension2Id, CTBom.ProcessId
GO

