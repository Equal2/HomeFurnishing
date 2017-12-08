USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingMaterialReceivePrint]    Script Date: 07/Sep/2015 5:36:26 PM ******/
IF object_id ('[Web].[spRep_WeavingMaterialReceivePrint]') IS NOT NULL 
DROP PROCEDURE [Web].[spRep_WeavingMaterialReceivePrint]
/****** Object:  StoredProcedure [Web].[spRep_WeavingMaterialReceivePrint]    Script Date: 07/Sep/2015 5:36:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure  [Web].[spRep_WeavingMaterialReceivePrint](@Id INT)
As
BEGIN
SELECT 
H.StockHeaderId,P.Name,H.SiteId,H.DivisionId,
PA.Address + City.CityName AS Address,
P.Mobile AS MobileNo,
 (SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.PersonId   AND RegistrationType ='Tin No' ) AS TinNo,
  DT.DocumentTypeShortName +'-'+ H.DocNo AS IssueNO,H.DocDate AS DATE,G.GodownName,
 PD.ProductName AS  StockName,L.Qty,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,D1.Dimension1Name AS Shade,D2.Dimension2Name AS Design, 
 CC.CostCenterName AS CostCenterName,DT.DocumentTypeName AS ReportTitle,H.GatePassHeaderId,
 NULL  AS SubReportProcList,'WeavingMaterialReceivePrint.rdl'  as  ReportName 
  FROM 
(
SELECT * FROM Web._StockHeaders WITH (nolock) WHERE StockHeaderId=@Id
) H
LEFT JOIN   Web.StockLines L WITH (nolock)  ON H.StockHeaderId=L.StockHeaderId 
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID =H.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId
LEFT JOIN Web.Godowns G WITH (nolock) ON G.GodownId=H.GodownId
LEFT JOIN Web.CostCenters CC WITH (nolock) ON CC.CostCenterId=L.CostCenterId
LEFT JOIN Web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=L.Dimension2Id
LEFT JOIN Web.Dimension1 D1 WITH (nolock) ON D1.Dimension1Id=L.Dimension1Id
End
GO


