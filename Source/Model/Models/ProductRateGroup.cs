using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductRateGroup : EntityBase, IHistoryLog
    {

        [Key]
        public int ProductRateGroupId { get; set; }

        [Display(Name = "Product Rate Group")]
        [MaxLength(50), Required]
        [Index("IX_ProductRateGroup_ProductRateGroupName", IsUnique = true,Order=1)]
        public string ProductRateGroupName { get; set; }


        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_ProductRateGroup_ProductRateGroupName", IsUnique = true, Order = 2)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_ProductRateGroup_ProductRateGroupName", IsUnique = true, Order = 3)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public string Processes { get; set; }

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
