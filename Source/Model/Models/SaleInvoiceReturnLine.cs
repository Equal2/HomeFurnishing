using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleInvoiceReturnLine : EntityBase, IHistoryLog
    {

        [Key]        
        public int SaleInvoiceReturnLineId { get; set; }

        [Display(Name = "Sale Order Cancel"), Required]
        [ForeignKey("SaleInvoiceReturnHeader")]
        public int SaleInvoiceReturnHeaderId { get; set; }
        public virtual SaleInvoiceReturnHeader  SaleInvoiceReturnHeader { get; set; }

        [Display(Name = "Sale Invoice"), Required]
        [ForeignKey("SaleInvoiceLine")]
        public int SaleInvoiceLineId { get; set; }
        public virtual SaleInvoiceLine SaleInvoiceLine { get; set; }

        [ForeignKey("SaleDispatchReturnLine")]
        [Display(Name = "Sale Goods Return")]
        public int? SaleDispatchReturnLineId { get; set; }
        public virtual SaleDispatchReturnLine SaleDispatchReturnLine { get; set; }


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

        public Decimal? Weight { get; set; }


        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }
        
        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Lock Reason Delete")]
        public string LockReasonDelete { get; set; }

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
