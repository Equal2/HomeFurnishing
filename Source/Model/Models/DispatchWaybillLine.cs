using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class DispatchWaybillLine : EntityBase, IHistoryLog
    {
        public DispatchWaybillLine()
        {
        }

        [Key]
        public int DispatchWaybillLineId { get; set; }

        [Display(Name = "DispatchWaybill"), Required]
        [ForeignKey("DispatchWaybillHeader")]
        public int DispatchWaybillHeaderId { get; set; }
        public virtual DispatchWaybillHeader DispatchWaybillHeader { get; set; }

        [Display(Name = "City"), Required]
        [ForeignKey("City")]
        public int CityId { get; set; }
        public virtual City City { get; set; }

        [Display(Name = "Receive Date")]
        public DateTime? ReceiveDateTime { get; set; }

        [Display(Name = "Receive Remark")]
        public string ReceiveRemark { get; set; }

        [Display(Name = "Forwarding Date")]
        public DateTime? ForwardingDateTime { get; set; }

        [Display(Name = "Forwarded By"),MaxLength(250)]
        public string ForwardedBy { get; set; }

        [Display(Name = "Forwarding Remark")]
        public string ForwardingRemark { get; set; }

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
