using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class JobOrderRateAmendmentLine : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderRateAmendmentLineId { get; set; }

        [ForeignKey("JobOrderAmendmentHeader")]
        public int JobOrderAmendmentHeaderId { get; set; }
        public virtual JobOrderAmendmentHeader JobOrderAmendmentHeader { get; set; }
        [ForeignKey("JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }

        [ForeignKey("JobWorker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }
        public decimal Qty { get; set; }
        public decimal JobOrderRate { get; set; }
        public decimal AmendedRate { get; set; }
        public decimal Rate { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int? Sr { get; set; }

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
        public string LockReason { get; set; }
    }
}
