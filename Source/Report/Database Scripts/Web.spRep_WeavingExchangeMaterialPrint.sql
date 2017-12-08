USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingExchangeMaterialPrint]    Script Date: 07/Sep/2015 5:31:08 PM ******/
IF object_id ('[Web].[spRep_WeavingExchangeMaterialPrint]') IS NOT NULL 
DROP PROCEDURE [Web].[spRep_WeavingExchangeMaterialPrint]
/****** Object:  StoredProcedure [Web].[spRep_WeavingExchangeMaterialPrint]    Script Date: 07/Sep/2015 5:31:08 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure  [Web].[spRep_WeavingExchangeMaterialPrint](@Id INT)
As
BEGIN
SELECT 
H.StockHeaderId,P.Name,H.SiteId,H.DivisionId,
PA.Address + City.CityName AS Address,
P.Mobile AS MobileNo,D1.Dimension1Name AS shade,
 (SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.PersonId   AND RegistrationType ='Tin No' ) AS TinNo,
  DT.DocumentTypeShortName +'-'+ H.DocNo AS IssueNO,H.DocDate AS DATE,G.GodownName,
 PD.ProductName AS  StockName,(CASE WHEN L.DocNature='I' THEN  - L.Qty ELSE L.Qty END) AS Qty,U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,
  (CASE WHEN  RLP.ProductName=PD.ProductName THEN NULL ELSE RLP.ProductName END) AS ItemName,
 CC.CostCenterName AS CostCenterName,CCD.Design AS Design,DT.DocumentTypeName AS ReportTitle,H.GatePassHeaderId,L.DocNature,
 'ProcGatePassPrint1 ' + convert(NVARCHAR,ISNULL(H.GatePassHeaderId,0)) +', ' + '''' +  DT.DocumentTypeShortName +'-'+ H.DocNo +''''   AS SubReportProcList,'WeavingExchangeMaterialPrint.rdl'  as  ReportName 
  FROM 
(
SELECT * FROM Web._StockHeaders WITH (nolock) WHERE StockHeaderId=@Id
) H
LEFT JOIN   Web.StockLines L WITH (nolock)  ON H.StockHeaderId=L.StockHeaderId 
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.Dimension1  D1 WITH (nolock) ON D1.Dimension1Id=L.Dimension1Id
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID =H.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId
LEFT JOIN Web.Godowns G WITH (nolock) ON G.GodownId=H.GodownId
LEFT JOIN Web.RequisitionLines RL WITH (nolock) ON RL.RequisitionLineId=L.RequisitionLineId
LEFT JOIN Web.Products RLP WITH (nolock) ON RLP.ProductId=RL.ProductId
LEFT JOIN Web.CostCenters CC WITH (nolock) ON CC.CostCenterId=L.CostCenterId
LEFT JOIN
(
SELECT jo.CostCenterId, Max(Pg.ProductGroupName) AS Design
FROM Web.RequisitionHeaders H
LEFT JOIN web.JobOrderHeaders jo ON h.ReferenceDocId = jo.JobOrderHeaderId 
LEFT JOIN web.JobOrderLines jl ON jo.JobOrderHeaderId = jl.JobOrderHeaderId 
LEFT JOIN web.Products p ON jl.ProductId = p.ProductId 
LEFT JOIN web.ProductGroups pg ON p.ProductGroupId = pg.ProductGroupId 
GROUP BY jo.CostCenterId 
) CCD  ON L.CostCenterId=CCD.CostCenterId

End
GO


