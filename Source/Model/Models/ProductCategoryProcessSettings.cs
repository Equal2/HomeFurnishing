using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductCategoryProcessSettings : EntityBase, IHistoryLog
    {
        public ProductCategoryProcessSettings()
        {
        }

        [Key]
        public int ProductCategoryProcessSettingsId { get; set; }

        [ForeignKey("ProductCategory")]
        [Display(Name = "Product Category")]
        [Index("IX_ProductCategoryProcessSettings_DocID", IsUnique = true, Order = 1)]
        public int ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }

        [Display(Name = "Process")]
        [Index("IX_ProductCategoryProcessSettings_DocID", IsUnique = true, Order = 2)]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "QA Group")]
        public int? QAGroupId { get; set; }
        public virtual QAGroup QAGroup { get; set; }

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
