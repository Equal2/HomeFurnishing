

ALTER Procedure  [Web].[ProcPurchaseInvoicePrint](@Id INT)
As
BEGIN

DECLARE @ReportName AS NVARCHAR(Max)  
DECLARE @DealUnitCount AS INT 
<<<<<<< .mine
DECLARE @DiscountPerCount AS INT 

=======
DECLARE @DiscountPerCount AS INT 

DECLARE @TotalAmount DECIMAL 
    SET @TotalAmount = (SELECT Max(Amount) FROM Web.PurchaseInvoiceHeaderCharges WHERE HeaderTableId = @Id AND ChargeId = 34 ) 
		
		
DECLARE @DebitNoteAmount DECIMAL 
  SET @DebitNoteAmount = 
    (
				
		SELECT sum(DL.Amount) AS DebitAmount 
		FROM web.PurchaseInvoiceHeaders H WITH (Nolock) 
		LEFT JOIN web.Ledgers L WITH (Nolock)  ON L.LedgerHeaderId = H.LedgerHeaderId
		LEFT JOIN web.LedgerLines DL WITH (Nolock) ON DL.ReferenceId = L.LedgerId 
		LEFT JOIN web.LedgerHeaders LH ON LH.LedgerHeaderId = DL.LedgerHeaderId 
		WHERE H.PurchaseInvoiceHeaderId = @Id
		AND LH.DocTypeId = 91

	) 
	
>>>>>>> .r806
SET @DealUnitCount = ( SELECT ISNULL(count(*),0) 
FROM Web.PurchaseInvoiceLines L 
LEFT JOIN web.PurchaseGoodsReceiptLines PGRL ON PGRL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId
LEFT JOIN web.Products P ON P.ProductId = PGRL.ProductId 
WHERE  L.DealUnitId <> P.UnitId --OR L.dealQty <> PGRL.Qty) 
AND L.PurchaseInvoiceHeaderId = @Id )


SET @DiscountPerCount = ( SELECT ISNULL(count(*),0) 
FROM Web.PurchaseInvoiceLines L 
WHERE L.PurchaseInvoiceHeaderId = @Id AND isnull(L.DiscountPer,0) > 0)


IF  ( @DealUnitCount  > 0 )
 SET @ReportName = 'PurchaseInvoice_Print_WithDeliveryUnit' 
ELSE 
 SET @ReportName = 'PurchaseInvoice_Print' 
 
IF  ( @DiscountPerCount  > 0 )
 SET @ReportName =  @ReportName + '_WithDiscPer' 

 declare @count bigint
 
 select @count=count(PIH.DocNo) from 
( SELECT * FROM [Web]._PurchaseInvoiceHeaders WITH (nolock) WHERE PurchaseInvoiceHeaderId	= @Id ) H 
LEFT JOIN [Web]._PurchaseInvoiceLines L WITH (nolock) ON L.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId 
LEFT JOIN web.PurchaseGoodsReceiptLines PIL WITH (nolock) ON PIL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId
LEFT JOIN Web.PurchaseGoodsReceiptHeaders PIH WITH (nolock) ON PIH.PurchaseGoodsReceiptHeaderId = PIL.PurchaseGoodsReceiptHeaderId
where PIH.DocNo IS NOT NULL


SELECT H.PurchaseInvoiceHeaderId, DT.DocumentTypeName, DT.DocumentTypeShortName, DT.DocumentTypeShortName +'-'+ H.DocNo AS PurchaseInvoiceNo, H.DocDate, H.DocNo, H.Remark,
P.Name AS SupplierName, PA.Address, City.CityName AS SupplierCityName, P.Mobile AS SupplierMobileNo,
C.Name AS Currency, C.Symbol AS CurrencySymbol, (SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WHERE PersonId =H.SupplierId   AND RegistrationType ='TIN No' ) AS TinNo, 
NULL  AS ContactPerson, PT.ProductTypeName, 
H.CreditDays, H.TermsAndConditions, STGP.ChargeGroupPersonName AS SalesTaxGroupName,  
H.SupplierDocNo, H.SupplierDocDate, L.PurchaseInvoiceLineId,
<<<<<<< .mine
L.PurchaseGoodsReceiptLineId, PD.ProductName, PIL.Qty AS Qty, L.DealQty AS DealQty, L.Rate, L.Amount AS Amount, L.Remark AS LineRemark, isnull(L.DiscountPer,0) AS DiscountPer,
IDT.DocumentTypeShortName +'-'+ PIH.DocNo AS ChallanNo,
PIHSDT.DocumentTypeShortName +'-'+ PIHS.DocNo as IndentNO,
@count as cntchallanNo,
 PIL.Specification, PG.ProductGroupName,H.SiteId, H.DivisionId,
