using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Models.Company.ViewModels
{
    public class LineChargeViewModel 
    {
        public int LineTableId { get; set; }

        public int HeaderTableId { get; set; }
        public int Id { get; set; }

        public int Sr { get; set; }

        [Display(Name = "Charge")]

        public int ChargeId { get; set; }
        public string ChargeName { get; set; }
        public string ChargeCode { get; set; }

        public int ? PersonID { get; set; }
        public bool? AddDeduct { get; set; }
        public string AddDeductName { get; set; }

        public bool AffectCost { get; set; }
        public string AffectCostName { get; set; }

        [Display(Name = "ChargeType")]
        public int? ChargeTypeId { get; set; }
        public string ChargeTypeName { get; set; }
        
        [Display(Name = "Calculate On")]
        public int? CalculateOnId { get; set; }
        public string CalculateOnName { get; set; }
        public string CalculateOnCode { get; set; }

     
        [Display(Name = "Ledger A/c (Dr.)")]
        public int? LedgerAccountDrId { get; set; }
        public string LedgerAccountDrName { get; set; }

        [Display(Name = "Ledger A/c (Cr.)")]
        public int? LedgerAccountCrId { get; set; }
        public string LedgerAccountCrName { get; set; }
        public int? ContraLedgerAccountId { get; set; }
        public string ContraLedgerAccountName { get; set; }

        
        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        public byte RateType { get; set; }

        public bool IncludedInBase { get; set; }
        public string IncludedInBaseName { get; set; }
        //[ForeignKey("ParentCharge")]
        [Display(Name = "Parent Charge")]
        public int? ParentChargeId { get; set; }
        //public virtual CalculationProduct ParentCharge { get; set; }


        [Display(Name = "Rate")]
        public decimal? Rate { get; set; }

        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal ? DealQty { get; set; }
        public int ? RoundOff { get; set; }
        public bool IsVisible { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }



    public class LineDetailListViewModel
    {
        public int LineTableId { get; set; }
        public int HeaderTableId { get; set; }
        public decimal ? Rate { get; set; }
        public decimal ? Amount { get; set; }
        public int ? PersonID { get; set; }
        public decimal ? DealQty { get; set; }
        public decimal Incentive { get; set; }
        public decimal Penalty { get; set; }
        public int ? CostCenterId { get; set; }
        public List<LineChargeViewModel> RLineCharges { get; set; }
        public List<HeaderChargeViewModel> RHeaderCharges { get; set; }
    }

}
