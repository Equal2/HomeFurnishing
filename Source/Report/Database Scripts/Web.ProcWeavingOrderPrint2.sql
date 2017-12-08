IF object_id ('[Web].[ProcWeavingOrderPrint2]') IS NOT NULL 
 DROP Procedure  [Web].[ProcWeavingOrderPrint2]
GO 

CREATE Procedure  [Web].[ProcWeavingOrderPrint2]
(@Id INT)  As  
BEGIN  


SELECT JOP.JobOrderHeaderId, P.PerkName, P.BaseDescription, JOP.Base, P.WorthDescription, JOP.Worth,
/*CASE WHEN  P.PerkName ='Time Incentive'   		 THEN '<b>Time Penalty</b> of Rs. ' + convert(VARCHAR,round(JOP.Worth,2)) + ' per Sq. Yard will be given if all pcs of Weaving Order will be delivered before delivery date.' 
     WHEN  P.PerkName ='Time Penality'  		 THEN '<b>Time Penalty</b> of Rs. ' + convert(VARCHAR,round(JOP.Worth,2)) + ' per Sq. Yard will be deducted if all pcs of Weaving Order will not be delivered after TimePenaltyDays days of delivery date.' 
     WHEN  P.PerkName ='Fragmentation Penality'  THEN '<b>Small Chunks Bazar Penalty </b> of Rs. ' + convert(VARCHAR,round(JOP.Worth,2)) + ' per Sq. Yard will be deducted if all pcs of Weaving Order will not be delivered in ' + convert(VARCHAR,round(JOP.Base,0)) + + ' Times.'
ELSE '' END AS Remark,*/
CASE WHEN  P.PerkName ='Time Incentive'   		 THEN 'Time Incentive of Rs. ' + convert(VARCHAR,CAST(JOP.Worth AS DECIMAL(19,2))) + ' per Sq. Yard will be given if all pcs of Weaving Order will be delivered before delivery date.' 
     WHEN  P.PerkName ='Time Penality'  		 THEN 'Time Penalty of Rs. ' + convert(VARCHAR,CAST(JOP.Worth AS DECIMAL(19,2))) + ' per Sq. Yard will be deducted if all pcs of Weaving Order will not be delivered after ' + convert(VARCHAR,CAST(JOP.Base AS DECIMAL(19,0))) + + ' days of delivery date.' 
     WHEN  P.PerkName ='Fragmentation Penality'  THEN 'Small Chunks Bazar Penalty of Rs. ' + convert(VARCHAR,CAST(JOP.Worth AS DECIMAL(19,2))) + ' per Sq. Yard will be deducted if all pcs of Weaving Order will not be delivered in ' + convert(VARCHAR,CAST(JOP.Base AS DECIMAL(19,0))) + ' Times.'
ELSE '' END AS Remark,
JOH.SiteId AS SiteId, JOH.DivisionId AS DivisionId,  NULL AS SubReportProcList, 'WeavingOrder_Print2.rdl' AS ReportName ,  'Weaving Order' AS ReportTitle   
FROM Web.JobOrderPerks JOP WITH (Nolock)
LEFT JOIN web.JobOrderHeaders JOH ON JOH.JobOrderHeaderId = JOP.JobOrderHeaderId 
LEFT JOIN web.Perks P ON P.PerkId = JOP.PerkId 
WHERE JOP.JobOrderHeaderId = 193814
End
GO
