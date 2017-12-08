

USE [RUG]
GO
IF OBJECT_ID ('[Web].[spRep_DebitNoteReport]') IS NOT NULL
	DROP PROCEDURE [Web].[spRep_DebitNoteReport]
GO

create  procedure   [Web].[spRep_DebitNoteReport]
@Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,	
	@DocumentType VARCHAR(20) = NULL,	 
    @LedgerAccount varchar(Max)=null,
	@Reasonaccount varchar(Max)=null,
	@LedgerHeaderId varchar(Max)=null
AS
BEGIN
	SELECT H.LedgerHeaderId,L.LedgerLineId,DT.DocumentTypeShortName +'-'+ H.DocNo AS VoucherNo, H.DocDate,
	LA.LedgerAccountName as PersonName,LAH.LedgerAccountName as ReasonAC,L.Amount,L.Remark as LineRemark,
 H.Siteid,H.DivisionId,DTS.DocumentTypeShortName +'-'+ LH.DocNo AS BillNo,S.siteName,
  'Debit Note Report' AS ReportTitle, 'DebitNoteReport.rdl'	 AS ReportName  , NULL AS SubReportProcList,D.DivisionName  	
FROM (
SELECT * FROM Web.Ledgerheaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
And (@FromDate is null or @FromDate <=H.DocDate ) 
AND ( @ToDate is null or @ToDate>=H.DocDate)
AND ( @LedgerHeaderId is null or H.LedgerHeaderId IN (SELECT Items FROM [dbo].[Split] (@LedgerHeaderId, ','))) 
) H
LEFT JOIN  Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
left join   Web.Divisions D with (nolock) on D.DivisionId=H.DivisionId
left join web.Sites S with (nolock) on S.SiteId=H.SiteId
LEFT JOIN   Web.LedgerLines L WITH (nolock)  ON H.LedgerHeaderId=L.LedgerHeaderId
left join web.Ledgers LD with (nolock) on L.ReferenceId=LD.LedgerId
left join  Web.Ledgerheaders LH with (nolock) on LD.LedgerHeaderId=LH.LedgerHeaderId
left join Web.DocumentTypes DTS with (nolock) on LH.DocTypeId=DTS.DocumentTypeId
left join web.LedgerAccounts LA with (nolock) on LA.LedgerAccountId=L.LedgerAccountId 
left join web.LedgerAccounts LAH with (nolock) on LAH.LedgerAccountId=H.LedgerAccountId
where 1=1
AND (@LedgerAccount is null or LA.LedgerAccountId IN (SELECT Items FROM [dbo].[Split] (@LedgerAccount, ',')))
AND (@Reasonaccount is null or LAH.LedgerAccountId IN (SELECT Items FROM [dbo].[Split] (@Reasonaccount, ',')))  
End

select * from Web.LedgerLines