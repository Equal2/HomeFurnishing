using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Rug_RetentionPercentage : EntityBase
    {
        public Rug_RetentionPercentage()
        {
        }

        [Key]
        public int Rug_RetentionPercentageId { get; set; }

        [Display(Name = "Product Category"), Required]
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; }
        public virtual Stock ProductCategory { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_Stock_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_Stock_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public Decimal RetentionPer { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
