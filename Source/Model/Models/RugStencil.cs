using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RugStencil : EntityBase, IHistoryLog
    {
        [Key]
        [Display(Name = "Stencil")]
        public int StencilId { get; set; }
        [ForeignKey("StencilId")]
        public virtual Product Product { get; set; }
                     
        [Display(Name = "Design")]
        public int ProductDesignId { get; set; }
        [ForeignKey("ProductDesignId")]
        public virtual ProductDesign ProductDesign { get; set; }

        [Display(Name = "Product Size")]
        public int ProductSizeId { get; set; }
        [ForeignKey("ProductSizeId")]
        public virtual ProductSize ProductSize { get; set; }

        [MaxLength(10)]
        public string FullHalf { get; set; }


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
