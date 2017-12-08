using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseInvoiceLine : EntityBase, IHistoryLog
    {
        public PurchaseInvoiceLine()
        {
        }

        [Key]        
        public int PurchaseInvoiceLineId { get; set; }

        [Display(Name = "Purchase Invoice"), Required]
        [ForeignKey("PurchaseInvoiceHeader")]
        public int PurchaseInvoiceHeaderId { get; set; }
        public virtual PurchaseInvoiceHeader PurchaseInvoiceHeader { get; set; }

        [Display(Name = "Purchase Goods Receipt"), Required]
        [ForeignKey("PurchaseGoodsReceiptLine")]
        public int PurchaseGoodsReceiptLineId { get; set; }
        public virtual PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine { get; set; }

        [ForeignKey("SalesTaxGroup")]
        public int? SalesTaxGroupId { get; set; }
        public virtual SalesTaxGroup SalesTaxGroup { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }
        public decimal DocQty { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }


        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }


        public int? Sr { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        //public decimal RatePerQty
        //{
        //    get
        //    {
        //        if (Amount == 0)
        //            return 0;
        //        else
        //            return (decimal)Amount / PurchaseGoodsReceiptLine.Qty;
        //    }
        //}

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
