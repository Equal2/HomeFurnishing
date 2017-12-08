using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseOrderInspectionRequestLine : EntityBase, IHistoryLog
    {
        [Key]
        public int PurchaseOrderInspectionRequestLineId { get; set; }

        [ForeignKey("PurchaseOrderInspectionRequestHeader")]
        public int PurchaseOrderInspectionRequestHeaderId { get; set; }
        public virtual PurchaseOrderInspectionRequestHeader PurchaseOrderInspectionRequestHeader { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("PurchaseOrderLine")]
        public int PurchaseOrderLineId { get; set; }
        public virtual PurchaseOrderLine PurchaseOrderLine { get; set; }

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

        public int? Sr { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
