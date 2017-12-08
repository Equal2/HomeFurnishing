using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSize : EntityBase, IHistoryLog
    {
        [Key]
        public int ProductSizeId { get; set; }

        [ForeignKey("ProductSizeType")]
        [Display(Name = "ProductSizeType")]
        public int ProductSizeTypeId { get; set; }        
        public virtual ProductSizeType ProductSizeType { get; set; }

        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("Size")]
        [Display(Name = "Size")]
        public int SizeId { get; set; }        
        public virtual Size Size { get; set; }

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
