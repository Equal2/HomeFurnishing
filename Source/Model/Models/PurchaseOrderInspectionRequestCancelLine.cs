using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseOrderInspectionRequestCancelLine : EntityBase, IHistoryLog
    {
        [Key]
        public int PurchaseOrderInspectionRequestCancelLineId { get; set; }

        [ForeignKey("PurchaseOrderInspectionRequestCancelHeader")]
        public int PurchaseOrderInspectionRequestCancelHeaderId { get; set; }
        public virtual PurchaseOrderInspectionRequestCancelHeader PurchaseOrderInspectionRequestCancelHeader { get; set; }
        public int? Sr { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("PurchaseOrderInspectionRequestLine")]
        public int PurchaseOrderInspectionRequestLineId { get; set; }
        public virtual PurchaseOrderInspectionRequestLine PurchaseOrderInspectionRequestLine { get; set; }
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
