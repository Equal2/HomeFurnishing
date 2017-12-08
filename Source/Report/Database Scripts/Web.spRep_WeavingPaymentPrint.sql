USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingPaymentPrint]    Script Date: 07/Sep/2015 5:28:21 PM ******/


IF object_id ('[Web].[spRep_WeavingPaymentPrint]') IS NOT NULL 
DROP PROCEDURE [Web].[spRep_WeavingPaymentPrint]
/****** Object:  StoredProcedure [Web].[spRep_WeavingPaymentPrint]    Script Date: 07/Sep/2015 5:28:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE  [Web].[spRep_WeavingPaymentPrint] (@Id INT)
AS
BEGIN
SELECT H.LedgerHeaderId,H.SiteId,H.DivisionId,HLA.LedgerAccountName,
H.DocNo,H.DocDate AS PaymentDate,H.Narration,
CC.CostCenterName,LAS.LedgerAccountName AS jobworkername,
L.ChqNo,L.Chqdate,L.Amount,L.Remark,DT.Nature,
NULL  AS ReportName,DT.DocumentTypeName + '   Entry' AS ReportTitle, NULL AS SubReportProcList
 FROM
(SELECT * FROM Web.LedgerHeaders WHERE LedgerHeaderId=@Id) H
LEFT JOIN Web.LedgerLines L WITH (nolock) ON L.LedgerHeaderId=H.LedgerHeaderId
LEFT JOIN Web.LedgerAccounts HLA WITH (nolock) ON HLA.LedgerAccountId=H.LedgerAccountId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId=H.DocTypeId
LEFT JOIN Web.CostCenters CC WITH (nolock) ON CC.CostCenterId = L.CostCenterId
LEFT JOIN Web.LedgerAccounts LAS WITH (nolock) ON LAS.LedgerAccountId=L.LedgerAccountId
END
GO


