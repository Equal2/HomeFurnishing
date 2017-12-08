using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDeliveryOrderLine : EntityBase, IHistoryLog
    {
        [Key]
        public int SaleDeliveryOrderLineId { get; set; }

        [ForeignKey("SaleOrderLine")]
        public int SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }

        [ForeignKey("SaleDeliveryOrderHeader")]
        public int SaleDeliveryOrderHeaderId { get; set; }
        public virtual SaleDeliveryOrderHeader SaleDeliveryOrderHeader { get; set; }
        public decimal Qty { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ? DueDate { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

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

        public int? Sr { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
