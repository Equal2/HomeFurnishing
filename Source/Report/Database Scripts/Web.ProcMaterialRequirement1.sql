IF object_id ('[Web].[ProcMaterialRequirement1]') IS NOT NULL 
 DROP Procedure  [Web].[ProcMaterialRequirement1]
GO 


CREATE procedure   [Web].[ProcMaterialRequirement1]  
@AsOnDate VARCHAR(20) = NULL,  
@DocumentCategoryId VARCHAR(Max) = NULL,     
@LoginSite VARCHAR(20) = NULL,      
@Division VARCHAR(20) = NULL,    
@ProdOrderHeaderId VARCHAR(Max) = NULL,                
@Product VARCHAR(Max) = NULL   
AS  
Begin  

DECLARE @TempTable TABLE ( ItemGroupName NVARCHAR (100), Rank INT  )

insert into @TempTable(ItemGroupName, Rank)
SELECT 'Woolen Yarn', 1
UNION 
SELECT 'Silk', 2

IF @DocumentCategoryId IS NULL SET @DocumentCategoryId = 331
IF @LoginSite IS NULL SET @LoginSite = 17

DECLARE @StartDate VARCHAR(20)

SET  @StartDate = '01/May/2015' 

DECLARE @TS AS Web.TypeParameterForBOM



INSERT INTO @TS  
SELECT L.SaleOrderLineId, L.ProductId, L.BalanceQty AS Qty 
FROM ( SELECT * FROM Web.ViewSaleOrderBalanceForPlanning WITH (Nolock) ) L
LEFT JOIN web.SaleOrderHeaders H WITH (Nolock) ON H.SaleOrderHeaderId = L.SaleOrderHeaderId
LEFT JOIN Web.Products P WITH (Nolock) ON L.ProductId  = P.ProductId
LEFT JOIN Web.FinishedProduct FP WITH (Nolock) ON FP.ProductId  = P.ProductId
WHERE H.DocDate > =@StartDate
AND isnull(FP.IsSample,0) = 0

--SELECT * FROM @T


SELECT @LoginSite AS SiteId, V1.ProductDivisionId, V1.ProductId, Max(isnull(TIG.Rank,100)) AS ProductGroupRank,
Max(D.DivisionName) AS ProductDivisionName, Max(P.ProductName) AS ProductName, Max(U.UnitName) AS Unit, 
Max(U.DecimalPlaces) AS DecimalPlaces, Max(PG.ProductGroupName) AS ProductGroup,
sum(isnull(V1.Qty,0)) AS Qty,
( SELECT TOP 1 Notes  FROM Web.ReportHeaders WITH (Nolock) WHERE SqlProc = 'ProcMaterialRequirement1' ) AS ReportNotes
FROM 
(
SELECT  H.ProductDivisionId, H.ProductId, H.Qty  
FROM 
(
SELECT * FROM [Web].[FGetBOMForProductForMaterialRequirement]  (@TS)  A
) H

) V1
LEFT JOIN web.Divisions D WITH (Nolock) ON D.DivisionId = V1.ProductDivisionId
LEFT JOIN web.Products P WITH (nolock) ON P.ProductId = V1.ProductId
LEFT JOIN web.Units U WITH (nolock) ON U.UnitId = P.UnitId 
LEFT JOIN web.ProductGroups PG WITH (nolock) ON PG.ProductGroupId = P.ProductGroupId 
LEFT JOIN @TempTable TIG ON TIG.ItemGroupName = PG.ProductGroupName
WHERE 1=1 
AND ( @Division is null or V1.ProductDivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
AND ( @Product is null or V1.ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ',')))  
GROUP BY  V1.ProductDivisionId, V1.ProductId


End
GO