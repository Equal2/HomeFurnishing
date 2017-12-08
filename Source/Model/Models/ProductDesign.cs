using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model.Models
{
    
    public class ProductDesign : EntityBase, IHistoryLog
    {
        public ProductDesign()
        {
        }

        [Key]
        public int ProductDesignId { get; set; }

        [Display(Name = "Product Design Name")]
        [MaxLength(50), Required]
        [Index("IX_ProductDesign_ProductDesignName", IsUnique=true) ]
        public string ProductDesignName { get; set; }

        [ForeignKey("ProductType")]
        [Display(Name = "Product Type")]
        public int? ProductTypeId { get; set; }
        public virtual ProductType ProductType { get; set; }

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
