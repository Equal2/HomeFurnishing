IF object_id ('[Web].[spStockLedger]') IS NOT NULL 
 DROP Procedure  [Web].[spStockLedger]
GO 

CREATE PROCEDURE [Web].[spStockLedger]     
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@GroupOn VARCHAR(Max) = NULL,
	@Product VARCHAR(20) = NULL,
	@Godown VARCHAR(20) = NULL,
	@Process VARCHAR(20) = NULL,
	@Dimension1 VARCHAR(20) = NULL,
	@Dimension2 VARCHAR(20) = NULL
AS
BEGIN

-- Set Group By Condition
DECLARE @CondStr NVARCHAR(Max)  
DECLARE @IsGroupOnGodown INT 
DECLARE @IsGroupOnProcess INT 
DECLARE @IsGroupOnProduct INT 
DECLARE @IsGroupOnDimension1 INT 
DECLARE @IsGroupOnDimension2 INT 
DECLARE @IsGroupOnLotNo INT 

SET @IsGroupOnGodown = charindex('Godown',@GroupOn)
SET @IsGroupOnProcess = charindex('Process',@GroupOn)
SET @IsGroupOnProduct = charindex('Product',@GroupOn)
SET @IsGroupOnDimension1 = charindex('Dimension1',@GroupOn)
SET @IsGroupOnDimension2 = charindex('Dimension2',@GroupOn)
SET @IsGroupOnLotNo = charindex('LotNo',@GroupOn)



