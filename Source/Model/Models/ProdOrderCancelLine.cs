using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProdOrderCancelLine : EntityBase, IHistoryLog
    {
        public ProdOrderCancelLine()
        {
        }

        [Key]        
        public int ProdOrderCancelLineId { get; set; }

        [Display(Name = "Production Order Cancel"), Required]
        [ForeignKey("ProdOrderCancelHeader")]
        public int ProdOrderCancelHeaderId { get; set; }
        public virtual ProdOrderCancelHeader  ProdOrderCancelHeader { get; set; }

        [Display(Name = "Production Order"), Required]
        [ForeignKey("ProdOrderLine")]
        public int ProdOrderLineId { get; set; }
        public virtual ProdOrderLine ProdOrderLine { get; set; }

        [Display(Name = "Qty"), Required]
        public decimal Qty { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }


        [ForeignKey("MaterialPlanCancelLine")]
        public int? MaterialPlanCancelLineId { get; set; }
        public virtual MaterialPlanCancelLine MaterialPlanCancelLine { get; set; }

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
