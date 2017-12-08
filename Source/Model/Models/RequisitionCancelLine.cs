using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RequisitionCancelLine : EntityBase, IHistoryLog
    {

        [Key]        
        public int RequisitionCancelLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("RequisitionCancelHeader")]
        public int RequisitionCancelHeaderId { get; set; }
        public virtual RequisitionCancelHeader  RequisitionCancelHeader { get; set; }

        [Display(Name = "Request Line")]
        [ForeignKey("RequisitionLine")]
        public int RequisitionLineId { get; set; }
        public virtual RequisitionLine RequisitionLine { get; set; }
        
        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        
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
