IF object_id ('[Web].[ProcPurchaseGoodsReturnPrint]') IS NOT NULL 
 DROP Procedure  [Web].[ProcPurchaseGoodsReturnPrint]
GO 

CREATE Procedure  [Web].[ProcPurchaseGoodsReturnPrint](@Id INT)
As
BEGIN

SELECT H.PurchaseGoodsReturnHeaderId, DT.DocumentTypeName, G.GodownName, Convert(NVARCHAR,isnull(H.GatePassHeaderId,'')) AS  GatePassHeaderId,
 DT.DocumentTypeShortName +'-'+ H.DocNo AS DocNo, H.DocDate,  H.Remark,
P.Name AS SupplierName, PA.Address, City.CityName AS SupplierCityName, P.Mobile AS SupplierMobileNo,
(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.SupplierId   AND RegistrationType ='TIN No' ) AS TinNo, 
NULL  AS ContactPerson, PT.ProductTypeName,  R.ReasonName,L.PurchaseGoodsReturnLineId,
L.PurchaseGoodsReceiptLineId, PD.ProductName, L.Qty AS Qty, L.DealQty AS DealQty,L.Remark AS LineRemark, 
IDT.DocumentTypeShortName +'-'+ PIH.DocNo AS ChallanNo, PIL.Specification, PG.ProductGroupName,H.SiteId, H.DivisionId,
U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces,
H.CreatedBy, H.CreatedDate, AL.ApproveDate, 
H.ModifiedBy +' ' + Replace(replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/'),'/20','/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate, 
AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy, 
'PurchaseGoodsReturn_Print.rdl' AS ReportName,
ProductTypeName + ' Purchase Goods Return' AS ReportTitle, 
'ProcGatePassPrint ' + convert(NVARCHAR,H.GatePassHeaderId) +', ' + '''' +  DT.DocumentTypeShortName +'-'+ H.DocNo +''''   AS SubReportProcList      
FROM ( SELECT * FROM [Web]._PurchaseGoodsReturnHeaders WITH (nolock) WHERE PurchaseGoodsReturnHeaderId	= @Id ) H 
LEFT JOIN [Web]._PurchaseGoodsReturnLines L WITH (nolock) ON L.PurchaseGoodsReturnHeaderId = H.PurchaseGoodsReturnHeaderId 
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web].People P WITH (nolock) ON P.PersonID = H.SupplierId  
LEFT JOIN [Web].PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN [Web].Cities City WITH (nolock) ON City.CityId = PA.CityId 
LEFT JOIN web.PurchaseGoodsReceiptLines PIL WITH (nolock) ON PIL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId
LEFT JOIN Web.PurchaseGoodsReceiptHeaders PIH WITH (nolock) ON PIH.PurchaseGoodsReceiptHeaderId = PIL.PurchaseGoodsReceiptHeaderId
LEFT JOIN [Web].Products PD WITH (nolock) ON PD.ProductId = PIL.ProductId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = PD.ProductGroupId
LEFT JOIN web.ProductTypes PT WITH (nolock) ON PT.ProductTypeId = PG.ProductTypeId
LEFT JOIN [Web].DocumentTypes IDT WITH (nolock) ON IDT.DocumentTypeId = PIH.DocTypeId 
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = PD.UnitId 
LEFT JOIN web.Reasons R WITH (Nolock) ON R.ReasonId = H.ReasonId
LEFT JOIN web.Godowns G WITH (Nolock) ON G.GodownId = H.GodownId 
LEFT JOIN
(
	SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate  
	FROM [Web].ActivityLogs AL WITH (nolock)
	WHERE AL.ActivityType =2
	GROUP BY AL.DocTypeId, AL.DocId
) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.PurchaseGoodsReturnHeaderId
WHERE H.PurchaseGoodsReturnHeaderId	= @Id

End
GO
