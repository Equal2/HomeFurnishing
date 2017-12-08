IF object_id ('[Web].[ProcSaleOrderPrint]') IS NOT NULL 
 DROP Procedure  [Web].[ProcSaleOrderPrint]
GO 

CREATE Procedure  [Web].[ProcSaleOrderPrint](@Id INT)  As  BEGIN  
 SELECT H.SiteId, H.DivisionId, H.SaleOrderHeaderId, DT.DocumentTypeName, H.DocDate, H.DocNo, H.DueDate, H.BuyerOrderNo, H.Remark,  P.Name AS SaleToBuyer, BP.Name AS BillToBuyer,  H.ShipAddress, 
 C.Name AS Currency, SM.ShipMethodName, DET.DeliveryTermsName,   CASE WHEN H.Priority = 10 THEN 'High' WHEN H.Priority = -10 THEN 'Low' ELSE 'Normal' END AS Priority, H.CreditDays, H.TermsAndConditions,     
 L.SaleOrderLineId, PD.ProductName, L.OrderQty AS Qty, L.DeliveryQty, L.DueDate AS LineDueDate, L.Rate, L.OrderAmount AS Amount, L.Remark AS LineRemark,  
  U.UnitName, isnull(U.DecimalPlaces,0) AS DecimalPlaces, 
  VRA.SqYardPerPcs * L.OrderQty AS AreaInSqYard, L.OrderAmount/L.OrderQty AS RatePerPcs,
  UD.UnitName AS DeliveryUnit, isnull(UD.DecimalPlaces,0) AS DeliveryUnitDecimalPlace,   H.CreatedBy, H.CreatedDate,   
  CASE WHEN L.AmendmentCount > 0 THEN 'A ' ELSE '' END + CASE WHEN L.CancelCount >0  THEN 'C ' ELSE '' END + CASE WHEN L.RateAmdCount > 0 THEN 'R ' ELSE '' END AS CurrentStatus,  H.ModifiedBy +' ' + replace(convert(NVARCHAR, H.ModifiedDate, 106), ' ', '/') + substring (convert(NVARCHAR,H.ModifiedDate),13,7) AS ModifiedBy, 
  H.ModifiedDate,   AL.ApproveBy +' ' + replace(convert(NVARCHAR, AL.ApproveDate, 106), ' ', '/') + substring (convert(NVARCHAR,AL.ApproveDate),13,7) AS ApproveBy,   AL.ApproveDate,
  NULL AS SubReportProcList, 'SaleOrder_Print' AS ReportName ,  'Sale Order' AS ReportTitle,
  S.SiteName, VDC.DivisionName, VDC.CompanyName, VDC.CompanyAddress, VDC.CompanyCity       
  FROM  Web.SaleOrderHeaders H  
  LEFT JOIN Web.DocumentTypes DT ON DT.DocumentTypeId = H.DocTypeId   
  LEFT JOIN Web.ViewSaleOrderLineTheir L ON L.SaleOrderHeaderId = H.SaleOrderHeaderId   
  LEFT JOIN Web.People P ON P.PersonID = H.SaleToBuyerId   LEFT JOIN Web.People BP ON BP.PersonID = H.BillToBuyerId   
  LEFT JOIN Web.Products PD ON PD.ProductId = L.ProductId   
  LEFT JOIN Web.Units U ON U.UnitId = PD.UnitId   
  LEFT JOIN Web.Units UD ON UD.UnitId = L.DeliveryUnitId    
  LEFT JOIN Web.Currencies C ON C.ID = H.CurrencyId   
  LEFT JOIN Web.ViewRugarea VRA ON VRA.ProductId = L.ProductId 
  LEFT JOIN Web.ShipMethods SM ON SM.ShipMethodId  = H.ShipMethodId  
  LEFT JOIN Web.DeliveryTerms DET ON DET.DeliveryTermsId = H.DeliveryTermsId  
  LEFT JOIN web.Sites S ON S.SiteId = H.SiteId
  LEFT JOIN web.ViewDivisionCompany  VDC ON VDC.DivisionId = H.DivisionId 
  LEFT JOIN  (      SELECT AL.DocTypeId, AL.DocId, Max(AL.CreatedBy) AS ApproveBy , max(AL.CreatedDate) AS ApproveDate        FROM  Web.ActivityLogs AL      WHERE AL.ActivityType =2      GROUP BY AL.DocTypeId, AL.DocId  ) AL ON AL.DocTypeId  = H.DocTypeId AND AL.DocId = H.SaleOrderHeaderId  
 WHERE H.SaleOrderHeaderId	=  @Id   
 ORDER BY L.SaleOrderLineId
 End
GO


