USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProductionPlanPrint]    Script Date: 7/7/2015 12:24:20 PM ******/
IF OBJECT_ID ('[Web].[spRep_ProductionCancelPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_ProductionCancelPrint]
GO

/****** Object:  StoredProcedure [Web].[spRep_ProductionPlanPrint]    Script Date: 7/7/2015 12:24:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create Procedure  [Web].[spRep_ProductionCancelPrint](@Id INT)
As
BEGIN
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT

SELECT @Dimension1IdCnt =  Count(*) FROM  Web.ProdOrderCancelHeaders H
LEFT JOIN   Web.ProdOrderCancelLines L WITH (nolock)  ON H.ProdOrderCancelHeaderId=L.ProdOrderCancelHeaderId
LEFT JOIN   Web.ProdOrderLines POL WITH (nolock)  ON L.ProdOrderLineId=POL.ProdOrderLineId
 WHERE POL.Dimension1Id IS NOT NULL AND H.ProdOrderCancelHeaderId= @Id

SELECT @Dimension2IdCnt =  Count(*) FROM  Web.ProdOrderCancelHeaders H
LEFT JOIN   Web.ProdOrderCancelLines L WITH (nolock)  ON H.ProdOrderCancelHeaderId=L.ProdOrderCancelHeaderId
LEFT JOIN   Web.ProdOrderLines POL WITH (nolock)  ON L.ProdOrderLineId=POL.ProdOrderLineId
 WHERE POL.Dimension2Id IS NOT NULL AND H.ProdOrderCancelHeaderId= @Id

 /*SELECT  TOP 1 * FROM Web.ProdOrderCancelHeaders
SELECT TOP 1 * FROM Web.ProdOrderCancelLines
SELECT TOP 1 * FROM Web.ProdOrderLines */

SELECT H.ProdOrderCancelHeaderId,DT.DocumentTypeShortName +'-'+ H.DocNo AS CancelNo, H.DocDate AS Canceldate,
 P.ProductName,L.Qty,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,H.Remark AS HeaderRemark,H.Siteid,H.DivisionId,
  DT.DocumentTypeName AS ReportTitle,
  CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name,
  CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'ProductionOrdCancelPrint_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'ProductionOrdCancelPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'ProductionOrdCancelPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'ProductionOrdCancelPrint.rdl'
	END AS ReportName  , NULL AS SubReportProcList	
FROM (SELECT * FROM Web.ProdOrderCancelHeaders WITH (nolock) WHERE ProdOrderCancelHeaderId= @Id) H
LEFT JOIN   Web.ProdOrderCancelLines L WITH (nolock)  ON H.ProdOrderCancelHeaderId=L.ProdOrderCancelHeaderId
LEFT JOIN  Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
left join   Web.ProdOrderLines POL with (nolock) on L.ProdOrderLineId=POL.ProdOrderLineId
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = POL.ProductId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = P.UnitId
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = POL.Dimension1Id  
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = POL.Dimension2Id 
WHERE H.ProdOrderCancelHeaderId	= @Id 
ORDER BY L.ProdOrderCancelLineId
End
GO



