using System;
using System.ComponentModel.DataAnnotations;

namespace Models.BasicSetup.ViewModels
{
    public class PaymentModeViewModel
    {
        public int PaymentModeId { get; set; }
        public int DocTypeId { get; set; }
        public string PaymentModeName { get; set; }
        public int? PaymentModeLedgerAccountId { get; set; }
        public int? LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public int? SiteId { get; set; }
        public int? DivisionId { get; set; }
        
    }
}
