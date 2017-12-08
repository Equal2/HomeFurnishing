using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleOrderRateAmendmentLine : EntityBase, IHistoryLog
    {
        [Key]
        public int SaleOrderRateAmendmentLineId { get; set; }

        [ForeignKey("SaleOrderAmendmentHeader")]
        public int SaleOrderAmendmentHeaderId { get; set; }
        public virtual SaleOrderAmendmentHeader SaleOrderAmendmentHeader { get; set; }

        [ForeignKey("SaleOrderLine")]
        public int SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }

        public decimal Qty { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

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
