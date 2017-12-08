USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_DyeingOrderPrint]    Script Date: 7/7/2015 11:54:24 AM ******/

IF OBJECT_ID ('[Web].[spRep_DyeingOrderPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_DyeingOrderPrint]	
GO

/****** Object:  StoredProcedure [Web].[spRep_DyeingOrderPrint]    Script Date: 7/7/2015 11:54:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure  [Web].[spRep_DyeingOrderPrint](@Id INT)
As
BEGIN
DECLARE @TotalAmount DECIMAL 
SET @TotalAmount = (SELECT Max(Amount) FROM web.jobOrderheadercharges WHERE HeaderTableId = @Id AND ChargeId = 34 ) 
SELECT H.JobOrderHeaderId,P.Name AS DyingHouse,PA.Address,City.CityName AS CityName, P.Mobile AS MobileNo
,(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo
 , DT.DocumentTypeShortName +'-'+ H.DocNo AS 'OrderNO', replace(convert(VARCHAR, H.DocDate, 106), ' ', '/')  AS 'ORDERDate',replace(convert(VARCHAR, H.DueDate, 106), ' ', '/')  AS DueDate
 , H.CreditDays,PD.ProductName,D1.Dimension1Name AS shade, D1.Description AS Colour,D2.Dimension2Name AS Design,POH.DocNo AS 'PoNo',
 L.Qty AS Qty,L.DealQty AS DealQty, L.Rate, L.Amount AS Amount,H.Remark, H.TermsAndConditions
 ,DT.DocumentTypeShortName, H.DocNo,L.JobOrderlineid
,L.DueDate AS LineDueDate,L.Remark AS LineRemark, U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces,   
  UD.UnitName AS DealUnit, isnull(UD.DecimalPlaces,0) AS DeliveryUnitDecimalPlace,
 H.ModifiedBy +' ' + Replace(replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/'),'/20','/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate,
 'Dyeing Order' AS ReportTitle,WD.DivisionName AS DIVISION, WP.Name AS OrderByName,
 H.SiteId AS SiteId,H.DivisionId, 'DyeingOrderPrint.rdl' AS ReportName,@TotalAmount AS NetAmount,
 H.GatePassHeaderId AS  GatePassHeaderId,'web.jobOrderheadercharges' AS ChargesTableName, 
--not under stand
 --'ProcGatePassPrint ' + convert(NVARCHAR,H.GatePassHeaderId) +', ' + '''' +  DT.DocumentTypeShortName +'-'+ H.DocNo +''''   AS SubReportProcList   
   
 --ProductTypeName + 'Dyeing Order' AS ReportTitle
 
 'spRep_TransactionCharges ' + convert(NVARCHAR,H.JobOrderHeaderId) + ', ' + '''web.jobOrderheadercharges''' AS SubReportProcList  
 
   FROM ( SELECT * FROM [Web]._JobOrderHeaders WITH (nolock) WHERE JobOrderHeaderId	= @Id ) H 
LEFT JOIN Web._JobOrderLines L WITH (nolock)  ON H.JobOrderHeaderId=L.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.JobWorkers JW WITH (nolock) ON JW.PersonId=H.JobWorkerId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = JW.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = PD.ProductGroupId
LEFT JOIN Web.Divisions WD WITH (nolock) ON WD.DivisionId=H.DivisionId
LEFT JOIN Web.Sites WS WITH (nolock) ON WS.SiteId=H.SiteId
LEFT JOIN Web._People WP WITH (nolock) ON   WP.PersonId=H.OrderById
LEFT JOIN web.ProductTypes PT WITH (nolock) ON PT.ProductTypeId = PG.ProductTypeId
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId 
LEFT JOIN Web.Units UD WITH (nolock) ON UD.UnitId = L.DealUnitId  
LEFT JOIN web.Dimension1 D1  WITH (nolock) ON D1.Dimension1Id=L.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=L.Dimension2Id
LEFT JOIN Web.ProdOrderLines PO WITH (nolock) ON PO.ProdOrderLineId=L.ProdOrderLineId
LEFT JOIN Web.ProdOrderHeaders  POH WITH (nolock) ON  POH.ProdOrderHeaderId=PO.ProdOrderHeaderId
WHERE H.JobOrderHeaderId	= @Id
End
GO






