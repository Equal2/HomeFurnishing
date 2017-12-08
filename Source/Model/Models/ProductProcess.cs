using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductProcess : EntityBase, IHistoryLog
    {
        [Key]
        public int ProductProcessId { get; set; }
        
        [Display(Name = "Product")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [Display(Name = "Dimension3")]
        [ForeignKey("Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }


        [Display(Name = "Dimension4")]
        [ForeignKey("Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

        [MaxLength(20)]
        public string PurchProd { get; set; }

        public int? Sr { get; set; }


        [Display(Name = "ProductRateGroup")]
        [ForeignKey("ProductRateGroup")]
        public int? ProductRateGroupId { get; set; }
        public virtual ProductRateGroup ProductRateGroup { get; set; }


        [Display(Name = "QA Group")]
        [ForeignKey("QAGroup")]
        public int? QAGroupId { get; set; }
        public virtual QAGroup QAGroup { get; set; }


        [MaxLength(250)]
        public string Instructions { get; set; }

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
