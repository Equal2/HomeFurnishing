using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleInvoiceLine : EntityBase, IHistoryLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("SaleInvoiceLineId")]
        public int SaleInvoiceLineId { get; set; }

        [Display(Name = "Sale Invoice"), Required]
        [ForeignKey("SaleInvoiceHeader")]
        public int SaleInvoiceHeaderId { get; set; }
        public virtual SaleInvoiceHeader SaleInvoiceHeader { get; set; }

        [Display(Name = "Sale Dispatch"), Required]
        [ForeignKey("SaleDispatchLine")]
        public int SaleDispatchLineId { get; set; }
        public virtual SaleDispatchLine SaleDispatchLine { get; set; }

        [Display(Name = "Sale Order")]
        [ForeignKey("SaleOrderLine")]
        public int? SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }


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


        [Display(Name = "Dimension3")]
        [ForeignKey("Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [Display(Name = "Dimension4")]
        [ForeignKey("Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }


        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        

        [ForeignKey("SalesTaxGroupProduct"), Display(Name = "Sales Tax Group Product")]
        public int? SalesTaxGroupProductId { get; set; }
        public virtual ChargeGroupProduct SalesTaxGroupProduct { get; set; }





        [ForeignKey("ProductInvoiceGroup"), Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public virtual ProductInvoiceGroup ProductInvoiceGroup { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal? UnitConversionMultiplier { get; set; }

        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }


        [Display(Name = "Weight")]
        public Decimal? Weight { get; set; }


        [ForeignKey("PromoCode"), Display(Name = "Promo Code")]
        public int? PromoCodeId { get; set; }
        public virtual PromoCode PromoCode { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }


        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Rate Remark")]
        public string RateRemark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        public int? Sr { get; set; }

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
