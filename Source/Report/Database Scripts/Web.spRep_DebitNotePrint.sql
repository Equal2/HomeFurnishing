USE [RUG]
GO
IF OBJECT_ID ('[Web].[spRep_DebitNotePrint]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_DebitNotePrint]
GO

Create Procedure  [Web].[spRep_DebitNotePrint](@Id INT)
As
BEGIN
SELECT H.LedgerHeaderId,L.LedgerLineId,DT.DocumentTypeShortName +'-'+ H.DocNo AS DebitNoteNo, H.DocDate AS Date,H.Remark as HeaderRemark,H.Narration,
 LA.LedgerAccountName as partyName,L.Amount,H.Siteid,H.DivisionId,DTS.DocumentTypeShortName +'-'+ LH.DocNo AS BillNo,
  'Debit Note' AS ReportTitle, 'DebitNotePrint.rdl'	 AS ReportName  , NULL AS SubReportProcList,D.DivisionName,LAH.LedgerAccountName as ReasonAC  	
FROM (SELECT * FROM Web.Ledgerheaders WITH (nolock) WHERE LedgerHeaderId= @Id) H
LEFT JOIN  Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
left join   Web.Divisions D with (nolock) on D.DivisionId=H.DivisionId
LEFT JOIN  Web.LedgerLines L WITH (nolock)  ON H.LedgerHeaderId=L.LedgerHeaderId
left join web.Ledgers LD with (nolock) on L.ReferenceId=LD.LedgerId
left join  Web.Ledgerheaders LH with (nolock) on LD.LedgerHeaderId=LH.LedgerHeaderId
left join Web.DocumentTypes DTS with (nolock) on LH.DocTypeId=DTS.DocumentTypeId
left join web.LedgerAccounts LA with (nolock) on LA.LedgerAccountId=L.LedgerAccountId 
left join web.LedgerAccounts LAH with (nolock) on LAH.LedgerAccountId=H.LedgerAccountId
WHERE H.LedgerHeaderId	= @Id
ORDER BY L.LedgerLineId
End
GO