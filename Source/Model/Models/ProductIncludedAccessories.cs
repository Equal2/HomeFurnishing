using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductIncludedAccessories : EntityBase, IHistoryLog
    {
     
        [Key]
        public int ProductIncludedAccessoriesId { get; set; }

        
        [Display(Name = "Product")]
        [Index("IX_ProductIncludedAccessories_ProductId", IsUnique = true, Order = 1)]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }


        [Display(Name = "Accessory")]
        [Index("IX_ProductIncludedAccessories_AccessoryId", IsUnique = true, Order = 2)]
        public int AccessoryId { get; set; }
        [ForeignKey("AccessoryId")]
        public virtual Product Accessory { get; set; }

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
