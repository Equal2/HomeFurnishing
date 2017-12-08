IF object_id ('[Web].[spRep_ProdOrderBalance]') IS NOT NULL 
 DROP Procedure  [Web].[spRep_ProdOrderBalance]
GO 


CREATE PROCEDURE [Web].[spRep_ProdOrderBalance]     
	@AsOnDate VARCHAR(20) = NULL,
    @LoginSite VARCHAR(20) = NULL,
    @LoginDivision VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,	
	@Process VARCHAR(20) = NULL,
    @ProductNature VARCHAR(Max) = NULL,
    @ProductType VARCHAR(Max) = NULL,
    @ProductGroup VARCHAR(Max) = NULL,
    @ProductCustomGroup VARCHAR(Max) = NULL,
    @PersonCustomGroup VARCHAR(Max) = NULL,
    @Product VARCHAR(Max) = NULL,  
    @Dimension1 VARCHAR(Max) = NULL,  
    @Dimension2 VARCHAR(Max) = NULL,  
	@ProdOrderHeaderId VARCHAR(Max) = NULL
AS
BEGIN

DECLARE @Dimension1IdCnt INT 
DECLARE @Dimension2IdCnt INT 

DECLARE @StatusOnDate NVARCHAR(20)

IF (@AsOnDate = NULL)
BEGIN
	SET @StatusOnDate = getdate()
END 
ELSE
BEGIN
	SET @StatusOnDate = @AsOnDate
END 


SELECT @Dimension1IdCnt = Count(*)
FROM Web.FProdOrderBalance(@StatusOnDate, @LoginSite,@LoginDivision, @FromDate, @ToDate, @DocumentType, @Process, @Product, @ProdOrderHeaderId) AS H 
LEFT JOIN Web.Products P WITH (NoLock) ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg WITH (NoLock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (NoLock) ON Pg.ProductTypeId = Pt.ProductTypeId
WHERE 1=1
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 IS NULL OR H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 IS NULL OR H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND H.Dimension1Id IS NOT NULL


SELECT @Dimension2IdCnt = Count(*)
FROM Web.FProdOrderBalance(@StatusOnDate, @LoginSite,@LoginDivision, @FromDate, @ToDate, @DocumentType, @Process, @Product, @ProdOrderHeaderId) AS H 
LEFT JOIN Web.Products P WITH (NoLock) ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups PG WITH (NoLock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (NoLock) ON Pg.ProductTypeId = Pt.ProductTypeId
WHERE 1=1
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 IS NULL OR H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 IS NULL OR H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
AND H.Dimension2Id IS NOT NULL


SELECT H.OrderDate, H.OrderNo, P.ProductName, H.Specification, 
Pt.ProductTypeName, PR.ProcessName, 
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN Dt2.Dimension2TypeName	
	 ELSE Dt1.Dimension1TypeName END AS Dimension1TypeName, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN Dt1.Dimension1TypeName	
	 ELSE Dt2.Dimension2TypeName END AS Dimension2TypeName, 	
CASE WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN D2.Dimension2Name
	 ELSE D1.Dimension1Name END AS Dimension1Name, 	
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN D1.Dimension1Name
	 ELSE D2.Dimension2Name END AS Dimension2Name, 	
H.BalanceQty, 
U.UnitName AS UnitName,
IsNull(U.DecimalPlaces,0) AS DecimalPlaces,
H.SiteId AS SiteId, H.DivisionId AS DivisionId,
S.SiteName AS SiteName, D.DivisionName AS DivisionName, 
CASE WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt > 0 THEN 'ProdOrderBalanceWithDoubleDimension.rdl'
	 WHEN @Dimension1IdCnt > 0 AND @Dimension2IdCnt = 0 THEN 'ProdOrderBalanceWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt > 0 THEN 'ProdOrderBalanceWithSingleDimension.rdl'
	 WHEN @Dimension1IdCnt = 0 AND @Dimension2IdCnt = 0 THEN 'ProdOrderBalance.rdl'
END AS ReportName, 
PR.ProcessName + ' Order Balance' AS ReportTitle, NULL AS SubReportProcList
FROM Web.FProdOrderBalance(@StatusOnDate, @LoginSite,@LoginDivision, @FromDate, @ToDate, @DocumentType, @Process, @Product, @ProdOrderHeaderId) AS H 
LEFT JOIN Web.Products P WITH (NoLock) ON H.ProductId = P.ProductId
LEFT JOIN Web.ProductGroups Pg WITH (NoLock) ON P.ProductGroupId = Pg.ProductGroupId
LEFT JOIN Web.ProductTypes Pt WITH (NoLock) ON Pg.ProductTypeId = Pt.ProductTypeId
LEFT JOIN Web.Dimension1Types Dt1 WITH (NoLock) ON Pt.Dimension1TypeId = Dt1.Dimension1TypeId
LEFT JOIN Web.Dimension2Types Dt2 WITH (NoLock) ON Pt.Dimension2TypeId = Dt2.Dimension2TypeId
LEFT JOIN Web.Units U WITH (NoLock) ON P.UnitId = U.UnitId
LEFT JOIN Web.Sites S WITH (NoLock) ON H.SiteId = S.SiteId
LEFT JOIN Web.Divisions D WITH (NoLock) ON H.DivisionId = D.DivisionId
LEFT JOIN Web.Dimension1 D1 WITH (NoLock) ON H.Dimension1Id = D1.Dimension1Id
LEFT JOIN Web.Dimension2 D2 WITH (NoLock) ON H.Dimension2Id = D2.Dimension2Id
LEFT JOIN web.Processes PR ON PR.ProcessId = H.ProcessId
WHERE 1=1
AND ( @ProductGroup IS NULL OR Pg.ProductGroupId IN (SELECT Items FROM [dbo].[Split] (@ProductGroup, ','))) 
AND ( @ProductType IS NULL OR Pt.ProductTypeId IN (SELECT Items FROM [dbo].[Split] (@ProductType, ','))) 
AND ( @ProductNature IS NULL OR Pt.ProductNatureId IN (SELECT Items FROM [dbo].[Split] (@ProductNature, ','))) 
AND ( @ProductCustomGroup IS NULL OR H.ProductId IN (SELECT ProductId FROM Web.ProductCustomGroupLines WHERE ProductCustomGroupHeaderId IN (SELECT Items FROM [dbo].[Split] (@ProductCustomGroup, ',')))) 
AND ( @Dimension1 IS NULL OR H.Dimension1Id IN (SELECT Items FROM [dbo].[Split] (@Dimension1, ','))) 
AND ( @Dimension2 IS NULL OR H.Dimension2Id IN (SELECT Items FROM [dbo].[Split] (@Dimension2, ','))) 
ORDER BY H.OrderDate
End
GO
