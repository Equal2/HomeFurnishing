using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseInvoiceRateAmendmentLine : EntityBase, IHistoryLog
    {
        [Key]
        public int PurchaseInvoiceRateAmendmentLineId { get; set; }

        [ForeignKey("PurchaseInvoiceAmendmentHeader")]
        public int PurchaseInvoiceAmendmentHeaderId { get; set; }
        public virtual PurchaseInvoiceAmendmentHeader PurchaseInvoiceAmendmentHeader { get; set; }

        [ForeignKey("PurchaseInvoiceLine")]
        public int PurchaseInvoiceLineId { get; set; }
        public virtual PurchaseInvoiceLine PurchaseInvoiceLine { get; set; }

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
