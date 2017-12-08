IF object_id ('[Web].[ProcGatePassPrint1]') IS NOT NULL 
DROP Procedure  [Web].[ProcGatePassPrint1]
GO 

CREATE Procedure  [Web].[ProcGatePassPrint1](@Id INT, @RefDocNo NVARCHAR(20) )
As
BEGIN
SELECT H.GatePassHeaderId,  DT.DocumentTypeShortName +'-'+ H.DocNo AS DocNo, H.DocDate,  H.Remark, P.Name AS PersonName, G.GodownName,  
L.GatePassLineId, L.Product, L.Specification, L.Qty, U.UnitName, U.DecimalPlaces,
@RefDocNo AS ReferenceDocNo,  'GatePassPrint.rdl'  AS ReportName, 'Gate Pass' AS ReportTitle
--NULL  AS SubReportProcList  
FROM Web.GatePassHeaders H
LEFT JOIN web.Godowns G ON G.GodownId = H.GodownId 
LEFT JOIN web.People P ON P.PersonID  = H.PersonID 
LEFT JOIN [Web].DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN web.GatePassLines L ON L.GatePassHeaderId = H.GatePassHeaderId 
LEFT JOIN web.Units U ON U.UnitId = L.UnitId 
WHERE H.GatePassHeaderId = @Id
End
GO

