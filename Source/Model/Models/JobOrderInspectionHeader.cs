using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderInspectionHeader : EntityBase, IHistoryLog
    {

        [Key]
        public int JobOrderInspectionHeaderId { get; set; }

        [Display(Name = "Inspection Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_JobOrderInspectionHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Inspection Date"),Required ]
        public DateTime DocDate { get; set; }

        [Display(Name = "Inspection No"),Required,MaxLength(20) ]
        [Index("IX_JobOrderInspectionHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_JobOrderInspectionHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_JobOrderInspectionHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "Job Worker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }


        [ForeignKey("InspectionBy"), Display(Name = "Inspection By")]
        public int InspectionById { get; set; }
        public virtual Employee InspectionBy { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }


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


        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
