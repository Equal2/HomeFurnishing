using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleEnquiryLine : EntityBase, IHistoryLog
    {
        public SaleEnquiryLine()
        {
        }

        [Key]
        public int SaleEnquiryLineId { get; set; }

        [ForeignKey("SaleEnquiryHeader")]
        [Index("IX_SaleEnquiryLine_SaleOrdeHeaderProductDueDate", IsUnique = true, Order = 1)]
        public int SaleEnquiryHeaderId { get; set; }
        public virtual SaleEnquiryHeader SaleEnquiryHeader { get; set; }

        [ForeignKey("Product"),Display(Name="Product")]
        [Index("IX_SaleEnquiryLine_SaleOrdeHeaderProductDueDate", IsUnique = true, Order = 2)]
        public int? ProductId { get; set; }
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

        [MaxLength(50)]
        public string Specification { get; set; }

        public decimal Qty { get; set; }

        [Display (Name ="Due Date")]
        [Index("IX_SaleEnquiryLine_SaleOrdeHeaderProductDueDate", IsUnique = true, Order = 3)]
        public DateTime ? DueDate { get; set; }


        [ForeignKey("Unit"), Display(Name = "Unit")]
        public string UnitId { get; set; }
        public virtual Unit Unit { get; set; }


        [ForeignKey("DealUnit"), Display(Name = "Delivery Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name="Delivery Qty")]
        public decimal DealQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal? UnitConversionMultiplier { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }


        //[Display(Name="Sales Tax Group")]
        //public int SalesTaxGroupId { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
