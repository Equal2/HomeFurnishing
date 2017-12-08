IF object_id ('[Web].[spSubTrialBalanceSummary]') IS NOT NULL 
 DROP Procedure  [Web].[spSubTrialBalanceSummary]
GO 

CREATE  PROCEDURE [Web].[spSubTrialBalanceSummary]     
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@LedgerAccountGroupId VARCHAR(20) = NULL
AS
BEGIN

SELECT VMain.LedgerAccountId,  max(LA.LedgerAccountName) AS LedgerAccountName, max(LAG.LedgerAccountGroupName) AS LedgerAccountGroupName, 
CASE WHEN abs(Sum(Isnull(VMain.Opening,0))) = 0 THEN NULL ELSE abs(Sum(Isnull(VMain.Opening,0))) END AS Opening, CASE WHEN Sum(Isnull(VMain.Opening,0)) >= 0 THEN 'Dr' ELSE 'Cr' END AS OpeningType, 
CASE WHEN Sum(isnull(Vmain.AmtDr,0)) = 0 THEN NULL ELSE Sum(isnull(Vmain.AmtDr,0)) END AS AmtDr, CASE WHEN sum(isnull(VMain.AmtCr,0)) = 0 THEN NULL ELSE sum(isnull(VMain.AmtCr,0)) END AS AmtCr,
abs(Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0))) AS Balance,
CASE WHEN ( Sum(Isnull(VMain.Opening,0)) + Sum(isnull(Vmain.AmtDr,0)) - sum(isnull(VMain.AmtCr,0))) >= 0 THEN 'Dr' ELSE 'Cr' END AS BalanceType 
FROM
(
SELECT H.LedgerAccountId ,  sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) AS Opening,
0 AS AmtDr,0 AS  AmtCr    
FROM web.Ledgers H WITH (Nolock)
LEFT JOIN web.LedgerHeaders LH WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
WHERE H.LedgerAccountId  IS NOT NULL 
AND ( @Site is null or LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or LH.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate > LH.DocDate ) 
GROUP BY H.LedgerAccountId 
UNION ALL 

SELECT H.LedgerAccountId, 0 AS Opening,
CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) > 0 THEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) ELSE 0 END AS AmtDr,
CASE WHEN sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0)) < 0 THEN abs(sum(isnull(H.AmtDr,0)) - sum(isnull(H.AmtCr,0))) ELSE 0 END AS AmtCr    
FROM web.Ledgers H WITH (Nolock)
LEFT JOIN web.LedgerHeaders LH WITH (Nolock) ON LH.LedgerHeaderId = H.LedgerHeaderId 
WHERE H.LedgerAccountId IS NOT NULL 
AND ( @Site is null or LH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or LH.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= LH.DocDate ) 
AND ( @ToDate is null or @ToDate >= LH.DocDate ) 
GROUP BY H.LedgerAccountId 
) AS VMain
LEFT JOIN web.LedgerAccounts LA  WITH (Nolock) ON LA.LedgerAccountId = VMain.LedgerAccountId 
LEFT JOIN web.LedgerAccountGroups LAG  WITH (Nolock) ON LAG.LedgerAccountGroupId = LA.LedgerAccountGroupId 
GROUP BY VMain.LedgerAccountId
HAVING Sum(Isnull(VMain.Opening,0)) <> 0 OR Sum(isnull(Vmain.AmtDr,0)) <>  OR Sum(isnull(Vmain.AmtCr,0)) <> 0
End
GO

