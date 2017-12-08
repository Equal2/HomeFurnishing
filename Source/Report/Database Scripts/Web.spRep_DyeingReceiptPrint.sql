USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_DyeingReceiptPrint]    Script Date: 7/7/2015 11:55:46 AM ******/

IF OBJECT_ID ('[Web].[spRep_DyeingReceiptPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_DyeingReceiptPrint]	
GO

/****** Object:  StoredProcedure [Web].[spRep_DyeingReceiptPrint]    Script Date: 7/7/2015 11:55:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure  [Web].[spRep_DyeingReceiptPrint](@Id INT)
As
BEGIN
SELECT H.JobReceiveHeaderId,P.Name,PA.Address,City.CityName AS CityName, P.Mobile AS MobileNo 
,(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo,
  DT.DocumentTypeShortName +'-'+ H.DocNo AS ReceiveNo,replace(convert(VARCHAR, H.DocDate, 106), ' ', '/')  AS DocDate,WG.GodownName,
 WP.Name AS ReceiveBy,WDT.DocumentTypeShortName+'-'+JOH.DocNo AS  DocNo,PD.ProductName,D2.Dimension2Name,D1.Dimension1Name,
convert(DECIMAL(18,4),isnull(L.Qty,0))+convert(DECIMAL(18,4),isnull(L.LossQty,0)) AS QtyDyed,L.Qty,L.LossQty,L.PassQty,L.Remark AS  LineRemark,
'DyeingReceiptPrint.rdl' AS ReportName,'Dyeing Receive' AS ReportTitle,H.Siteid,H.DivisionId,H.Remark AS  HeaderRemark,
U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces
 FROM ( SELECT * FROM Web._JobReceiveHeaders WITH (nolock) WHERE JobReceiveHeaderId	= @Id ) H 
 LEFT JOIN Web._JobReceiveLines L ON L.JobReceiveHeaderId=H.JobReceiveHeaderId
 LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
 LEFT JOIN	 web.JobWorkers JW WITH (nolock) ON JW.PersonId=H.JobWorkerId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = JW.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.Godowns WG WITH (nolock) ON  WG.GodownId=H.GodownId
LEFT JOIN Web._People WP WITH (nolock) ON   WP.PersonId=H.JobReceiveById
LEFT JOIN Web.JobOrderLines JOL WITH (nolock) ON  JOL.JoborderLineId=L.JobOrderLineId  
LEFT JOIN web.Dimension1 D1  WITH (nolock) ON D1.Dimension1Id=JOL.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=JOL.Dimension2Id 
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
LEFT JOIN Web.JobOrderHeaders JOH WITH (nolock) ON JOL.JobOrderHeaderId=JOH.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes WDT WITH (nolock) ON WDT.DocumentTypeId=JOH.DocTypeId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId 
WHERE H.JobReceiveHeaderId	= @Id
ORDER BY L.JobReceiveLineId
End
GO


