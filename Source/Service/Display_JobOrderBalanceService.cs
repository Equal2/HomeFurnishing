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
    public interface IDisplay_JobOrderBalanceService : IDisposable
    {
        IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter);
    
        IEnumerable<JobOrderBalancelOrderNoWiseViewModel> JobOrderBalanceDetail(DisplayFilterSettings Settings);
    }

    public class Display_JobOrderBalanceService : IDisplay_JobOrderBalanceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

      

        public Display_JobOrderBalanceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }

        public IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = ReportFormat.JobWorkerWise, text = ReportFormat.JobWorkerWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormat.MonthWise, text = ReportFormat.MonthWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormat.ProdTypeWise, text = ReportFormat.ProdTypeWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormat.ProductNatureWiseSummary, text = ReportFormat.ProductNatureWiseSummary });
            ResultList.Add(new ComboBoxResult { id = ReportFormat.OrderNoWise, text = ReportFormat.OrderNoWise });
            ResultList.Add(new ComboBoxResult { id = ReportFormat.ProcessWise, text = ReportFormat.ProcessWise });

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


     
      
        
        public IEnumerable<JobOrderBalancelOrderNoWiseViewModel> JobOrderBalanceDetail(DisplayFilterSettings Settings)
        {
            var FormatSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var SiteSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var ProductNatureSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "ProductNature" select H).FirstOrDefault();
            var ProductTypeSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "ProductType" select H).FirstOrDefault();
            var JobWorkerSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "JobWorker" select H).FirstOrDefault();
            var ProcessSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in Settings.DisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();



            string Format = FormatSetting.Value;
            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string ProductNature = ProductNatureSetting.Value;
            string ProductType = ProductTypeSetting.Value;
            string JobWorker = JobWorkerSetting.Value;
            string Process = ProcessSetting.Value;
            string TextHidden = TextHiddenSetting.Value;


            string mQry, mCondStr;


            mCondStr = "";
            if (!string.IsNullOrEmpty(SiteId)) mCondStr += " AND H.SiteId = @Site ";
            if (!string.IsNullOrEmpty(DivisionId)) mCondStr += " AND H.DivisionId = @Division ";
            if (!string.IsNullOrEmpty(FromDate)) mCondStr += " AND H.DocDate >= @FromDate ";
            if (!string.IsNullOrEmpty(ToDate)) mCondStr += " AND H.DocDate <= @ToDate ";
            if (!string.IsNullOrEmpty(ProductNature)) mCondStr += " AND PT.ProductNatureId = @ProductNature ";
            if (!string.IsNullOrEmpty(ProductType)) mCondStr += " AND PT.ProductTypeId = @ProductType ";
            if (!string.IsNullOrEmpty(JobWorker)) mCondStr += " AND H.JobWorkerId = @JobWorker ";
            if (!string.IsNullOrEmpty(Process)) mCondStr += " AND H.ProcessId = @Process ";


            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterProductNature = new SqlParameter("@ProductNature", !string.IsNullOrEmpty(ProductNature) ? ProductNature : (object)DBNull.Value);
            SqlParameter SqlParameterProductType = new SqlParameter("@ProductType", !string.IsNullOrEmpty(ProductType) ? ProductType : (object)DBNull.Value);
            SqlParameter SqlParameterJobWorker = new SqlParameter("@JobWorker", !string.IsNullOrEmpty(JobWorker) ? JobWorker : (object)DBNull.Value);
            SqlParameter SqlParameterProcess = new SqlParameter("@Process", !string.IsNullOrEmpty(Process) ? Process : (object)DBNull.Value);



            mQry = @"With CteJobOrderBalance as
                            (
                                    SELECT   H.JobOrderHeaderId, H.DocTypeId,
                                     H.DocNo,H.DocDate AS DocDate,H.DueDate AS DueDate, H.JobWorkerId,P.Name+' | '+P.Code as SupplierName,L.ProductId,PD.ProductName,
                                     L.Specification, U.UnitName, AU.UnitName AS DealUnitName, PG.ProductTypeId, PT.ProductTypeName, PT.ProductNatureId, PN.ProductNatureName,H.ProcessId,PS.ProcessName,  
                                     isnull(L.Qty,0) - IsNull(VCancel.CancelQty,0) + IsNull(VQtyAmendment.AmendmentQty,0) as OrderQty,       
                                    isnull(L.Qty,0) - (IsNull(VReceive.ReceiveQty,0) - IsNULL(VReturn.ReturnQty,0)) - IsNull(VCancel.CancelQty,0) + IsNull(VQtyAmendment.AmendmentQty,0) AS BalanceQty,
                                     convert(DECIMAL(18,4),isnull(ROUND(CAST(L.DealQty AS float) / CAST((CASE WHEN isnull(L.Qty,0)=0 THEN 1 ELSE L.Qty END) AS float), 4)*(isnull(L.Qty,0) - (IsNull(VReceive.ReceiveQty,0) - IsNULL(VReturn.ReturnQty,0)) - IsNull(VCancel.CancelQty,0) + IsNull(VQtyAmendment.AmendmentQty,0)),0)) AS BalanceDealQty,
                                    --(isnull(L.Qty,0) - (IsNull(VReceive.ReceiveQty,0) - IsNULL(VReturn.ReturnQty,0)) - IsNull(VCancel.CancelQty,0) + IsNull(VQtyAmendment.AmendmentQty,0)) * (L.Rate + IsNULL(VRateAmendment.AmendmentRate,0)) AS BalanceAmount,
                                     convert(DECIMAL(18,4),isnull(ROUND(CAST(L.DealQty AS float) / CAST((CASE WHEN isnull(L.Qty,0)=0 THEN 1 ELSE L.Qty END) AS float), 4)*(isnull(L.Qty,0) - (IsNull(VReceive.ReceiveQty,0) - IsNULL(VReturn.ReturnQty,0)) - IsNull(VCancel.CancelQty,0) + IsNull(VQtyAmendment.AmendmentQty,0)),0)) * (L.Rate + IsNULL(VRateAmendment.AmendmentRate,0)) AS BalanceAmount,
                                      isnull(L.Rate ,0) as Rate,
                                    D1.Dimension1Name as Dimension1,D2.Dimension2Name as Dimension2,D3.Dimension3Name as Dimension3,D4.Dimension4Name as Dimension4,L.LotNo ,
                                    Poh.DocNo As ProdOrderNo
                                    FROM Web.JobOrderHeaders H WITH (NoLock) 
                                    LEFT JOIN Web.JobOrderLines L WITH (NoLock) ON L.JobOrderHeaderId = H.JobOrderHeaderId
                                    LEFT JOIN (                                        
	                                    SELECT L.JobOrderLineId, Sum(L.Qty + IsNull(L.LossQty,0)) AS ReceiveQty
	                                    FROM Web.JobReceiveLines L WITH (NoLock)
	                                    GROUP BY L.JobOrderLineId
                                    ) AS VReceive ON L.JobOrderLineId = VReceive.JobOrderLineId
                                    LEFT JOIN (
	                                    SELECT L.JobOrderLineId, Sum(L.Qty) AS CancelQty
	                                    FROM Web.JobOrderCancelLines L WITH (NoLock)
	                                    GROUP BY L.JobOrderLineId
                                    ) AS VCancel ON L.JobOrderLineId = VCancel.JobOrderLineId
                                    LEFT JOIN (
	                                    SELECT Jrl.JobOrderLineId, Sum(L.Qty) AS ReturnQty
	                                    FROM Web.JobReturnLines L WITH (NoLock)
	                                    LEFT JOIN Web.JobReceiveLines Jrl WITH (NoLock) ON L.JobReceiveLineId = Jrl.JobReceiveLineId
	                                    GROUP BY Jrl.JobOrderLineId
                                    ) AS VReturn ON L.JobOrderLineId = VReturn.JobOrderLineId
                                    LEFT JOIN (
	                                    SELECT L.JobOrderLineId, Sum(L.Qty) AS AmendmentQty
	                                    FROM Web.JobOrderQtyAmendmentLines L WITH (NoLock)
	                                    GROUP BY L.JobOrderLineId
                                    ) AS VQtyAmendment ON L.JobOrderLineId = VQtyAmendment.JobOrderLineId
                                    LEFT JOIN (
	                                    SELECT L.JobOrderLineId, Sum(L.Rate) AS AmendmentRate
	                                    FROM Web.JobOrderRateAmendmentLines L WITH (NoLock)
	                                    GROUP BY L.JobOrderLineId
                                    ) AS VRateAmendment ON L.JobOrderLineId = VRateAmendment.JobOrderLineId
                                    LEFT JOIN Web.People P WITH (Nolock) ON P.PersonID=H.JobWorkerId
                                    LEFT JOIN Web.Processes PS WITH (Nolock) ON PS.ProcessId=H.ProcessId
                                    LEFT JOIN Web.Products PD WITH (Nolock) ON PD.ProductId=L.ProductId
                                    LEFT JOIN Web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=PD.ProductGroupId
                                    LEFT JOIN web.ProductTypes PT WITH (Nolock) ON PT.ProductTypeId=PG.ProductTypeId
                                    LEFT JOIN web.ProductNatures PN WITH (Nolock) ON PN.ProductNatureId=PT.ProductNatureId                            
                                    LEFT JOIN Web.Units U WITH (Nolock) ON U.UnitId=PD.UnitId
                                    LEFT JOIN Web.Units Au WITH (Nolock) ON Au.UnitId=L.DealUnitId 
                                    LEFT JOIN web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id=L.Dimension1Id
                                    LEFT JOIN web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id=L.Dimension2Id
                                    LEFT JOIN web.Dimension3 D3 WITH (Nolock) ON D3.Dimension3Id=L.Dimension3Id
                                    LEFT JOIN web.Dimension4 D4 WITH (Nolock) ON D4.Dimension4Id=L.Dimension4Id
                                    LEFT JOIN Web.ProdOrderLines Pol On L.ProdOrderLineId = Pol.ProdOrderLineId
                                    LEFT JOIN Web.ProdOrderHeaders Poh On Pol.ProdOrderHeaderId = Poh.ProdOrderHeaderId
                                    WHERE 1=1 and  (isnull(L.Qty,0) - (IsNull(VReceive.ReceiveQty,0) - IsNULL(VReturn.ReturnQty,0)) - IsNull(VCancel.CancelQty,0) + IsNull(VQtyAmendment.AmendmentQty,0)) >0 "
                                    + mCondStr + ") ";

            if (Format == ReportFormat.OrderNoWise || Format =="" || string.IsNullOrEmpty(Format))
            {
                mQry += @"  Select H.JobOrderHeaderId, H.DocTypeId, H.DocNo,format(H.DocDate,'dd/MMM/yyyy') as DocDate,format(H.DueDate,'dd/MMM/yyyy') as DueDate, H.SupplierName,H.ProductName, H.Specification, H.UnitName,
                            H.DealUnitName,H.OrderQty,H.BalanceQty, H.BalanceDealQty,H.BalanceAmount,H.Rate,H.Dimension1,H.Dimension2,H.Dimension3 ,H.Dimension4, H.ProdOrderNo,H.LotNo 
                            From cteJobOrderBalance H 
                            Order By H.DocDate, H.DocTypeId, H.DocNo
                        ";
            }
            else if (Format == ReportFormat.JobWorkerWise)
            {
                
                   mQry += @"  Select H.JobWorkerId, Max(H.SupplierName) SupplierName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                            Max(H.DealUnitName) As DealUnitName, Max(H.UnitName) As UnitName
                            From cteJobOrderBalance H 
                            Group By H.JobWorkerId
                            Order By Max(H.SupplierName) 
                         ";
            }
            else if (Format == ReportFormat.ProdTypeWise)
            {
                mQry += @"  Select H.ProductTypeId, Max(H.ProductTypeName) ProductTypeName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format, 
                            Max(H.DealUnitName) As DealUnitName, Max(H.UnitName) As UnitName                            
                            From cteJobOrderBalance H 
                            Group By H.ProductTypeId
                            Order By Max(H.ProductTypeName)
                         ";
            }
            else if (Format == ReportFormat.ProductNatureWiseSummary)
            {
                mQry += @"  Select H.ProductNatureId, Max(H.ProductNatureName) ProductNatureName,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                            Max(H.DealUnitName) As DealUnitName, Max(H.UnitName) As UnitName                            
                            From cteJobOrderBalance H 
                            Group By H.ProductNatureId
                            Order By  Max(H.ProductNatureName)
                         ";
            }
            else if (Format == ReportFormat.MonthWise )
            {
                mQry += @"  Select format(Max(DATEADD(dd,-(DAY(H.DocDate)-1),H.DocDate)),'dd/MMM/yyyy') AS FromDate,format(Max(DATEADD(dd,-(DAY(DATEADD(mm,1,H.DocDate))),DATEADD(mm,1,H.DocDate))),'dd/MMM/yyyy') as ToDate, Max(Right(Convert(Varchar,H.DocDate,106),8)) as Month,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty, Sum(H.BalanceAmount) as BalanceAmount,'Order No Wise' as Format,
                            Max(H.DealUnitName) As DealUnitName, Max(H.UnitName) As UnitName                            
                            From cteJobOrderBalance H 
                            Group By Substring(convert(NVARCHAR,H.DocDate,11),0,6)
                            Order by Substring(convert(NVARCHAR,H.DocDate,11),0,6)
                         ";
            }
            else if (Format == ReportFormat.ProcessWise)
            {
                mQry += @"  Select H.ProcessId, Max(H.ProcessName) ProcessName, Sum(H.BalanceQty) as BalanceQty, Sum(H.BalanceDealQty) as BalanceDealQty,Sum(H.OrderQty) as OrderQty,Sum(H.BalanceAmount) as BalanceAmount,'Job Worker Wise Summary' as Format,
                            Max(H.DealUnitName) As DealUnitName, Max(H.UnitName) As UnitName                            
                            From cteJobOrderBalance H 
                            Group By H.ProcessId
                            Order By  Max(H.ProcessName)
                         ";
            }

            IEnumerable<JobOrderBalancelOrderNoWiseViewModel> TrialBalanceSummaryList = db.Database.SqlQuery<JobOrderBalancelOrderNoWiseViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterProductNature, SqlParameterProductType, SqlParameterJobWorker, SqlParameterProcess).ToList();


            return TrialBalanceSummaryList;

        }

      
        public void Dispose()
        {
        }
    }

     public class Display_JobOrderBalanceViewModel
    {
        public string Format { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ProductNature { get; set; }
        public string ProductType { get; set;}
        public string JobWorker { get; set; }
        public string SiteIds { get; set; }
        public string DivisionIds { get; set; }  
        public string Process { get; set; }
        public string TextHidden { get; set; }
        public ReportHeaderCompanyDetail ReportHeaderCompanyDetail { get; set; }


    }

    [Serializable()]
    public class DisplayFilterSettings
    {
        public string Format { get; set; }
        public List<DisplayFilterParameters> DisplayFilterParameters { get; set; }
    }

    [Serializable()]
    public class DisplayFilterParameters
    {
        public string ParameterName { get; set; }
        public bool IsApplicable { get; set; }
        public string Value { get; set; }
    }

    public class ReportFormat
    {
        public const string JobWorkerWise = "Job Worker Wise Summary";
        public const string MonthWise = "Month Wise Summary";
        public const string ProdTypeWise = "Product Type Wise Summary";
        public const string ProductNatureWiseSummary = "Product Nature Wise Summary";
        public const string OrderNoWise = "Order No Wise";
        public const string ProcessWise = "Process Wise Symmary";
    }
    public class JobOrderBalancelOrderNoWiseViewModel
    {
        public int? JobOrderHeaderId { get; set; }
        public int? DocTypeId { get; set; }
        public string DocNo { get; set; }
        public string DocDate { get; set; }

        public string DueDate { get; set; }

        public string SupplierName { get; set; }

        public string ProductName { get; set; }

        public int? ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }

        public int? ProductNatureId { get; set; }
        public string ProductNatureName { get; set; }
        public string Month { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int? JobWorkerId { get; set; }       
        public string Specification { get; set; }

        public int? ProcessId { get; set; }
         public string ProcessName { get; set; }          
        public string UnitName { get; set; }
        public string DealUnitName { get; set; }

        public decimal? OrderQty { get; set; }
        public decimal? BalanceQty { get; set; }
        public decimal? BalanceDealQty { get; set; }
        public decimal? BalanceAmount { get; set; }
        public string Format { get; set; }
        public string Dimension1 { get; set; }
        public string Dimension2 { get; set; }
        public string Dimension3 { get; set; }
        public string Dimension4 { get; set; }
        public string ProdOrderNo { get; set; }
        public string LotNo { get; set; }
        
        public decimal? Rate { get; set; }


    }

}

