using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public abstract class ARateList : EntityBase, IHistoryLog
    {

        [Key]
        public int RateListId { get; set; }

        [Display(Name = "WEF"), Required]
        public DateTime WEF { get; set; }

        [ForeignKey("Process")]
        [Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("PersonRateGroup")]
        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public virtual PersonRateGroup PersonRateGroup { get; set; }

        [ForeignKey("DocType")]
        [Display(Name = "Document Type")]
        public int? DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public int? DocId { get; set; }


        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }
        public Decimal WeightageGreaterOrEqual { get; set; }
        public Decimal Rate { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

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


    public class RateList : ARateList
    { }

    public class RateListHistory : ARateList
    { }
}
