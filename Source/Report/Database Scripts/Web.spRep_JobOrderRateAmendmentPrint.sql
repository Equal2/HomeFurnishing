USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_JobOrderRateAmendmentPrint]    Script Date: 7/7/2015 12:20:59 PM ******/
IF OBJECT_ID ('[Web].[spRep_JobOrderRateAmendmentPrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_JobOrderRateAmendmentPrint]	
GO


/****** Object:  StoredProcedure [Web].[spRep_JobOrderRateAmendmentPrint]    Script Date: 7/7/2015 12:20:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create Procedure  [Web].[spRep_JobOrderRateAmendmentPrint](@Id INT)
As
BEGIN
DECLARE @TotalAmount DECIMAL
SET @TotalAmount = (SELECT Max(Amount) FROM web.jobOrderRateAmendmentLines WHERE JobOrderAmendmentHeaderId = @Id) 

SELECT  H.JobOrderAmendmentHeaderId,L.JobOrderRateAmendmentLineId,P.Name AS DyingHouse,PA.Address,City.CityName AS CityName, P.Mobile AS MobileNo,DT.DocumentTypeName AS ReportTitle
,(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo
 , DT.DocumentTypeShortName +'-'+ H.DocNo AS 'AmendmentNO',  H.DocDate AS AmendantDate,PD.ProductName,D1.Dimension1Name AS shade,D1.Description AS Colour, D2.Dimension2Name AS Design,DTPOH.DocumentTypeShortName +'-'+  POH.DocNo AS  ProductOrderNo,
 DTJOH.DocumentTypeShortName +'-'+  JOH.DocNo AS DyeingOrderNo,
  H.ModifiedBy +' ' + Replace(replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/'),'/20','/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, 
 L.Qty AS Qty, U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces, L.JobOrderRate AS OldRate,L.AmendedRate AS Newrate,L.Rate AS RateDIFF,
 L.Amount AS DiffAmt,H.SiteId AS SiteId,H.DivisionId,H.Remark,@TotalAmount AS Netamount,'JobOrderRateAmendmentPrint.rdl' as ReportName
 
   FROM ( SELECT * FROM Web._JobOrderAmendmentHeaders WITH (nolock) WHERE JobOrderAmendmentheaderId	= @Id ) H 
   LEFT JOIN Web._JobOrderRateAmendmentLines L WITH (nolock) ON L.JobOrderAmendmentHeaderId=H.JobOrderAmendmentheaderId   
LEFT JOIN   Web._JobOrderLines JOL WITH (nolock)  ON JOL.JobOrderLineId=L.JobOrderLineId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.JobWorkers JW WITH (nolock) ON JW.PersonId=H.JobWorkerId
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = JW.PersonId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities City WITH (nolock) ON City.CityId = PA.CityId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = JOL.ProductId 
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId 
LEFT JOIN web.Dimension1 D1  WITH (nolock) ON D1.Dimension1Id=JOL.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (nolock) ON D2.Dimension2Id=JOL.Dimension2Id
LEFT JOIN Web.ProdOrderLines PO WITH (nolock) ON PO.ProdOrderLineId=JOL.ProdOrderLineId
LEFT JOIN  Web.ProdOrderHeaders  POH WITH (nolock) ON  POH.ProdOrderHeaderId=PO.ProdOrderHeaderId
LEFT JOIN Web.DocumentTypes DTPOH WITH (nolock) ON DTPOH.DocumentTypeId = POH.DocTypeId 
LEFT JOIN  web.jobOrderheaders  JOH WITH (nolock) ON  JOH.JobOrderHeaderId=JOL.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes DTJOH WITH (nolock) ON DTJOH.DocumentTypeId = JOH.DocTypeId 
WHERE H.JobOrderAmendmentHeaderId= @Id
End

GO


