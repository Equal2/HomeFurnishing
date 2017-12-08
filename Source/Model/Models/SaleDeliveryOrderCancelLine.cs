using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDeliveryOrderCancelLine : EntityBase, IHistoryLog, IProductUidLastStatus
    {
        [Key]
        public int SaleDeliveryOrderCancelLineId { get; set; }

        [ForeignKey("SaleDeliveryOrderCancelHeader")]
        public int SaleDeliveryOrderCancelHeaderId { get; set; }
        public virtual SaleDeliveryOrderCancelHeader SaleDeliveryOrderCancelHeader { get; set; }

        public int? Sr { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("SaleDeliveryOrderLine")]
        public int SaleDeliveryOrderLineId { get; set; }
        public virtual SaleDeliveryOrderLine SaleDeliveryOrderLine { get; set; }

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

        public int? ProductUidLastTransactionDocId { get; set; }

        public string ProductUidLastTransactionDocNo { get; set; }

        public int? ProductUidLastTransactionDocTypeId { get; set; }

        public DateTime? ProductUidLastTransactionDocDate { get; set; }

        public int? ProductUidLastTransactionPersonId { get; set; }

        public int? ProductUidCurrentGodownId { get; set; }

        public int? ProductUidCurrentProcessId { get; set; }

        [MaxLength(10)]
        public string ProductUidStatus { get; set; }
    }
}
