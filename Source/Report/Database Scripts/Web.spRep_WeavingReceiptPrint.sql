USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingReceiptPrint]    Script Date: 02/Sep/2015 4:37:38 PM ******/

IF object_id ('[Web].[spRep_WeavingReceiptPrint]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_WeavingReceiptPrint]
/****** Object:  StoredProcedure [Web].[spRep_WeavingReceiptPrint]    Script Date: 02/Sep/2015 4:37:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE  [Web].[spRep_WeavingReceiptPrint] (@Id INT)
AS 
BEGIN 
 SELECT 
 H.JobReceiveHeaderId,max(H.SiteId) AS SiteId,max(H.DivisionId) AS DivisionId,max(H.JobWorkerDocNo) AS JobWorkerDocNo,
 max(P.Name) AS Name,max(PA.Address) AS Address,max(City.CityName) AS CityName, max(P.Mobile) AS MobileNo,
(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =max(H.JobWorkerId)   AND RegistrationType ='Tin No' ) AS TinNo, 
 max(DT.DocumentTypeShortName +'-'+ H.DocNo) AS ReceiveNo,max(replace(convert(VARCHAR, H.DocDate, 106), ' ', '/'))  AS DocDate,
 max(WG.GodownName) AS GodownName,
 Max(WP.Name) AS ReceiveBy, 
 max(CC.CostCenterName) AS PurjaNo,sum(L.Qty) AS PCS, 
 max(Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END) as size,
 max(PQ.ProductQualityName) AS Quality,
  max(PG.ProductGroupName) AS Design,
  max(COL.ColourName) AS Color,
  sum(L.Weight) AS Weight,
 sum(L.PenaltyAmt) AS Penalty,
  max(replace(convert(VARCHAR,JOH.DueDate, 106), ' ', '/')) AS DueDate,
 'WeavingReceiptPrint.rdl'  AS ReportName,'Weaving Receipt' AS ReportTitle, NULL AS SubReportProcList,
  dbo.SplitString ((SELECT  Web.FGetListofJobReceiveBarCode (H.JobReceiveHeaderId,JOL.JobOrderLineId)),',',22) AS  BarCode ,
  max(H.Remark) AS HeaderRemark,
  (SELECT Remark + ',' FROM  Web._JobReceiveLines WHERE JobReceiveHeaderId=H.JobReceiveHeaderId AND JobOrderLineId=JOL.JobOrderLineId FOR XML PATH('')) AS Remark
 FROM  (SELECT  * FROM Web._JobReceiveHeaders WITH (nolock) WHERE JobReceiveHeaderId	= @Id ) H 
     LEFT JOIN Web._JobReceiveLines L ON L.JobReceiveHeaderId=H.JobReceiveHeaderId
     LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
     LEFT JOIN	 web.JobWorkers JW WITH (nolock) ON JW.PersonId=H.JobWorkerId
	 LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = JW.PersonId
	 LEFT JOIN Web._People WP WITH (nolock) ON   WP.PersonId=H.JobReceiveById
	 LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
	 LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
	 LEFT JOIN Web.Godowns WG WITH (nolock) ON  WG.GodownId=H.GodownId
     LEFT JOIN Web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderLineId = L.JobOrderLineId
     LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
     LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = PD.ProductGroupId
     LEFT Join Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId=PD.ProductId 
	 LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=PD.ProductId
	 LEFT JOIN Web.ProductQualities PQ WITH (nolock)  ON FP.ProductQualityId = PQ.ProductQualityId
	 LEFT JOIN Web.Colours COL WITH (nolock) ON COL.ColourId=FP.ColourId
     LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JoL.JobOrderHeaderId
     LEFT JOIN Web.DocumentTypes WDT WITH (nolock) ON WDT.DocumentTypeId=JOH.DocTypeId 
     LEFT JOIN Web.CostCenters CC WITH (nolock) ON CC.CostCenterId=JOH.CostCenterId
  WHERE H.JobReceiveHeaderId=@Id
  GROUP BY H.JobReceiveHeaderId,JOL.ProductId,JOL.JobOrderLineId
  END
GO


