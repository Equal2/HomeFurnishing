IF object_id ('[Web].[spRep_StockReconcilationForDyeingRequirement]') IS NOT NULL 
DROP Procedure  [Web].[spRep_StockReconcilationForDyeingRequirement]
GO 

CREATE PROCEDURE Web.spRep_StockReconcilationForDyeingRequirement 
@LoginSite VARCHAR(20) = NULL,      
@LoginDivision VARCHAR(20) = NULL,      
@AsOnDate VARCHAR(20) = NULL,                   
@Product VARCHAR(Max) = NULL,     
@ProdOrderHeaderId VARCHAR(Max) = NULL 	 
AS 
BEGIN 

--#Create variables of Material Plan Header

--DECLARE @DocumentCategoryId VARCHAR(20) = NULL
DECLARE @Division VARCHAR(20) = NULL
DECLARE @StartDate NVARCHAR(20) 
SET @Division = @LoginDivision
SET @StartDate = '01/May/2015'

DECLARE @DyeingPlanTypeId INT;
DECLARE @SaleOrderPlanTypeId INT;
DECLARE @WeavingPlanTypeId INT;
DECLARE @FinishedMaterialPlanTypeId INT;


SET @DyeingPlanTypeId = 187
SET @SaleOrderPlanTypeId = 313
SET @WeavingPlanTypeId = 273
SET @FinishedMaterialPlanTypeId = 110

DECLARE @DivisionId INT;
DECLARE @SiteId INT;






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
SELECT VBalProd.* FROM
(
SELECT VProdOrder.ProdOrderLineId, Max(VProdOrder.ProductId) AS ProductId,  NULL AS Dimension1Id, NULL AS Dimension2Id,
Max(VProdOrder.ProcessId) AS ProcessId, IsNull(Sum(VProdOrder.Qty),0) AS Qty
From
(   	
		SELECT L.ProdOrderLineId, L.Qty , H.ProdOrderHeaderId, H.DivisionId, H.SiteId , H.DocTypeId, H.DocNo AS ProdOrderNo,  
		L.ProductId, L.Dimension1Id, L.Dimension2Id, L.ProcessId , H.DocDate, H.DueDate, H.Remark  
		FROM  Web.ProdOrderLines L   
		LEFT JOIN Web.ProdOrderHeaders H ON L.ProdOrderHeaderId = H.ProdOrderHeaderId  
		WHERE H.DocDate > =@StartDate AND H.DocTypeId IN (@SaleOrderPlanTypeId,@WeavingPlanTypeId,@FinishedMaterialPlanTypeId)
		AND ( @ProdOrderHeaderId is null or L.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
		
		UNION ALL  
		SELECT L.ProdOrderLineId, - L.Qty, NULL AS ProdOrderHeaderId, NULL AS DivisionId, NULL AS SiteId, NULL AS DocTypeId, NULL AS ProdOrderNo, 
		NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL ProcessId,  NULL AS DocDate, NULL AS DueDate, NULL AS Remark   
		FROM  Web.ProdOrderCancelLines L   
		LEFT JOIN Web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId    
		LEFT JOIN Web.ProdOrderHeaders H ON POL.ProdOrderHeaderId = H.ProdOrderHeaderId  
		WHERE 1=1 AND H.DocDate > =@StartDate AND H.DocTypeId IN (@SaleOrderPlanTypeId,@WeavingPlanTypeId,@FinishedMaterialPlanTypeId)
		AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
		
		UNION ALL  
		SELECT L.ProdOrderLineId,  - L.Qty, NULL AS ProdOrderHeaderId, NULL AS DivisionId, NULL AS SiteId, NULL AS DocTypeId, NULL AS ProdOrderNo, 		
		NULL AS ProductId, NULL Dimension1Id, NULL Dimension2Id, NULL ProcessId,  NULL AS DocDate  , NULL AS DueDate  , NULL AS Remark   
		FROM  Web.MaterialPlanForProdOrders L 
		LEFT JOIN Web.MaterialPlanHeaders H ON l.MaterialPlanHeaderId = H.MaterialPlanHeaderId
		LEFT JOIN Web.ProdOrderLines POL ON POL.ProdOrderLineId = L.ProdOrderLineId    
		LEFT JOIN Web.ProdOrderHeaders POH ON POL.ProdOrderHeaderId = POH.ProdOrderHeaderId  
		WHERE H.DocTypeId = @DyeingPlanTypeId
		AND POH.DocDate > =@StartDate	
		--AND 1=2   
		AND POH.DocTypeId IN (@SaleOrderPlanTypeId,@WeavingPlanTypeId,@FinishedMaterialPlanTypeId)
		AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ','))) 
		
) AS VProdOrder 
GROUP BY VProdOrder.ProdOrderLineId
HAVING IsNull(Sum(VProdOrder.Qty),0) > 0
)  VBalProd;			
			



