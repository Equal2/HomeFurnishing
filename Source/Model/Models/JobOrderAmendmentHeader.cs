using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class JobOrderAmendmentHeader : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderAmendmentHeaderId { get; set; }

        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public DateTime DocDate { get; set; }

        [MaxLength(10)]
        public string DocNo { get; set; }

        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("JobWorker")]
        public int ? JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [ForeignKey("OrderBy"), Display(Name = "Order By")]
        public int OrderById { get; set; }
        public virtual Employee OrderBy { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int Status { get; set; }

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
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public string LockReason { get; set; }
    }
}
