
SELECT * FROM Web.Reportheaders WHERE ReportName=
Web.spRep_PeriodicSaleOrderStatus
-----------
ALTER PROCEDURE Web.spRep_PeriodicSaleOrderStatus 
    @StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,
	@SaleOrderHeaderId VARCHAR(Max) = NULL,
	@SaleToByer VARCHAR(Max) = NULL,    
    @Product VARCHAR(Max) = NULL ,
    @ReportType VARCHAR(Max)='Job Worker Wise Summary',
    @StartValueInt INT =0,
    @IntervalInt INT=30
    
AS 
BEGIN 

SET @StatusOnDate=(CASE WHEN @StatusOnDate IS NULL  THEN @ToDate ELSE @StatusOnDate END )

	-- Create Temp Table For Collect All Record
	Declare @TempTable TABLE (
	GroupId INT NOT NULL , 
	SiteId INT , 
	DivisionId INT, 
	OpeningOrder FLOAT, 
     NewOrder FLOAT ,
	 OpeningReceive FLOAT,
	  NewReceive FLOAT ,
	  Duedate  DATETIME)
	Insert INTO @TempTable (GroupId, SiteId, DivisionId, OpeningOrder,NewOrder,OpeningReceive,NewReceive,Duedate)
			--- For Opening Order
			
    SELECT 
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 
	Sum(Isnull(H.Balance,0)) AS OpeningOrder,	
	0 AS NewOrder, 0 AS OpeningReceive ,0 AS NewReceive ,H.DueDate AS Duedate
	FROM Web.FSaleOrderBalance(@FromDate, @Site,@Division, NULL, @FromDate, @DocumentType,@SaleOrderHeaderId, @SaleToByer,@Product) AS H 
   	GROUP BY H.SiteId, H.DivisionId,H.DueDate,
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		 
	ELSE H.PersonId  END 
	
	
	UNION ALL 
	
	SELECT 
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 0 AS OpeningOrder,	 
	 Sum(isnull(NetOrderQty,0)) AS NewOrder,	 
	 0 AS OpeningReceive ,0 AS NewReceive,H.DueDate AS Duedate
	FROM Web.FSaleOrder(@ToDate, @Site,@Division, @FromDate, @ToDate, @DocumentType,@SaleOrderHeaderId, @SaleToByer,@Product) AS H 
	GROUP BY H.SiteId, H.DivisionId,H.DueDate,
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		 
	ELSE H.PersonId END  

    	UNION ALL 
	
	SELECT 
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 0 AS OpeningOrder, 0 AS NewOrder,
	  Sum(H.DispatchQty) AS OpeningReceive
	 ,0 AS NewReceive ,H.DueDate AS Duedate 
	FROM Web.FSaleDispatch(@ToDate, @Site,@Division, @FromDate, @ToDate,NULL,@SaleOrderHeaderId,@SaleToByer,@Product) AS H 
	WHERE 1=1  AND H.DocDate < @FromDate
	GROUP BY H.SiteId, H.DivisionId,H.DueDate,
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId
		 ELSE H.PersonId END  
		 
		 
		 UNION ALL 
		 SELECT 
	(CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END) AS GroupId, 
	H.SiteId, H.DivisionId, 0 AS OpeningOrder, 0 AS NewOrder,
	  0 AS OpeningReceive
	 ,Sum(H.DispatchQty) AS NewReceive ,H.DueDate AS Duedate 
	FROM Web.FSaleDispatch(@ToDate, @Site,@Division, @FromDate, @ToDate,NULL,@SaleOrderHeaderId,@SaleToByer,@Product) AS H 
	WHERE 1=1 AND  H.DocDate BETWEEN @FromDate AND @ToDate
	GROUP BY H.SiteId, H.DivisionId,H.DueDate,
	CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId
		 ELSE H.PersonId END  
		 
		 
		 
		SELECT 
		        VMain.GroupId,
		        Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName  
				ELSE Pp.Name END) AS GroupOnDesc,		        
		        Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product' 
				ELSE 'Job Worker Name' END) AS GroupOnTitle,
	        		   
		      SUM(VMain.OpeningOrder) AS OpeningOrder,
		     SUM(VMain.NewOrder) AS NewOrder ,
		      Sum(VMain.OpeningReceive) AS OpeningReceive ,	
		     sum(VMain.NewReceive) AS NewReceive, 
		     SUM(VMain.OpeningOrder)-Sum(VMain.OpeningReceive) AS OpenBalance,
		      SUM(VMain.NewOrder)- sum(VMain.NewReceive) AS  NewBalance, 
		      	0  AS DecimalPlaces,
		    	SUM(VMain.OpeningOrder)-Sum(VMain.OpeningReceive)+SUM(VMain.NewOrder)- sum(VMain.NewReceive) AS Balance,
		         VMain.SiteId, VMain.DivisionId,
		         'PeriodicSaleOrderStatus.rdl' AS ReportName, 'Periodic Sale Order Status' AS ReportTitle, NULL AS SubReportProcList,
		  (CASE WHEN getdate() > VMain.DueDate THEN 'Late' 	 		   
			 WHEN @IntervalInt <> 0 AND getdate() + @StartValueInt > VMain.DueDate  THEN '<' + Convert(NVARCHAR,@StartValueInt) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*1 > VMain.DueDate  THEN Convert(NVARCHAR,@StartValueInt  ) + '-'+ Convert(NVARCHAR, @StartValueInt + (@IntervalInt*1)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*2 > VMain.DueDate   THEN Convert(NVARCHAR,@StartValueInt + (@IntervalInt*1)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*3 > VMain.DueDate    THEN Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*3)) 
			Else '>' + Convert(NVARCHAR,@StartValueInt + @IntervalInt*3) 
			END) AS Ageing,
		   max(CASE WHEN getdate() > VMain.DueDate THEN '1' 		   
			 WHEN @IntervalInt <> 0 AND getdate() + @StartValueInt > VMain.DueDate  THEN '2'
			 WHEN getdate() + @StartValueInt + @IntervalInt*1 > VMain.DueDate  THEN '3'
			 WHEN getdate() + @StartValueInt + @IntervalInt*2 > VMain.DueDate   THEN '4'
			 WHEN getdate() + @StartValueInt + @IntervalInt*3 > VMain.DueDate    THEN '5'
			Else '6'
			END) AS AgeingSr
		FROM ( SELECT * FROM @TempTable ) VMain 
		LEFT JOIN web.Products P ON P.PRoductId = VMain.GroupId 
		LEFT JOIN web._People Pp ON Pp.PersonId = VMain.GroupId 		
		GROUP BY VMain.GroupId ,VMain.SiteId, VMain.DivisionId,
		CASE WHEN getdate() > VMain.DueDate THEN 'Late' 	 		   
			 WHEN @IntervalInt <> 0 AND getdate() + @StartValueInt > VMain.DueDate  THEN '<' + Convert(NVARCHAR,@StartValueInt) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*1 > VMain.DueDate  THEN  Convert(NVARCHAR,@StartValueInt  ) + '-'+ Convert(NVARCHAR, @StartValueInt + (@IntervalInt*1)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*2 > VMain.DueDate   THEN Convert(NVARCHAR,@StartValueInt + (@IntervalInt*1)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*3 > VMain.DueDate    THEN  Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*3)) 
			Else '>' + Convert(NVARCHAR,@StartValueInt + @IntervalInt*3) 
			END
  
	
	 /*	Declare @TempTable TABLE (
	GroupId INT NOT NULL , 
	SiteId INT , 
	DivisionId INT, 
	OpeningOrder FLOAT, 
     NewOrder FLOAT ,
	 OpeningReceive FLOAT,
	  NewReceive FLOAT ,
	  Duedate  DATETIME)

Insert INTO @TempTable (GroupId, SiteId, DivisionId, OpeningOrder,NewOrder,OpeningReceive,NewReceive,Duedate)
	  SELECT 
	 CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 
	Sum(Isnull(H.Balance,0)) AS OpeningOrder,	
	0 AS NewOrder, 0 AS OpeningReceive ,0 AS NewReceive ,H.DueDate AS Duedate
	FROM Web.FSaleOrderBalance(@FromDate, @Site,@Division, NULL, @ToDate, @DocumentType,@SaleOrderHeaderId, @SaleToByer,@Product) AS H
		GROUP BY H.SiteId, H.DivisionId,H.DueDate,(CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END )

	Union All
		SELECT 
	 CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 0 AS OpeningOrder,	 
	 Sum(isnull(NetOrderQty,0)) AS NewOrder,	 
	 0 AS OpeningReceive ,0 AS NewReceive,H.DueDate AS Duedate
	FROM Web.FSaleOrder(@StatusOnDate, @Site,@Division, @FromDate, @ToDate,@DocumentType,@SaleOrderHeaderId, @SaleToByer,@Product) AS H 
	
	GROUP BY H.SiteId, H.DivisionId,H.DueDate, (CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END )
	Union All
		SELECT 
	 CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 0 AS OpeningOrder, 0 AS NewOrder,
	  Sum(H.DispatchQty) AS OpeningReceive
	 ,0 AS NewReceive ,H.DueDate AS Duedate 
	FROM Web.FSaleDispatch(@StatusOnDate, @Site,@Division, @FromDate, @ToDate,@DocumentType,@SaleOrderHeaderId, @SaleToByer,@Product) AS H 
	
	GROUP BY H.SiteId, H.DivisionId,H.DueDate, (CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END)
	Union All
	SELECT 
  CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId  END AS GroupId, 
	H.SiteId, H.DivisionId, 0 AS OpeningOrder, 0 AS NewOrder,
	  0 AS OpeningReceive
	 ,Sum(H.DispatchQty) AS NewReceive ,H.DueDate AS Duedate 
	FROM Web.FSaleDispatch(@StatusOnDate, @Site,@Division, @FromDate, @ToDate,@DocumentType,@SaleOrderHeaderId, @SaleToByer,@Product) AS H 
		
	GROUP BY H.SiteId, H.DivisionId,H.DueDate,(CASE WHEN @ReportType ='Product Wise Summary' THEN H.ProductId		
	ELSE H.PersonId END )

	select H.GroupId,
	Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName  
				ELSE Pp.Name END) AS GroupOnDesc,		        
		        Max(CASE WHEN @ReportType ='Product Wise Summary' THEN 'Product' 
				ELSE 'Job Worker Name' END) AS GroupOnTitle,
  
 max(H.DivisionId) as Divisionid,

 SUM(H.OpeningOrder) AS OpeningOrder,
 SUM(H.NewOrder) AS NewOrder ,
Sum(H.OpeningReceive) AS OpeningReceive ,	
sum(H.NewReceive) AS NewReceive,

 SUM(H.OpeningOrder)-Sum(H.OpeningReceive) AS OpenBalance,
 SUM(H.NewOrder)- sum(H.NewReceive) AS  NewBalance,
 SUM(H.OpeningOrder)-Sum(H.OpeningReceive)+SUM(H.NewOrder)- sum(H.NewReceive) AS Balance,
 'PeriodicSaleOrderStatus.rdl' AS ReportName, 'Periodic Sale Order Status' AS ReportTitle, NULL AS SubReportProcList,
 (CASE WHEN getdate() > H.DueDate THEN 'Late' 	 		   
			 WHEN @IntervalInt <> 0 AND getdate() + @StartValueInt > H.DueDate  THEN '<' + Convert(NVARCHAR,@StartValueInt) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*1 > H.DueDate  THEN Convert(NVARCHAR,@StartValueInt  ) + '-'+ Convert(NVARCHAR, @StartValueInt + (@IntervalInt*1)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*2 > H.DueDate   THEN Convert(NVARCHAR,@StartValueInt + (@IntervalInt*1)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*3 > H.DueDate    THEN Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*3)) 
			Else '>' + Convert(NVARCHAR,@StartValueInt + @IntervalInt*3) 
			END) AS Ageing,
			 max(CASE WHEN getdate() > H.DueDate THEN '1' 		   
			 WHEN @IntervalInt <> 0 AND getdate() + @StartValueInt > H.DueDate  THEN '2'
			 WHEN getdate() + @StartValueInt + @IntervalInt*1 > H.DueDate  THEN '3'
			 WHEN getdate() + @StartValueInt + @IntervalInt*2 > H.DueDate   THEN '4'
			 WHEN getdate() + @StartValueInt + @IntervalInt*3 > H.DueDate    THEN '5'
			Else '6'
			END) AS AgeingSr
  from @TempTable H
  LEFT JOIN web._People Pp ON Pp.PersonId =H.GroupId
  LEFT JOIN web.Products P ON P.PRoductId = H.GroupId 
 Group By H.GroupId,
 CASE WHEN getdate() > H.DueDate THEN 'Late' 	 		   
			 WHEN @IntervalInt <> 0 AND getdate() + @StartValueInt > H.DueDate  THEN '<' + Convert(NVARCHAR,@StartValueInt) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*1 > H.DueDate  THEN  Convert(NVARCHAR,@StartValueInt  ) + '-'+ Convert(NVARCHAR, @StartValueInt + (@IntervalInt*1)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*2 > H.DueDate   THEN Convert(NVARCHAR,@StartValueInt + (@IntervalInt*1)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)) 
			 WHEN getdate() + @StartValueInt + @IntervalInt*3 > H.DueDate    THEN  Convert(NVARCHAR,@StartValueInt + (@IntervalInt*2)+1 ) + '-' + Convert(NVARCHAR,@StartValueInt + (@IntervalInt*3)) 
			Else '>' + Convert(NVARCHAR,@StartValueInt + @IntervalInt*3) end
			Order By Max(CASE WHEN @ReportType ='Product Wise Summary' THEN P.ProductName  
				ELSE Pp.Name END)*/
			
