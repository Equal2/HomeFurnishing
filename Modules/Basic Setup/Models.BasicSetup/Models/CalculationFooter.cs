using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class CalculationFooter : EntityBase, IHistoryLog
    {

        [Key]
        public int CalculationFooterLineId { get; set; }


        [ForeignKey("Calculation")]
        [Display(Name = "Calculation")]
        [Index("IX_CalculationLine_CalculationLineName", IsUnique = true, Order = 1)]
        public int CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }

        public int Sr { get; set; }

        [ForeignKey("Charge")]
        [Display(Name = "Charge")]
        [Index("IX_CalculationLine_CalculationLineName", IsUnique = true, Order = 2)]
        public int ChargeId { get; set; }
        public virtual Charge Charge { get; set; }


        public bool? AddDeduct { get; set; }


        public bool AffectCost { get; set; }


        [ForeignKey("ChargeType")]
        [Display(Name = "ChargeType")]
        public int? ChargeTypeId { get; set; }
        public virtual ChargeType ChargeType { get; set; }


        [ForeignKey("CalculateOn")]
        [Display(Name = "Calculate On")]
        public int? CalculateOnId { get; set; }
        public virtual Charge CalculateOn { get; set; }


        [ForeignKey("ProductCharge")]
        [Display(Name = "Line Charge")]
        public int? ProductChargeId { get; set; }
        public virtual Charge ProductCharge { get; set; }

        [ForeignKey("CostCenter")]
        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }


        [ForeignKey("Person")]
        [Display(Name = "Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }
        

        public byte RateType { get; set; }


        public bool IncludedInBase { get; set; }

        [ForeignKey("ParentCharge")]
        [Display(Name = "Parent Charge")]
        public int? ParentChargeId { get; set; }
        public virtual Charge ParentCharge { get; set; }


        public decimal Rate { get; set; }

        public decimal Amount { get; set; }


        public decimal? RoundOff { get; set; }

        [ForeignKey("LedgerAccountDr")]
        [Display(Name = "Ledger A/c (Dr.)")]
        public int? LedgerAccountDrId { get; set; }
        public virtual LedgerAccount LedgerAccountDr { get; set; }



        [ForeignKey("LedgerAccountCr")]
        [Display(Name = "Ledger A/c (Cr.)")]
        public int? LedgerAccountCrId { get; set; }
        public virtual LedgerAccount LedgerAccountCr { get; set; }

        public bool IsVisible { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
