USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingPaymentReport]    Script Date: 07/Sep/2015 5:29:31 PM ******/
IF object_id ('[Web].[spRep_WeavingPaymentReport]') IS NOT NULL 
DROP PROCEDURE [Web].[spRep_WeavingPaymentReport]
/****** Object:  StoredProcedure [Web].[spRep_WeavingPaymentReport]    Script Date: 07/Sep/2015 5:29:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [Web].[spRep_WeavingPaymentReport]
@Site VARCHAR(20) = NULL,
@Division VARCHAR(20) = NULL,
@FromDate VARCHAR(20) = NULL,
@ToDate VARCHAR(20) = NULL,
@JobWorkerId VARCHAR(Max) = NULL,
@CostCenterId VARCHAR(Max)=NULL
AS
BEGIN
SELECT H.DocDate,H.DocNo,CC.CostCenterName,LA.LedgerAccountName,
L.Amount,L.ChqNo,L.ChqDate,LH.DocNo AS RefNo,L.Remark,
NULL AS  SubReportProcList ,
	'WeavingPaymentReport.rdl' AS ReportName,
   'Weaving Advance Payment Report' AS  ReportTitle
 FROM
(
SELECT  * FROM Web.LedgerHeaders H WHERE 1=1
				AND (@Site IS NULL OR H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ',')))
				AND (@Division IS NULL OR H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ',')))
				AND (@FromDate is null or @FromDate <= H.DocDate ) 
				AND (@ToDate is null or @ToDate >= H.DocDate )

) H
LEFT JOIN Web.LedgerLines L WITH (nolock) ON L.LedgerHeaderId=H.LedgerHeaderId
LEFT JOIN Web.LedgerAccounts LA WITH (nolock) ON LA.LedgerAccountId=L.LedgerAccountId
LEFT JOIN Web.CostCenters CC WITH (nolock) ON CC.CostCenterId = L.CostCenterId
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId=H.DocTypeId
LEFT JOIN Web.ledgers Led WITH (nolock) ON Led.LedgerId=L.ReferenceId
LEFT JOIN Web.LedgerHeaders LH WITH (nolock) ON LH.LedgerHeaderId=Led.LedgerHeaderId
WHERE L.LedgerLineId IS NOT NULL
AND (@JobWorkerId IS NULL OR L.LedgerAccountId IN (SELECT Items FROM [dbo].[Split] (@JobWorkerId, ',')))
AND (@CostCenterId IS NULL OR L.CostCenterId IN (SELECT Items FROM [dbo].[Split] (@CostCenterId, ',')))
ORDER BY H.DocDate
end
GO


