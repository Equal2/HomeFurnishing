CREATE PROCEDURE GETPURCHASEORDER_ForNotification
AS
BEGIN
SELECT
POH.[DocNo] as OrderNumber,
POH.[DocDate] as OrderDate,
POH.[ShipDate],
POH.STATUS,
POHD.[ProgressPer],
people.Name,
u.Email   
FROM [dbo].[PurchaseOrderHeaders] POH INNER JOIN [dbo].[PurchaseOrderHeaderDetail] POHD
ON POH.PurchaseOrderHeaderId=POHD.PurchaseOrderHeaderId
LEFT OUTER JOIN [dbo].[People] people on POH.SupplierID=people.PersonID
left outer join [dbo].[Users] u on u.Id=people.ApplicationUser_Id

END