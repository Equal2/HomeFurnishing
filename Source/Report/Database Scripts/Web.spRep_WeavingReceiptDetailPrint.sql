USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingReceiptDetailPrint]    Script Date: 07/Sep/2015 5:35:12 PM ******/

IF object_id ('[Web].[spRep_WeavingReceiptDetailPrint]') IS NOT NULL 
DROP PROCEDURE [Web].[spRep_WeavingReceiptDetailPrint]

/****** Object:  StoredProcedure [Web].[spRep_WeavingReceiptDetailPrint]    Script Date: 07/Sep/2015 5:35:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE  [Web].[spRep_WeavingReceiptDetailPrint] (@Id INT)
AS
BEGIN 
SELECT 
 H.JobReceiveHeaderId,H.SiteId AS SiteId,H.DivisionId AS DivisionId,
P.Name AS Name,
PA.Address AS Address,City.CityName AS CityName,P.Mobile AS MobileNo,
(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo, 
DT.DocumentTypeShortName +'-'+ H.DocNo AS ReceiveNo,
H.DocDate AS DATE,
WG.GodownName AS GodownName,
WP.Name AS ReceiveBy,H.JobWorkerDocNo AS JobWorkerDocNo,
PU.ProductUidName AS Barcode,JOH.DocNo AS OrderNo,PG.ProductGroupName AS Design,
(Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END) as size,
L.Qty AS PCS,
(ROUND(CAST(JOL.DealQty AS float) / CAST(JOL.Qty AS float), 4))*L.Qty AS Area,L.Remark AS LineRemark,
'WeavingReceiptDetailPrint.rdl'  AS ReportName,'Weaving Detail Receipt' AS ReportTitle, NULL AS SubReportProcList
 FROM 
(
SELECT  * FROM Web._JobReceiveHeaders WITH (nolock) WHERE JobReceiveHeaderId=@Id
) H
LEFT JOIN Web._JobReceiveLines L ON L.JobReceiveHeaderId=H.JobReceiveHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID =H.JobWorkerId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web._People WP WITH (nolock) ON   WP.PersonId=H.JobReceiveById
LEFT JOIN web.ProductUids PU ON PU.ProductUIDId = L.ProductUidId 
LEFT JOIN web.JobOrderLines JOL WITH (nolock) ON JOL.JobOrderLineId=L.JobOrderLineId
LEFT JOIN Web._JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JoL.JobOrderHeaderId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = PD.ProductGroupId 
LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=PD.ProductId
LEFT JOIN Web.Godowns WG WITH (nolock) ON  WG.GodownId=H.GodownId
end
GO


