IF object_id ('[Web].[ProcYarnDyeingProdOrderPrint]') IS NOT NULL 
 DROP Procedure  [Web].[ProcYarnDyeingProdOrderPrint]
GO 


CREATE Procedure  [Web].[ProcYarnDyeingProdOrderPrint](@Id INT)  
As  
BEGIN  
DECLARE @MaterialPlanHeaderId AS INT 
SET @MaterialPlanHeaderId = (SELECT MaterialPlanHeaderId FROM web.ProdOrderHeaders WITH (Nolock) WHERE ProdOrderHeaderId	=  @Id)


SELECT H.SiteId, H.DivisionId, DP.ProductId, MPL.Dimension1Id, MPL.Dimension2Id, H.ProdOrderHeaderId, DT.DocumentTypeShortName, H.DocDate, H.DocNo, H.DueDate, H.Remark,
P.ProductName, U.UnitName, U.DecimalPlaces,  BD.Qty AS Lagat, VD.DesignQuality,
D1.Dimension1Name, D1.Description AS ShadeColour, D2.Dimension2Name, VD.Pcs AS DesignPcs, VD.SQArea AS DesignArea,
MPL.Qty AS ProdPlanQty, MPL.Remark AS LineRemark,
H.ModifiedBy +' ' + replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, 
H.ModifiedDate,   AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy,   AL.ApproveDate,
NULL AS SubReportProcList, 'YarnDyeingPlan_Print' AS ReportName ,  'Dyeing Plan' AS ReportTitle               
FROM ( SELECT * FROM web.ProdOrderHeaders WITH (Nolock) WHERE ProdOrderHeaderId	=  @Id   )  H 
LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.ProdOrderLines MPL WITH (Nolock) ON MPL.ProdOrderHeaderId = H.ProdOrderHeaderId 
LEFT JOIN web.Products P WITH (Nolock) ON P.ProductId = MPL.ProductId 
LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId = P.UnitId 
LEFT JOIN web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id = MPL.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id = MPL.Dimension2Id
LEFT JOIN 
(
SELECT P.ProductGroupId, Max(PG.ProductGroupName) AS ProductGroupName, Max(PQ.Weight) AS DesignQuality, sum(VP.Qty) AS Pcs,  sum(VP.SQArea) AS SQArea
FROM
(
SELECT CASE WHEN PT.ProductTypeName <> 'Rug' THEN BD.ProductId ELSE POL.ProductId END AS ProductId, POL.Qty , 
CASE WHEN PT.ProductTypeName <> 'Rug' THEN POL.Qty*(SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard] (VRS1.StandardSizeArea))
 ELSE POL.Qty*(SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard] (VRS.StandardSizeArea)) END AS SQArea
FROM web.MaterialPlanForProdOrders L WITH (Nolock)
LEFT JOIN web.ProdOrderLines POL WITH (Nolock) ON POL.ProdOrderLineId = L.ProdOrderLineId
LEFT JOIN web.Products P ON P.ProductId = POL.ProductId 
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId  
LEFT JOIN web.ProductTypes PT ON PT.ProductTypeId = PG.ProductTypeId 
LEFT JOIN ( SELECT BaseProductId, Max(ProductId) AS ProductId FROM web.BomDetails  GROUP BY BaseProductId ) 
BD ON BD.BaseProductId = POL.ProductId 
LEFT JOIN Web.ViewRugSize VRS WITH (Nolock) ON VRS.ProductId = POL.ProductId
LEFT JOIN Web.ViewRugSize VRS1 WITH (Nolock) ON VRS1.ProductId = BD.ProductId 
WHERE L.MaterialPlanHeaderId = @MaterialPlanHeaderId
) VP
LEFT JOIN web.Products P WITH (Nolock) ON P.ProductId = VP.ProductId 
LEFT JOIN web.FinishedProduct FP WITH (Nolock) ON FP.ProductId = VP.ProductId
LEFT JOIN web.ProductQualities PQ WITH (Nolock) ON FP.ProductQualityId = PQ.ProductQualityId 
LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId = P.ProductGroupId
GROUP BY P.ProductGroupId 
) VD ON VD.ProductGroupId = convert(INT,D2.Description)
LEFT JOIN web.Products DP WITH (Nolock) ON DP.ProductName = VD.ProductGroupName
LEFT JOIN WEb.BomDetails BD WITH (Nolock) ON BD.BaseProductId = DP.ProductId AND BD.Dimension1Id =  MPL.Dimension1Id  AND BD.ProductId  =  MPL.ProductId
LEFT JOIN  (      SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate        FROM  Web.ActivityLogs AL      WHERE AL.ActivityType =2      GROUP BY AL.DocTypeId, AL.DocId  ) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.MaterialPlanHeaderId  
WHERE H.ProdOrderHeaderId	=  @Id   
ORDER BY MPL.ProdOrderLineId
End
GO
