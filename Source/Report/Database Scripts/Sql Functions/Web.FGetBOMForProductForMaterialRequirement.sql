IF object_id ('[Web].[FGetBOMForProductForMaterialRequirement]') IS NOT NULL 
DROP FUNCTION [Web].[FGetBOMForProductForMaterialRequirement]
GO 

CREATE FUNCTION [Web].[FGetBOMForProductForMaterialRequirement]
(	@T AS Web.[TypeParameterForBOM] READONLY)  
RETURNS TABLE
AS 
RETURN

WITH CTBom AS  
(    
	SELECT row_number() OVER (PARTITION BY B.BaseProductId ORDER BY B.BaseProductId, B.ProductId) AS Sr, 
	T.Id, B.BaseProductId, B.ProductId, (B.Qty/B.BatchQty) * T.Qty * (SELECT * FROM Web.funcConvertSqFeetToSqYard(Vrs.StandardSizeArea)) AS ToPQty , 
	B.ProcessId, B.Dimension1Id, B.Dimension2Id, 1 AS LEVEL  
	FROM 
	(
	SELECT T.Id AS Id, T.ProductId, NULL AS Dimension1Id, NULL AS Dimension2Id, NULL AS ProcessId, T.Qty AS  Qty    
    FROM @T AS T 	
	)  T  
	INNER JOIN Web.BomDetails B WITH (Nolock) On B.BaseProductId = T.ProductId
	LEFT JOIN Web.ViewRugSize Vrs WITH (Nolock) ON B.BaseProductId = Vrs.ProductId    
    --WHERE 1=1 AND B.BaseProductId <> B.ProductId 
	UNION ALL      
	SELECT row_number() OVER (PARTITION BY B.BaseProductId ORDER BY B.BaseProductId, B.ProductId) AS Sr,
	CTBom.Id, B.BaseProductId, B.ProductId, (B.Qty/B.BatchQty)  * CTBom.ToPQty AS ToPQty ,  
	B.ProcessId,  B.Dimension1Id, B.Dimension2Id, LEVEL + 1  
	FROM Web.BomDetails B WITH (Nolock)    
	--INNER JOIN Web.Processes on b.ProcessId = Web.Processes.ProcessId
	INNER JOIN CTBom on b.BaseProductId = CTBom.ProductId  
	--WHERE 1=1 AND B.BaseProductId <> B.ProductId  
    
)      

SELECT   Max(P.DivisionId) AS ProductDivisionId, CTBom.ProductId, CTBom.Dimension1Id, D2.Dimension2Id, CTBom.ProcessId,
ceiling(Sum(CTBom.ToPQty)) AS Qty  
FROM CTBom   
LEFT JOIN @T POL ON POL.Id =  CTBom.Id 
LEFT JOIN web.Products P WITH (Nolock) ON P.ProductId = POL.ProductId
LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId = P.ProductGroupId 
LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Description = convert(NVARCHAR,PG.ProductGroupId) 
LEFT JOIN web.Products BP WITH (Nolock) ON BP.ProductId = CTBom.ProductId
LEFT JOIN web.ProductGroups BPG WITH (Nolock) ON BPG.ProductGroupId = BP.ProductGroupId 
--WHERE CTBom.Dimension1Id IS NOT NULL
WHERE BPG.ProductTypeId <> 16
GROUP BY  CTBom.ProductId, CTBom.ProcessId, CTBom.Dimension1Id, D2.Dimension2Id
--OPTION (MAXRECURSION 1000)
GO

