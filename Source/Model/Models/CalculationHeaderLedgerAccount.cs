using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class CalculationHeaderLedgerAccount : EntityBase, IHistoryLog
    {

        [Key]
        public int CalculationHeaderLedgerAccountId { get; set; }

        [ForeignKey("Calculation")]
        [Display(Name = "Calculation")]        
        public int CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }


        [Index("IX_CalculationHeaderLedgerAccount_UniqueRow", IsUnique = true, Order = 1)]
        [ForeignKey("CalculationFooter")]
        public int CalculationFooterId { get; set; }
        public virtual CalculationFooter CalculationFooter { get; set; }


        [Index("IX_CalculationHeaderLedgerAccount_UniqueRow", IsUnique = true, Order = 2)]
        [Display(Name = "Document Type"), Required]
        [ForeignKey("DocType")]        
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }


        [ForeignKey("LedgerAccountDr")]
        [Display(Name = "Ledger A/c (Dr.)")]
        public int? LedgerAccountDrId { get; set; }
        public virtual LedgerAccount LedgerAccountDr { get; set; }



        [ForeignKey("LedgerAccountCr")]
        [Display(Name = "Ledger A/c (Cr.)")]
        public int? LedgerAccountCrId { get; set; }
        public virtual LedgerAccount LedgerAccountCr { get; set; }

        [ForeignKey("ContraLedgerAccount")]
        [Display(Name = "Contra Ledger A/c")]
        public int? ContraLedgerAccountId { get; set; }
        public virtual LedgerAccount ContraLedgerAccount { get; set; }


        [ForeignKey("CostCenter")]
        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}
