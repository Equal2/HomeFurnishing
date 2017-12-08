using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public abstract class ARateListLine : EntityBase, IHistoryLog
    {

        [Key]
        public int RateListLineId { get; set; }


        [Display(Name = "RateListHeader")]
        [ForeignKey("RateListHeader")]
        public int RateListHeaderId { get; set; }
        public virtual RateListHeader RateListHeader { get; set; }


        [ForeignKey("PersonRateGroup")]
        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public virtual PersonRateGroup PersonRateGroup { get; set; }


        [ForeignKey("ProductRateGroup")]
        [Display(Name = "Product Rate Group")]
        public int? ProductRateGroupId { get; set; }
        public virtual ProductRateGroup ProductRateGroup { get; set; }


        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }


        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        public Decimal Rate { get; set; }

        public Decimal Discount { get; set; }

        public Decimal Incentive { get; set; }

        public Decimal Loss { get; set; }

        public Decimal UnCountedQty { get; set; }
        
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


    public class RateListLine : ARateListLine
    { }

    public class RateListLineHistory : ARateListLine
    { }
}
