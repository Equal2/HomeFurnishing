using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PromoCode : EntityBase, IHistoryLog
    {
        public PromoCode()
        {
        }

        [Key]
        [Display(Name = "PromoCode Id")]
        public int PromoCodeId { get; set; }

        [MaxLength(50, ErrorMessage = "PromoCode Name cannot exceed 50 characters"), Required]
        [Index("IX_PromoCode_PromoCode", IsUnique = true)]
        [Display(Name = "PromoCode Name")]
        public string PromoCodeName { get; set; }

        [Display(Name = "From Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime FromDate { get; set; }

        [Display(Name = "To Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ToDate { get; set; }
        
        [ForeignKey("Product"), Display(Name = "Product")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("ProductGroup"), Display(Name = "Product Group")]
        public int? ProductGroupId { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }

        [ForeignKey("ProductCategory"), Display(Name = "Product Category")]
        public int? ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        [ForeignKey("ProductType"), Display(Name = "Product Type")]
        public int? ProductTypeId { get; set; }
        public virtual ProductType ProductType { get; set; }
        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }
        public Decimal? MinQty { get; set; }
        public Decimal? MinInvoiceValue { get; set; }
        public Decimal? DiscountPer { get; set; }
        public Decimal? FlatDiscount { get; set; }
        public Decimal? MaxDiscountAmount { get; set; }
        public Boolean IsApplicableOnce { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
