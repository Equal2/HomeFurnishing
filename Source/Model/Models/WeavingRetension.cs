using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class WeavingRetension : EntityBase, IHistoryLog
    {
        public WeavingRetension()
        {
        }

        [Key]
        public int WeavingRetensionId { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_WeavingRetension_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_WeavingRetension_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("ProductCategory")]
        [Display(Name = "Product Category")]
        public int? ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        [ForeignKey("ProductQuality")]
        [Display(Name = "Product Quality")]
        public int? ProductQualityId { get; set; }
        public virtual ProductQuality ProductQuality { get; set; }

        [ForeignKey("Person")]
        [Display(Name = "Person")]
        public int? PersonId { get; set; }
        public virtual Person  Person { get; set; }

        [Display(Name = "RetensionPer")]
        public Decimal RetensionPer { get; set; }

        [Display(Name = "MinimumAmount")]
        public Decimal MinimumAmount { get; set; }

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
