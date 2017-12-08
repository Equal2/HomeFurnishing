using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class JobOrderInspectionRequestCancelLine : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderInspectionRequestCancelLineId { get; set; }

        [ForeignKey("JobOrderInspectionRequestCancelHeader")]
        public int JobOrderInspectionRequestCancelHeaderId { get; set; }
        public virtual JobOrderInspectionRequestCancelHeader JobOrderInspectionRequestCancelHeader { get; set; }
        public int? Sr { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("JobOrderInspectionRequestLine")]
        public int JobOrderInspectionRequestLineId { get; set; }
        public virtual JobOrderInspectionRequestLine JobOrderInspectionRequestLine { get; set; }
        public decimal Qty { get; set; }

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

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
