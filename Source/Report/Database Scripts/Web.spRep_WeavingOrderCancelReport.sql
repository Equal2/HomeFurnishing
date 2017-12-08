USE [RUG]
GO

/****** Object:  StoredProcedure [Web].[spRep_WeavingOrderCancelReport]    Script Date: 31/Aug/2015 1:04:51 PM ******/
IF object_id ('[Web].[spRep_WeavingOrderCancelReport]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_WeavingOrderCancelReport]
GO
/****** Object:  StoredProcedure [Web].[spRep_WeavingOrderCancelReport]    Script Date: 31/Aug/2015 1:04:51 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Web].[spRep_WeavingOrderCancelReport]
  @FromDate VARCHAR(20) = NULL,
  @ToDate VARCHAR(20) = NULL,
  @Site VARCHAR(20) = NULL,
  @Division VARCHAR(20) = NULL,     
  @JobOrderHeaderId VARCHAR(Max) = NULL,  
  @DocumentType VARCHAR(20) = NULL ,
  @PersonCustomGroup VARCHAR(Max) = NULL, 
  @ProductGroup VARCHAR(Max) = NULL ,   	   
  @CancelById varchar(Max)=NULL,
  @Worker VARCHAR(Max) = NULL,
  @Product VARCHAR(Max) = NULL,
  @ProdOrderHeaderId varchar(Max)=NULL,  
  @Process VARCHAR(Max) = NULL,
  @ProductType VARCHAR(Max) = NULL,
  @ProductCustomGroup VARCHAR(Max) = NULL, 
  @ProductNature VARCHAR(Max) = NULL
AS
BEGIN

select  H.SiteId, H.DivisionId,DT.DocumentTypeShortName + '-' + H.DocNo AS CancelNo,
CC.CostCenterName as CostCenterNo,
H.DocDate as CancelDate, DTPI.DocumentTypeShortName + '-' + JOH.DocNo AS OrderNo,
PS.Name AS Name,PG.ProductGroupName as  Design,Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName else VRS.StandardSizeName END as size,
L.Qty as PCS,(ROUND(CAST(JOL.DealQty AS float) / CAST(JOL.Qty AS float), 4))*L.Qty as Area,U.DecimalPlaces AS DecimalPlaces,
'WeavingOrderCancelReport.rdl' AS ReportName,'Weaving Order Cancel Report' AS ReportTitle
 from 
(
SELECT * FROM [Web]._JobOrderCancelHeaders H WITH (nolock) WHERE 1=1
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @DocumentType is null or H.DocTypeId IN (SELECT Items FROM [dbo].[Split] (@DocumentType, ','))) 
AND ( @Worker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@Worker, ',')))
AND ( @FromDate is null or @FromDate <= H.DocDate ) 
AND ( @ToDate is null or @ToDate >= H.DocDate ) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @PersonCustomGroup IS NULL OR H.JobWorkerId IN (SELECT PersonId FROM Web.PersonCustomGroupLines WITH (nolock) WHERE PersonCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@PersonCustomGroup, ',')))) 
)H
LEFT JOIN Web.DocumentTypes DT WITH (nolock) ON DT.DocumentTypeId = H.DocTypeId 
LEFT JOIN Web._JobOrderCancelLines L WITH (nolock) ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId
LEFT JOIN Web.JobOrderLines JOl WITH (nolock) ON L.JobOrderLineId = JOL.JobOrderLineId
LEFT JOIN Web.JobOrderHeaders JOH WITH (nolock) ON JOH.JobOrderHeaderId = JOL.JobOrderHeaderId 
LEFT JOIN Web.DocumentTypes DTPI WITH (nolock) ON DTPI.DocumentTypeId = JOH.DocTypeId
LEFT JOIN Web._People PS WITH (nolock) ON PS.PersonID = H.JobWorkerId   
LEFT JOIN Web.Products P WITH (nolock) ON P.ProductId = JOL.ProductId 
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId
LEFT JOIN Web.ProductTypes PT WITH (nolock)  ON Pg.ProductTypeId = Pt.ProductTypeId  
LEFT JOIN web.CostCenters CC with (nolock) on CC.CostCenterId=JOH.CostCenterId
LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=P.ProductId
LEFT JOIN [Web].Units U WITH (nolock) ON U.UnitId = P.UnitId 
LEFT JOin Web.ProdOrderLines POL With (Nolock) On POL.ProdOrderLineId=JOL.ProdOrderLineId
Where 1=1 
AND ( @CancelById is null or H.OrderById IN (SELECT Items FROM [dbo].[Split] (@CancelById, ','))) 
AND ( @Product is null or JOl.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ',')))  
AND ( @ProductType is null or PG.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ',')))
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @JobOrderHeaderId is null or JOH.JobOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@JobOrderHeaderId, ',')))
AND ( @ProdOrderHeaderId is null or POL.ProdOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProdOrderHeaderId, ',')))
AND ( @ProductCustomGroup IS NULL OR P.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WITH (nolock) WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
Order By H.DocDate
End
GO


