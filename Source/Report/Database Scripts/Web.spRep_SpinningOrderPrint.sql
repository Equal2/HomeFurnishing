IF object_id ('[Web].[spRep_SpinningOrderPrint]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_SpinningOrderPrint]
GO 


CREATE Procedure  [Web].[spRep_SpinningOrderPrint](@Id INT)
As
BEGIN
DECLARE @TotalAmount DECIMAL 
SET @TotalAmount = (SELECT Max(Amount) FROM web.JobOrderheaderCharges WHERE HeaderTableId = @Id AND ChargeId = 34 ) 

SELECT H.JobOrderHeaderId, P.Name AS JobWorkerName, PA.Address , C.CityName AS CityName, P.Mobile AS MobileNo,
(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='Tin No' ) AS TinNo,
(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.JobWorkerId   AND RegistrationType ='PAN No' ) AS PANNo,
DT.DocumentTypeShortName +'-'+ H.DocNo AS OrderNO, H.DocDate, H.DueDate,
H.CreditDays, PD.ProductName, PODT.DocumentTypeShortName +'-'+ POH.DocNo AS ProdOrderNo, G.GodownName, POB.Name AS OrderByName,
L.Qty AS Qty,L.DealQty AS DealQty, L.Rate, L.Amount AS Amount,H.Remark, H.TermsAndConditions,
L.JobOrderlineid, L.DueDate AS LineDueDate,L.Remark AS LineRemark, U.UnitName AS UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces,   
H.ModifiedBy +' ' + Replace(replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/'),'/20','/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate,
H.SiteId AS SiteId,H.DivisionId, 'Spinning Order' AS ReportTitle, 'SpinningOrderPrint.rdl' AS ReportName,
@TotalAmount AS NetAmount, NULL  AS SubReportProcList   
FROM ( SELECT * FROM [Web]._JobOrderHeaders WITH (nolock) WHERE JobOrderHeaderId	= @Id ) H 
LEFT JOIN Web._JobOrderLines L WITH (nolock)  ON H.JobOrderHeaderId=L.JobOrderHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web.People POB WITH (nolock) ON POB.PersonID = H.OrderById
LEFT JOIN Web.People P WITH (nolock) ON P.PersonID = H.JobWorkerId
LEFT JOIN Web.PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN Web.Cities C WITH (nolock) ON C.CityId = PA.CityId
LEFT JOIN Web.Products PD WITH (nolock) ON PD.ProductId = L.ProductId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = PD.ProductGroupId
LEFT JOIN Web.Units U WITH (nolock) ON U.UnitId = PD.UnitId 
LEFT JOIN Web.ProdOrderLines PO WITH (nolock) ON PO.ProdOrderLineId=L.ProdOrderLineId
LEFT JOIN Web.ProdOrderHeaders  POH WITH (nolock) ON  POH.ProdOrderHeaderId=PO.ProdOrderHeaderId
LEFT JOIN Web.DocumentTypes PODT WITH (nolock) ON PODT.DocumentTypeId = POH.DocTypeId 
LEFT JOIN web.Godowns G ON G.GodownId = H.GodownId
WHERE H.JobOrderHeaderId	= @Id
End
GO