END
GO

----------------
ALTER FUNCTION [Web].[FSaleOrderBalance]
(
	@StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,
	@SaleOrderHeaderId VARCHAR(Max) = NULL,
	@SaleToByer VARCHAR(Max) = NULL,    
    @Product VARCHAR(Max) = NULL 
)
RETURNS TABLE
AS 

RETURN
	
SELECT
SOH.SiteId AS SiteId,
SOH.DivisionId AS DivisionId,
SOH.SaleOrderHeaderId AS SaleOrderHeaderId,
L.SaleOrderLineId,
SOH.DocDate AS DocDate,  
DT.DocumentTypeShortName+ '-'+SOH.DocNo AS DocNo, 
SOH.DueDate AS Duedate, 
--Isnull(L.Qty,0) AS Qty,
--Isnull(SOCL.CancelQty,0) AS CancelQty,
--Isnull(SDL.DispatchQty,0) AS DispatchQty,
	SOH.SaleToBuyerId AS PersonId , 
	L.ProductId,
Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0)-Isnull(SDL.DispatchQty,0) AS Balance,
(isnull(L.Amount,0)/(CASE WHEN Convert(BIGINT,isnull(L.Qty,0))=0 THEN 1 ELSE Convert(BIGINT,isnull(isnull(L.Qty,0),1)) END)) * isnull(Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0)-Isnull(SDL.DispatchQty,0),0)  AS BalAmt,
(Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0)-Isnull(SDL.DispatchQty,0))*( SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard]  (VRS.StandardSizeArea)) AS BalArea
FROM  Web.Saleorderlines L  WITH (Nolock)
LEFT JOIN Web.SaleorderHeaders SOH WITH (Nolock) ON SOH.SaleOrderHeaderId=L.SaleOrderHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = SOH.DocTypeId
LEFT JOIN 
(
	SELECT L.SaleOrderLineId,Sum(Isnull(L.Qty,0)) AS CancelQty FROM Web._SaleOrderCancelLines L  WITH (NoLock)
	LEFT JOIN Web._SaleOrderCancelHeaders H WITH (NoLock) ON L.SaleOrderCancelHeaderId = H.SaleOrderCancelHeaderId
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
	WHERE 1=1 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)     
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ','))) 
	GROUP BY  L.SaleOrderLineId  
) SOCL ON SOCL.SaleOrderLineId=L.SaleOrderLineId
LEFT JOIN 
(
    SELECT L.SaleOrderLineId, Sum(Isnull(L.Qty,0)) AS AmdQty
	FROM  Web.SaleOrderQtyAmendmentLines L  WITH (NoLock)
	LEFT JOIN Web.SaleOrderAmendmentHeaders  H WITH (NoLock) ON L.SaleOrderAmendmentHeaderId  = H.SaleOrderAmendmentHeaderId
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
	WHERE 1=1 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)     
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ','))) 
	GROUP BY  L.SaleOrderLineId 	
) SOAL ON SOAL.SaleOrderLineId=L.SaleOrderLineId
LEFT JOIN 
(
    SELECT PL.SaleOrderLineId, Sum(Isnull(PL.Qty,0)) AS DispatchQty,max(SDH.DocDate) AS ShipDate
	FROM  Web._SaleDispatchLines SDL WITH (NoLock)
	LEFT JOIN Web._SaleDispatchHeaders SDH WITH (NoLock) ON SDH.SaleDispatchHeaderId   = SDL.SaleDispatchHeaderId 
	LEFT JOIN web.PackingLines PL WITH (NOLOCK) ON PL.PackingLineId = SDL.PackingLineId 
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId =PL.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
	WHERE 1=1 AND PL.SaleOrderLineId IS NOT NULL 
	AND ( @StatusOnDate IS NULL OR SDH.DocDate <= @StatusOnDate)   
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))  
	GROUP BY PL.SaleOrderLineId
) SDL ON SDL.SaleOrderLineId=L.SaleOrderLineId
LEFT JOIN web.ViewRugSize VRS ON VRS.ProductId = L.ProductId
 WHERE 1=1 AND L.Qty IS NOT NULL 
 AND Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0)-Isnull(SDL.DispatchQty,0) >0
 AND ( @FromDate is null or @FromDate <= SOH.DocDate )   
 AND ( @ToDate is null or @ToDate >= SOH.DocDate ) 
 AND ( @Site is null or SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))   
 AND ( @Division is null or SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
 AND ( @DocumentType is null or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ','))) 
 AND ( @SaleToByer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  Web.[Split] (@SaleToByer, ','))) 
 AND ( @Product is null or L.ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ',')))
