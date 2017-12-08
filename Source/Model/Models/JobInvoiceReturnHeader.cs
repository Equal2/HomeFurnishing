using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceReturnHeader : EntityBase, IHistoryLog
    {

        public JobInvoiceReturnHeader()
        {
            Nature = TransactionNatureConstants.Return;
        }

        [Key]        
        public int JobInvoiceReturnHeaderId { get; set; }
                        
        [Display(Name = "Return Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_JobInvoiceReturnHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Cancel Date"),Required ]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Return No"),Required,MaxLength(20,ErrorMessage = "{0} can not exceed {1} characters")]
        [Index("IX_JobInvoiceReturnHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_JobInvoiceReturnHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_JobInvoiceReturnHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        public Decimal? ExchangeRate { get; set; }

        [ForeignKey("JobWorker")]
        [Display(Name = "JobWorker Name")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("JobReturnHeader")]
        [Display(Name = "Job Goods Return")]
        public int? JobReturnHeaderId { get; set; }
        public virtual JobReturnHeader JobReturnHeader { get; set; }

        [ForeignKey("SalesTaxGroupPerson")]
        [Display(Name = "SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }

        //[ForeignKey("SalesTaxGroupPerson")]
        //[Display(Name = "SalesTaxGroupPerson")]
        //public int? SalesTaxGroupPersonId { get; set; }
        //public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }

        [MaxLength(20)]
        public string Nature { get; set; }

        [Display(Name = "Remark"),Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

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
