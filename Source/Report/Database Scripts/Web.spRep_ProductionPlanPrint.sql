USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProductionPlanPrint]    Script Date: 13/Jul/2015 3:26:28 PM ******/
DROP PROCEDURE [Web].[spRep_ProductionPlanPrint]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProductionPlanPrint]    Script Date: 13/Jul/2015 3:26:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


Create Procedure  [Web].[spRep_ProductionPlanPrint](@Id INT)
As
BEGIN
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT
SELECT @Dimension1IdCnt =  Count(*) FROM  Web._ProdOrderLines L WHERE L.Dimension1Id IS NOT NULL AND L.ProdOrderHeaderId= @Id
SELECT @Dimension2IdCnt =  Count(*) FROM  Web._ProdOrderLines L WHERE L.Dimension2Id IS NOT NULL AND L.ProdOrderHeaderId= @Id
SELECT H.ProdOrderHeaderId,DT.DocumentTypeShortName +'-'+ H.DocNo AS PlanNo, H.DocDate AS Plandate,H.DueDate,
 P.ProductName,L.Qty,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,L.Remark,H.Remark AS HeaderRemark,H.Siteid,H.DivisionId,
  DT.DocumentTypeName AS ReportTitle,  
  CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name ,
  CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'ProductionPlanPrint_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'ProductionPlanPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'ProductionPlanPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'ProductionPlanPrint.rdl'
	END AS ReportName  , NULL AS SubReportProcList	
FROM (SELECT * FROM Web._ProdOrderHeaders WITH (nolock) WHERE ProdOrderHeaderId= @Id) H
LEFT JOIN   Web._ProdOrderLines L WITH (nolock)  ON H.ProdOrderHeaderId=L.ProdOrderHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = P.UnitId
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = L.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = L.Dimension2Id 
WHERE H.ProdOrderHeaderId	= @Id 
ORDER BY L.ProdOrderLineId
End

GO


