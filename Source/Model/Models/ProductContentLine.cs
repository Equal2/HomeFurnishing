using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductContentLine : EntityBase, IHistoryLog
    {
        public ProductContentLine()
        {
        }

        [Key]
        public int ProductContentLineId { get; set; }

        [Display(Name = "Product Custom Group")]
        [ForeignKey("ProductContentHeader")]
        public int ProductContentHeaderId { get; set; }
        public virtual ProductContentHeader ProductContentHeader { get; set; }

        [Display(Name = "Product Group"), Required]
        [ForeignKey("ProductGroup")]
        public int ProductGroupId{ get; set; }
        public virtual ProductGroup ProductGroup{ get; set; }

        [Display(Name = "Content Per")]
        public decimal ContentPer { get; set; }

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
