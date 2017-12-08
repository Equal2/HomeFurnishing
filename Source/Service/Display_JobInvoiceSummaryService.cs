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
    public interface IDisplay_JobInvoiceSummaryService : IDisposable
    {
        IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter);
        IQueryable<ComboBoxResult> GetFilterRepotType(string term, int? filter);
        
            IEnumerable<JobInvoiceSummaryViewModels> JobInvoiceSummary(DisplayFilterJobInvoiceSummarySettings Settings);
    }

    public class Display_JobInvoiceSummaryService : IDisplay_JobInvoiceSummaryService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

      

        public Display_JobInvoiceSummaryService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }

        public IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = ReportFormatJobInvoiceSummary.JobWorkerWise, text = ReportFormatJobInvoiceSummary.JobWorkerWise });          
            ResultList.Add(new ComboBoxResult { id = ReportFormatJobInvoiceSummary.MonthWise, text = ReportFormatJobInvoiceSummary.MonthWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormatJobInvoiceSummary.ProductWise, text = ReportFormatJobInvoiceSummary.ProductWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormatJobInvoiceSummary.ProductGroupWise, text = ReportFormatJobInvoiceSummary.ProductGroupWise });

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
        public IQueryable<ComboBoxResult> GetFilterRepotType(string term, int? filter)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = ReportTypeJobInvoiceSummary.JobInvoice, text = ReportTypeJobInvoiceSummary.JobInvoice });
            ResultList.Add(new ComboBoxResult { id = ReportTypeJobInvoiceSummary.JobInvoiceReturn, text = ReportTypeJobInvoiceSummary.JobInvoiceReturn });
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





        public IEnumerable<JobInvoiceSummaryViewModels> JobInvoiceSummary(DisplayFilterJobInvoiceSummarySettings Settings)
        {
            var FormatSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var ReportTypeSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "ReportType" select H).FirstOrDefault();

            

            var SiteSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();            
            var JobWorkerSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "JobWorker" select H).FirstOrDefault();
            var ProcessSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in Settings.DisplayFilterJobInvoiceSummaryParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();

            
            string Format = FormatSetting.Value;
            string ReportType = ReportTypeSetting.Value;
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

            string mQry, mCondStr="";


            mCondStr = "";
            if (!string.IsNullOrEmpty(SiteId)) mCondStr += " AND H.SiteId = @Site ";
            if (!string.IsNullOrEmpty(DivisionId)) mCondStr += " AND H.DivisionId = @Division ";
            if (!string.IsNullOrEmpty(FromDate)) mCondStr += " AND H.DocDate >= @FromDate ";
            if (!string.IsNullOrEmpty(ToDate)) mCondStr += " AND H.DocDate <= @ToDate "; 
            if (!string.IsNullOrEmpty(Process)) mCondStr += "  AND H.ProcessId =@Process ";
            if (!string.IsNullOrEmpty(JobWorker)) mCondStr += " AND  P.PersonID = @JobWorker ";
            
            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);            
            SqlParameter SqlParameterJobWorker = new SqlParameter("@JobWorker", !string.IsNullOrEmpty(JobWorker) ? JobWorker : (object)DBNull.Value);
            SqlParameter SqlParameterProcess = new SqlParameter("@Process", !string.IsNullOrEmpty(Process) ? Process : (object)DBNull.Value);


            mQry = @"WITH cteJobInvoiceLineCharges AS
                            (
	                          SELECT H.JobInvoiceLineId AS JobInvoiceLineId,
                                isnull(H.GrossAmount,0)+isnull(JIRAL.GrossAmount,0) AS GrossAmount,
                                isnull(H.IncentiveAmt,0)+isnull(JIRAL.IncentiveAmt,0) AS IncentiveAmt,
                                isnull(H.IncentiveRate,0) AS IncentiveRate,
                                isnull(H.PenaltyAmt,0)+isnull(JIRAL.PenaltyAmt,0) AS PenaltyAmt,
                                isnull(H.NetAmount,0)+isnull(JIRAL.NetAmount,0) AS NetAmount,
                                isnull(H.CGSTAmt,0)+isnull(JIRAL.CGSTAmt,0) AS CGSTAmt,
                                isnull(H.SGSTAmt,0)+isnull(JIRAL.SGSTAmt,0) AS SGSTAmt,
                                H.CGSTPer AS CGSTPer,H.SGSTPer AS SGSTPer,
                                isnull(H.TaxableAmount,0)+isnull(JIRAL.TaxableAmount,0) AS TaxableAmount,
                                isnull(H.GSTAmt,0)+isnull(JIRAL.GSTAmt,0) AS GSTAmt,
                                H.IGSTAmt AS IGSTAmt,H.IGSTPer AS IGSTPer
                                  FROM Web.ViewJobInvoiceLineCharges H
                                LEFT JOIN
	                                (
		                                SELECT JIRAL.JobInvoiceLineId,sum(isnull(LC.GrossAmount,0)) AS GrossAmount,sum(isnull(LC.IncentiveAmt,0)) AS IncentiveAmt,
		                                sum(isnull(LC.PenaltyAmt,0)) AS PenaltyAmt,sum(isnull(LC.NetAmount,0)) AS NetAmount,
		                                sum(isnull(LC.CGSTAmt,0)) AS CGSTAmt,sum(isnull(LC.SGSTAmt,0)) AS SGSTAmt,sum(isnull(LC.TaxableAmount,0)) AS TaxableAmount,
		                                sum(isnull(LC.GSTAmt,0)) AS GSTAmt		
		                                FROM Web.JobInvoiceRateAmendmentLines JIRAL WITH (Nolock)
		                                LEFT JOIN Web.ViewJobInvoiceRateAmendmentLinecharges LC ON LC.LineTableId=JIRAL.JobInvoiceRateAmendmentLineId
		                                GROUP BY JIRAL.JobInvoiceLineId
	                                ) JIRAL ON JIRAL.JobInvoiceLineId=H.JobInvoiceLineId
                            ),
                          cteJobInvoiceReturnLineCharges AS
                            (
	                          SELECT L.LineTableId,
                                Sum(CASE WHEN C.ChargeName='Gross Amount' THEN L.Amount ELSE 0 END) AS GrossAmount,
                                Sum(CASE WHEN C.ChargeName='Incentive' THEN L.Amount ELSE 0 END) AS IncentiveAmt,
                                Max(CASE WHEN C.ChargeName='Incentive' THEN L.Rate ELSE 0 END) AS IncentiveRate,
                                Sum(CASE WHEN C.ChargeName='Penalty' THEN L.Amount ELSE 0 END) AS PenaltyAmt,
                                Sum(CASE WHEN C.ChargeName='Net Amount' THEN L.Amount ELSE 0 END) AS NetAmount,
                                sum(CASE WHEN C.ChargeName='CGST' THEN L.Amount ELSE 0 END) AS CGSTAmt ,
                                sum(CASE WHEN C.ChargeName='SGST' THEN L.Amount ELSE 0 END) AS SGSTAmt,
                                max(CASE WHEN C.ChargeName='CGST' THEN L.Rate ELSE 0 END) AS CGSTPer ,
                                max(CASE WHEN C.ChargeName='SGST' THEN L.Rate ELSE 0 END) AS SGSTPer,
                                --New Added
                                sum(CASE WHEN C.ChargeName IN ('Sales Taxable Amount','Sales Tax Taxable Amt') THEN L.Amount ELSE 0 END) AS TaxableAmount,
                                Sum(CASE WHEN C.ChargeName ='IGST' THEN L.Amount ELSE 0 End) AS IGSTAmt,
                                Max(CASE WHEN C.ChargeName ='IGST' THEN L.Rate ELSE 0 End) AS IGSTPer
                                 FROM 
                                Web.JobInvoiceReturnLineCharges L WITH (Nolock)
                                LEFT JOIN web.Charges C WITH (Nolock) ON C.ChargeId=L.ChargeId
                                GROUP BY L.LineTableId
                            ),
                           cteJobInvoice AS
                            (
                                            SELECT 
                                            P.Name AS Name ,
                                            H.DocDate AS DocDate,
                                            PD.ProductName AS ProductName,
                                            PG.ProductGroupName AS ProductGroupName,
                                            isnull(L.Qty,0) as Qty,
                                            U.UnitName AS UnitName,convert(decimal(18,4),isnull(U.DecimalPlaces,0)) AS DecimalPlaces,
                                            isnull(L.DealQty,0) as DealQty,
                                            Au.UnitName AS DealUnit,
                                            convert(decimal(18,4),isnull(AU.DecimalPlaces,0)) AS AUDecimalPlaces,
                                            isnull(L.Amount,0) AS Amount,
                                            isnull(LC.TaxableAmount,0) AS TaxableAmount,
                                            (CASE WHEN CGP.ChargeGroupPersonName IN ('State (Registered)','Ex-State (Registered)') THEN 0 ELSE isnull(LC.IGSTAmt,0) END) AS IGST,
                                            (CASE WHEN CGP.ChargeGroupPersonName IN ('State (Registered)','Ex-State (Registered)') THEN 0 ELSE isnull(LC.CGSTAmt,0) END) AS CGST,
                                            (CASE WHEN CGP.ChargeGroupPersonName IN ('State (Registered)','Ex-State (Registered)') THEN 0 ELSE isnull(LC.SGSTAmt,0) END) AS SGST,
                                            isnull(LC.NetAmount,0) AS InvoiceAmount
                                            FROM Web.JobInvoiceHeaders H
                                            LEFT JOIN Web.JobInvoiceLines L WITH (Nolock) ON L.JobInvoiceHeaderId=H.JobInvoiceHeaderId
                                            LEFT JOIN web.People P WITH (Nolock) ON P.PersonID=isnull(H.JobWorkerId,L.JobWorkerId)
                                            LEFT JOIN web.JobReceiveLines JRL WITH (Nolock) ON JRL.JobReceiveLineId=L.JobReceiveLineId
                                            LEFT JOIN web.JobOrderLines JOL WITH (Nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
                                            LEFT JOIN web.Products PD WITH (Nolock) ON PD.ProductId=JOL.ProductId
                                            LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId=PD.UnitId
                                            LEFT JOIN web.Units AU WITH (Nolock) ON AU.UnitId=L.DealUnitId
                                            LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=PD.ProductGroupId	
                                            LEFT JOIN web.ChargeGroupPersons CGP WITH (Nolock) ON CGP.ChargeGroupPersonId=H.SalesTaxGroupPersonId	   
                                            Left Join cteJobInvoiceLineCharges LC ON LC.JobInvoiceLineId=L.JobInvoicelineId
                                            where 1=1 " + mCondStr + @"
                            ),
                           cteJobInvoiceReturn AS
                            (
                                    SELECT 
                                    P.Name AS Name,
                                    H.DocDate AS DocDate,			
                                    PD.ProductName AS ProductName,
                                    PG.ProductGroupName AS ProductGroupName,
                                    (CASE WHEN H.Nature='Return' THEN isnull(L.Qty,0) ELSE 0 END) AS Qty, 
                                    U.UnitName AS UnitName,convert(decimal(18,4),isnull(U.DecimalPlaces,0)) AS DecimalPlaces,
                                    (CASE WHEN H.Nature='Return' THEN isnull(L.DealQty,0) ELSE 0 END) AS DealQty,
                                    AU.UnitName  AS DealUnit,
                                    convert(decimal(18,4),isnull(AU.DecimalPlaces,0)) AS AUDecimalPlaces,
                                    isnull(L.Amount,0) AS Amount,
                                    isnull(JIRL.TaxableAmount,0) AS TaxableAmount,
                                    (CASE WHEN CGP.ChargeGroupPersonName IN ('State (Registered)','Ex-State (Registered)') THEN 0 ELSE isnull(JIRL.IGST,0) END) AS IGST,
                                    (CASE WHEN CGP.ChargeGroupPersonName IN ('State (Registered)','Ex-State (Registered)') THEN 0 ELSE isnull(JIRL.CGST,0) END) AS CGST,
                                    (CASE WHEN CGP.ChargeGroupPersonName IN ('State (Registered)','Ex-State (Registered)') THEN 0 ELSE isnull(JIRL.SGST,0) END) AS SGST,
                                    isnull(JIRL.NetAmount,0) AS InvoiceAmount
                                    FROM Web.JobInvoiceReturnLines L
                                    LEFT JOIN Web.JobInvoiceReturnHeaders H WITH (Nolock) ON H.JobInvoiceReturnHeaderId=L.JobInvoiceReturnHeaderId
                                    LEFT JOIN 
                                    (	SELECT 
                                    H.LineTableId,H.TaxableAmount,H.IGSTAmt AS IGST,H.IGSTPer AS IGSTRate,H.CGSTAmt AS CGST,
                                    H.CGSTPer AS CGSTRate,H.SGSTAmt AS SGST, H.SGSTPer AS SGSTRate,H.NetAmount AS NetAmount
                                    FROM Web.ViewJobInvoiceReturnLineCharges H
                                    ) JIRL ON JIRL.LineTableId=L.JobInvoiceReturnLineId
                                    LEFT JOIN web.People P WITH (Nolock) ON P.PersonID=H.JobWorkerId 
                                    LEFT JOIN Web.JobInvoiceLines JIL WITH (Nolock) ON JIL.JobInvoiceLineId=L.JobInvoiceLineId
                                    LEFT JOIN web.ChargeGroupProducts CGPD WITH (Nolock) ON JIL.SalesTaxGroupProductId = CGPD.ChargeGroupProductId
                                    LEFT JOIN web.JobInvoiceHeaders JIH WITH (Nolock) ON JIH.JobInvoiceHeaderId=JIL.JobInvoiceHeaderId
                                    LEFT JOIN web.ChargeGroupPersons CGP WITH (Nolock) ON CGP.ChargeGroupPersonId=JIH.SalesTaxGroupPersonId			
                                    LEFT JOIN Web.jobreceivelines JRL WITH (Nolock) ON  JRL.JobReceiveLineId=JIL.JobReceiveLineId
                                    LEFT JOIN web.JobOrderLines JOL WITH (Nolock) ON JOL.JobOrderLineId=JRL.JobOrderLineId
                                    LEFT JOIN web.Products PD WITH (Nolock) ON PD.ProductId=JOL.ProductId
                                    LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=PD.ProductGroupId
                                    LEFT JOIN Web.SalesTaxProductCodes STC WITH (Nolock) ON STC.SalesTaxProductCodeId= IsNull(PD.SalesTaxProductCodeId, Pg.DefaultSalesTaxProductCodeId)
                                    LEFT JOIN web.Units U WITH (Nolock) ON U.UnitId=PD.UnitId  
                                    LEFT JOIN web.Units AU WITH (Nolock) ON AU.UnitId=L.DealUnitId	                                  					
                                    WHERE 1=1 AND H.Nature IN ('Debit Note', 'Return','Credit Note') " + mCondStr + @"
                            ),
                         cteJobInvoiceWithReturn AS
                            (
                           SELECT Name, DocDate, ProductName, ProductGroupName, Qty, UnitName, DecimalPlaces, DealQty, 
                                  DealUnit, AUDecimalPlaces, Amount, TaxableAmount, IGST,CGST,SGST, InvoiceAmount from cteJobInvoice
                             Union All 
                           SELECT Name, DocDate, ProductName, ProductGroupName, Qty, UnitName, DecimalPlaces, DealQty, 
                                  DealUnit, AUDecimalPlaces, Amount, TaxableAmount, IGST,CGST,SGST, InvoiceAmount from cteJobInvoiceReturn
                            )";
            string CteName = string.Empty;
            if (ReportType == ReportTypeJobInvoiceSummary.JobInvoice)
            {
                CteName = "cteJobInvoice";
            }
            else if (ReportType == ReportTypeJobInvoiceSummary.JobInvoiceReturn)
            {
                CteName = "cteJobInvoiceReturn";
            }
            else
            {
                CteName = "cteJobInvoiceWithReturn";
            }
           

            if (Format == ReportFormatJobInvoiceSummary.JobWorkerWise)
                {
                    mQry += @"    SELECT H.Name as GroupOnText,sum(isnull(H.Qty,0)) as Qty,max(H.UnitName) as UnitName,max(H.DecimalPlaces) as DecimalPlaces,sum(isnull(H.DealQty,0)) as DealQty, 
                                  max(H.DealUnit) as DealUnit,max(H.AUDecimalPlaces) as AUDecimalPlaces,sum(isnull(H.Amount,0)) as Amount,sum(isnull(H.TaxableAmount,0)) as TaxableAmount,Sum(isnull(H.IGST,0)) as IGST,Sum(isnull(H.CGST,0)) as CGST,sum(isnull(H.SGST,0)) as SGST,
                                  Sum(isnull(H.InvoiceAmount,0)) as  InvoiceAmount
                                  from "+ CteName + @" H
                                  Group By H.Name
                                  order By H.Name
                                  ";
                }
                else if(Format == ReportFormatJobInvoiceSummary.MonthWise)
                {
                    mQry += @"    SELECT STUFF(CONVERT(CHAR(11),max(H.DocDate), 100), 5,5, '-') as GroupOnText ,sum(isnull(H.Qty,0)) as Qty,max(H.UnitName) as UnitName,max(H.DecimalPlaces) as DecimalPlaces,sum(isnull(H.DealQty,0)) as DealQty, 
                                  max(H.DealUnit) as DealUnit,max(H.AUDecimalPlaces) as AUDecimalPlaces,sum(isnull(H.Amount,0)) as Amount,sum(isnull(H.TaxableAmount,0)) as TaxableAmount,Sum(isnull(H.IGST,0)) as IGST,Sum(isnull(H.CGST,0)) as CGST,sum(isnull(SGST,0)) as SGST,
                                  Sum(isnull(H.InvoiceAmount,0)) as  InvoiceAmount
                                  from " + CteName + @" H
                                  Group By Substring(convert(NVARCHAR,H.DocDate,11),0,6)
                                  order By Substring(convert(NVARCHAR,H.DocDate,11),0,6)
                                  ";
                }
                else if(Format == ReportFormatJobInvoiceSummary.ProductWise)
                {
                    mQry += @"    SELECT H.ProductName as GroupOnText,sum(isnull(H.Qty,0)) as Qty,max(H.UnitName) as UnitName,max(H.DecimalPlaces) as DecimalPlaces,sum(isnull(H.DealQty,0)) as DealQty, 
                                  max(H.DealUnit) as DealUnit,max(H.AUDecimalPlaces) as AUDecimalPlaces,sum(isnull(H.Amount,0)) as Amount,sum(isnull(H.TaxableAmount,0)) as TaxableAmount,Sum(isnull(H.IGST,0)) as IGST,Sum(isnull(H.CGST,0)) as CGST,sum(isnull(SGST,0)) as SGST,
                                  Sum(isnull(H.InvoiceAmount,0)) as  InvoiceAmount
                                  from " + CteName + @" H
                                  Group By H.ProductName
                                  ";
                }
                else if(Format == ReportFormatJobInvoiceSummary.ProductGroupWise)
                {
                    mQry += @"    SELECT H.ProductGroupName as GroupOnText,sum(isnull(H.Qty,0)) as Qty,max(H.UnitName) as UnitName,max(H.DecimalPlaces) as DecimalPlaces,sum(isnull(H.DealQty,0)) as DealQty, 
                                  max(H.DealUnit) as DealUnit,max(H.AUDecimalPlaces) as AUDecimalPlaces,sum(isnull(H.Amount,0)) as Amount,sum(isnull(H.TaxableAmount,0)) as TaxableAmount,Sum(isnull(H.IGST,0)) as IGST,Sum(isnull(H.CGST,0)) as CGST,sum(isnull(SGST,0)) as SGST,
                                  Sum(isnull(H.InvoiceAmount,0)) as  InvoiceAmount
                                  from " + CteName + @" H
                                  Group By H.ProductGroupName
                                  ";
                }

            IEnumerable<JobInvoiceSummaryViewModels> JobInvoiceSummary = db.Database.SqlQuery<JobInvoiceSummaryViewModels>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate,SqlParameterJobWorker, SqlParameterProcess).ToList();


            return JobInvoiceSummary;

        }

      
        public void Dispose()
        {
        }
    }

     public class Display_JobInvoiceSummaryViewModel
    {
        public string Format { get; set; }
        public string ReportType { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }        
        public string JobWorker { get; set; }
        public string SiteIds { get; set; }
        public string DivisionIds { get; set; }
        public string Process { get; set; }
        public string TextHidden { get; set; }


    }

    [Serializable()]
    public class DisplayFilterJobInvoiceSummarySettings
    {
        public string Format { get; set; }
        public List<DisplayFilterJobInvoiceSummaryParameters> DisplayFilterJobInvoiceSummaryParameters { get; set; }
    }

    [Serializable()]
    public class DisplayFilterJobInvoiceSummaryParameters
    {
        public string ParameterName { get; set; }
        public bool IsApplicable { get; set; }
        public string Value { get; set; }
    }

    public class ReportFormatJobInvoiceSummary
    {
        public const string JobWorkerWise = "Job Worker Wise Summary";
        public const string MonthWise = "Month Wise Summary";
        public const string ProductWise= "Product Wise Summary";
        public const string ProductGroupWise = "Product Group Wise Summary";
    }

    public class ReportTypeJobInvoiceSummary
    {
        public const string JobInvoice = "Job Invoice";
        public const string JobInvoiceReturn = "Job Invoice Return/Debit Credit";   
    }



    public class JobInvoiceSummaryViewModels
    {
        public string  GroupOnText { get; set; }
        public decimal? Qty { get; set; }
        public string UnitName { get; set; }
        public decimal? DecimalPlaces { get; set; }
        public decimal? DealQty { get; set; }           
        public string DealUnit { get; set; }
        public decimal? AUDecimalPlaces { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TaxableAmount { get; set; }
        public decimal? IGST { get; set; }
        public decimal?  CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal?  InvoiceAmount { get; set; }
        public string Format { get; set; }
        public string ReportType { get; set; }

    }
   

}

