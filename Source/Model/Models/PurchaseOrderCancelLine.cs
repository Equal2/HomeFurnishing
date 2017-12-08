using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderCancelLine : EntityBase, IHistoryLog
    {
        public PurchaseOrderCancelLine()
        {
            //ProductSuppliers = new List<ProductSupplier>();
        }

        [Key]        
        public int PurchaseOrderCancelLineId { get; set; }

        [Display(Name = "Purchase Order Cancel"), Required]
        [ForeignKey("PurchaseOrderCancelHeader")]
        public int PurchaseOrderCancelHeaderId { get; set; }
        public virtual PurchaseOrderCancelHeader  PurchaseOrderCancelHeader { get; set; }

        [Display(Name = "Purchase Order"), Required]
        [ForeignKey("PurchaseOrderLine")]
        public int PurchaseOrderLineId { get; set; }
        public virtual PurchaseOrderLine PurchaseOrderLine { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? Sr { get; set; }

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
