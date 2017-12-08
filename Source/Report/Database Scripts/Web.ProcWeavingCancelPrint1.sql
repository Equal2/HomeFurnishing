USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingCancelPrint]    Script Date: 07/Sep/2015 5:23:56 PM ******/

IF object_id ('[Web].[spRep_WeavingCancelPrint]') IS NOT NULL 
DROP PROCEDURE [Web].[spRep_WeavingCancelPrint]
/****** Object:  StoredProcedure [Web].[spRep_WeavingCancelPrint]    Script Date: 07/Sep/2015 5:23:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_WeavingCancelPrint](@Id INT)
AS
BEGIN
SELECT H.JobOrderCancelHeaderId,
 dbo.SplitString ((SELECT  Web.FGetListofJobCancelBarCode (JOL.ProductUidHeaderId,H.DocTypeId,H.DocNO,@Id)),',',22) AS  BarCode,
H.SiteId, H.DivisionId,DT.DocumentTypeShortName + '-' + H.DocNo AS CancelNo,H.DocDate AS DATE,
PC.Name AS CancelBy,P.Name AS Holdername,
PA.Address AS Address,City.CityName AS CityName,P.Mobile AS MobileNo,
(SELECT TOP 1 isnull(RegistrationNo,'')  FROM web.PersonRegistrations WHERE PersonId = H.JobWorkerId AND RegistrationType = 'PAN No' ) AS PanNo,
PQ.ProductQualityName AS Quality,
PG.ProductGroupName as  Design,
Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END as size,
COL.ColourName AS Color,
L.Qty as PCS,
(ROUND(CAST(JOL.DealQty AS float) / CAST(JOL.Qty AS float), 4))*L.Qty as Area,
JOL.Rate AS Rate,
 DTPI.DocumentTypeShortName + '-' + JOH.DocNo AS OrderNo,
 'WeavingCancelPrint.rdl'  AS ReportName,DT.DocumentTypeName + ' Print' AS ReportTitle,
 'ProcWeavingCancelPrint  ' + convert(NVARCHAR,H.JobOrderCancelHeaderId) AS SubReportProcList
 FROM 
(
SELECT  * FROM Web.JobOrderCancelHeaders WITH (nolock) WHERE JobOrderCancelHeaderId= @Id
) H
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.JobOrderCancelLines L ON L.JobOrderCancelHeaderId=H.JobOrderCancelHeaderId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = H.JobWorkerId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.People PC WITH (nolock) ON PC.PersonID = H.OrderById
LEFT JOIN Web.JobOrderLines JOL WITH (nolock) ON L.JobOrderLineId = JOL.JobOrderLineId
LEFT JOIN Web.JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId 
LEFT JOIN Web.DocumentTypes DTPI WITH (nolock) ON DTPI.DocumentTypeId = JOH.DocTypeId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
LEFT Join Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId=PD.ProductId 
LEFT JOIN Web.Colours COL WITH (nolock) ON COL.ColourId=FP.ColourId
LEFT JOIN Web.ProductQualities PQ WITH (nolock)  ON FP.ProductQualityId = PQ.ProductQualityId
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = PD.ProductGroupId
LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=PD.ProductId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = PD.UnitId 
END
GO


