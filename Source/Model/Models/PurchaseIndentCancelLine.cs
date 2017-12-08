using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseIndentCancelLine : EntityBase, IHistoryLog
    {
        public PurchaseIndentCancelLine()
        {
        }

        [Key]        
        public int PurchaseIndentCancelLineId { get; set; }

        [Display(Name = "Purchase Indent Cancel"), Required]
        [ForeignKey("PurchaseIndentCancelHeader")]
        public int PurchaseIndentCancelHeaderId { get; set; }
        public virtual PurchaseIndentCancelHeader  PurchaseIndentCancelHeader { get; set; }

        [Display(Name = "Purchase Indent"), Required]
        [ForeignKey("PurchaseIndentLine")]
        public int PurchaseIndentLineId { get; set; }
        public virtual PurchaseIndentLine PurchaseIndentLine { get; set; }

        [ForeignKey("MaterialPlanCancelLine")]
        public int? MaterialPlanCancelLineId { get; set; }
        public virtual MaterialPlanCancelLine MaterialPlanCancelLine { get; set; }


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