-- For Opening
SELECT SH.SiteId, SH.DivisionId,  Max(Dt1.Dimension1TypeName) AS Dimension1TypeName, Max(Dt2.Dimension2TypeName) AS Dimension2TypeName,
--H.ProductId, H.GodownId, H.ProcessId, H.Dimension1Id, H.Dimension2Id,  
CASE WHEN @IsGroupOnGodown > 0 THEN max(G.GodownName) ELSE NULL END AS GodownName, 
CASE WHEN @IsGroupOnProcess > 0 THEN max(PR.ProcessName) ELSE NULL END AS ProcessName, 
CASE WHEN @IsGroupOnProduct > 0 THEN max(P.ProductName) ELSE NULL END AS ProductName, 
CASE WHEN @IsGroupOnDimension1 > 0 THEN max(D1.Dimension1Name) ELSE NULL END AS Dimension1Name, 
CASE WHEN @IsGroupOnDimension2 > 0 THEN max(D2.Dimension2Name) ELSE NULL END AS Dimension2Name, 
CASE WHEN @IsGroupOnLotNo > 0 THEN H.LotNo ELSE NULL END AS LotNo, 
'Opening' AS DocNo, @FromDate AS DocDate, 'Opening' AS TransactionType,
max(U.UnitName) AS UnitName,max(U.DecimalPlaces) AS UnitDecimalPlaces,
max(S.SiteName) AS SiteName,max(D.DivisionName) AS DivisionName, 
sum(isnull(H.Qty_Rec,0)) - sum(isnull(H.Qty_Iss,0)) AS Opening,
0 AS RecQty,0 AS  IssQty    
FROM web.Stocks H WITH (Nolock)
LEFT JOIN web.StockHeaders SH WITH (Nolock) ON SH.StockHeaderId = H.StockHeaderId 
LEFT JOIN web.Products P  WITH (Nolock) ON P.ProductId = H.ProductId 
LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId = P.UnitId 
LEFT JOIN Web.Sites S WITH (Nolock) ON S.SiteId = SH.SiteId
LEFT JOIN Web.Divisions D WITH (Nolock) ON D.DivisionId  = SH.DivisionId
LEFT JOIN Web.Godowns G WITH (Nolock) ON G.GodownId = SH.GodownId
LEFT JOIN Web.Processes PR WITH (Nolock) ON PR.ProcessId = H.ProcessId
LEFT JOIN web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id = H.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id = H.Dimension2Id
LEFT JOIN Web.ProductGroups Pg WITH (Nolock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (Nolock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1Types Dt1 WITH (Nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (Nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
WHERE  H.ProductId IS NOT NULL 
AND ( @Site is null or SH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or SH.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate > SH.DocDate ) 
AND ( @Product is null or H.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @Godown is null or H.GodownId IN (SELECT Items FROM [dbo].[Split] (@Godown, ','))) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @Dimension1 is null or H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
GROUP BY SH.SiteId, SH.DivisionId, 
CASE WHEN @IsGroupOnGodown > 0 THEN H.GodownId ELSE NULL END , 
CASE WHEN @IsGroupOnProcess > 0 THEN H.ProcessId ELSE NULL END , 
CASE WHEN @IsGroupOnProduct > 0 THEN H.ProductId ELSE NULL END , 
CASE WHEN @IsGroupOnDimension1 > 0 THEN H.Dimension1Id ELSE NULL END , 
CASE WHEN @IsGroupOnDimension2 > 0 THEN H.Dimension2Id ELSE NULL END , 
CASE WHEN @IsGroupOnLotNo > 0 THEN H.LotNo ELSE NULL END 
HAVING Round(sum(isnull(H.Qty_Rec,0)) - sum(isnull(H.Qty_Iss,0)),4) <> 0


UNION ALL 

-- For Current Transaction
SELECT SH.SiteId, SH.DivisionId, Max(Dt1.Dimension1TypeName) AS Dimension1TypeName, Max(Dt2.Dimension2TypeName) AS Dimension2TypeName,
--H.ProductId, H.GodownId, H.ProcessId, H.Dimension1Id, H.Dimension2Id, 
CASE WHEN @IsGroupOnGodown > 0 THEN max(G.GodownName) ELSE NULL END AS GodownName, 
CASE WHEN @IsGroupOnProcess > 0 THEN max(PR.ProcessName) ELSE NULL END AS ProcessName, 
CASE WHEN @IsGroupOnProduct > 0 THEN max(P.ProductName) ELSE NULL END AS ProductName, 
CASE WHEN @IsGroupOnDimension1 > 0 THEN max(D1.Dimension1Name) ELSE NULL END AS Dimension1Name, 
CASE WHEN @IsGroupOnDimension2 > 0 THEN max(D2.Dimension2Name) ELSE NULL END AS Dimension2Name, 
CASE WHEN @IsGroupOnLotNo > 0 AND isnull(PSD.LotManagement,0)  = 1 THEN H.LotNo ELSE NULL END AS LotNo, 
Max(SH.DocNo) AS DocNo, Max(SH.DocDate) AS DocDate, Max(DT.DocumentTypeShortName) AS TransactionType,
max(U.UnitName) AS UnitName,max(U.DecimalPlaces) AS UnitDecimalPlaces,
max(S.SiteName) AS SiteName,max(D.DivisionName) AS DivisionName,
0 AS Opening, sum(isnull(H.Qty_Rec,0))  AS RecQty, sum(isnull(H.Qty_Iss,0))  AS IssQty
FROM web.Stocks H WITH (Nolock)
LEFT JOIN web.StockHeaders SH WITH (Nolock) ON SH.StockHeaderId = H.StockHeaderId 
LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = SH.DocTypeId 
LEFT JOIN web.Products P  WITH (Nolock) ON P.ProductId = H.ProductId 
LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId = P.UnitId 
LEFT JOIN Web.Sites S WITH (Nolock) ON S.SiteId = SH.SiteId
LEFT JOIN Web.Divisions D WITH (Nolock) ON D.DivisionId  = SH.DivisionId
LEFT JOIN Web.Godowns G WITH (Nolock) ON G.GodownId = SH.GodownId
LEFT JOIN Web.Processes PR WITH (Nolock) ON PR.ProcessId = H.ProcessId
LEFT JOIN web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id = H.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id = H.Dimension2Id
LEFT JOIN Web.ProductGroups Pg WITH (Nolock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (Nolock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1Types Dt1 WITH (Nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (Nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN web.ProductSiteDetails  PSD WITH (Nolock) ON PSD.ProductId = P.ProductId AND PSD.DivisionId = P.DivisionId 
WHERE  H.ProductId IS NOT NULL 
AND ( @Site is null or SH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or SH.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate <= SH.DocDate ) 
AND ( @ToDate is null or @ToDate >= SH.DocDate ) 
AND ( @Product is null or H.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @Godown is null or H.GodownId IN (SELECT Items FROM [dbo].[Split] (@Godown, ','))) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @Dimension1 is null or H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
GROUP BY SH.StockHeaderId, SH.SiteId, SH.DivisionId, 
CASE WHEN @IsGroupOnGodown > 0 THEN H.GodownId ELSE NULL END , 
CASE WHEN @IsGroupOnProcess > 0 THEN H.ProcessId ELSE NULL END , 
CASE WHEN @IsGroupOnProduct > 0 THEN H.ProductId ELSE NULL END , 
CASE WHEN @IsGroupOnDimension1 > 0 THEN H.Dimension1Id ELSE NULL END , 
CASE WHEN @IsGroupOnDimension2 > 0 THEN H.Dimension2Id ELSE NULL END , 
CASE WHEN @IsGroupOnLotNo > 0 AND isnull(PSD.LotManagement,0)  = 1  THEN H.LotNo ELSE NULL END 
End
GO

