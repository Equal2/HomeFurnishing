using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveQAPenalty : EntityBase, IHistoryLog
    {

        [Key]        
        public int JobReceiveQAPenaltyId { get; set; }

        public int Sr { get; set; }

        
        [Display(Name = "Job Receive QA No.")]
        [ForeignKey("JobReceiveQALine")]
        public int JobReceiveQALineId { get; set; }        
        public virtual JobReceiveQALine JobReceiveQALine { get; set; }


        [Display(Name = "Reason")]
        [ForeignKey("Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }
        
        [Display(Name = "Amount")]
        public decimal  Amount { get; set; }

        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
