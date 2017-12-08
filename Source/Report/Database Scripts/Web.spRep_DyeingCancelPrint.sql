USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_DyeingCancelPrint]    Script Date: 7/7/2015 11:58:15 AM ******/

IF OBJECT_ID ('[Web].[spRep_DyeingCancelPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_DyeingCancelPrint]	
GO
/****** Object:  StoredProcedure [Web].[spRep_DyeingCancelPrint]    Script Date: 7/7/2015 11:58:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE Procedure  [Web].[spRep_DyeingCancelPrint](@Id INT)
As
BEGIN
SELECT H.JobOrderCancelHeaderId,P.Name
,PA.Address,City.CityName AS CityName, P.Mobile AS MobileNo 
,(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo,
  DT.DocumentTypeShortName +'-'+ H.DocNo AS CancelNo,replace(convert(VARCHAR, H.DocDate, 106), ' ', '/')  AS CancelDate,WR.ReasonName AS Reason,WP.Name AS CancelBy,
 PD.ProductName,D1.Dimension1Name AS shade, D2.Dimension2Name AS Design,WDT.DocumentTypeShortName+'-'+JOH.DocNo AS  OrderNo,L.Qty,U.UnitName,POH.DocNo AS ProductOrderNo, isnull(U.DecimalPlaces,0) AS DecimalPlaces,H.Siteid,H.DivisionId,
 'DyeingCancelPrint.rdl' AS ReportName,'Dyeing Cancel' AS ReportTitle , NULL AS SubReportProcList 
FROM ( SELECT * FROM Web._JobOrderCancelHeaders WITH (nolock) WHERE JobOrderCancelHeaderId	= @Id ) H
LEFT JOIN Web._JobOrderCancelLines L WITH (nolock)  ON H.JobOrderCancelHeaderId=L.JobOrderCancelHeaderId 
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.JobWorkers JW WITH (nolock) ON JW.PersonId=H.JobWorkerId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = JW.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN web.Reasons WR WITH (nolock) ON WR.ReasonId=H.ReasonId 
LEFT JOIN Web._JobOrderLines JOL WITH (nolock) ON  JOL.JoborderLineId=L.JobOrderLineId  
LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOL.JobOrderHeaderId=JOH.JobOrderHeaderId
LEFT JOIN web.Dimension1 D1  WITH (nolock) ON D1.Dimension1Id=JOL.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=JOL.Dimension2Id 
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId
LEFT JOIN Web.DocumentTypes WDT WITH (nolock) ON WDT.DocumentTypeId=JOH.DocTypeId 
LEFT JOIN Web._ProdOrderLines PO WITH (nolock) ON PO.ProdOrderLineId=JOL.ProdOrderLineId
LEFT JOIN  Web._ProdOrderHeaders  POH WITH (nolock) ON  POH.ProdOrderHeaderId=PO.ProdOrderHeaderId
LEFT JOIN Web._People WP WITH (nolock) ON   WP.PersonId=H.OrderById 
WHERE H.JobOrderCancelHeaderId	= @Id
ORDER BY L.JobOrderCancelLineId
End

GO


