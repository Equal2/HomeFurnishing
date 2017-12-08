USE [RUG]
GO

/****** Object:  UserDefinedFunction [Web].[FJobReceiveBalance]    Script Date: 04/Aug/2015 12:16:15 PM ******/

IF OBJECT_ID ('[Web].[FJobReceiveBalance]') IS NOT NULL
	DROP FUNCTION [Web].[FJobReceiveBalance]	
GO
/****** Object:  UserDefinedFunction [Web].[FJobReceiveBalance]    Script Date: 04/Aug/2015 12:16:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create FUNCTION [Web].[FJobReceiveBalance]
(
      @StatusOnDate VARCHAR(20) = NULL,
      @Site VARCHAR(20) = NULL,
      @Division VARCHAR(20) = NULL,
	  @FromDate VARCHAR(20) = NULL,
	  @ToDate VARCHAR(20) = NULL,
	  @DocumentType VARCHAR(20) = NULL,
      @JobWorker VARCHAR(Max) = NULL,
      @Process VARCHAR(Max) = NULL,
      @Product VARCHAR(Max) = NULL, 
      @JobreceiveHeaderId VARCHAR(Max) = NULL
    )
    
    RETURNS TABLE
AS 

RETURN
    SELECT 
     VJobReceiveBal.JobReceiveLineId,
     max(VJobReceiveBal.ProcessId) AS ProcessId,
     sum(VJobReceiveBal.Qty)  AS BalQty,
     max(VJobReceiveBal.JobReceiveHeaderId) AS JobReceiveHeaderId,
     max(VJobReceiveBal.DocTypeId) AS DocTypeId,
     max(VJobReceiveBal.DocumentTypeShortName) AS DocumentTypeShortName,	 
     max(VJobReceiveBal.JobReceiveNo) AS JobReceiveNo,
     Max(VJobReceiveBal.ProductId) AS ProductId,
     max(VJobReceiveBal.Dimension1Id) AS Dimension1Id,
     max(VJobReceiveBal.Dimension2Id) AS Dimension2Id,
     max(VJobReceiveBal.Specification) AS Specification,
     max(VJobReceiveBal.JobWorkerId) AS JobWorkerId,
     max(VJobReceiveBal.DocDate) AS ReceiveDate,
     max(VJobReceiveBal.DivisionId) AS DivisionId,
     max(VJobReceiveBal.SiteId) AS SiteId,  
      max(VJobReceiveBal.DocumentTypeName) AS DocumentTypeName	  
     FROM 
    (
    SELECT L.JobReceiveLineId,H.ProcessId,L.Qty,H.JobReceiveHeaderId,H.DocTypeId,Dt.DocumentTypeShortName, Dt.DocumentTypeShortName + '-' +  H.DocNo AS JobReceiveNo,
    JOL.ProductId,JOL.Dimension1Id,JOL.Dimension2Id,JOL.Specification,H.JobWorkerId AS JobWorkerId,H.DocDate,H.DivisionId,H.SiteId,DT.DocumentTypeName  
    FROM (
    SELECT H.* FROM Web._JobReceiveHeaders H WITH (nolock)
    WHERE 1=1
    AND ( @JobreceiveHeaderId IS NULL OR H.JobReceiveHeaderId IN (SELECT Items FROM  Web.[Split] (@JobreceiveHeaderId, ',')))     
	AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
	AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)     
	AND ( @DocumentType IS NULL OR H.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))     
	AND ( @JobWorker IS NULL OR H.JobWorkerId IN (SELECT Items FROM  Web.[Split] (@JobWorker, ',')))     
	AND ( @Process IS NULL OR H.ProcessId IN (SELECT Items FROM  Web.[Split] (@Process, ','))) 
	) AS H
	LEFT JOIN   Web._JobReceiveLines  L ON L.JobReceiveHeaderId=H.JobReceiveHeaderId
	LEFT JOIN Web._JobOrderlines JOL ON JOL.JobOrderLineId=L.JobOrderLineId
	LEFT JOIN Web.DocumentTypes Dt ON H.DocTypeId = Dt.DocumentTypeId
   
	
	UNION ALL
	
	SELECT L.JobReceiveLineId,NULL As ProcessId,- L.Qty,NULL AS JobReceiveHeaderId,NULL AS  DocTypeId,NULL AS DocumentTypeShortName,NULL AS JobReceiveNo,
    NULL AS  ProductId,NULL AS Dimension1Id,NULL AS Dimension2Id,NULL AS Specification,NULL AS JobWorkerId,NULL AS DocDate,NULL AS DivisionId,NULL AS SiteId,NULL As  DocumentTypeName
	FROM Web._JobReturnLines L 
	LEFT JOIN Web._jobreturnheaders H WITH (nolock) ON H.JobReturnHeaderId=L.JobReturnHeaderId
	WHERE 1=1
	AND (@StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate) 
    
    UNION ALL 
    
    SELECT L.JobReceiveLineId,NULL As ProcessId,- JRL.Qty,NULL AS JobReceiveHeaderId,NULL AS  DocTypeId,NULL AS DocumentTypeShortName,NULL AS JobReceiveNo,
    NULL AS  ProductId,NULL AS Dimension1Id,NULL AS Dimension2Id,NULL AS Specification,NULL AS JobWorkerId,NULL AS DocDate,NULL AS DivisionId,NULL AS SiteId ,NULL as DocumentTypeName
    FROM Web.JobInvoiceLines L
    LEFT JOIN  Web._JobReceiveLines JRL WITH (nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
    LEFT JOIN Web._JobInvoiceHeaders H WITH (nolock) ON L.JobInvoiceHeaderId=H.JobInvoiceHeaderId
    WHERE 1=1
    AND (@StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate) 
    ) AS VJobReceiveBal    
  WHERE 1=1
  AND VJobReceiveBal.JobReceiveLineId IS NOT NULL
  AND ( @Product IS NULL OR ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ',')))
  GROUP BY VJobReceiveBal.JobReceiveLineId
  HAVING IsNull(Sum(VJobReceiveBal.Qty),0) > 0
GO


