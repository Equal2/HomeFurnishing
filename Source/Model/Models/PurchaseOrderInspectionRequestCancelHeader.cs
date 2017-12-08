using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseOrderInspectionRequestCancelHeader : EntityBase, IHistoryLog
    {


        [Key]
        public int PurchaseOrderInspectionRequestCancelHeaderId { get; set; }

        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

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

        [ForeignKey("Supplier")]
        [Display(Name = "Supplier Name")]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [MaxLength(10)]
        public string RequestBy { get; set; }
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

        [MaxLength(50)]
        public string OMSId { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
    }
}