GO

---------------ALTER FUNCTION [Web].[FSaleOrder]
(
	@StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,
	@SaleOrderHeaderId VARCHAR(Max) = NULL,
	@SaleToByer VARCHAR(Max) = NULL,    
    @Product VARCHAR(Max) = NULL 
)
RETURNS TABLE
AS 

RETURN
	
SELECT
SOH.SiteId AS SiteId,
SOH.DivisionId AS DivisionId,
SOH.SaleOrderHeaderId AS SaleOrderHeaderId,
L.SaleOrderLineId,
SOH.DocDate AS DocDate,  
DT.DocumentTypeShortName+ '-'+SOH.DocNo AS DocNo, 
SOH.DueDate AS Duedate, 
--Isnull(L.Qty,0) AS Qty,
--Isnull(SOCL.CancelQty,0) AS CancelQty,
--Isnull(SDL.DispatchQty,0) AS DispatchQty,
	SOH.SaleToBuyerId AS PersonId , 
	L.ProductId,
Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0) AS NetOrderQty,
(isnull(L.Amount,0)/(CASE WHEN Convert(BIGINT,isnull(L.Qty,0))=0 THEN 1 ELSE Convert(BIGINT,isnull(isnull(L.Qty,0),1)) END)) * isnull(Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0),0)  AS NetOrderAmt,
(Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0))*( SELECT SqYard FROM  Web.[FuncConvertSqFeetToSqYard]  (VRS.StandardSizeArea)) AS NetOrderArea
FROM  Web.Saleorderlines L  WITH (Nolock)
LEFT JOIN Web.SaleorderHeaders SOH WITH (Nolock) ON SOH.SaleOrderHeaderId=L.SaleOrderHeaderId
LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId = SOH.DocTypeId
LEFT JOIN 
(
	SELECT L.SaleOrderLineId,Sum(Isnull(L.Qty,0)) AS CancelQty FROM Web._SaleOrderCancelLines L  WITH (NoLock)
	LEFT JOIN Web._SaleOrderCancelHeaders H WITH (NoLock) ON L.SaleOrderCancelHeaderId = H.SaleOrderCancelHeaderId
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
	WHERE 1=1 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)     
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ','))) 
	GROUP BY  L.SaleOrderLineId  
) SOCL ON SOCL.SaleOrderLineId=L.SaleOrderLineId
LEFT JOIN 
(
    SELECT L.SaleOrderLineId, Sum(Isnull(L.Qty,0)) AS AmdQty
	FROM  Web.SaleOrderQtyAmendmentLines L  WITH (NoLock)
	LEFT JOIN Web.SaleOrderAmendmentHeaders  H WITH (NoLock) ON L.SaleOrderAmendmentHeaderId  = H.SaleOrderAmendmentHeaderId
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
	WHERE 1=1 
	AND ( @StatusOnDate IS NULL OR H.DocDate <= @StatusOnDate)     
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ','))) 
	GROUP BY  L.SaleOrderLineId 	
) SOAL ON SOAL.SaleOrderLineId=L.SaleOrderLineId
LEFT JOIN 
(
    SELECT PL.SaleOrderLineId, Sum(Isnull(PL.Qty,0)) AS DispatchQty,max(SDH.DocDate) AS ShipDate
	FROM  Web._SaleDispatchLines SDL WITH (NoLock)
	LEFT JOIN Web._SaleDispatchHeaders SDH WITH (NoLock) ON SDH.SaleDispatchHeaderId   = SDL.SaleDispatchHeaderId 
	LEFT JOIN web.PackingLines PL WITH (NOLOCK) ON PL.PackingLineId = SDL.PackingLineId 
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId =PL.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId 
	WHERE 1=1 AND PL.SaleOrderLineId IS NOT NULL 
	AND ( @StatusOnDate IS NULL OR SOH.DocDate <= @StatusOnDate)   
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))  
	GROUP BY PL.SaleOrderLineId
) SDL ON SDL.SaleOrderLineId=L.SaleOrderLineId
LEFT JOIN web.ViewRugSize VRS ON VRS.ProductId = L.ProductId
 WHERE 1=1 AND L.Qty IS NOT NULL 
 AND (Isnull(L.Qty,0)- Isnull(SOCL.CancelQty,0)+Isnull(SOAL.AmdQty,0)) >0
 AND ( @FromDate is null or @FromDate <= SOH.DocDate )   
 AND ( @ToDate is null or @ToDate >= SOH.DocDate ) 
 AND ( @Site is null or SOH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))   
 AND ( @Division is null or SOH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))   
 AND ( @DocumentType is null or SOH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ','))) 
 AND ( @SaleToByer is null or SOH.SaleToBuyerId IN (SELECT Items FROM  Web.[Split] (@SaleToByer, ','))) 
 AND ( @Product is null or L.ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ',')))
