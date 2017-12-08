using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Models.BasicSetup.ViewModels
{
    public class LedgerHeaderViewModel
    {
        public int LedgerHeaderId { get; set; }
        public int? DocHeaderId { get; set; }

        [Display(Name = "Doc Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Payment For")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PaymentFor { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]              
        public int DivisionId { get; set; }
        public string DivisionName{ get; set; }
        public int? ProcessId { get; set; }
        public string  ProcessName { get; set; }
        public int SiteId { get; set; }
        public string SiteName{ get; set; }

        [Display(Name = "LedgerAccount"),Required]        
        public int? LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? GodownId { get; set; }
        public string GodownName { get; set; }
        public Decimal? ExchangeRate { get; set; }
        public int? CreditDays { get; set; }

        [MaxLength(20)]
        public string AdjustmentType { get; set; }
        public string Narration { get; set; }
        public string NarrationName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int Status { get; set; }
        public int DocumentCategoryId { get; set; }

        [Display(Name = "AccountName")]
        public string AccountName { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }

    }
}
