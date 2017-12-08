
IF OBJECT_ID ('[Web].[ProcPackingRegister]') IS NOT NULL
	DROP PROCEDURE [Web].[ProcPackingRegister]
GO



CREATE procedure  [Web].[ProcPackingRegister]      
@Site VARCHAR(20) = NULL,      
@Division VARCHAR(20) = NULL,      
@FromDate VARCHAR(20) = NULL,      
@ToDate VARCHAR(20) = NULL,      
@DateFilterOn VARCHAR(20) = NULL,   
@JobWorker VARCHAR(Max) = NULL,   
@ProductNature VARCHAR(Max) = NULL,      
@ProductCategory VARCHAR(Max) = NULL,      
@ProductType VARCHAR(Max) = NULL,      
@ProductCollection VARCHAR(Max) = NULL,      
@ProductQuality VARCHAR(Max) = NULL,      
@ProductGroup VARCHAR(Max) = NULL,      
@ProductStyle VARCHAR(Max) = NULL,      
@ProductDesign VARCHAR(Max) = NULL,      
@ProductShape VARCHAR(Max) = NULL,      
@ProductSize VARCHAR(Max) = NULL,      
@ProductInvoiceGroup VARCHAR(Max) = NULL,      
@ProductCustomGroup VARCHAR(Max) = NULL,      
@ProductTag VARCHAR(Max) = NULL,      
@Product VARCHAR(Max) = NULL,        
@SaleOrderHeaderId VARCHAR(Max) = NULL,      
@PackingHeaderId VARCHAR(Max) = NULL  
as  
Begin  
SELECT L.CreatedDate, H.SiteId, H.DivisionId, H.PackingHeaderId, H.DocDate, H.DocNo, L.Qty, L.BaleNo,  
P.ProductName, SOH.DocNo AS SaleOrderNo, PU.ProductUidName , PG.ProductGroupName, VRA.SizeName, PQ.ProductQualityName, 
VRA.Area *  L.Qty AS AreaInFeet, VRA.SqYardPerPcs *  L.Qty AS AreaInSqYard,
'PackingRegister' AS ReportName , 'Packing Register' AS ReportTitle,NULL AS SubReportProcList,
S.SiteName, VDC.DivisionName, VDC.CompanyName, VDC.CompanyAddress, VDC.CompanyCity         
FROM Web.PackingHeaders H WITH (Nolock)
LEFT JOIN Web.PackingLines L WITH (Nolock) ON L.PackingHeaderId = H.PackingHeaderId 
LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.SaleOrderLines SOL WITH (Nolock) ON SOL.SaleOrderLineId = L.SaleOrderLineId 
LEFT JOIN Web.SaleOrderHeaders SOH WITH (Nolock) ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
LEFT JOIN Web.ProductUids PU WITH (Nolock) ON  PU.ProductUIDId = L.ProductUidId 
LEFT JOIN Web.FinishedProduct FPD WITH (Nolock) ON P.ProductId = FPD.ProductId  
LEFT JOIN Web.Units U WITH (Nolock) ON U.UnitId = P.UnitId   
LEFT JOIN Web.ViewRugArea VRA ON VRA.ProductId = P.ProductId   
LEFT JOIN Web.ProductQualities PQ ON PQ.ProductQualityId = FPD.ProductQualityId    
LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId  = P.ProductGroupId 
LEFT JOIN web.Sites S ON S.SiteId = H.SiteId
LEFT JOIN web.ViewDivisionCompany  VDC ON VDC.DivisionId = H.DivisionId    
WHERE 1=1  
AND ( @Site is null or H.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ',')))   
AND ( @Division is null or H.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ',')))   
AND ( @FromDate is null or @FromDate <= ( CASE WHEN @DateFilterOn = '2' THEN L.CreatedDate ELSE  H.DocDate END ) )   
AND ( @ToDate is null or @ToDate >= ( CASE WHEN @DateFilterOn = '2' THEN dateadd (Day,-1, L.CreatedDate) ELSE  H.DocDate END ) )    
AND ( @JobWorker is null or H.JobWorkerId IN (SELECT Items FROM [dbo].[Split] (@JobWorker, ',')))  
AND ( @ProductCollection is null or FPD.ProductCollectionId IN (SELECT Items FROM [dbo].[Split] (@ProductCollection, ',')))   
AND ( @ProductQuality is null or FPD.ProductCollectionId IN (SELECT Items FROM [dbo].[Split] (@ProductQuality, ',')))   
AND ( @ProductGroup is null or P.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ',')))   
AND ( @ProductStyle is null or FPD.ProductStyleId IN (SELECT Items FROM [dbo].[Split] (@ProductStyle, ',')))   
AND ( @ProductDesign is null or FPD.ProductDesignId IN (SELECT Items FROM [dbo].[Split] (@ProductDesign, ',')))   
AND ( @ProductShape is null or VRA.ProductShapeId IN (SELECT Items FROM [dbo].[Split] (@ProductShape, ',')))   
AND ( @ProductSize is null or VRA.SizeId IN (SELECT Items FROM [dbo].[Split] (@ProductSize, ',')))   
AND ( @ProductInvoiceGroup is null or FPD.ProductInvoiceGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductInvoiceGroup, ',')))   
AND ( @Product is null or P.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ',')))   
AND ( @SaleOrderHeaderId is null or SOL.SaleOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@SaleOrderHeaderId, ',')))   
AND ( @PackingHeaderId is null or H.PackingHeaderId IN (SELECT Items FROM [dbo].[Split] (@PackingHeaderId, ',')))  
ORDER BY H.DocDate, H.DocNo, Try_Parse(Replace(L.BaleNo,'-','') AS BIGINT)
END
GO