GO

--------------
ALTER FUNCTION [Web].[FSaleDispatch]
(
	@StatusOnDate VARCHAR(20) = NULL,
    @Site VARCHAR(20) = NULL,
    @Division VARCHAR(20) = NULL,
	@FromDate VARCHAR(20) = NULL,
	@ToDate VARCHAR(20) = NULL,
	@DocumentType VARCHAR(20) = NULL,
	@SaleOrderHeaderId VARCHAR(Max) = NULL,
	@SaleToByer VARCHAR(Max) = NULL,    
    @Product VARCHAR(Max) = NULL 
)
RETURNS TABLE
AS 

RETURN
	
   /*	SELECT 
	SDH.SaleToBuyerId AS PersonId,
	SDH.SiteId AS SiteId,
    SDH.DivisionId AS DivisionId,
	PL.SaleOrderLineId, Isnull(PL.Qty,0) AS DispatchQty,SDH.DocDate AS Dispatchdate,
	PL.ProductId AS ProductId,
	SOH.DueDate AS DueDate,
	SOH.DocDate
	FROM  Web._SaleDispatchLines SDL WITH (NoLock)
	LEFT JOIN Web._SaleDispatchHeaders SDH WITH (NoLock) ON SDH.SaleDispatchHeaderId   = SDL.SaleDispatchHeaderId 
	LEFT JOIN web.PackingLines PL WITH (NOLOCK) ON PL.PackingLineId = SDL.PackingLineId 
	LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId =PL.SaleOrderLineId
	LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId
	WHERE 1=1 AND PL.SaleOrderLineId IS NOT NULL   
	AND ( @StatusOnDate IS NULL OR SDH.DocDate <= @StatusOnDate)   
	AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))         
	AND ( @DocumentType IS NULL OR SDH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))     
	AND ( @Site IS NULL OR SDH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
	AND ( @Division IS NULL OR SDH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ','))) 
	AND ( @FromDate is null or @FromDate <= SDH.DocDate )   
    AND ( @ToDate is null or @ToDate >= SDH.DocDate ) 
    AND ( @SaleToByer IS NULL OR SDH.SaleToBuyerId IN (SELECT Items FROM  Web.[Split] (@SaleToByer, ','))) */
    
    
    SELECT 
    SOH.SaleToBuyerId AS PersonId,
	SDH.SiteId AS SiteId,
    SDH.DivisionId AS DivisionId,
    Isnull(PL.Qty,0) AS DispatchQty,
    SDH.DocDate AS Dispatchdate,
    PL.SaleOrderLineId,
    PL.ProductId AS ProductId,
	SOH.DueDate AS DueDate,
	SOH.DocDate
     FROM 
