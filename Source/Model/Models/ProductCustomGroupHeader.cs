using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductCustomGroupHeader : EntityBase, IHistoryLog
    {
        public ProductCustomGroupHeader()
        {
            ProductCustomGroupLine = new List<ProductCustomGroupLine>();
        }

        [Key]
        public int ProductCustomGroupId { get; set; }

        [Display(Name = "Product Custom Group")]
        [MaxLength(100), Required]
        [Index("IX_ProductCustomGroup_ProductCustomGroupName", IsUnique=true) ]
        public string ProductCustomGroupName { get; set; }
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

        public ICollection<ProductCustomGroupLine> ProductCustomGroupLine { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
