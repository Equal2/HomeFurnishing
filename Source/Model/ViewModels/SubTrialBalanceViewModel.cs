using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SubTrialBalanceViewModel
    {
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public string LedgerAccountGroupName { get; set; }
        public decimal ? AmtDr { get; set; }
        public decimal ? AmtCr { get; set; }
        public string ReportType { get; set; }
        public string OpenReportType { get; set; }
    }

    public class LedgerAccountGroupBalanceViewModel
    {
        public int LedgerAccountId { get; set; }
        public string GroupOn { get; set; }
        public string LedgerAccountName { get; set; }
        public string LedgerAccountGroupName { get; set; }
        public decimal? AmtDr { get; set; }
        public decimal? AmtCr { get; set; }
    }

    public class SubTrialBalanceSummaryViewModel
    {
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public string LedgerAccountGroupName { get; set; }
        public decimal ? AmtDr { get; set; }
        public decimal ? AmtCr { get; set; }
        public decimal ? Opening { get; set; }
        public decimal ? Balance { get; set; }
        public string OpeningDrCr { get; set; }
        public string BalanceDrCr { get; set; }
        public string ReportType { get; set; }
        public string OpenReportType { get; set; }

        public decimal? TotalAmtDr { get; set; }
        public decimal? TotalAmtCr { get; set; }

        public decimal? TotalOpening { get; set; }
        public string TotalOpeningDrCr { get; set; }
        public decimal? TotalBalance { get; set; }
        public string TotalBalanceDrCr { get; set; }
    }

}
