using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class CalculationLineLedgerAccount : EntityBase, IHistoryLog
    {

        [Key]
        public int CalculationLineLedgerAccountId { get; set; }

        [ForeignKey("Calculation")]
        [Display(Name = "Calculation")]        
        public int CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }


        [Index("IX_CalculationLineLedgerAccount_UniqueRow", IsUnique = true, Order = 1)]
        [ForeignKey("CalculationProduct")]
        public int CalculationProductId { get; set; }
        public virtual CalculationProduct CalculationProduct { get; set; }


        [Index("IX_CalculationLineLedgerAccount_UniqueRow", IsUnique = true, Order = 2)]
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


        public bool? IsVisibleLedgerAccountDr { get; set; }

        [ForeignKey("filterLedgerAccountGroupsDr")]
        [Display(Name = "filterLedgerAccountGroupsDr")]
        public int? filterLedgerAccountGroupsDrId { get; set; }
        public virtual LedgerAccountGroup filterLedgerAccountGroupsDr { get; set; }
        public bool? IsVisibleLedgerAccountCr { get; set; }

        [ForeignKey("filterLedgerAccountGroupsCr")]
        [Display(Name = "filterLedgerAccountGroupsCr")]
        public int? filterLedgerAccountGroupsCrId { get; set; }
        public virtual LedgerAccountGroup filterLedgerAccountGroupsCr { get; set; }


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
