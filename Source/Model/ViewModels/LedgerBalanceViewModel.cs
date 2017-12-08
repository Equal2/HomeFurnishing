using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class LedgerBalanceViewModel
    {
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public string ContraLedgerAccountName { get; set; }
        public string DocNo { get; set; }
        public string DocumentTypeShortName { get; set; }
        public string DocDate { get; set; }
        public int DocTypeId { get; set; }
        public string Narration { get; set; }
        public int LedgerId { get; set; }
        public decimal ? AmtDr { get; set; }
        public decimal? Balance { get; set; }
        public string BalanceType { get; set; }
        public int DocHeaderId { get; set; }
        public decimal ? AmtCr { get; set; }

        public string BalanceDrCr { get; set; }

        public string SiteText { get; set; }
        public string DivisionText { get; set; }
        public string ReportType { get; set; }
        public string OpenReportType { get; set; }
        public decimal? TotalAmtDr { get; set; }
        public decimal? TotalAmtCr { get; set; }

    }
}
