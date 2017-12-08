
// New namespace imports:

using Model.ViewModel;
using System;
namespace Model.ViewModels
{
    public class LedgerAccountOpeningViewModel
    {
        public int LedgerHeaderId { get; set; }
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public string DocNo { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string PartyDocNo { get; set; }
        public DateTime? PartyDocDate { get; set; }
        public Decimal Amount { get; set; }
        public string DrCr { get; set; }
        public string Narration { get; set; }
        public LedgerSettingViewModel LedgerSetting { get; set; }
    }
}
