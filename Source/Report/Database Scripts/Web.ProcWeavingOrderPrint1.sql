IF object_id ('[Web].[ProcWeavingOrderPrint1]') IS NOT NULL 
 DROP Procedure  [Web].[ProcWeavingOrderPrint1]
GO 

CREATE Procedure  [Web].[ProcWeavingOrderPrint1]
(@Id INT)  As  
BEGIN  
DECLARE @JobOrderArea FLOAT  

SET @JobOrderArea = 
(
SELECT Sum(V.ProductArea)   AS OrderArea
FROM
(
SELECT L.Qty* VRA.SqYardPerPcs   AS ProductArea
FROM web.JobOrderLines L 
LEFT JOIN Web.ViewRugarea VRA WITH (Nolock) ON VRA.ProductId = L.ProductId   
WHERE JobOrderHeaderId = @Id
) V
)

SELECT H.JobOrderHeaderId, P.ProductName, Max(PG.ProductGroupName) AS ProductGroupName, D1.Dimension1Name, 
Max(P.UnitId) AS Unit, sum(H.Qty) AS ReqQty, @JobOrderArea AS OrderArea,
'ProcWeavingOrderPrint2 ' + convert(NVARCHAR,H.JobOrderHeaderId) AS SubReportProcList, 
--NULL AS SubReportProcList, 
Max(JOH.SiteId) AS SiteId, Max(JOH.DivisionId) AS DivisionId, 'WeavingOrder_Print1.rdl' AS ReportName ,  'Weaving Order' AS ReportTitle   
FROM web.JobOrderBoms H
LEFT JOIN WEb.JobOrderHeaders JOH ON JOH.JobOrderHeaderId = H.JobOrderHeaderId 
LEFT JOIN web.Products P ON P.ProductId = H.ProductId 
LEFT JOIN web.Dimension1 D1 ON D1.Dimension1Id = H.Dimension1Id 
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId 
WHERE H.JobOrderHeaderId = @Id
GROUP BY H.JobOrderHeaderId, P.ProductName, D1.Dimension1Name
 End
GO
