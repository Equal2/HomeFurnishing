USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialPlanPrint]    Script Date: 24/Jul/2015 12:20:06 PM ******/
DROP PROCEDURE [Web].[spRep_MaterialPlanPrint]
GO
IF OBJECT_ID ('[Web].[spRep_MaterialPlanPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_MaterialPlanPrint]
GO

/****** Object:  StoredProcedure [Web].[spRep_MaterialPlanPrint]    Script Date: 24/Jul/2015 12:20:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [Web].[spRep_MaterialPlanPrint]
@Id INT
as
begin 

	DECLARE @Dimension1IdCnt INT 
    DECLARE @Dimension2IdCnt INT
 select @Dimension1IdCnt =  Count(*)  FROM Web._MaterialPlanLines L WHERE Dimension1Id IS NOT NULL AND L.MaterialPlanHeaderId=@Id
 select @Dimension2IdCnt =  Count(*)  FROM Web._MaterialPlanLines L WHERE Dimension2Id IS NOT NULL AND L.MaterialPlanHeaderId=@Id
 
 	SELECT H.MaterialPlanHeaderId,DT.DocumentTypeName AS ReportTitle, DT.DocumentTypeShortName,H.DueDate, 
 	DT.DocumentTypeShortName +'-'+ H.DocNo AS DocNo,H.DocDate,H.Remark AS HeaderRemark,
 	H.SiteId,H.DivisionId, NULL AS SubReportProcList,L.MaterialPlanLineId,PD.ProductName,
 	U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces,L.Remark ,
 	L.RequiredQty AS Requiredqty,L.ProdPlanQty AS ProdPlanqty,L.PurchPlanQty,L.StockPlanQty AS StockPlanqty,
 		 CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
     CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name,
	 CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'MaterialPlanPrint_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'MaterialPlanPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'MaterialPlanPrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'MaterialPlanPrint.rdl'
	END AS ReportName

 	FROM (SELECT * FROM Web._MaterialPlanHeaders WITH (nolock) WHERE MaterialPlanHeaderId= @Id) H
 	LEFT JOIN Web._MaterialPlanLines L WITH (nolock) ON L.MaterialPlanHeaderId=H.MaterialPlanHeaderId
    LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
    LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
	LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId 
	LEFT JOIN  Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id = L.Dimension1Id  
    LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id = L.Dimension2Id
    WHERE H.MaterialPlanHeaderId	= @Id 
    ORDER BY L.MaterialPlanLineId
end
GO


