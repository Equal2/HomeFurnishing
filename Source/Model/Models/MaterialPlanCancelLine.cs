using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanCancelLine : EntityBase, IHistoryLog
    {
        [Key]
        public int MaterialPlanCancelLineId { get; set; }

        [ForeignKey("MaterialPlanCancelHeader")]
        public int MaterialPlanCancelHeaderId { get; set; }
        public virtual MaterialPlanCancelHeader MaterialPlanCancelHeader { get; set; }

        [ForeignKey("MaterialPlanLine")]
        public int MaterialPlanLineId { get; set; }
        public virtual MaterialPlanLine MaterialPlanLine { get; set; }
        public decimal Qty { get; set; }
        
        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

        public int ? Sr { get; set; }
    }
}