WITH CTBom AS  
(    
	SELECT row_number() OVER (PARTITION BY B.BaseProductId ORDER BY B.BaseProductId, B.ProductId) AS Sr, 
	T.ProdOrderLineId, B.BaseProductId, B.ProductId, (B.Qty/B.BatchQty) * T.Qty * IsNull((SELECT * FROM Web.funcConvertSqFeetToSqYard(Vrs.StandardSizeArea)),1) AS ToPQty , 
	isnull(T.Qty,0) AS ToPlanArea , CASE WHEN PG.ProductTypeId IN (13,14,18,19) THEN B.Qty ELSE 0 END AS Lagat,
	B.ProcessId, B.Dimension1Id, B.Dimension2Id, 1 AS LEVEL  
	FROM @TmpTable  T  
	INNER JOIN Web.BomDetails B WITH (Nolock) On B.BaseProductId = T.ProductId
	LEFT JOIN web.Products P ON P.ProductId = B.ProductId 
	LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId 
	LEFT JOIN Web.ViewRugSize Vrs WITH (Nolock) ON B.BaseProductId = Vrs.ProductId
	UNION ALL      
	SELECT row_number() OVER (PARTITION BY B.BaseProductId ORDER BY B.BaseProductId, B.ProductId) AS Sr,
	CTBom.ProdOrderLineId, B.BaseProductId, B.ProductId, (B.Qty/B.BatchQty)  * CTBom.ToPQty * IsNull((SELECT * FROM Web.funcConvertSqFeetToSqYard(Vrs.StandardSizeArea)),1) AS ToPQty ,  
	isnull(CTBom.ToPQty,0) AS ToPlanArea ,  CASE WHEN PG.ProductTypeId IN (13,14,18,19) THEN B.Qty ELSE 0 END AS Lagat,
	B.ProcessId,  B.Dimension1Id, B.Dimension2Id, LEVEL + 1  
	FROM Web.BomDetails B WITH (Nolock)     
		INNER JOIN web.Products P ON P.ProductId = B.ProductId 
	INNER JOIN web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId 
	INNER JOIN Web.ViewRugSize Vrs WITH (Nolock) ON B.BaseProductId = Vrs.ProductId
	INNER JOIN CTBom on b.BaseProductId = CTBom.ProductId    
)      


SELECT A.*, P.ProductId, PQ.Weight AS DesignQuality,
CASE WHEN BD.Qty IS NULL THEN A.BOMLagat ELSE  BD.Qty END AS Lagat
FROM
(
SELECT Max(CTBom.Lagat) AS BOMLagat, CTBom.ProductId, CTBom.Dimension1Id, CASE WHEN Max(PT.ProductTypeName) <> 'Rug' THEN D21.Dimension2Id ELSE D2.Dimension2Id END AS Dimension2Id, CTBom.ProcessId,
max(ReqP.ProductName) AS ProductName, Max(ReqP.UnitId) AS Unit, max(ReqD.Dimension1Name) AS Dimension1Name, 
CASE WHEN Max(PT.ProductTypeName) = 'Rug' THEN Max(P.ProductId)  ELSE Max(P1.ProductId) END AS BaseProductId,
--Max(P.ProductId)  AS BaseProductId,
CASE WHEN Max(PT.ProductTypeName) <> 'Rug' THEN Max(D21.Dimension2Name) ELSE Max(D2.Dimension2Name) END AS Dimension2Name,

Round(Sum(CTBom.ToPQty),3) AS Qty,Round(Sum(CTBom.ToPlanArea),3) AS ToPlanArea,
@LoginSite AS SiteId,@Division AS DivisionId, 'Stock Reconcilation For Dyeing Requirement' AS ReportTitle, 'StockReconcilationForDyeingRequirement.rdl' AS ReportName,
 NULL  AS SubReportProcList    
FROM CTBom     
LEFT JOIN web.ProdOrderLines POL WITH (Nolock) ON POL.ProdOrderLineId = CTBom.ProdOrderLineId  
LEFT JOIN web.Products ReqP ON ReqP.ProductId = CTBom.ProductId
LEFT JOIN web.Dimension1 ReqD ON ReqD.Dimension1Id  = CTBom.Dimension1Id
LEFT JOIN web.Products P WITH (Nolock) ON P.ProductId = POL.ProductId
LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId = P.ProductGroupId 
LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Description = convert(NVARCHAR,PG.ProductGroupId)
---- For Dimension2 of Finished Material
LEFT JOIN web.ProductTypes PT WITH (Nolock) ON PT.ProductTypeId = PG.ProductTypeId  
LEFT JOIN ( SELECT BaseProductId, Max(ProductId) AS ProductId FROM web.BomDetails WITH (Nolock) GROUP BY BaseProductId ) 
BD ON BD.BaseProductId = POL.ProductId 
LEFT JOIN web.Products P1 WITH (Nolock) ON P1.ProductId = BD.ProductId
LEFT JOIN web.ProductGroups PG1 WITH (Nolock) ON PG1.ProductGroupId = P1.ProductGroupId 
LEFT JOIN web.Dimension2 D21 WITH (Nolock) ON D21.Description = convert(NVARCHAR,PG1.ProductGroupId)  
WHERE CTBom.Dimension1Id IS NOT NULL
GROUP BY CTBom.ProductId, CTBom.ProcessId, CTBom.Dimension1Id, D2.Dimension2Id, D21.Dimension2Id
) A
LEFT JOIN web.Products P WITH (Nolock) ON P.ProductName  = A.Dimension2Name
LEFT JOIN web.FinishedProduct FP WITH (Nolock) ON FP.ProductId = A.BaseProductId
LEFT JOIN web.ProductQualities PQ WITH (Nolock) ON FP.ProductQualityId = PQ.ProductQualityId 
LEFT JOIN WEb.BomDetails BD WITH (Nolock) ON BD.BaseProductId = P.ProductId AND BD.Dimension1Id =  A.Dimension1Id  AND BD.ProductId  =  A.ProductId
End
GO