using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderHeaderCharge : CalculationHeaderCharge 
    {

        [ForeignKey("PurchaseOrderHeader")]
        public int HeaderTableId { get; set; }
        public virtual PurchaseOrderHeader PurchaseOrderHeader { get; set; }


        //public PurchaseOrderHeaderCharge()
        //{
        //}

        //[Key]
        //public int PurchaseOrderHeaderChargeId { get; set; }

        //[Index("IX_PurchaseOrderHeaderCharge_PurchaseOrderHeaderChargeId", IsUnique = true, Order = 1)]
        //public int PurchaseOrderHeaderId { get; set; }

        //public int Sr { get; set; }

        //[ForeignKey("Charge")]
        //[Display(Name = "Charge")]
        //[Index("IX_PurchaseOrderHeaderCharge_PurchaseOrderHeaderChargeId", IsUnique = true, Order = 2)]
        //public int ChargeId { get; set; }
        //public virtual Charge Charge { get; set; }


        //public bool? AddDeduct { get; set; }


        //public bool AffectCost { get; set; }


        //[ForeignKey("ChargeType")]
        //[Display(Name = "ChargeType")]
        //public int? ChargeTypeId { get; set; }
        //public virtual ChargeType ChargeType { get; set; }


        //[ForeignKey("CalculateOn")]
        //[Display(Name = "Calculate On")]
        //public int? CalculateOnId { get; set; }
        //public virtual CalculationFooter CalculateOn { get; set; }


        //[ForeignKey("ProductCharge")]
        //[Display(Name = "Line Charge")]
        //public int? ProductChargeId { get; set; }
        //public virtual CalculationProduct ProductCharge { get; set; }


        //[ForeignKey("LedgerAccountDr")]
        //[Display(Name = "Ledger A/c (Dr.)")]
        //public int? LedgerAccountDrId { get; set; }
        //public virtual LedgerAccount LedgerAccountDr { get; set; }


        //[ForeignKey("LedgerAccountCr")]
        //[Display(Name = "Ledger A/c (Cr.)")]
        //public int? LedgerAccountCrId { get; set; }
        //public virtual LedgerAccount LedgerAccountCr { get; set; }

        //[ForeignKey("CostCenter")]
        //[Display(Name = "Cost Center")]
        //public int? CostCenterId { get; set; }
        //public virtual CostCenter CostCenter { get; set; }


        
        //public byte RateType { get; set; }


        //public bool IncludedInBase { get; set; }

        ////[ForeignKey("ParentCharge")]
        //[Display(Name = "Parent Charge")]
        //public int? ParentChargeId { get; set; }
        ////public virtual CalculationFooter ParentCharge { get; set; }

        //public Decimal Rate { get; set; }

        //public Decimal Amount { get; set; }

        //public bool IsVisible { get; set; }

        //[MaxLength(50)]
        //public string OMSId { get; set; }
    }
}
