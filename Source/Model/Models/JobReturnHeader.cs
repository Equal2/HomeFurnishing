using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class JobReturnHeader : EntityBase, IHistoryLog
    {

        public JobReturnHeader()
        {
            JobReturnLines = new List<JobReturnLine>();
        }
        [Key]
        public int JobReturnHeaderId { get; set; }

        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [MaxLength(10)]
        public string DocNo { get; set; }
        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("JobWorker")]
        [Display(Name = "JobWorker Name")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [ForeignKey("OrderBy"), Display(Name = "Order By")]
        public int OrderById { get; set; }
        public virtual Employee OrderBy { get; set; }

        [ForeignKey("Godown")]
        public int  GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [ForeignKey("GatePassHeader")]
        [Display(Name = "Gatepass No.")]
        public int? GatePassHeaderId { get; set; }
        public virtual GatePassHeader GatePassHeader { get; set; }

        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }



        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }


        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public ICollection<JobReturnLine> JobReturnLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
    }
}
