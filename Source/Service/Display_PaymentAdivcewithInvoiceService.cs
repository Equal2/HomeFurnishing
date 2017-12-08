using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure;
using Model.ViewModel;
using System;
using Data.Models;
using System.Data.SqlClient;
using Model.ViewModels;

namespace Service
{
    public interface IDisplay_PaymentAdivceWithInvoiceService : IDisposable
    {
        IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter);
    
        IEnumerable<PaymentAdivceViewModel> FinishingPaymentAdvice(DisplayFilterPaymentAdviceSettings Settings);
    }

    public class Display_PaymentAdivceWithInvoiceService : IDisplay_PaymentAdivceWithInvoiceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

      

        public Display_PaymentAdivceWithInvoiceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }

        public IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = ReportFormatPaymentAdvice.JobWorkerWise, text = ReportFormatPaymentAdvice.JobWorkerWise });          
            ResultList.Add(new ComboBoxResult { id = ReportFormatPaymentAdvice.ProcessWise, text = ReportFormatPaymentAdvice.ProcessWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormatPaymentAdvice.OrderNoWise, text = ReportFormatPaymentAdvice.OrderNoWise });

            var list = (from D in ResultList
                        where (string.IsNullOrEmpty(term) ? 1 == 1 : (D.text.ToLower().Contains(term.ToLower())))
                        orderby D.text
                        select new ComboBoxResult
                        {
                            id = D.id,
                            text = D.text
                        }
             );
            return list.AsQueryable();
        }


     
      
        
        public IEnumerable<PaymentAdivceViewModel> FinishingPaymentAdvice(DisplayFilterPaymentAdviceSettings Settings)
        {
            var FormatSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var SiteSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();            
            var JobWorkerSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "JobWorker" select H).FirstOrDefault();
            var ProcessSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in Settings.DisplayFilterPaymentAdviceParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();


            



            string Format = FormatSetting.Value;
            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;          
            string JobWorker = JobWorkerSetting.Value;
            string Process = ProcessSetting.Value;
            string TextHidden = TextHiddenSetting.Value;

            //if (Process == "53")
            //{
            //    int? ProcessIds = Convert.ToInt32(Process);
            //    Process = string.Join(",", db.Process.Where(p => p.ParentProcessId == ProcessIds).Select(p => p.ProcessId.ToString()));
            //}

            string mQry, mCondStr,jobInvCondstr="", jobInvRetCondstr = "",Ledger="",LedgerCondHeader="",HavingCondition="",CteProcess="";


            mCondStr = "";
            if (!string.IsNullOrEmpty(SiteId)) mCondStr += " AND H.SiteId = @Site ";
            if (!string.IsNullOrEmpty(DivisionId)) mCondStr += " AND H.DivisionId = @Division ";
            if (!string.IsNullOrEmpty(FromDate)) mCondStr += " AND H.DocDate >= @FromDate ";
            if (!string.IsNullOrEmpty(ToDate)) mCondStr += " AND H.DocDate <= @ToDate "; 
            if (!string.IsNullOrEmpty(Process)) mCondStr += "  AND H.ProcessId IN (SELECT ProcessId FROM cteProcess) ";
            if (!string.IsNullOrEmpty(JobWorker)) jobInvCondstr += " AND L.JobWorkerId = @JobWorker ";
            if (!string.IsNullOrEmpty(JobWorker)) jobInvRetCondstr += " AND H.JobWorkerId = @JobWorker ";
            if (!string.IsNullOrEmpty(JobWorker)) Ledger += " AND LA.PersonId = @JobWorker ";

            if (!string.IsNullOrEmpty(SiteId)) LedgerCondHeader += " AND H.SiteId = @Site ";
            if (!string.IsNullOrEmpty(DivisionId)) LedgerCondHeader += " AND H.DivisionId = @Division ";
            if (!string.IsNullOrEmpty(FromDate)) LedgerCondHeader += " AND H.PaymentFor >= @FromDate ";
            if (!string.IsNullOrEmpty(ToDate)) LedgerCondHeader += " AND H.PaymentFor <= @ToDate ";
            if (!string.IsNullOrEmpty(Process)) LedgerCondHeader += "  AND H.ProcessId IN (SELECT ProcessId FROM cteProcess) ";

            if (!string.IsNullOrEmpty(Process)) CteProcess += "  AND p.ProcessId=@Process ";

            
            if (TextHidden== "DebitNote")
            {
                HavingCondition = " HAVING sum(H.DebitNote) > 0";
            }
            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);            
            SqlParameter SqlParameterJobWorker = new SqlParameter("@JobWorker", !string.IsNullOrEmpty(JobWorker) ? JobWorker : (object)DBNull.Value);
            SqlParameter SqlParameterProcess = new SqlParameter("@Process", !string.IsNullOrEmpty(Process) ? Process : (object)DBNull.Value);





            mQry = @"
                          WITH cteProcess AS
                            (
	                            SELECT P.ProcessId, p.ProcessName, 0 AS LEVEL   FROM Web.Processes p 
	                            WHERE 1=1 "+ CteProcess + @"
	                            UNION ALL
	                            SELECT p.processId, p.processname, level+1 
	                            FROM Web.Processes p
	                            INNER JOIN cteProcess cp ON p.ParentProcessId = cp.ProcessId
                            ),
                        CtePaymentAdvice as
                            (
                          SELECT 
                                H.JobInvoiceHeaderId,
                                H.DocTypeId,
                                DT.DocumentTypeName,
                                H.DocNo AS InvoiceNo,format(H.DocDate,'dd/MMM/yy') AS InvoiceDate,
                                JOH.DocNo AS IssueChallanNo,format(JOH.DocDate,'dd/MMM/yy') AS IssueDate,
                                (PG.ProductGroupName) as Design,
                                (PD.ProductName) AS ProductName,
                                ((Case When JOH.UnitconversionForId='5' THEN VRS.ManufaturingSizeName When JOH.UnitconversionForId='1' THEN VRS.StandardSizeName else VRS.FinishingSizeName END)) as size,
                                (STC.Code) AS SalesTaxProductCodes,
                                (CASE WHEN CGP.ChargeGroupPersonName IS NOT NULL THEN 
                                CASE WHEN CGP.ChargeGroupPersonName='State (Unregistered)' THEN 'Yes' ELSE 'No' END  ELSE NULL END) AS  ChargeGroupPersonName,
                                L.Sr AS Sr,
                                L.JobInvoiceLineId,

                               L.JobWorkerId,
						        H.ProcessId AS ProcessId,
						        isnull(L.Qty,0) AS Qty,
						        isnull(L.DealQty,0) AS DealQty,	 
						        U.UnitName AS DUnitName,
						        convert(int,U.DecimalPlaces) AS DDecimalPlaces,
						        isnull(L.Rate,0)+isnull(JIRAL.Rate,0) AS Rate,
						        isnull(L.Amount,0)+isnull(JIRAL.Amount,0) AS Amount,
                                isnull(LC.IncentiveAmt,0) AS IncentiveAmt,
						        isnull(LC.PenaltyAmt,0)+isnull(QA.PenaltyAmt,0) AS PenaltyAmt,                                   
						        isnull(LC.GrossAmount,0)+isnull(JIRAL.GrossAmount,0) AS GrossAmt,
						        (CASE WHEN CGP.ChargeGroupPersonName IN ('Ex-State (Registered)','State (Registered)') THEN  isnull(LC.GSTAmt,0) +isnull(JIRAL.GSTAmt,0) ELSE 0 END)  AS GSTAmt,
						        isnull(LC.NetAmount,0)+isnull(JIRAL.NetAmount,0) AS NetAmt,
						        0 AS TDSAmount,0 AS Advance,
						        0 AS DebitNote,
						        0 AS CreditNote,
						        0 GSTDebitCredit
						FROM 
						(
						SELECT 
						H.DocTypeId,H.DocNo,H.DocDate,H.JobInvoiceHeaderId, H.JobWorkerId, H.ProcessId,H.SalesTaxGroupPersonId  
						FROM 	Web.JobInvoiceHeaders H WITH (Nolock)
						WHERE 1=1 " + mCondStr +
                        @"                    
						) H
                        LEFT JOIN web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId=H.DocTypeId
						LEFT JOIN Web.JobInvoiceLines L WITH (Nolock) ON L.JobInvoiceHeaderId=H.JobInvoiceHeaderId						
						LEFT JOIN
						(
							SELECT JIRAL.JobInvoiceLineId,sum(isnull(JIRAL.Amount,0)) AS Amount,sum(isnull(LC.GrossAmount,0)) AS GrossAmount,
							sum(isnull(LC.GSTAmt,0)) AS GSTAmt,sum(isnull(LC.NetAmount,0)) AS NetAmount,Sum(Isnull(JIRAL.Rate,0)) AS Rate
							FROM Web.JobInvoiceRateAmendmentLines JIRAL WITH (Nolock)
							LEFT JOIN Web.ViewJobInvoiceRateAmendmentLinecharges LC ON LC.LineTableId=JIRAL.JobInvoiceRateAmendmentLineId
							GROUP BY JIRAL.JobInvoiceLineId
						) JIRAL ON JIRAL.JobInvoiceLineId=L.JobInvoiceLineId
						LEFT JOIN Web.JobReceiveLines JRL WITH (Nolock) ON JRL.JobReceiveLineId=L.JObReceiveLineId
                        LEFT JOIN Web.JobOrderLines JOL WITH (Nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
                        LEFT JOIN web.JobOrderHeaders JOH WITH (Nolock) ON JOH.JobOrderHeaderId=JOL.JobOrderHeaderId
                        LEFT JOIN web.Products PD WITH (Nolock) ON PD.ProductId=JOL.ProductId
                        LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=PD.ProductGroupId
                        LEFT JOIN web.ViewRugSize VRS with (nolock) on VRS.ProductId=PD.ProductId
                        LEFT JOIN Web.SalesTaxProductCodes STC WITH (Nolock) ON STC.SalesTaxProductCodeId= IsNull(PD.SalesTaxProductCodeId, PG.DefaultSalesTaxProductCodeId)
						LEFT JOIN Web.Units U WITH (Nolock) ON U.UnitId=L.DealUnitId
						LEFT JOIN 
						(
							SELECT Qa.JobReceiveLineId,Sum(isnull(Qa.PenaltyAmt,0)) AS PenaltyAmt 
							FROM Web.JobReceiveQALines Qa WITH (Nolock)
							GROUP BY Qa.JobReceiveLineId
						) QA ON QA.JobReceiveLineId=JRL.JobReceiveLineId
						LEFT JOIN Web.ViewJobInvoiceLineCharges LC WITH (Nolock) ON LC.JobInvoiceLineId=L.JobInvoiceLineId
						LEFT JOIN Web.ChargeGroupPersons CGP WITH (Nolock) ON CGP.ChargeGroupPersonId=H.SalesTaxGroupPersonId
						WHERE 1=1 " +  jobInvCondstr +
                        @"
                       UNION ALL
						
						SELECT 
                            H.JobInvoiceReturnHeaderId AS JobInvoiceHeaderId,						 
                            H.DocTypeId,
                            DT.DocumentTypeName,
                            JIH.DocNo  AS InvoiceNo,format(JIH.DocDate,'dd/MMM/yy') AS InvoiceDate,
                            H.DocNo AS IssueChallanNo,format(H.DocDate,'dd/MMM/yy') AS IssueDate,
                            NULL as Design,
                            NULL AS ProductName,
                            NULL as size,
                            NULL AS SalesTaxProductCodes,
                            NULL  AS  ChargeGroupPersonName,
                            NULL AS Sr,
                            L.JobInvoiceLineId,
							H.JobWorkerId,
							H.ProcessId AS ProcessId,
							-(CASE WHEN H.Nature='Return' THEN isnull(L.Qty,0) ELSE 0 END) AS Qty,
						    -(CASE WHEN H.Nature='Return' THEN isnull(L.DealQty,0) ELSE 0 END) AS DealQty,
							U.UnitName AS DUnitName,
							U.DecimalPlaces AS DDecimalPlaces ,							
							0 AS Rate,
							- (CASE WHEN H.Nature='Return' THEN isnull(L.Amount,0) ELSE 0 END) AS Amount,
                            0 AS IncentiveAmt,
							0 AS PenaltyAmt,
							0 AS GrossAmt,
							0 AS GSTAmt,
							0 AS NetAmt,
							0 AS TDSAmount,
							0 AS Advance,
							(CASE WHEN H.Nature='Debit Note' THEN isnull(L.Amount,0) ELSE 0 END) AS DebitNote,
							(CASE WHEN H.Nature='Credit Note' THEN isnull(L.Amount,0) ELSE 0 END) AS CreditNote,
                        (CASE WHEN CGP.ChargeGroupPersonName IN ('Ex-State (Registered)','State (Registered)') AND H.Nature='Debit Note'  THEN  -(isnull(LC.CGSTAmt,0)+isnull(LC.SGSTAmt,0)) ELSE 0 END)+(CASE WHEN CGP.ChargeGroupPersonName IN ('Ex-State (Registered)','State (Registered)') AND H.Nature='Credit Note' THEN  (isnull(LC.CGSTAmt,0)+isnull(LC.SGSTAmt,0)) ELSE 0 END)  AS GSTDebitCredit
						FROM Web.JobInvoiceReturnHeaders H
                        LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId=H.DocTypeId
						LEFT JOIN Web.JobInvoiceReturnLines L WITH (Nolock) ON L.JobInvoiceReturnHeaderId=H.JobInvoiceReturnHeaderId
						LEFT JOIN Web.JobInvoiceLines JIL WITH (Nolock) ON JIL.JobInvoiceLineId=L.JobInvoiceLineId
						LEFT JOIN Web.jobinvoiceheaders JIH WITH (Nolock) ON JIH.JobInvoiceHeaderId=JIL.JobInvoiceHeaderId
						LEFT JOIN Web.ChargeGroupPersons CGP WITH (Nolock) ON CGP.ChargeGroupPersonId=JIH.SalesTaxGroupPersonId
						LEFT JOIN Web.Units U WITH (Nolock) ON U.UnitId=L.DealUnitId
						LEFT JOIN Web.ViewJobInvoiceReturnLineCharges LC WITH (Nolock) ON LC.LineTableId=L.JobInvoiceReturnLineId
						WHERE 1=1 " + mCondStr + jobInvRetCondstr +
                        @"

						UNION ALL
						
						SELECT
                            H.LedgerHeaderId AS JobInvoiceHeaderId,						 
                            H.DocTypeId,
                            H.DocumentTypeName,
                            NUll AS InvoiceNo,NUll AS InvoiceDate,
                            H.DocNo AS IssueChallanNo,format(H.DocDate,'dd/MMM/yy') AS IssueDate,
                            NULL as Design,
                            NULL AS ProductName,
                            NULL as size,
                            NULL AS SalesTaxProductCodes,
                            NULL  AS  ChargeGroupPersonName,
                            NULL AS Sr,
                            NULL AS JobInvoiceLineId,

							LA.PersonId AS JobWorkerId , 
							H.ProcessId AS ProcessId, 
							0 AS Qty,
							0 AS DealQty,
							NULL  AS DUnitName,
							NULL AS DDecimalPlaces ,
							0 AS Rate,0 AS Amount,
                            0 AS IncentiveAmt,
							0 AS PenaltyAmt,
							0 AS GrossAmt,
							0 AS GSTAmt,
							0 AS NetAmt,  
							(CASE WHEN H.DocumentCategoryId = 75 THEN isnull(L.Amount,0) ELSE 0 END)  AS  TDSAmount,  
							(CASE WHEN H.DocumentCategoryId = 73 AND H.AdjustmentType='Advance' THEN isnull(L.Amount,0)-isnull(Cancel.Amount,0) ELSE 0 END ) AS  Advance,
							(CASE WHEN H.DocumentCategoryId = 511 and H.DocHeaderId Is null  THEN isnull(L.Amount,0) ELSE 0 END ) AS  DebitNote,
							(CASE WHEN H.DocumentCategoryId IN (517,512) THEN isnull(L.Amount,0) ELSE 0 END ) AS  CreditNote,
							0 AS GSTDebitCredit
						FROM 
						(
							SELECT H.*, DT.DocumentCategoryId,DT.DocumentTypeName FROM Web.Ledgerheaders H WITH (nolock) 
							LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId=H.DocTypeId
							WHERE 1=1 AND DT.DocumentCategoryId IN (35, 75,73,511,517,512)
							" + LedgerCondHeader + 
                        @" ) H
						LEFT JOIN Web.LedgerLines L WITH (nolock)  ON H.LedgerHeaderId=L.LedgerHeaderId
						LEFT JOIN 
						(
							SELECT L.LedgerLineId,sum(isnull(LL.Amount,0)) AS Amount  FROM Web.LedgerLines H
							LEFT JOIN web.Ledgers L ON L.LedgerId=H.ReferenceId
							LEFT JOIN Web.LedgerLines LL ON LL.LedgerLineId=L.LedgerLineId
							WHERE H.ReferenceId IS NOT NULL 
							GROUP BY L.LedgerLineId
						) Cancel ON Cancel.LedgerLineId=L.LedgerLineId
						left join web.LedgerAccounts LA with (nolock) on LA.LedgerAccountId=L.LedgerAccountId 
						WHERE 1=1  "+ Ledger+ 
                         @") ";
            if (Format == ReportFormatPaymentAdvice.OrderNoWise)
            {

                mQry += @"  SELECT  
                                    H.JobInvoiceHeaderId,
                                    max(H.DocTypeId) as DocTypeId,
                                    max(H.DocumentTypeName) as DocumentTypeName,
                                    max(H.InvoiceNo) as InvoiceNo,max(H.InvoiceDate) as InvoiceDate,
                                    max(H.IssueChallanNo) as IssueChallanNo,max(H.IssueDate) as IssueDate,
                                    max(H.Design) as Design,
                                    max(H.ProductName) as ProductName,
                                    max(H.size) as size,
                                    max(H.SalesTaxProductCodes) as SalesTaxProductCodes,
                                    max(H.ChargeGroupPersonName) as ChargeGroupPersonName,                                    
                                    max(H.JobInvoiceLineId) as JobInvoiceLineId,   
                                    max(PS.ProcessName) AS ProcessName,max(P.Name)+' ('+isnull(max(P.Code),'')+')' AS Name,Sum(H.Qty) AS PCS,sum(Isnull(H.DealQty,0)) AS Area,max(isnull(H.DUnitName,'')) AS DealUnit,Max(H.DDecimalPlaces) AS DecimalPlaces,
                                    max(H.Rate) AS Rate,sum(H.Amount) AS Amount, Sum(isnull(H.IncentiveAmt,0)) AS IncentiveAmt,sum(H.PenaltyAmt) AS PenaltyAmt,sum(H.GrossAmt) AS GrossAmount,sum(H.GSTAmt) AS GSTAmt,sum(H.NetAmt) AS NetAmt,sum(H.TDSAmount) AS TDSAmount,
                                    sum(H.Advance) AS Advance,sum(H.DebitNote) AS DebitNote,sum(H.CreditNote) AS CreditNote,sum(H.GSTDebitCredit) AS GSTDebitCredit,sum(H.NetAmt-H.DebitNote+H.CreditNote+H.GSTDebitCredit-H.TDSAmount-H.Advance)  AS NetPayable
                                FROM  CtePaymentAdvice H                       
                                LEFT JOIN Web.People P WITH (Nolock) ON P.PersonID=H.JobWorkerId
                                LEFT JOIN Web.Processes PS WITH (Nolock) ON PS.ProcessId=H.ProcessId 
                               GROUP BY H.JobInvoiceHeaderId,H.IssueChallanNo,H.ProductName,PS.ProcessName,Isnull(H.Rate,0)
                               " + HavingCondition + @"
                               ORDER BY convert(date,max(H.InvoiceDate)),max(H.InvoiceNo),isnull(H.Rate,0),min(H.Sr)
                         ";
            }
          else  if (Format == ReportFormatPaymentAdvice.JobWorkerWise)
            {
                
                   mQry += @"  SELECT  H.JobWorkerId,
                                    max(P.Name)+' ('+isnull(max(P.Code),'')+')' AS Name,Sum(H.Qty) AS PCS,sum(Isnull(H.DealQty,0)) AS Area,max(isnull(H.DUnitName,'')) AS DealUnit,Max(H.DDecimalPlaces) AS DecimalPlaces,
                                    max(H.Rate) AS Rate,sum(H.Amount) AS Amount,Sum(isnull(H.IncentiveAmt,0)) AS IncentiveAmt,sum(H.PenaltyAmt) AS PenaltyAmt,sum(H.GrossAmt) AS GrossAmount,sum(H.GSTAmt) AS GSTAmt,sum(H.NetAmt) AS NetAmt,sum(H.TDSAmount) AS TDSAmount,
                                    sum(H.Advance) AS Advance,sum(H.DebitNote) AS DebitNote,sum(H.CreditNote) AS CreditNote,sum(H.GSTDebitCredit) AS GSTDebitCredit,sum(H.NetAmt-H.DebitNote+H.CreditNote+H.GSTDebitCredit-H.TDSAmount-H.Advance)  AS NetPayable,'Order No Wise' as Format
                                FROM  CtePaymentAdvice H                       
                                LEFT JOIN Web.People P WITH (Nolock) ON P.PersonID=H.JobWorkerId
                                GROUP BY H.JobWorkerId 
                               " + HavingCondition + @"
                                Order By max(P.Name)   
                         ";
            }
            else if (Format == ReportFormatPaymentAdvice.ProcessWise)
            {
                mQry += @" SELECT   H.ProcessId,
                                    max(PS.ProcessName) AS ProcessName,Sum(H.Qty) AS PCS,sum(Isnull(H.DealQty,0)) AS Area,max(isnull(H.DUnitName,'')) AS DealUnit,Max(H.DDecimalPlaces) AS DecimalPlaces,
                                    max(H.Rate) AS Rate,sum(H.Amount) AS Amount,Sum(isnull(H.IncentiveAmt,0)) AS IncentiveAmt,sum(H.PenaltyAmt) AS PenaltyAmt,sum(H.GrossAmt) AS GrossAmount,sum(H.GSTAmt) AS GSTAmt,sum(H.NetAmt) AS NetAmt,sum(H.TDSAmount) AS TDSAmount,
                                    sum(H.Advance) AS Advance,sum(H.DebitNote) AS DebitNote,sum(H.CreditNote) AS CreditNote,sum(H.GSTDebitCredit) AS GSTDebitCredit,sum(H.NetAmt-H.DebitNote+H.CreditNote+H.GSTDebitCredit-H.TDSAmount-H.Advance)  AS NetPayable,'Job Worker Wise Summary' as Format
                        FROM  CtePaymentAdvice H                       
                        LEFT JOIN Web.Processes PS WITH (Nolock) ON PS.ProcessId=H.ProcessId
                        GROUP BY H.ProcessId 
                        Order By max(PS.ProcessName)  
                         ";
            }

            IEnumerable<PaymentAdivceViewModel> TrialBalanceSummaryList = db.Database.SqlQuery<PaymentAdivceViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate,SqlParameterJobWorker, SqlParameterProcess).ToList();


            return TrialBalanceSummaryList;

        }

      
        public void Dispose()
        {
        }
    }

     public class Display_PaymentAdviceViewModel
    {
        public string Format { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }        
        public string JobWorker { get; set; }
        public string SiteIds { get; set; }
        public string DivisionIds { get; set; }
        public string Process { get; set; }
        public string TextHidden { get; set; }


    }

    [Serializable()]
    public class DisplayFilterPaymentAdviceSettings
    {
        public string Format { get; set; }
        public List<DisplayFilterPaymentAdviceParameters> DisplayFilterPaymentAdviceParameters { get; set; }
    }

    [Serializable()]
    public class DisplayFilterPaymentAdviceParameters
    {
        public string ParameterName { get; set; }
        public bool IsApplicable { get; set; }
        public string Value { get; set; }
    }

    public class ReportFormatPaymentAdvice
    {
        public const string JobWorkerWise = "Job Worker Wise Summary";
        public const string OrderNoWise = "Order No Wise";
        public const string ProcessWise = "Process Wise Symmary";
    }




    public class PaymentAdivceViewModel
    {

        public int?  JobInvoiceHeaderId { get; set; }
        public int? DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string IssueChallanNo { get; set; }
        public string IssueDate { get; set; }
        public string Design { get; set; }
        public string ProductName { get; set; }
        public string size { get; set; }
        public string SalesTaxProductCodes { get; set; }
        public string ChargeGroupPersonName { get; set; }
        public int? Sr { get; set; }
        public int? JobInvoiceLineId { get; set; }


        public int? JobWorkerId { get; set; }        
        public int? ProcessId { get; set; }
        public string Name { get; set; }
        public string ProcessName { get; set; }
        public decimal? PCS { get; set; }
        public decimal? Area { get; set; }
        public string DealUnit { get; set; }
        public int?  DecimalPlaces { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }

        public decimal ? IncentiveAmt { get; set; }
        public decimal? PenaltyAmt { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? GSTAmt { get; set; }
        public decimal? NetAmt { get; set; }
        public decimal? TDSAmount { get; set; }
        public decimal? Advance { get; set; }
        public decimal? DebitNote { get; set; }
        public decimal? CreditNote { get; set; }
        public decimal? GSTDebitCredit { get; set; }
        public decimal? NetPayable { get; set; }
        public string ReportName { get; set; }
        public string ReportTitle { get; set; }
        public string SubReportProcList { get; set; }
        public string Format { get; set; }



    }
    public class DetailViewModel
    {


        
        public int? JobWorkerId { get; set; }
        public int? ProcessId { get; set; }
        public string Name { get; set; }
        public string ProcessName { get; set; }
        public decimal? PCS { get; set; }
        public decimal? Area { get; set; }
        public string UnitName { get; set; }
        public int? DecimalPlaces { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PenaltyAmt { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? GSTAmt { get; set; }
        public decimal? NetAmt { get; set; }
        public decimal? TDSAmount { get; set; }
        public decimal? Advance { get; set; }
        public decimal? DebitNote { get; set; }
        public decimal? CreditNote { get; set; }
        public decimal? GSTDebitCredit { get; set; }
        public decimal? NetPayable { get; set; }
        public string ReportName { get; set; }
        public string ReportTitle { get; set; }
        public string SubReportProcList { get; set; }
        public string Format { get; set; }



    }

}

