using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseOrderLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("PurchaseOrderLine")]
        [Display(Name = "PurchaseOrderLine")]
        public int? PurchaseOrderLineId { get; set; }
        public virtual PurchaseOrderLine PurchaseOrderLine { get; set; }
        public Decimal? CancelQty { get; set; }
        public DateTime? CancelDate { get; set; }
        public Decimal? AmendmentQty { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public Decimal? ReceiveQty { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public Decimal? InvoiceQty { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public Decimal? PaymentQty { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Decimal? ReturnQty { get; set; }
        public DateTime? ReturnDate { get; set; }
        public Decimal? RateAmendmentRate { get; set; }
        public DateTime? RateAmendmentDate { get; set; }
        [MaxLength(100)]
        public string ExcessGoodsReceiptReviewBy { get; set; }
        

    }
}
