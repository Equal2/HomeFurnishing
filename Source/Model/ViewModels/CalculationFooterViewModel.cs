using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;

namespace Model.ViewModel
{
    public class CalculationFooterViewModel
    {
        public int Id { get; set; }
        public string CalculationFooterName { get; set; }

        public int CalculationId { get; set; }
        public string CalculationName { get; set; }
        public int HeaderTableId { get; set; }

        public int Sr { get; set; }

        public int ChargeId { get; set; }
        public string ChargeName { get; set; }
        public string ChargeCode { get; set; }

        public byte ?AddDeduct { get; set; }
        public string AddDeductName { get; set; }

        public bool AffectCost { get; set; }
        public string AffectCostName { get; set; }

        public int? ChargeTypeId { get; set; }
        public string ChargeTypeName { get; set; }

        public int? CalculateOnId { get; set; }
        public string CalculateOnName { get; set; }
        public string CalculateOnCode { get; set; }

        public int? ProductChargeId { get; set; }
        public string ProductChargeName { get; set; }
        public string ProductChargeCode { get; set; }


        public int? LedgerAccountDrId { get; set; }
        public string LedgerAccountDrName { get; set; }

        public int? LedgerAccountCrId { get; set; }
        public string LedgerAccountCrName { get; set; }

        public int? ContraLedgerAccountId { get; set; }
        public string ContraLedgerAccountName { get; set; }


        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }


        public byte RateType { get; set; }

        public bool IsVisible { get; set; }

        public bool IncludedInBase { get; set; }
        public string IncludedInBaseName { get; set; }
        //[ForeignKey("ParentCharge")]
        public int? ParentChargeId { get; set; }
        //public virtual CalculationFooter ParentCharge { get; set; }


        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
        public Boolean IsActive { get; set; }

        public string IncludedCharges { get; set; }
        public string IncludedChargesCalculation { get; set; }
    }

    public class LineReferenceIds
    {
        public int LineId { get; set; }
        public int RefLineId { get; set; }
    }


    public class LineChargeRates
    {
        public int LineId { get; set; }
        public List<CalculationProductViewModel> ChargeRates { get; set; }
    }

    public class ReferenceLineChargeViewModel
    {
        public int LineId { get; set; }
        //public List<JobOrderHeaderCharge> HeaderCharges { get; set; }
        //public List<JobOrderLineCharge> Linecharges { get; set; }

        public List<CalculationFooter> HeaderCharges { get; set; }
        public List<CalculationProduct> Linecharges { get; set; }
        public decimal PenaltyAmt { get; set; }
        public int? ChargeGroupProductId { get; set; }
    }

}
