USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_StockIssuePrint]    Script Date: 7/2/2015 6:30:54 PM ******/

IF OBJECT_ID ('[Web].[spRep_StockIssuePrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_StockIssuePrint]	
GO
/****** Object:  StoredProcedure [Web].[spRep_StockIssuePrint]    Script Date: 7/2/2015 6:30:54 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure  [Web].[spRep_StockIssuePrint](@Id INT)
As
BEGIN
DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT
SELECT @Dimension1IdCnt =  Count(*) FROM  Web._StockLines L WHERE L.Dimension1Id IS NOT NULL AND L.StockHeaderId= @Id

SELECT @Dimension2IdCnt =  Count(*) FROM  Web._StockLines L WHERE L.Dimension2Id IS NOT NULL AND L.StockHeaderId= @Id

SELECT H.StockHeaderId,P.Name
,PA.Address + City.CityName AS Address, P.Mobile AS MobileNo 
,(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.PersonId   AND RegistrationType ='Tin No' ) AS TinNo,
  DT.DocumentTypeShortName +'-'+ H.DocNo AS SlipNo, H.DocDate AS DATE,G.GodownName,  
 PD.ProductName,L.Qty,L.LotNo,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,L.Remark,H.Remark AS HeaderRemark,H.Siteid,H.DivisionId,
  DT.DocumentTypeName AS ReportTitle,H.GatePassHeaderId,
  
  CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
  
  CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'StoreIssuePrint_DoubleDimension.rdl'
	 	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'StoreIssuePrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'StoreIssuePrint_SingleDimension.rdl'
	 	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'StoreIssuePrint.rdl'
	END AS ReportName  ,
	'ProcGatePassPrint1 ' + convert(NVARCHAR,isnull(H.GatePassHeaderId,0)) + ', ' + H.DocNo AS SubReportProcList 
FROM (SELECT * FROM Web._StockHeaders WITH (nolock) WHERE StockHeaderId= @Id) H
LEFT JOIN   Web._StockLines L WITH (nolock)  ON H.StockHeaderId=L.StockHeaderId 
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID =H.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = PD.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId
LEFT JOIN Web.Godowns G WITH (nolock) ON G.GodownId=H.GodownId
LEFT JOIN web.Dimension1 D1  WITH (nolock) ON D1.Dimension1Id=L.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=L.Dimension2Id
LEFT JOIN Web.Dimension1Types Dt1 WITH (nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
WHERE H.StockHeaderId	= @Id 
ORDER BY L.StockLineId
End
GO



