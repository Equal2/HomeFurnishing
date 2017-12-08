using Model.Models;
using Model.ViewModel;
using Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class FinancialDisplayViewModel
    {
        public string ReportType { get; set; }
        public string DisplayType { get; set; }
        public string SiteIds { get; set; }
        public string DivisionIds { get; set; }
        public string CostCenterIds { get; set; }
        public string DrCr { get; set; }
        public bool IsIncludeZeroBalance { get; set; }
        public bool IsShowContraAccount { get; set; }
        public bool IsIncludeOpening { get; set; }
        public bool IsShowDetail { get; set; }
        public bool IsFullHierarchy { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public ReportHeaderCompanyDetail ReportHeaderCompanyDetail { get; set; }
        public int? LedgerAccount { get; set; }
        public string LedgerAccountGroup { get; set; }
         
    }
}
