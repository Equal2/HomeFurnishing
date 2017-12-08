using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseInvoiceReturnLine : EntityBase, IHistoryLog
    {

        [Key]        
        public int PurchaseInvoiceReturnLineId { get; set; }

        [Display(Name = "Purchase Order Cancel"), Required]
        [ForeignKey("PurchaseInvoiceReturnHeader")]
        public int PurchaseInvoiceReturnHeaderId { get; set; }
        public virtual PurchaseInvoiceReturnHeader  PurchaseInvoiceReturnHeader { get; set; }

        [Display(Name = "Purchase Invoice"), Required]
        [ForeignKey("PurchaseInvoiceLine")]
        public int PurchaseInvoiceLineId { get; set; }
        public virtual PurchaseInvoiceLine PurchaseInvoiceLine { get; set; }

        [ForeignKey("PurchaseGoodsReturnLine")]
        [Display(Name = "Purchase Goods Return")]
        public int? PurchaseGoodsReturnLineId { get; set; }
        public virtual PurchaseGoodsReturnLine PurchaseGoodsReturnLine { get; set; }


        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }


        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
