using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobReceiveHeader : EntityBase, IHistoryLog
    {
        public JobReceiveHeader()
        {
            JobReceiveLines = new List<JobReceiveLine>();
        }

        [Key]
        public int JobReceiveHeaderId { get; set; }

        [Display(Name = "Job Receive Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_JobReceiveHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Job Receive Date"),Required ]
        public DateTime DocDate { get; set; }

        [Display(Name = "Job Receive No"),Required,MaxLength(20) ]
        [Index("IX_JobReceiveHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_JobReceiveHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_JobReceiveHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("Machine"), Display(Name = "Machine")]
        public int? MachineId { get; set; }
        public virtual Product Machine { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "Job Worker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [Display(Name = "JobWorker Doc. No."), MaxLength(20)]
        public string JobWorkerDocNo { get; set; }

        [ForeignKey("JobReceiveBy"), Display(Name = "Job Receive By")]
        public int JobReceiveById { get; set; }
        public virtual Person JobReceiveBy { get; set; }

        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }      

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }


        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<JobReceiveLine> JobReceiveLines { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