=======
L.PurchaseGoodsReceiptLineId, PD.ProductName, --PIL.Qty AS Qty, 
( SELECT Qty FROM Web.FPurchaseGoodsReceipt( NULL,NULL ,NULL ,NULL,NULL,NULL,NULL,NULL,NULL, L.PurchaseGoodsReceiptLineId ) ) AS Qty,
L.DealQty AS DealQty, L.Rate, L.Amount AS Amount, L.Remark AS LineRemark, isnull(L.DiscountPer,0) AS DiscountPer,
IDT.DocumentTypeShortName +'-'+ PIH.DocNo AS ChallanNo,
PIHSDT.DocumentTypeShortName +'-'+ PIHS.DocNo as IndentNO,
@count as cntchallanNo,
 PIL.Specification, PG.ProductGroupName,H.SiteId, H.DivisionId,
>>>>>>> .r806
U.UnitName AS Unit, isnull(U.DecimalPlaces,0) AS DecimalPlaces, UD.UnitName AS DealUnit, isnull(UD.DecimalPlaces,0) AS DeliveryUnitDecimalPlace, 
H.CreatedBy, H.CreatedDate, AL.ApproveDate, 
H.ModifiedBy +' ' + Replace(replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/'),'/20','/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate, 
AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy, 
<<<<<<< .mine
@ReportName +'.rdl' AS ReportName, ProductTypeName + ' Purchase Invoice' AS ReportTitle,
=======
@ReportName +'.rdl' AS ReportName, ProductTypeName + ' Purchase Invoice' AS ReportTitle,
@TotalAmount as NetAmount, @DebitNoteAmount AS DebitNoteAmount, isnull(@TotalAmount,0) -  isnull(@DebitNoteAmount,0) NetPayableAmount, 
>>>>>>> .r806
'ProcPurchaseInvoiceHeaderCharges ' + convert(NVARCHAR,H.PurchaseInvoiceHeaderId)  AS SubReportProcList       
FROM ( SELECT * FROM [Web]._PurchaseInvoiceHeaders WITH (nolock) WHERE PurchaseInvoiceHeaderId	= @Id ) H 
LEFT JOIN [Web]._PurchaseInvoiceLines L WITH (nolock) ON L.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId 
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN [Web].People P WITH (nolock) ON P.PersonID = H.SupplierId  
LEFT JOIN [Web].PersonAddresses PA WITH (nolock) ON PA.PersonId = P.PersonID 
LEFT JOIN [Web].Cities City WITH (nolock) ON City.CityId = PA.CityId 
LEFT JOIN web.PurchaseGoodsReceiptLines PIL WITH (nolock) ON PIL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId
LEFT JOIN Web.PurchaseGoodsReceiptHeaders PIH WITH (nolock) ON PIH.PurchaseGoodsReceiptHeaderId = PIL.PurchaseGoodsReceiptHeaderId
<<<<<<< .mine
/* aad by gopal*/
left join web.PurchaseIndentLines PILS with (nolock) On PILS.PurchaseIndentLineId=PIL.PurchaseIndentLineId
left join web.PurchaseIndentHeaders PIHS with (nolock) On PIHS.PurchaseIndentHeaderId=PILS.PurchaseIndentHeaderId
LEFT JOIN [Web].DocumentTypes PIHSDT WITH (nolock) ON PIHSDT.DocumentTypeId = PIHS.DocTypeId 
/*add end gopal*/
=======
--LEFT JOIN ( SELECT Qty FROM Web.FPurchaseGoodsReceipt( NULL,NULL ,NULL ,NULL,NULL,NULL,NULL,NULL,NULL )) AS GPIL  ON GPIL.PurchaseGoodsReceiptLineId = L.PurchaseGoodsReceiptLineId
/* aad by gopal*/
left join web.PurchaseIndentLines PILS with (nolock) On PILS.PurchaseIndentLineId=PIL.PurchaseIndentLineId
left join web.PurchaseIndentHeaders PIHS with (nolock) On PIHS.PurchaseIndentHeaderId=PILS.PurchaseIndentHeaderId
LEFT JOIN [Web].DocumentTypes PIHSDT WITH (nolock) ON PIHSDT.DocumentTypeId = PIHS.DocTypeId 
/*add end gopal*/
>>>>>>> .r806
LEFT JOIN [Web].Products PD WITH (nolock) ON PD.ProductId = PIL.ProductId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = PD.ProductGroupId
LEFT JOIN web.ProductTypes PT WITH (nolock) ON PT.ProductTypeId = PG.ProductTypeId
LEFT JOIN [Web].DocumentTypes IDT WITH (nolock) ON IDT.DocumentTypeId = PIH.DocTypeId 
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = PD.UnitId 
LEFT JOIN [Web].Units UD WITH (nolock) ON UD.UnitId = L.DealUnitId  
LEFT JOIN [Web].Currencies C WITH (nolock) ON C.ID = H.CurrencyId 
LEFT JOIN web.ChargeGroupPersons  STGP WITH (nolock) ON STGP.ChargeGroupPersonId = H.SalesTaxGroupId
LEFT JOIN
(
	SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate  
	FROM [Web].ActivityLogs AL WITH (nolock)
	WHERE AL.ActivityType =2
	GROUP BY AL.DocTypeId, AL.DocId
) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.PurchaseInvoiceHeaderId
WHERE H.PurchaseInvoiceHeaderId	= @Id
End
GO
