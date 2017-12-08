using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public abstract class CalculationLineCharge : EntityBase
    {
        [Key]
        public int Id { get; set; }

        public int HeaderTableId { get; set; }

        public int LineTableId { get; set; }


        public int Sr { get; set; }

        [ForeignKey("Charge")]
        [Display(Name = "Charge")]

        public int ChargeId { get; set; }
        public virtual Charge Charge { get; set; }


        public byte? AddDeduct { get; set; }


        public bool AffectCost { get; set; }


        [ForeignKey("ChargeType")]
        [Display(Name = "ChargeType")]
        public int? ChargeTypeId { get; set; }
        public virtual ChargeType ChargeType { get; set; }


        [ForeignKey("CalculateOn")]
        [Display(Name = "Calculate On")]
        public int? CalculateOnId { get; set; }
        public virtual Charge CalculateOn { get; set; }


        [ForeignKey("Person"), Display(Name = "Person")]
        public int? PersonID { get; set; }
        public virtual Person Person { get; set; }


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


        public byte RateType { get; set; }


        public bool IncludedInBase { get; set; }

        [ForeignKey("ParentCharge")]
        [Display(Name = "Parent Charge")]
        public int? ParentChargeId { get; set; }
        public virtual Charge ParentCharge { get; set; }


        [Display(Name = "Rate")]
        public decimal? Rate { get; set; }

        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal? DealQty { get; set; }

        public bool IsVisible { get; set; }

        public string IncludedCharges { get; set; }
        public string IncludedChargesCalculation { get; set; }



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


        [MaxLength(50)]
        public string OMSId { get; set; }




        ////int Id { get; set; }


        ////int LineTableId { get; set; }

        ////int Sr { get; set; }

        ////[ForeignKey("Charge")]
        ////[Display(Name = "Charge")]

        //// int ChargeId { get; set; }



        //// bool? AddDeduct { get; set; }


        //// bool AffectCost { get; set; }


        //// int? ChargeTypeId { get; set; }



        //// int? CalculateOnId { get; set; }


        //// int? LedgerAccountDrId { get; set; }



        //// int? LedgerAccountCrId { get; set; }


        //// int? CostCenterId { get; set; }



        //// byte RateType { get; set; }


        //// bool IncludedInBase { get; set; }

        //// int? ParentChargeId { get; set; }

        //// decimal? Rate { get; set; }

        //// decimal? Amount { get; set; }

        //// bool IsVisible { get; set; }

        ////string OMSId { get; set; }
    }
}
