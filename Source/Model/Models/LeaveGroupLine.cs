using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class LeaveGroupLine : EntityBase, IHistoryLog
    {

        [Key]
        public int LeaveGroupLineId { get; set; }

        [Display(Name = "LeaveGroup"), Required]
        [ForeignKey("LeaveGroup")]
        public int LeaveGroupId { get; set; }
        public virtual LeaveGroup LeaveGroup { get; set; }

        public decimal PaymentPer { get; set; }

        public int LeaveAllowed { get; set; }

        public int MaxContinueLeaveAllowed { get; set; }

        public bool IsCarryForward { get; set; }

        public bool IsEncashment { get; set; }

        public bool IsHolydayIncluded { get; set; }

        public bool IsAllowedNegetive { get; set; }


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
