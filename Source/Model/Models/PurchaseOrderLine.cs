using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderLine : EntityBase, IHistoryLog
    {
        public PurchaseOrderLine()
        {
            PurchaseOrderCancelLines = new List<PurchaseOrderCancelLine>();
            PurchaseGoodsReceiptLines = new List<PurchaseGoodsReceiptLine>();
        }

        [Key]        
        public int PurchaseOrderLineId { get; set; }

        [Display(Name = "Purchase Order")]
        [ForeignKey("PurchaseOrderHeader")]
        public int PurchaseOrderHeaderId { get; set; }
        public virtual PurchaseOrderHeader  PurchaseOrderHeader { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("PurchaseIndentLine")]
        public int ? PurchaseIndentLineId { get; set; }
        public virtual PurchaseIndentLine PurchaseIndentLine { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

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

        [ForeignKey("SalesTaxGroup")]
        public int? SalesTaxGroupId { get; set; }
        public virtual SalesTaxGroup SalesTaxGroup { get; set; }
        
        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Ship Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ShipDate { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        public DateTime ? DueDate { get; set; }

        [Display(Name = "Unit Conversion Multiplier"),Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Rate")]
        public Decimal? Rate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount")]
        public Decimal? Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Product UID Header")]
        [ForeignKey("ProductUidHeader")]
        public int ? ProductUidHeaderId { get; set; }
        public virtual ProductUidHeader ProductUidHeader { get; set; }

        public int ? Sr { get; set; }

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

        public ICollection<PurchaseOrderCancelLine> PurchaseOrderCancelLines { get; set; }
        public ICollection<PurchaseGoodsReceiptLine> PurchaseGoodsReceiptLines { get; set; }     

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
