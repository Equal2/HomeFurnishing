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
    public interface IDisplay_ProdOrderBalanceService : IDisposable
    {
        IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter);
    
        IEnumerable<ProdOrderBalancelOrderNoWiseViewModel> ProdOrderBalanceDetail(ProdDisplayFilterSettings Settings);
    }

    public class Display_ProdOrderBalanceService : IDisplay_ProdOrderBalanceService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

      

        public Display_ProdOrderBalanceService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }

        public IQueryable<ComboBoxResult> GetFilterFormat(string term, int? filter)
        {
            List<ComboBoxResult> ResultList = new List<ComboBoxResult>();
            ResultList.Add(new ComboBoxResult { id = ProdReportFormat.CustomerWise, text = ProdReportFormat.CustomerWise });
            ResultList.Add(new ComboBoxResult { id = ProdReportFormat.MonthWise, text = ProdReportFormat.MonthWise });
            ResultList.Add(new ComboBoxResult { id = ProdReportFormat.ProdTypeWise, text = ProdReportFormat.ProdTypeWise });
            ResultList.Add(new ComboBoxResult { id = ProdReportFormat.ProductNatureWiseSummary, text = ProdReportFormat.ProductNatureWiseSummary });
            ResultList.Add(new ComboBoxResult { id = ProdReportFormat.OrderNoWise, text = ProdReportFormat.OrderNoWise });
            ResultList.Add(new ComboBoxResult { id = ProdReportFormat.ProcessWise, text = ProdReportFormat.ProcessWise });

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


     
      
        
        public IEnumerable<ProdOrderBalancelOrderNoWiseViewModel> ProdOrderBalanceDetail(ProdDisplayFilterSettings Settings)
        {
            var FormatSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "Format" select H).FirstOrDefault();
            var SiteSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "Site" select H).FirstOrDefault();
            var DivisionSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "Division" select H).FirstOrDefault();
            var FromDateSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "FromDate" select H).FirstOrDefault();
            var ToDateSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "ToDate" select H).FirstOrDefault();
            var ProductNatureSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "ProductNature" select H).FirstOrDefault();
            var ProductTypeSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "ProductType" select H).FirstOrDefault();
            var CustomerSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "Customer" select H).FirstOrDefault();
            var ProcessSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "Process" select H).FirstOrDefault();
            var TextHiddenSetting = (from H in Settings.ProdDisplayFilterParameters where H.ParameterName == "TextHidden" select H).FirstOrDefault();



            string Format = FormatSetting.Value;
            string SiteId = SiteSetting.Value;
            string DivisionId = DivisionSetting.Value;
            string FromDate = FromDateSetting.Value;
            string ToDate = ToDateSetting.Value;
            string ProductNature = ProductNatureSetting.Value;
            string ProductType = ProductTypeSetting.Value;
            string Customer = CustomerSetting.Value;
            string Process = ProcessSetting.Value;
            string TextHidden = TextHiddenSetting.Value;


            string mQry, mCondStr,mCondLinestr="";


            mCondStr = "";
            if (!string.IsNullOrEmpty(SiteId)) mCondStr += "  AND H.SiteId = @Site ";
            if (!string.IsNullOrEmpty(DivisionId)) mCondStr += "  AND H.DivisionId = @Division ";
            if (!string.IsNullOrEmpty(FromDate)) mCondStr += "  AND H.DocDate >= @FromDate ";
            if (!string.IsNullOrEmpty(ToDate)) mCondStr += "  AND H.DocDate <= @ToDate ";
            if (!string.IsNullOrEmpty(Customer)) mCondStr += " AND H.BuyerId= @Customer ";

            if (!string.IsNullOrEmpty(ProductNature)) mCondLinestr += " AND PT.ProductNatureId = @ProductNature ";
            if (!string.IsNullOrEmpty(ProductType)) mCondLinestr += " AND PT.ProductTypeId = @ProductType ";            
            if (!string.IsNullOrEmpty(Process)) mCondLinestr += " AND POL.ProcessId = @Process ";


            SqlParameter SqlParameterSiteId = new SqlParameter("@Site", !string.IsNullOrEmpty(SiteId) ? SiteId : (object)DBNull.Value);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@Division", !string.IsNullOrEmpty(DivisionId) ? DivisionId : (object)DBNull.Value);
            SqlParameter SqlParameterFromDate = new SqlParameter("@FromDate", FromDate);
            SqlParameter SqlParameterToDate = new SqlParameter("@ToDate", ToDate);
            SqlParameter SqlParameterProductNature = new SqlParameter("@ProductNature", !string.IsNullOrEmpty(ProductNature) ? ProductNature : (object)DBNull.Value);
            SqlParameter SqlParameterProductType = new SqlParameter("@ProductType", !string.IsNullOrEmpty(ProductType) ? ProductType : (object)DBNull.Value);
            SqlParameter SqlParameterCustomer = new SqlParameter("@Customer", !string.IsNullOrEmpty(Customer) ? Customer : (object)DBNull.Value);
            SqlParameter SqlParameterProcess = new SqlParameter("@Process", !string.IsNullOrEmpty(Process) ? Process : (object)DBNull.Value);

            mQry = @"With CteProdOrderBalance as
                            (
                                               SELECT
                                                    H.DivisionId, H.SiteId,H.ProdOrderHeaderId,H.DocTypeId,PS.ProcessName AS Process,H.DocDate AS PlanDate, H.DocNo AS PlanNo,H.DueDate AS DueDate,
                                                    P.ProductName,D1.Dimension1Name,D2.Dimension2Name,D3.Dimension3Name,D4.dimension4Name, 
                                                    Pp.Name AS BuyerName,H.BuyerId,POL.ProdOrderLineId,POL.Specification, POL.ProcessId, isnull(POL.Qty,0) AS PlanQty,	
                                                    U.UnitName AS Unit,(CASE WHEN (Isnull(POL.Qty,0)  - isnull(POCL.CancelQty,0) - IsNull(VGoodsReceipt.Qty,0)) < 0 THEN 0
                                                    ELSE Isnull(POL.Qty,0)  - isnull(POCL.CancelQty,0) - IsNull(VGoodsReceipt.Qty,0) END)   AS BalQty,   
                                                    H.Remark AS Remark,PT.ProductNatureId,PN.ProductNatureName,PT.ProductTypeId,PT.ProductTypeName
                                                     FROM 
                                                    (
                                                    SELECT H.* FROM Web.ProdOrderHeaders H WITH (NoLock)
		                                                    WHERE 1=1 " + mCondStr + @"
                                                    ) H
                                                    LEFT JOIN Web.DocumentTypes Dt WITH (Nolock) ON H.DocTypeId = Dt.DocumentTypeId
                                                    LEFT JOIN Web.DocumentCategories DC WITH (Nolock) ON DC.DocumentCategoryId=Dt.DocumentCategoryId
                                                    LEFT JOIN Web.ProdOrderLines POL WITH (Nolock) ON POL.ProdOrderHeaderId=H.ProdOrderHeaderId
                                                    LEFT JOIN 
                                                    (
	                                                    SELECT L.ProdOrderLineId,isnull(sum(L.Qty),0) AS CancelQty FROM Web._ProdOrderCancelHeaders H WITH (Nolock) 
	                                                    LEFT JOIN Web.ProdOrderCancelLines L WITH (Nolock) ON L.ProdOrderCancelHeaderId=H.ProdOrderCancelHeaderId
	                                                    WHERE 1=1 AND L.ProdOrderLineId IS NOT NULL
	                                                    GROUP BY L.ProdOrderLineId
                                                    ) POCL ON POCL.ProdOrderLineId=POL.ProdOrderLineId
                                                    LEFT JOIN 
                                                    (
		                                                    SELECT 
		                                                    Max(H.DocDate) AS LastOrderDate,
		                                                    L.ProdOrderLineId, 
		                                                    sum(isnull(L.Qty,0) + isnull(JOAH.AmendmentQty,0) - isnull(JOCL.CancelQty,0)) AS Qty
		
		                                                     FROM 
		                                                    (
		                                                    SELECT  H.*	FROM Web.JobOrderHeaders H WITH (NoLock)
			                                                    WHERE 1=1    
			                                                    AND ( @FromDate IS NULL OR H.DocDate >= @FromDate)     
			                                                    AND ( @ToDate IS NULL OR H.DocDate <= @ToDate)     
		   	                                                    AND ( @Site IS NULL OR H.SiteId IN (SELECT Items FROM  Web.[Split] (@Site, ',')))     
			                                                    AND ( @Division IS NULL OR H.DivisionId IN (SELECT Items FROM  Web.[Split] (@Division, ',')))
		                                                    )  AS H
		                                                    LEFT JOIN Web.DocumentTypes DT WITH (Nolock) ON DT.DocumentTypeId=H.DocTypeId
		                                                    LEFT JOIN Web.JobOrderLines L WITH (Nolock) ON L.JobOrderHeaderId=H.JobOrderHeaderId
		                                                    LEFT JOIN
		                                                    (
		                                                        SELECT L.JobOrderLineId,sum(isnull(L.Qty,0)) AS CancelQty
			                                                    FROM Web.JobOrderCancelHeaders H WITH (NoLock)
			                                                    LEFT JOIN Web.JobOrderCancelLines L  WITH (NoLock) ON L.JobOrderCancelHeaderId = H.JobOrderCancelHeaderId 
			                                                    GROUP BY  L.JobOrderLineId
		                                                    ) JOCL ON JOCL.JobOrderLineId=L.JobOrderLineId
		                                                    LEFT JOIN 
		                                                    (
		                                                            SELECT L.JobOrderLineId,sum(isnull(L.Qty,0)) AS AmendmentQty
				                                                    FROM  Web.JobOrderAmendmentHeaders H WITH (NoLock)
				                                                    LEFT JOIN Web.JobOrderQtyAmendmentLines L WITH (NoLock) ON L.JobOrderAmendmentHeaderId = H.JobOrderAmendmentHeaderId 
				                                                    GROUP BY L.JobOrderLineId
		                                                    ) JOAH ON JOAH.JobOrderLineId=L.JobOrderLineId
		                                                    LEFT JOIN 
		                                                    (
		                                                            SELECT L.JobOrderLineId,sum(isnull(L.Rate,0)) AS AmendmentRate,sum(isnull(L.Amount,0)) AS Amount
				                                                    FROM Web.JobOrderAmendmentHeaders H WITH (NoLock)
				                                                    LEFT JOIN Web.JobOrderRateAmendmentLines L WITH (NoLock) ON L.JobOrderAmendmentHeaderId = H.JobOrderAmendmentHeaderId 
				                                                    GROUP BY L.JobOrderLineId
				
		                                                    )JORAH ON JORAH.JobOrderLineId=L.JobOrderLineId
		                                                    WHERE L.JobOrderLineId IS NOT NULL
		                                                    GROUP BY L.ProdOrderLineId
                                                    )  AS VGoodsReceipt ON POL.ProdOrderLineId = VGoodsReceipt.ProdOrderLineId
                                                    LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId=POL.ProductId
                                                    LEFT JOIN Web.Units U WITH (Nolock) ON U.UnitId=P.UnitId
                                                    LEFT JOIN Web.Dimension1 D1 WITH (Nolock) ON D1.Dimension1Id=POL.Dimension1Id
                                                    LEFT JOIN Web.Dimension2 D2 WITH (Nolock) ON D2.Dimension2Id=POL.Dimension2Id
                                                    LEFT JOIN web.Dimension3 D3 WITH (Nolock) ON D3.Dimension3Id=POL.Dimension3Id
                                                    LEFT JOIN Web.Dimension4 D4 WITH (Nolock) ON D4.Dimension4Id=POL.Dimension4Id
                                                    LEFT JOIN Web.Processes PS WITH (Nolock) ON PS.ProcessId=POL.ProcessId
                                                    LEFT JOIN web.People Pp WITH (Nolock) ON Pp.PersonID=H.BuyerId
                                                    LEFT JOIN web.ProductGroups PG WITH (Nolock) ON PG.ProductGroupId=P.ProductGroupId
                                                    LEFT JOIN Web.ProductTypes PT WITH (Nolock) ON PT.ProductTypeId=PG.ProductTypeId
                                                    LEFT JOIN web.ProductNatures PN WITH (Nolock) ON PN.ProductNatureId=PT.ProductNatureId
                                                    WHERE 1=1 AND (Isnull(POL.Qty,0) - isnull(POCL.CancelQty,0) - IsNull(VGoodsReceipt.Qty,0))  > 0"
                                    + mCondLinestr + ") ";

            if (Format == ProdReportFormat.OrderNoWise || Format =="" || string.IsNullOrEmpty(Format))
            {
                mQry += @"  Select H.Process,H.ProdOrderHeaderId, H.DocTypeId, H.PlanNo,format(H.PlanDate,'dd/MMM/yyyy') as PlanDate,format(H.DueDate,'dd/MMM/yyyy') as DueDate, H.BuyerName,H.ProductName, H.Specification, H.Unit,
                            H.PlanQty, H.BalQty,H.Dimension1Name,H.Dimension2Name,H.Dimension3Name ,H.dimension4Name,H.Remark
                            From CteProdOrderBalance H 
                            Order By H.PlanDate, H.DocTypeId, H.PlanNo
                        ";
            }
            else if (Format == ProdReportFormat.CustomerWise)
            {
                
                   mQry += @"  Select H.BuyerId, Max(H.BuyerName) BuyerName, Sum(H.PlanQty) as PlanQty,max(H.Unit) as Unit, Sum(H.BalQty) as BalQty,'Plan No Wise' as Format
                            From CteProdOrderBalance H 
                            Group By H.BuyerId
                            Order By Max(H.BuyerName) 
                         ";
            }
            else if (Format == ProdReportFormat.ProdTypeWise)
            {
                mQry += @"  Select H.ProductTypeId, Max(H.ProductTypeName) ProductTypeName, Sum(H.PlanQty) as PlanQty,max(H.Unit) as Unit, Sum(H.BalQty) as BalQty,'Plan No Wise' as Format                            
                            From CteProdOrderBalance H 
                            Group By H.ProductTypeId
                            Order By Max(H.ProductTypeName)
                         ";
            }
            else if (Format == ProdReportFormat.ProductNatureWiseSummary)
            {
                mQry += @"  Select H.ProductNatureId, Max(H.ProductNatureName) ProductNatureName, Sum(H.BalQty) as BalQty, Sum(H.PlanQty) as PlanQty,max(H.Unit) as Unit,'Plan No Wise' as Format                            
                            From CteProdOrderBalance H 
                            Group By H.ProductNatureId
                            Order By  Max(H.ProductNatureName)
                         ";
            }
            else if (Format == ProdReportFormat.MonthWise )
            {
                mQry += @"  Select format(Max(DATEADD(dd,-(DAY(H.PlanDate)-1),H.PlanDate)),'dd/MMM/yyyy') AS FromDate,format(Max(DATEADD(dd,-(DAY(DATEADD(mm,1,H.PlanDate))),DATEADD(mm,1,H.PlanDate))),'dd/MMM/yyyy') as ToDate, Max(Right(Convert(Varchar,H.PlanDate,106),8)) as Month,sum(H.PlanQty) as PlanQty,max(H.Unit) as Unit,Sum(H.BalQty) as BalQty,'Plan No Wise' as Format                            
                            From CteProdOrderBalance H 
                            Group By Substring(convert(NVARCHAR,H.PlanDate,11),0,6)
                            Order by Substring(convert(NVARCHAR,H.PlanDate,11),0,6)
                         ";
            }
            else if (Format == ProdReportFormat.ProcessWise)
            {
                mQry += @"  Select H.ProcessId, Max(H.Process) Process, sum(H.PlanQty) as PlanQty,max(H.Unit) as Unit,Sum(H.BalQty) as BalQty,'Plan No Wise' as Format                            
                            From CteProdOrderBalance H 
                            Group By H.ProcessId
                            Order By  Max(H.Process)
                         ";
            }

            IEnumerable<ProdOrderBalancelOrderNoWiseViewModel> TrialBalanceSummaryList = db.Database.SqlQuery<ProdOrderBalancelOrderNoWiseViewModel>(mQry, SqlParameterSiteId, SqlParameterDivisionId, SqlParameterFromDate, SqlParameterToDate, SqlParameterProductNature, SqlParameterProductType, SqlParameterCustomer, SqlParameterProcess).ToList();


            return TrialBalanceSummaryList;

        }

      
        public void Dispose()
        {
        }
    }

     public class Display_ProdOrderBalanceViewModel
    {
        public string Format { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ProductNature { get; set; }
        public string ProductType { get; set;}
        public string Customer { get; set; }
        public string SiteIds { get; set; }
        public string DivisionIds { get; set; }  
        public string Process { get; set; }
        public string TextHidden { get; set; }


    }

    [Serializable()]
    public class ProdDisplayFilterSettings
    {
        public string Format { get; set; }
        public List<ProdDisplayFilterParameters> ProdDisplayFilterParameters { get; set; }
    }

    [Serializable()]
    public class ProdDisplayFilterParameters
    {
        public string ParameterName { get; set; }
        public bool IsApplicable { get; set; }
        public string Value { get; set; }
    }

    public class ProdReportFormat
    {
        public const string CustomerWise = "Customer Wise Summary";
        public const string MonthWise = "Month Wise Summary";
        public const string ProdTypeWise = "Product Type Wise Summary";
        public const string ProductNatureWiseSummary = "Product Nature Wise Summary";
        public const string OrderNoWise = "Plan No Wise";
        public const string ProcessWise = "Process Wise Symmary";
    }
    public class ProdOrderBalancelOrderNoWiseViewModel
    {
        public int ? DivisionId { get; set; }
        public int? SiteId { get; set; }
        public int? ProdOrderHeaderId { get; set; }
        public int? DocTypeId { get; set; }
        public string Process { get; set; }
        public int ? ProcessId { get; set; }
        public string PlanDate { get; set; }
        public string PlanNo { get; set; }
        public string DueDate { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string dimension4Name { get; set; }
        public string BuyerName { get; set; }
        public int ? BuyerId { get; set; }
        public int ? ProdOrderLineId { get; set; }
        public string Specification { get; set; }
        public decimal? PlanQty { get; set; }
        public string Unit { get; set; }
        public decimal? BalQty { get; set; }
        public string Remark { get; set; }
        public string Format { get; set; }

        public int ? ProductNatureId { get; set; }
        public string ProductNatureName { get; set; }

        public int? ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Month { get; set; }
    }

}

