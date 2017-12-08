using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSupplier : EntityBase, IHistoryLog
    {
     
        [Key]
        public int ProductSupplierId { get; set; }

        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }        
        public virtual Product Product { get; set; }

        [ForeignKey("Supplier")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }        
        public virtual Supplier Supplier { get; set; }

        [Display(Name = "Lead Time")]
        public int? LeadTime { get; set; }

        [Display(Name = "Minimum Order Qty")]
        public decimal? MinimumOrderQty { get; set; }

        [Display(Name = "Maximum Order Qty")]
        public decimal? MaximumOrderQty { get; set; }

        [Display(Name = "Cost")]
        public decimal? Cost { get; set; }

        public bool Default { get; set; }

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
