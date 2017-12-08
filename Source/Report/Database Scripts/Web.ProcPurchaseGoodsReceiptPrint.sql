IF object_id ('[Web].[ProcPurchaseGoodsReceiptPrint]') IS NOT NULL 
 DROP Procedure  [Web].[ProcPurchaseGoodsReceiptPrint]
GO 

CREATE Procedure  [web].[ProcPurchaseGoodsReceiptPrint](@Id INT)
As
BEGIN

DECLARE @ReportName AS NVARCHAR(Max)
DECLARE @CountDealUnit AS INT  
DECLARE @CountLotNo AS INT 
DECLARE @CountSortQty AS INT 

SET @CountDealUnit = ( SELECT ISNULL(count(*),0) FROM Web._PurchaseGoodsReceiptLines L WITH (Nolock)
								LEFT JOIN web.Products P WITH (Nolock) ON P.ProductId = L.ProductId 
								WHERE L.DealUnitId <> P.UnitId 
								AND L.PurchaseGoodsReceiptHeaderId = @Id
								)

SET @CountLotNo = ( SELECT ISNULL(count(*),0) FROM Web._PurchaseGoodsReceiptLines L WITH (Nolock) 								
								WHERE ISNULL(L.LotNo,'') <> ''
								AND L.PurchaseGoodsReceiptHeaderId = @Id
								)
								
SET @CountSortQty = ( SELECT ISNULL(count(*),0) FROM Web._PurchaseGoodsReceiptLines L WITH (Nolock) 								
							WHERE ISNULL(L.DocQty,0) <> ISNULL(L.Qty,0)
							AND L.PurchaseGoodsReceiptHeaderId = @Id
					)
								
IF  @CountDealUnit > 0 
	SET @ReportName = 'PurchaseGoodsReceipt_Print_WithDealUnit'
ELSE
	SET @ReportName = 'PurchaseGoodsReceipt_Print'
		  
IF  @CountLotNo > 0 
	SET @ReportName =  @ReportName + '_WithLotNo'


IF  @CountSortQty > 0 
	SET @ReportName =  @ReportName + '_WithSortQty'
								
SELECT H.SiteId, H.DivisionId, H.PurchaseGoodsReceiptHeaderId, DT.DocumentTypeShortName, H.DocDate, H.DocNo, 
DT.DocumentTypeShortName +'-'+ H.DocNo AS PurchaseChallanNo,PG.ProductGroupName,DU.UnitName AS DealUnit,
SP.Name AS SupplierName, PA.Address, SC.CityName AS SupplierCityName,SP.Mobile AS SupplierMobileNo,
POH.DocNo AS PurchaseOrderNo, L.PurchaseGoodsReceiptLineId, L.Specification,
(SELECT TOP 1  PR.RegistrationNo  FROM web.PersonRegistrations PR WITH (Nolock) WHERE PersonId =H.SupplierId   AND RegistrationType ='TIN No' ) AS TinNo,
H.SupplierDocNo, H.SupplierDocDate, H.Remark, G.GodownName, P.ProductName, U.UnitName AS Unit, U.DecimalPlaces , DU.UnitName AS DeliveryUnit, DU.DecimalPlaces AS DeliveryUnitDecimalPlace,
L.Qty, L.DealQty, L.DocQty,  Isnull(L.DocQty,0) - Isnull(L.Qty,0) AS ShortQty, L.LotNo, L.BaleNo, L.Remark AS LineRemark,
NULL  AS SalesTaxGroupName,  
H.ModifiedBy +' ' + replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, H.ModifiedDate, 
AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy, AL.ApproveDate,
PT.ProductTypeName + ' Purchase Challan' AS ReportTitle, @ReportName +'.rdl' AS ReportName,
--CASE WHEN @CountDealUnit > 0 THEN 'PurchaseGoodsReceipt_Print_WithDealUnit.rdl'  ELSE  
  --	CASE WHEN  @CountLotNo > 0 THEN  'PurchaseGoodsReceipt_Print_WithLotNo.rdl' ELSE 'PurchaseGoodsReceipt_Print.rdl' END  END AS ReportName,
NULL AS SubReportProcList   
FROM Web._PurchaseGoodsReceiptHeaders H WITH (Nolock)
LEFT JOIN Web._PurchaseGoodsReceiptLines L WITH (Nolock) ON L.PurchaseGoodsReceiptHeaderId = H.PurchaseGoodsReceiptHeaderId 
LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.People SP WITH (Nolock) ON SP.PersonID = H.SupplierId 
LEFT JOIN web.PersonAddresses PA WITH (Nolock) ON PA.PersonId  = H.SupplierId 
LEFT JOIN web.Cities SC WITH (Nolock) ON SC.CityId = PA.CityId 
LEFT JOIN web.Godowns G WITH (Nolock) ON G.GodownId = H.GodownId 
LEFT JOIN web.Products P WITH (Nolock) ON P.ProductId = L.ProductId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = P.ProductGroupId
LEFT JOIN web.ProductTypes PT WITH (nolock) ON PT.ProductTypeId = PG.ProductTypeId
LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId = P.UnitId 
LEFT JOIN web.Units DU WITH (Nolock) ON DU.UnitId = L.DealUnitId 
LEFT JOIN web.PurchaseOrderLines POL WITH (Nolock) ON POL.PurchaseOrderLineId = L.PurchaseOrderLineId 
LEFT JOIN web.PurchaseOrderHeaders POH WITH (Nolock) ON POH.PurchaseOrderHeaderId = POL.PurchaseOrderHeaderId 
LEFT JOIN
(
	SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate  
	FROM ActivityLogs AL
	WHERE AL.ActivityType =2
	GROUP BY AL.DocTypeId, AL.DocId
) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.PurchaseGoodsReceiptHeaderId
WHERE H.PurchaseGoodsReceiptHeaderId = @Id
End
GO
