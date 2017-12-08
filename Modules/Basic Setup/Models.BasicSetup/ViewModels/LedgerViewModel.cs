using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.ViewModels
{
    public class LedgerViewModel 
    {
        [Key]
        public int LedgerId { get; set; }

        public int? DocHeaderId { get; set; }

        [Display(Name = "Doc Type"), Required]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site")]
        [Index("IX_Ledger_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public string SiteName { get; set; }


        [Display(Name = "HeaderLedgerAccount"), Required]
        public int? HeaderLedgerAccountId { get; set; }
        public string HeaderLedgerAccountName { get; set; }


        [Display(Name = "LedgerAccount"), Required]
        public int LedgerAccountId { get; set; }
        public string LedgerAccountName { get; set; }

        [Display(Name = "Contra Ledger Account")]
        public int? ContraLedgerAccountId { get; set; }
        public string ContraLedgerAccountName { get; set; }

        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        [Display(Name = "AmtDr")]
        public Decimal AmtDr { get; set; }

        [Display(Name = "AmtCr")]
        public Decimal AmtCr { get; set; }

        [MaxLength(250)]
        public string HeaderNarration { get; set; }

        [MaxLength(250)]
        public string Narration { get; set; }

        public string ContraText { get; set; }

        public int CreditDays { get; set; }

        public string Remark { get; set; }

        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}
