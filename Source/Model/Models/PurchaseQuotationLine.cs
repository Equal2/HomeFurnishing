using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseQuotationLine : EntityBase, IHistoryLog
    {
        public PurchaseQuotationLine()
        {
        }

        [Key]        
        public int PurchaseQuotationLineId { get; set; }

        [Display(Name = "Purchase Quotation")]
        [ForeignKey("PurchaseQuotationHeader")]
        public int PurchaseQuotationHeaderId { get; set; }
        public virtual PurchaseQuotationHeader  PurchaseQuotationHeader { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "PurchaseIndentLine")]
        [ForeignKey("PurchaseIndentLine")]
        public int ? PurchaseIndentLineId { get; set; }
        public virtual PurchaseIndentLine PurchaseIndentLine { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [MaxLength(50)]
        public string Specification { get; set; }

        [Display(Name = "Lot No."), MaxLength(20)]
        public string LotNo { get; set; }

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

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }
        
        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

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
