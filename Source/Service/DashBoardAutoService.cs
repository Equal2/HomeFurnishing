using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModel;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface IDashBoardAutoService : IDisposable
    {
        IEnumerable<DashBoardSale> GetVehicleSale();
        IEnumerable<DashBoardProfit> GetVehicleProfit();
        IEnumerable<DashBoardOutstanding> GetVehicleOutstanding();
        IEnumerable<DashBoardStock> GetVehicleStock();
        IEnumerable<DashBoardSale> GetSpareSale();
        IEnumerable<DashBoardProfit> GetSpareProfit();
        IEnumerable<DashBoardOutstanding> GetSpareOutstanding();
        IEnumerable<DashBoardStock> GetSpareStock();
        IEnumerable<DashBoardChartData> GetChartData();
        IEnumerable<DashBoardPieChartData> GetPieChartData();
        IEnumerable<DashBoardVehicleSaleChartData> GetVehicleSaleChartData();
    }

    public class DashBoardAutoService : IDashBoardAutoService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        string mQry = "";
        public DashBoardAutoService()
        {
        }

        public IEnumerable<DashBoardSale> GetVehicleSale()
        {


            mQry = @"DECLARE @Month INT 
                    DECLARE @Year INT
                    SELECT @Month =  Datepart(MONTH,getdate())
                    SELECT @Year =  Datepart(YEAR,getdate())
                    DECLARE @FromDate DATETIME
                    DECLARE @ToDate DATETIME
                    SELECT @FromDate = DATEADD(month,@Month-1,DATEADD(year,@Year-1900,0)), @ToDate = DATEADD(day,-1,DATEADD(month,@Month,DATEADD(year,@Year-1900,0))) 


                    SELECT Convert(NVARCHAR,Convert(DECIMAL(18,2),Round(IsNull(Sum(Hc.Amount),0)/10000000,2))) + ' Crore' AS SaleAmount
                    FROM Web.SaleInvoiceHeaders H 
                    LEFT JOIN Web.SaleInvoiceHeaderCharges Hc ON H.SaleInvoiceHeaderId = Hc.HeaderTableId
                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
                    WHERE C.ChargeName = 'Net Amount'
                    AND  H.DocDate BETWEEN @FromDate AND @ToDate
                    AND D.DocumentCategoryId = 464 ";

            IEnumerable<DashBoardSale> VehicleSale = db.Database.SqlQuery<DashBoardSale>(mQry).ToList();
            return VehicleSale;
        }
        public IEnumerable<DashBoardProfit> GetVehicleProfit()
        {
            mQry = @"DECLARE @Month INT 
                    DECLARE @Year INT
                    SELECT @Month =  Datepart(MONTH,getdate())
                    SELECT @Year =  Datepart(YEAR,getdate())
                    DECLARE @FromDate DATETIME
                    DECLARE @ToDate DATETIME
                    SELECT @FromDate = DATEADD(month,@Month-1,DATEADD(year,@Year-1900,0)), @ToDate = DATEADD(day,-1,DATEADD(month,@Month,DATEADD(year,@Year-1900,0))) 

                    SELECT Convert(NVARCHAR,Convert(DECIMAL(18,2),Round((IsNull(Sum(VSale.SaleAmount),0) - IsNull(Sum(VPurchase.PurchaseAmount),0))/10000000,2))) + ' Crore' AS ProfitAmount
                    FROM (
	                    SELECT VProductUid.ProductUidId, Sum(Hc.Amount) AS SaleAmount
	                    FROM Web.SaleInvoiceHeaders H 
	                    LEFT JOIN Web.SaleInvoiceHeaderCharges Hc ON H.SaleInvoiceHeaderId = Hc.HeaderTableId
	                    LEFT JOIN (
		                    SELECT Max(Pl.ProductUidId) AS ProductUidId, L.SaleInvoiceHeaderId
		                    FROM Web.SaleInvoiceLines L 
		                    LEFT JOIN Web.SaleDispatchLines Sdl ON L.SaleDispatchLineId = Sdl.SaleDispatchLineId
		                    LEFT JOIN Web.PackingLines Pl ON Sdl.PackingLineId = Pl.PackingLineId
		                    WHERE Pl.ProductUidId IS NOT NULL
		                    GROUP BY L.SaleInvoiceHeaderId
	                    ) AS VProductUid ON H.SaleInvoiceHeaderId = VProductUid.SaleInvoiceHeaderId
	                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
	                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
	                    WHERE C.ChargeName = 'Net Amount'
	                    AND  H.DocDate BETWEEN @FromDate AND @ToDate
	                    AND D.DocumentCategoryId = 464
	                    GROUP BY VProductUid.ProductUidId
                    ) AS VSale
                    LEFT JOIN (
	                    SELECT VProductUid.ProductUidId, Sum(Hc.Amount) AS PurchaseAmount
	                    FROM Web.JobInvoiceHeaders H 
	                    LEFT JOIN Web.JobInvoiceHeaderCharges Hc ON H.JobInvoiceHeaderId = Hc.HeaderTableId
	                    LEFT JOIN (
		                    SELECT Max(Jrl.ProductUidId) AS ProductUidId, L.JobInvoiceHeaderId
		                    FROM Web.JobInvoiceLines L 
		                    LEFT JOIN Web.JobReceiveLines Jrl ON L.JobReceiveLineId = Jrl.JobReceiveLineId
		                    WHERE Jrl.ProductUidId IS NOT NULL
		                    GROUP BY L.JobInvoiceHeaderId
	                    ) AS VProductUid ON H.JobInvoiceHeaderId = VProductUid.JobInvoiceHeaderId
	                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
	                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
	                    WHERE C.ChargeName = 'Net Amount'
	                    AND D.DocumentCategoryId = 461
	                    GROUP BY VProductUid.ProductUidId
                    ) AS VPurchase ON VSale.ProductUidId = VPurchase.ProductUidId ";


            IEnumerable<DashBoardProfit> VehicleProfit = db.Database.SqlQuery<DashBoardProfit>(mQry).ToList();
            return VehicleProfit;
        }
        public IEnumerable<DashBoardOutstanding> GetVehicleOutstanding()
        {
            mQry = @"WITH cteLedgerAccountGroups AS
	                    (
		                    SELECT lag.LedgerAccountGroupId, Lag.LedgerAccountGroupName, Lag.ParentLedgerAccountGroupId, 0  Level       
		                    FROM Web.LedgerAccountGroups Lag 
		                    WHERE Lag.LedgerAccountGroupId =1026
		                    UNION ALL
		                    SELECT lag.LedgerAccountGroupId, Lag.LedgerAccountGroupName, Lag.ParentLedgerAccountGroupId, LEVEL + 1 
		                    FROM Web.LedgerAccountGroups Lag
		                    INNER JOIN cteLedgerAccountGroups cte ON lag.ParentLedgerAccountGroupId = cte.LedgerAccountGroupId
	                    ), 	
                    cteBills AS
	                    (	
		                    SELECT LH.LedgerHeaderId, LH.DocTypeId , Max(LH.SiteId) AS SiteId, Max(LH.DivisionId) as DivisionId, Max(LH.DocDate) AS DocDate, Max(LH.DocNo) AS DocNo, SIH.SaleInvoiceHeaderId, La.LedgerAccountId,  Sum(led.AmtDr) AS BillAmt
		                    FROM web.LedgerHeaders LH
		                    LEFT JOIN web.Ledgers Led ON LH.LedgerHeaderId = Led.LedgerHeaderId 
		                    LEFT JOIN web.LedgerAccounts La ON Led.LedgerAccountId = La.LedgerAccountId 
		                    Left JOIN web.SaleInvoiceHeaders SIH ON SIH.SaleInvoiceHeaderId  = LH.DocHeaderId 
		                    Where IsNull(Led.AmtDr ,0)>0 --AND La.LedgerAccountId =20187
	 	                    GROUP BY LH.LedgerHeaderId, LH.DocTypeId, La.LedgerAccountId,SIH.SaleInvoiceHeaderId	 	 	
	                    ) , 
                    cteBillAdj AS
	                    (	
		                    SELECT  La.LedgerAccountId, LH.DocTypeId,  Sum(led.AmtCr) AS AdjAmt
		                    FROM web.LedgerHeaders LH
		                    LEFT JOIN web.Ledgers Led ON LH.LedgerHeaderId  = Led.LedgerHeaderId 
		                    LEFT JOIN 	web.LedgerAccounts La ON La.LedgerAccountId = Led.LedgerAccountId 
		                    Where IsNull(Led.AmtCr,0)>0  --AND La.LedgerAccountId  =20187
	 	                    GROUP BY La.LedgerAccountId, Lh.DocTypeId  
	                    ), 
	

                    cteResultAfterAdj AS
                    (
                    SELECT p.SaleInvoiceHeaderId, p.BillRowNumber,p.LedgerHeaderId, p.DocTypeId AS InvoiceDocTypeId , P.DocDate AS InvoiceDate, P.DocNo AS InvoiceNo, P.siteId, P.DivisionId, P.LedgerAccountId AS BillToPartyId,  P.BillAmt, s.DocTypeId AS AdjDocTypeId,
                    CASE WHEN IsNull(p.Sum_BillAmt,0) - IsNull(s.Sum_AdjAmt,0) + IsNull(s.AdjAmt,0) < IsNull(s.Sum_AdjAmt,0) -IsNull(p.Sum_BillAmt,0) + IsNull(p.BillAmt,0)  
                    THEN CASE WHEN IsNull(p.BillAmt,0) <  IsNull(p.Sum_BillAmt,0)  - IsNull(s.Sum_AdjAmt,0)  + IsNull(s.AdjAmt,0)   
                            THEN IsNull(p.BillAmt,0)   
                            ELSE CASE WHEN IsNull(s.AdjAmt,0) < IsNull(p.Sum_BillAmt,0)  - IsNull(s.Sum_AdjAmt,0)  + IsNull(s.AdjAmt,0)  
                                      THEN IsNull(s.AdjAmt,0) ELSE IsNull(p.Sum_BillAmt,0)  - IsNull(s.Sum_AdjAmt,0)  + IsNull(s.AdjAmt,0) END END  
                    ELSE  
                    CASE WHEN IsNull(p.BillAmt,0) <  IsNull(s.Sum_AdjAmt,0)  - IsNull(p.Sum_BillAmt,0)  + IsNull(p.BillAmt,0)  
                    THEN IsNull(p.BillAmt,0) ELSE   
                       CASE WHEN IsNull(s.AdjAmt,0) < IsNull(s.Sum_AdjAmt,0)  - IsNull(p.Sum_BillAmt,0)  + IsNull(p.BillAmt,0)  
                            THEN IsNull(s.AdjAmt,0)  WHEN IsNull(s.Sum_AdjAmt,0)=0 THEN 0 ELSE IsNull(s.Sum_AdjAmt,0)  - IsNull(p.Sum_BillAmt,0)  + IsNull(p.BillAmt,0) END END END AS AdjAmt
                    FROM (  
	
	                    SELECT Row_Number() Over (Order By cteBills.LedgerHeaderId) as BillRowNumber, cteBills.*,  
	                    SUM(BillAmt) OVER(PARTITION BY LedgerAccountId ORDER BY LedgerAccountId, DocDate, DocTypeId, DocNo, SaleInvoiceHeaderId) sum_BillAmt   
	                    FROM cteBills



                    ) As p   
                    LEFT JOIN (  
  	                    SELECT cteBillAdj.*, sum(cteBillAdj.AdjAmt) OVER ( PARTITION BY LedgerAccountId ORDER BY DocTypeId) sum_AdjAmt   	
	                    FROM cteBillAdj
	
                    ) AS s  ON  s.LedgerAccountId   = p.LedgerAccountId   
                    AND IsNull(p.sum_BillAmt,0)  > IsNull(s.sum_AdjAmt,0)  - IsNull(s.AdjAmt,0)    
                    AND IsNull(s.sum_AdjAmt,0)  > IsNull(p.sum_BillAmt,0)  - IsNull(p.BillAmt,0)    
                    ),  


                    cteFinalResult as 
                    (SELECT
                    Max(Dt.DocumentTypeName) AS DocType, Convert(Varchar,Max(R.InvoiceDate),106) AS InvoiceDate, Max(R.InvoiceNo) as InvoiceNo, Max(PG.ProductGroupName) AS Model, Max(IsNull(Cust.Name + ' ' + Cust.Suffix,LA.LedgerAccountName)) LedgerAccountName, 
                    Max(UID.ProductUidName) AS Chassis, Max(Site.SiteName) as SiteName, Max(SalesExe.Name) AS SalesExecutiveName, Max(Fin.Name) AS FinancierName, 
                    datediff(day,Max(SIH.DocDate), getdate()) AS Ageing, Max(SIHA.Value) AS FinanceAmount, Max(R.BillAmt) AS InvoiceAmount, 
                    Sum(R.AdjAmt) AS ReceiveAmount, Max(R.BillAmt)- IsNull(Sum(R.AdjAmt),0) AS BalanceAmount,  Max(R.BillToPartyId) as BillToPartyId		
                    FROM cteResultAfterAdj r
	                    LEFT JOIN Web.LedgerAccounts LA ON r.BillToPartyId = LA.LedgerAccountId 
	                    Left Join Web.People Cust On LA.PersonId = Cust.PersonId
	                    LEFT JOIN web.DocumentTypes Dt ON R.InvoiceDocTypeId = Dt.DocumentTypeId 
	                    LEFT JOIN web.SaleInvoiceHeaders SIH ON r.SaleInvoiceHeaderId = SIH.SaleInvoiceHeaderId 
	                    LEFT JOIN (
				                    SELECT L.SaleInvoiceHeaderId, Max(PL.ProductId) AS ProductId, Max(PL.ProductUidId) ProductUidId
				                    FROM Web.SaleInvoiceHeaders H
				                    LEFT Join Web.SaleInvoiceLines L ON H.SaleInvoiceHeaderId = L.SaleInvoiceHeaderId 
				                    LEFT JOIN web.SaleDispatchLines DL ON L.SaleDispatchLineId = DL.SaleDispatchLineId 
				                    LEFT JOIN web.PackingLines PL ON DL.PackingLineId = PL.PackingLineId 			
				                    WHERE PL.ProductUidId IS NOT NULL 
				                    GROUP BY L.SaleInvoiceHeaderId 
				                    ) AS SIL ON SIH.SaleInvoiceHeaderId = SIL.SaleInvoiceHeaderId
	                    LEFT JOIN web.Products P ON SIL.ProductId = P.ProductId
	                    LEFT JOIN web.ProductGroups PG ON P.ProductGroupId = PG.ProductGroupId 
	                    LEFT JOIN web.ProductUids UID ON SIL.ProductUidId = UID.ProductUIDId 
	                    LEFT JOIN web.Sites Site ON R.SiteId = Site.SiteId 
	                    LEFT JOIN web.People Customer ON SIH.SaleToBuyerId = Customer.PersonID 
	                    LEFT JOIN web.People SalesExe ON SIH.SalesExecutiveId = SalesExe.PersonID 
	                    LEFT JOIN Web.People Fin ON SIH.FinancierId = Fin.PersonID 
	                    LEFT JOIN web.SaleInvoiceHeaderAttributes SIHA ON SIHA.HeaderTableId = SIH.SaleInvoiceHeaderId AND SIHA.DocumentTypeHeaderAttributeId =4
	                    LEFT JOIN (
				                    SELECT H.SaleInvoiceHeaderId, HC.Amount AS NetAmount   
				                    FROM Web.SaleInvoiceHeaders H
				                    LEFT JOIN web.SaleInvoiceHeaderCharges HC ON H.SaleInvoiceHeaderId = HC.HeaderTableId 
				                    LEFT JOIN Web.Charges C ON HC.ChargeId = C.ChargeId 
				                    WHERE H.DocTypeId =634 And C.ChargeName ='Net Amount'
				                    ) AS SIHC ON SIH.SaleInvoiceHeaderId = SIHC.SaleInvoiceHeaderId
	                    LEFT JOIN cteLedgerAccountGroups Lag ON La.LedgerAccountGroupId=Lag.LedgerAccountGroupId
	                    WHERE lag.LedgerAccountGroupId IS NOT NULL 
                    Group By R.BillRowNumber
                    )
                    select Convert(NVARCHAR,Convert(DECIMAL(18,2),Round(Sum(H.BalanceAmount)/10000000,2))) + ' Crore'   as OutstandingAmount
                    from cteFinalResult H  ";


            IEnumerable<DashBoardOutstanding> VehicleOutstanding = db.Database.SqlQuery<DashBoardOutstanding>(mQry).ToList();
            return VehicleOutstanding;
        }
        public IEnumerable<DashBoardStock> GetVehicleStock()
        {
            mQry = @"SELECT Convert(NVARCHAR,Convert(DECIMAL(18,0),IsNull(Sum(VStock.StockQty),0))) AS StockQty, 
                        Convert(NVARCHAR,Convert(DECIMAL(18,2),Round(IsNull(Sum(VStock.StockAmount),0)/10000000,2))) + ' Crore'   AS StockAmount
                        FROM (
	                        SELECT L.Qty AS StockQty,
	                        (SELECT hC.Amount FROM Web.JobInvoiceHeaderCharges hc WITH (Nolock) 
			                        LEFT JOIN Web.Charges cc ON cc.ChargeId = hc.ChargeId 
			                        WHERE cc.ChargeName ='Net Amount' 
			                        AND HC.HeaderTableId = H.JobInvoiceHeaderId ) StockAmount
	                        FROM Web.JobInvoiceHeaders  H WITH (Nolock)
	                        LEFT JOIN web.Sites S  WITH (NoLock) ON S.SiteId = H.SiteId 
	                        LEFT JOIN web.Divisions D  WITH (NoLock) ON D.DivisionId = H.DivisionId 
	                        LEFT JOIN web.Processes PR WITH (NoLock) ON PR.ProcessId = H.ProcessId 
	                        LEFT JOIN web.JobInvoiceLines L WITH (NoLock) ON L.JobInvoiceHeaderId = H.JobInvoiceHeaderId 
	                        LEFT JOIN web.JobReceiveLines R WITH (NoLock) ON R.JobReceiveLineId = L.JobReceiveLineId 
	                        LEFT JOIN web.JobOrderLines JOL WITH (NoLock) ON JOL.JobOrderLineId = R.JobOrderLineId 
	                        LEFT JOIN web.Products P WITH (NoLock) ON P.ProductId = JOL.ProductId 
	                        LEFT JOIN web.ProductGroups PG WITH (NoLock) ON PG.ProductGroupId = P.ProductGroupId 
	                        LEFT JOIN web.ProductTypes PT WITH (NoLock) ON PT.ProductTypeId  = PG.ProductTypeId 
	                        LEFT JOIN web.ProductCategories PC WITH (NoLock) ON PC.ProductCategoryId = P.ProductCategoryId 
	                        LEFT JOIN web.ProductNatures PN WITH (NoLock) ON PN.ProductNatureId = PT.ProductNatureId 
	                        LEFT JOIN 
	                        (
		                        SELECT PL.ProductUidId, Max(PH.DocDate) AS PackingDate, Max(PL.PackingLineId) AS  PackingLineId
		                        FROM web.PackingLines PL WITH (NoLock)
		                        LEFT JOIN web.PackingHeaders PH WITH (NoLock) ON Pl.PackingHeaderId = Ph.PackingHeaderId 
		                        LEFT JOIN web.SaleDispatchLines SDL  WITH (NoLock) ON SDL.PackingLineId = PL.PackingLineId 
		                        LEFT JOIN web.SaleDispatchReturnLines SDRL  WITH (NoLock) ON SDRL.SaleDispatchLineId= SDL.SaleDispatchLineId 
		                        WHERE SDRL.SaleDispatchReturnLineId IS NULL 
		                        GROUP BY PL.ProductUidId 
		                        ) AS PL ON PL.ProductUidId = R.ProductUidId
	                        WHERE PR.ProcessName ='Purchase' AND PL.PackingDate IS NULL 
	                        AND JOL.ProductId IS NOT NULL AND PN.ProductNatureName ='LOB' 
                        ) AS VStock ";


            IEnumerable<DashBoardStock> VehicleStock = db.Database.SqlQuery<DashBoardStock>(mQry).ToList();
            return VehicleStock;
        }




        public IEnumerable<DashBoardSale> GetSpareSale()
        {
            mQry = @"SELECT Convert(NVARCHAR,Convert(DECIMAL(18,2),Round(IsNull(Sum(Hc.Amount),0)/10000000,2))) + ' Crore' AS SaleAmount
                    FROM Web.SaleInvoiceHeaders H 
                    LEFT JOIN Web.SaleInvoiceHeaderCharges Hc ON H.SaleInvoiceHeaderId = Hc.HeaderTableId
                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
                    WHERE C.ChargeName = 'Net Amount'
                    AND  H.DocDate BETWEEN '01/Oct/2017' AND '13/Oct/2017'
                    AND D.DocumentCategoryId IN (244,4012) ";

            IEnumerable<DashBoardSale> SpareSale = db.Database.SqlQuery<DashBoardSale>(mQry).ToList();
            return SpareSale;
        }
        public IEnumerable<DashBoardProfit> GetSpareProfit()
        {
            mQry = @"SELECT Convert(NVARCHAR,Convert(DECIMAL(18,2),Round((IsNull(VSale.SaleAmount,0) - IsNull(VPurchase.PurchaseAmount,0))/10000000,2))) + ' Crore' AS ProfitAmount
                    FROM (
	                    SELECT Sum(Hc.Amount) AS SaleAmount
	                    FROM Web.SaleInvoiceHeaders H 
	                    LEFT JOIN Web.SaleInvoiceHeaderCharges Hc ON H.SaleInvoiceHeaderId = Hc.HeaderTableId
	                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
	                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
	                    WHERE C.ChargeName = 'Net Amount'
	                    AND  H.DocDate BETWEEN '01/Apr/2017' AND '11/Oct/2017'
	                    AND D.DocumentCategoryId = 464
                    ) AS VSale
                    LEFT JOIN (
	                    SELECT Sum(Hc.Amount) AS PurchaseAmount
	                    FROM Web.JobInvoiceHeaders H 
	                    LEFT JOIN Web.JobInvoiceHeaderCharges Hc ON H.JobInvoiceHeaderId = Hc.HeaderTableId
	                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
	                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
	                    WHERE C.ChargeName = 'Net Amount'
	                    AND  H.DocDate BETWEEN '01/Apr/2017' AND '11/Oct/2017'
	                    AND D.DocumentCategoryId = 461
                    ) AS VPurchase ON 1=1 ";


            IEnumerable<DashBoardProfit> SpareProfit = db.Database.SqlQuery<DashBoardProfit>(mQry).ToList();
            return SpareProfit;
        }
        public IEnumerable<DashBoardOutstanding> GetSpareOutstanding()
        {
            mQry = @"WITH cteLedgerAccountGroups AS
	                (
		                SELECT lag.LedgerAccountGroupId, Lag.LedgerAccountGroupName, Lag.ParentLedgerAccountGroupId, 0  Level       
		                FROM Web.LedgerAccountGroups Lag 
		                WHERE Lag.LedgerAccountGroupId =1026
		                UNION ALL
		                SELECT lag.LedgerAccountGroupId, Lag.LedgerAccountGroupName, Lag.ParentLedgerAccountGroupId, LEVEL + 1 
		                FROM Web.LedgerAccountGroups Lag
		                INNER JOIN cteLedgerAccountGroups cte ON lag.ParentLedgerAccountGroupId = cte.LedgerAccountGroupId
	                )
                SELECT Convert(NVARCHAR,Convert(DECIMAL(18,2),Round((IsNull(Sum(L.AmtDr),0) - IsNull(Sum(L.AmtCr),0))/10000000,2))) + ' Crore'  AS OutstandingAmount
                FROM cteLedgerAccountGroups Ag
                LEFT JOIN Web.LedgerAccounts A ON Ag.ledgerAccountGroupId = A.LedgerAccountGroupId
                LEFT JOIN Web.Ledgers L ON A.LedgerAccountId = L.LedgerAccountId ";


            IEnumerable<DashBoardOutstanding> SpareOutstanding = db.Database.SqlQuery<DashBoardOutstanding>(mQry).ToList();
            return SpareOutstanding;
        }
        public IEnumerable<DashBoardStock> GetSpareStock()
        {
            mQry = @"SELECT Convert(NVARCHAR,Convert(DECIMAL(18,0),IsNull(Sum(VStock.StockQty),0))) AS StockQty, 
                        Convert(NVARCHAR,Convert(DECIMAL(18,2),Round(IsNull(Sum(VStock.StockAmount),0)/10000000,2))) + ' Crore'   AS StockAmount
                        FROM (
	                        SELECT L.Qty AS StockQty,
	                        (SELECT hC.Amount FROM Web.JobInvoiceHeaderCharges hc WITH (Nolock) 
			                        LEFT JOIN Web.Charges cc ON cc.ChargeId = hc.ChargeId 
			                        WHERE cc.ChargeName ='Net Amount' 
			                        AND HC.HeaderTableId = H.JobInvoiceHeaderId ) StockAmount
	                        FROM Web.JobInvoiceHeaders  H WITH (Nolock)
	                        LEFT JOIN web.Sites S  WITH (NoLock) ON S.SiteId = H.SiteId 
	                        LEFT JOIN web.Divisions D  WITH (NoLock) ON D.DivisionId = H.DivisionId 
	                        LEFT JOIN web.Processes PR WITH (NoLock) ON PR.ProcessId = H.ProcessId 
	                        LEFT JOIN web.JobInvoiceLines L WITH (NoLock) ON L.JobInvoiceHeaderId = H.JobInvoiceHeaderId 
	                        LEFT JOIN web.JobReceiveLines R WITH (NoLock) ON R.JobReceiveLineId = L.JobReceiveLineId 
	                        LEFT JOIN web.JobOrderLines JOL WITH (NoLock) ON JOL.JobOrderLineId = R.JobOrderLineId 
	                        LEFT JOIN web.Products P WITH (NoLock) ON P.ProductId = JOL.ProductId 
	                        LEFT JOIN web.ProductGroups PG WITH (NoLock) ON PG.ProductGroupId = P.ProductGroupId 
	                        LEFT JOIN web.ProductTypes PT WITH (NoLock) ON PT.ProductTypeId  = PG.ProductTypeId 
	                        LEFT JOIN web.ProductCategories PC WITH (NoLock) ON PC.ProductCategoryId = P.ProductCategoryId 
	                        LEFT JOIN web.ProductNatures PN WITH (NoLock) ON PN.ProductNatureId = PT.ProductNatureId 
	                        LEFT JOIN 
	                        (
		                        SELECT PL.ProductUidId, Max(PH.DocDate) AS PackingDate, Max(PL.PackingLineId) AS  PackingLineId
		                        FROM web.PackingLines PL WITH (NoLock)
		                        LEFT JOIN web.PackingHeaders PH WITH (NoLock) ON Pl.PackingHeaderId = Ph.PackingHeaderId 
		                        LEFT JOIN web.SaleDispatchLines SDL  WITH (NoLock) ON SDL.PackingLineId = PL.PackingLineId 
		                        LEFT JOIN web.SaleDispatchReturnLines SDRL  WITH (NoLock) ON SDRL.SaleDispatchLineId= SDL.SaleDispatchLineId 
		                        WHERE SDRL.SaleDispatchReturnLineId IS NULL 
		                        GROUP BY PL.ProductUidId 
		                        ) AS PL ON PL.ProductUidId = R.ProductUidId
	                        WHERE PR.ProcessName ='Purchase' AND PL.PackingDate IS NULL 
	                        AND JOL.ProductId IS NOT NULL AND PN.ProductNatureName ='LOB' 
                        ) AS VStock ";


            IEnumerable<DashBoardStock> SpareStock = db.Database.SqlQuery<DashBoardStock>(mQry).ToList();
            return SpareStock;
        }


        public IEnumerable<DashBoardChartData> GetChartData()
        {
            mQry = @"SELECT DATENAME(month, H.DocDate) AS Month, 
            Sum(CASE WHEN Ag.LedgerAccountGroupNature = 'Income' THEN L.AmtDr ELSE 0 END) AS Income, 
            Sum(CASE WHEN Ag.LedgerAccountGroupNature = 'Expense' THEN L.AmtDr ELSE 0 END) AS Expense
            FROM Web.Ledgers L
            LEFT JOIN Web.LedgerHeaders H ON L.LedgerHeaderId = H.LedgerHeaderId 
            LEFT JOIN Web.LedgerAccounts A ON L.LedgerAccountId = A.LedgerAccountId
            LEFT JOIN Web.LedgerAccountGroups Ag ON A.LedgerAccountGroupId = Ag.LedgerAccountGroupId
            GROUP BY DATENAME(month, H.DocDate)
            ORDER BY DatePart(month,Max(H.DocDate)) ";

            IEnumerable<DashBoardChartData> ChartData = db.Database.SqlQuery<DashBoardChartData>(mQry).ToList();
            return ChartData;
        }


        public IEnumerable<DashBoardPieChartData> GetPieChartData()
        {
            mQry = @"DECLARE @Month INT 
                    DECLARE @Year INT
                    SELECT @Month =  Datepart(MONTH,getdate())
                    SELECT @Year =  Datepart(YEAR,getdate())
                    DECLARE @FromDate DATETIME
                    DECLARE @ToDate DATETIME
                    SELECT @FromDate = DATEADD(month,@Month-1,DATEADD(year,@Year-1900,0)), @ToDate = DATEADD(day,-1,DATEADD(month,@Month,DATEADD(year,@Year-1900,0))) 

                    SELECT S.SiteName As label, Sum(Hc.Amount) AS value,
                    CASE WHEN row_number() OVER (ORDER BY S.SiteName) = 1 THEN '#f56954'
	                     WHEN row_number() OVER (ORDER BY S.SiteName) = 2 THEN '#00a65a'
	                     WHEN row_number() OVER (ORDER BY S.SiteName) = 3 THEN '#f39c12'
	                     WHEN row_number() OVER (ORDER BY S.SiteName) = 4 THEN '#00c0ef'
	                     WHEN row_number() OVER (ORDER BY S.SiteName) = 5 THEN '#3c8dbc'
	                     WHEN row_number() OVER (ORDER BY S.SiteName) = 6 THEN '#d2d6de'
	                     ELSE '#f56954'
                    END AS color 
                    FROM Web.SaleInvoiceHeaders H 
                    LEFT JOIN Web.SaleInvoiceHeaderCharges Hc ON H.SaleInvoiceHeaderId = Hc.HeaderTableId
                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
                    LEFT JOIN Web.Sites S ON h.SiteId = S.SiteId
                    WHERE C.ChargeName = 'Net Amount'
                    AND  H.DocDate BETWEEN @FromDate AND @ToDate
                    AND D.DocumentCategoryId = 4012
                    GROUP BY S.SiteName ";

            IEnumerable<DashBoardPieChartData> PieChartData = db.Database.SqlQuery<DashBoardPieChartData>(mQry).ToList();
            return PieChartData;
        }


        public IEnumerable<DashBoardVehicleSaleChartData> GetVehicleSaleChartData()
        {
            mQry = @"SELECT DATENAME(month, H.DocDate) AS Month, 
                    Round(Sum(Hc.Amount)/10000000,2) AS Amount
                    FROM Web.SaleInvoiceHeaders H 
                    LEFT JOIN Web.SaleInvoiceHeaderCharges Hc ON H.SaleInvoiceHeaderId = Hc.HeaderTableId
                    LEFT JOIN Web.DocumentTypes D ON H.DocTypeId = D.DocumentTypeId
                    LEFT JOIN Web.Charges C ON Hc.ChargeId = C.ChargeId
                    WHERE C.ChargeName = 'Net Amount'
                    AND D.DocumentCategoryId = 464
                    GROUP BY DATENAME(month, H.DocDate)
                    ORDER BY DatePart(month,Max(H.DocDate)) ";

            IEnumerable<DashBoardVehicleSaleChartData> ChartData = db.Database.SqlQuery<DashBoardVehicleSaleChartData>(mQry).ToList();
            return ChartData;
        }


        

        public void Dispose()
        {
        }
    }
    public class DashBoardSale
    {
        public string SaleAmount { get; set; }
    }
    public class DashBoardProfit
    {
        public string ProfitAmount { get; set; }
    }
    public class DashBoardOutstanding
    {
        public string OutstandingAmount { get; set; }
    }
    public class DashBoardStock
    {
        public string StockQty { get; set; }
        public string StockAmount { get; set; }
    }

    public class DashBoardChartData
    {
        public string Month { get; set; }
        public Decimal Income { get; set; }
        public Decimal Expense { get; set; }
    }


    public class DashBoardPieChartData
    {
        public string label { get; set; }
        public Decimal value { get; set; }
        public string color { get; set; }
        public string highlight { get; set; }
    }

    public class DashBoardVehicleSaleChartData
    {
        public string Month { get; set; }
        public Decimal Amount { get; set; }
    }

}
