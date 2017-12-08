USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[ProcWeavingCancelPrint]    Script Date: 07/Sep/2015 5:26:30 PM ******/
IF object_id ('[Web].[ProcWeavingCancelPrint]') IS NOT NULL 
DROP PROCEDURE [Web].[ProcWeavingCancelPrint]

/****** Object:  StoredProcedure [Web].[ProcWeavingCancelPrint]    Script Date: 07/Sep/2015 5:26:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create Procedure  [Web].[ProcWeavingCancelPrint]
(@Id INT)  As  
BEGIN  
 


SELECT H.JobOrderCancelHeaderId, P.ProductName, Max(PG.ProductGroupName) AS ProductGroupName, D1.Dimension1Name, 
Max(P.UnitId) AS Unit, sum(H.Qty) AS CanQty,
Max(JOCH.SiteId) AS SiteId, Max(JOCH.DivisionId) AS DivisionId, 'WeavingCancelOrder_Print.rdl' AS ReportName ,  'Weaving Cancel' AS ReportTitle   
FROM web.JobOrderCancelBoms H
LEFT JOIN Web.JobOrderCancelHeaders JOCH ON JOCH.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId 
LEFT JOIN web.Products P ON P.ProductId = H.ProductId 
LEFT JOIN web.Dimension1 D1 ON D1.Dimension1Id = H.Dimension1Id 
LEFT JOIN web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId 
WHERE H.JobOrderCancelHeaderId= @Id
GROUP BY H.JobOrderCancelHeaderId , P.ProductName, D1.Dimension1Name
 End
GO