(SELECT * FROM Web._SaleDispatchHeaders SDH WITH (Nolock)
 WHERE 1=1
 AND ( @StatusOnDate IS NULL OR SDH.DocDate <= @StatusOnDate)
 AND ( @Site IS NULL OR SDH.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
 AND ( @Division IS NULL OR SDH.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ','))) 
 AND ( @FromDate is null or @FromDate <= SDH.DocDate )   
 AND ( @ToDate is null or @ToDate >= SDH.DocDate ) 
 AND ( @DocumentType IS NULL OR SDH.DocTypeId IN (SELECT Items FROM  Web.[Split] (@DocumentType, ',')))
) SDH
LEFT JOIN  Web._SaleDispatchLines SDL WITH (NoLock) ON SDL.SaleDispatchHeaderId=SDH.SaleDispatchHeaderId
LEFT JOIN Web.PackingLines PL WITH (NOLOCK) ON PL.PackingLineId = SDL.PackingLineId
LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId =PL.SaleOrderLineId
LEFT JOIN web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId
WHERE 1=1 AND PL.SaleOrderLineId IS NOT NULL  
 AND ( @Product IS NULL OR PL.ProductId IN (SELECT Items FROM  Web.[Split] (@Product, ','))) 
 AND ( @SaleOrderHeaderId IS NULL OR SOH.SaleOrderHeaderId IN (SELECT Items FROM  Web.[Split] (@SaleOrderHeaderId, ',')))
 AND ( @SaleToByer IS NULL OR SOH.SaleToBuyerId IN (SELECT Items FROM  Web.[Split] (@SaleToByer, ',')))
GO

