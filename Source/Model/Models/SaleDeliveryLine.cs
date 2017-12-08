using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDeliveryLine : EntityBase, IHistoryLog
    {
        public SaleDeliveryLine()
        {
        }

        [Key]
        public int SaleDeliveryLineId { get; set; }

        [Display(Name = "Sale Dispatch"), Required]
        [ForeignKey("SaleDeliveryHeader")]
        public int SaleDeliveryHeaderId { get; set; }
        public virtual SaleDeliveryHeader SaleDeliveryHeader { get; set; }

        [Display(Name = "SaleInvoice No"), Required]
        [ForeignKey("SaleInvoiceLine")]
        public int SaleInvoiceLineId { get; set; }
        public virtual SaleInvoiceLine SaleInvoiceLine { get; set; }
        
        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        public int Sr { get; set; }


        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }

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

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

    }
}
