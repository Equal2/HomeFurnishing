IF object_id ('[Web].[spStockInHand]') IS NOT NULL 
 DROP Procedure  [Web].[spStockInHand]
GO 

CREATE PROCEDURE [Web].[spStockInHand]     
	@ProductType VARCHAR(Max) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate NVARCHAR(20) = NULL,
	@ToDate NVARCHAR(20) = NULL,
	@GroupOn VARCHAR(Max) = NULL,
	@ShowBalance VARCHAR(Max) = NULL,
	@Product VARCHAR(20) = NULL,
	@Godown VARCHAR(20) = NULL,
	@Process VARCHAR(20) = NULL,
	@Dimension1 VARCHAR(20) = NULL,
	@Dimension2 VARCHAR(20) = NULL,
	@ProductNature VARCHAR(Max) = NULL,    
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL
	
AS
BEGIN

--SET @GroupOn ='Godown,Process,Product'

-- Set Group By Condition
IF ( isnull(@GroupOn,'') = '' ) 
SET @GroupOn ='Product'

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



-- Main Qry
SELECT VMain.SiteId, VMain.DivisionId, 
VMain.ProductId, VMain.GodownId, VMain.ProcessId, VMain.Dimension1Id, VMain.Dimension2Id,  
VMain.LotNo, max(P.ProductName) AS ProductName, max(U.UnitName) AS UnitName,max(U.DecimalPlaces) AS UnitDecimalPlaces,
max(S.SiteName) AS SiteName,max(D.DivisionName) AS DivisionName,max(G.GodownName) AS GodownName,max(PG.ProductGroupName) AS ProductGroupName,
max(D1.Dimension1Name) AS Dimension1Name,max(D2.Dimension2Name) AS Dimension2Name,max(PR.ProcessName) AS ProcessName,
Max(Dt1.Dimension1TypeName) AS Dimension1TypeName, Max(Dt2.Dimension2TypeName) AS Dimension2TypeName,
Sum(Isnull(VMain.Opening,0)) AS Opening,Sum(Isnull(VMain.RecQty,0)) AS RecQty,Sum(Isnull(VMain.IssQty,0)) AS IssQty,
Sum(Isnull(VMain.Opening,0)) + Sum(Isnull(VMain.RecQty,0)) - Sum(Isnull(VMain.IssQty,0)) AS BalQty
INTO #MyTempTable
FROM
(
-- For Opening
SELECT SH.SiteId, SH.DivisionId, 
CASE WHEN @IsGroupOnGodown > 0 THEN H.GodownId ELSE NULL END AS GodownId, 
CASE WHEN @IsGroupOnProcess > 0 THEN H.ProcessId ELSE NULL END AS ProcessId, 
CASE WHEN @IsGroupOnProduct > 0 THEN H.ProductId ELSE NULL END AS ProductId, 
CASE WHEN @IsGroupOnDimension1 > 0 THEN H.Dimension1Id ELSE NULL END AS Dimension1Id, 
CASE WHEN @IsGroupOnDimension2 > 0 THEN H.Dimension2Id ELSE NULL END AS Dimension2Id, 
CASE WHEN @IsGroupOnLotNo > 0 THEN H.LotNo ELSE NULL END AS LotNo, 
sum(isnull(H.Qty_Rec,0)) - sum(isnull(H.Qty_Iss,0)) AS Opening, 0 AS RecQty,0 AS  IssQty    
FROM web.Stocks H WITH (Nolock)
LEFT JOIN web.StockHeaders SH WITH (Nolock) ON SH.StockHeaderId = H.StockHeaderId 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups PG ON P.ProductGroupId = PG.ProductGroupId
LEFT JOIN Web.ProductTypes PT ON PG.ProductTypeId = PT.ProductTypeId
WHERE  H.ProductId IS NOT NULL 
AND ( @Site is null or SH.SiteId IN (SELECT Items FROM [dbo].[Split] (@Site, ','))) 
AND ( @Division is null or SH.DivisionId IN (SELECT Items FROM [dbo].[Split] (@Division, ','))) 
AND ( @FromDate is null or @FromDate > SH.DocDate ) 
AND ( @Product is null or H.ProductId IN (SELECT Items FROM [dbo].[Split] (@Product, ','))) 
AND ( @Godown is null or H.GodownId IN (SELECT Items FROM [dbo].[Split] (@Godown, ','))) 
AND ( @Process is null or H.ProcessId IN (SELECT Items FROM [dbo].[Split] (@Process, ','))) 
AND ( @Dimension1 is null or H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 is null or H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
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
SELECT SH.SiteId, SH.DivisionId, 
CASE WHEN @IsGroupOnGodown > 0 THEN H.GodownId ELSE NULL END AS GodownId, 
CASE WHEN @IsGroupOnProcess > 0 THEN H.ProcessId ELSE NULL END AS ProcessId, 
CASE WHEN @IsGroupOnProduct > 0 THEN H.ProductId ELSE NULL END AS ProductId, 
CASE WHEN @IsGroupOnDimension1 > 0 THEN H.Dimension1Id ELSE NULL END AS Dimension1Id, 
CASE WHEN @IsGroupOnDimension2 > 0 THEN H.Dimension2Id ELSE NULL END AS Dimension2Id, 
CASE WHEN @IsGroupOnLotNo > 0 THEN H.LotNo ELSE NULL END AS LotNo, 
0 AS Opening, sum(isnull(H.Qty_Rec,0))  AS RecQty, sum(isnull(H.Qty_Iss,0))  AS IssQty
FROM web.Stocks H WITH (Nolock)
LEFT JOIN web.StockHeaders SH WITH (Nolock) ON SH.StockHeaderId = H.StockHeaderId 
LEFT JOIN Web.Products P ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups PG ON P.ProductGroupId = PG.ProductGroupId
LEFT JOIN Web.ProductTypes PT ON PG.ProductTypeId = PT.ProductTypeId
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
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
GROUP BY SH.SiteId, SH.DivisionId,
CASE WHEN @IsGroupOnGodown > 0 THEN H.GodownId ELSE NULL END , 
CASE WHEN @IsGroupOnProcess > 0 THEN H.ProcessId ELSE NULL END , 
CASE WHEN @IsGroupOnProduct > 0 THEN H.ProductId ELSE NULL END , 
CASE WHEN @IsGroupOnDimension1 > 0 THEN H.Dimension1Id ELSE NULL END , 
CASE WHEN @IsGroupOnDimension2 > 0 THEN H.Dimension2Id ELSE NULL END , 
CASE WHEN @IsGroupOnLotNo > 0 THEN H.LotNo ELSE NULL END 
) AS VMain
LEFT JOIN web.Products P  WITH (Nolock) ON P.ProductId = VMain.ProductId 
LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId = P.UnitId 
LEFT JOIN Web.Sites S WITH (Nolock) ON S.SiteId = VMain.SiteId
LEFT JOIN Web.Divisions D WITH (Nolock) ON D.DivisionId  = VMain.DivisionId
LEFT JOIN Web.Godowns G WITH (Nolock) ON G.GodownId = VMain.GodownId
LEFT JOIN Web.Processes PR WITH (Nolock) ON PR.ProcessId = VMain.ProcessId
LEFT JOIN web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id = VMain.Dimension1Id
LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id = VMain.Dimension2Id
LEFT JOIN Web.ProductGroups Pg WITH (Nolock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (Nolock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1Types Dt1 WITH (Nolock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (Nolock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
GROUP BY VMain.SiteId, VMain.DivisionId, VMain.ProductId, VMain.GodownId, VMain.ProcessId, VMain.Dimension1Id, VMain.Dimension2Id, VMain.LotNo

 
-- To Filter Data According To ShowBalance

IF (@ShowBalance = 'Not Zero')
	SELECT * FROM #MyTempTable  WHERE Round(BalQty,4) <> 0
ELSE IF (@ShowBalance = 'Zero')
	SELECT * FROM #MyTempTable  WHERE Round(BalQty,4) = 0
ELSE IF (@ShowBalance = 'Greater Than Zero')
	SELECT * FROM #MyTempTable  WHERE Round(BalQty,4) > 0
ELSE IF (@ShowBalance = 'Less Than Zero')
	SELECT * FROM #MyTempTable  WHERE Round(BalQty,4) < 0
ELSE IF (@ShowBalance = 'Period Negative')
	SELECT * FROM #MyTempTable  WHERE Round(BalQty,4) < 0 AND Round(Opening,4) >= 0 
ELSE 
	SELECT * FROM #MyTempTable  

End
GO



