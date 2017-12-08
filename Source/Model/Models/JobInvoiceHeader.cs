using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceHeader : EntityBase, IHistoryLog
    {
        public JobInvoiceHeader()
        {
            JobInvoiceLines = new List<JobInvoiceLine>();
        }

        [Key]
        public int JobInvoiceHeaderId { get; set; }

        [Display(Name = "Invoice Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_JobInvoiceHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Invoice Date"),Required ]
        public DateTime  DocDate { get; set; }

        [Display(Name = "Invoice No"),Required,MaxLength(20) ]
        [Index("IX_JobInvoiceHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "JobWorker Doc. No."), MaxLength(20)]
        public string JobWorkerDocNo { get; set; }

        [Display(Name = "Job Worker Doc Date")]
        public DateTime? JobWorkerDocDate { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_JobInvoiceHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_JobInvoiceHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        [ForeignKey("JobWorker")]
        public int ? JobWorkerId { get; set; }
        public virtual Person JobWorker { get; set; }

        [ForeignKey("Financier")]
        public int? FinancierId { get; set; }
        public virtual Person Financier { get; set; }


        public int Status { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Job Receive")]
        [ForeignKey("JobReceiveHeader")]
        public int ? JobReceiveHeaderId { get; set; }
        public virtual JobReceiveHeader JobReceiveHeader { get; set; }

        public int? CreditDays { get; set; }


        [ForeignKey("SalesTaxGroupPerson")]
        [Display(Name = "SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }

        [MaxLength(20)]
        public string GovtInvoiceNo { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<JobInvoiceLine> JobInvoiceLines { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }


        [MaxLength(50)]
        public string OMSId { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
    }
}
