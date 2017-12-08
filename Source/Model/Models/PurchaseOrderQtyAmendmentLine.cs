using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderQtyAmendmentLine : EntityBase, IHistoryLog
    {
        [Key]
        public int PurchaseOrderQtyAmendmentLineId { get; set; }

        [ForeignKey("PurchaseOrderAmendmentHeader")]
        public int PurchaseOrderAmendmentHeaderId { get; set; }
        public virtual PurchaseOrderAmendmentHeader PurchaseOrderAmendmentHeader { get; set; }

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

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
