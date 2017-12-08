using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("JobOrderLine")]
        [Display(Name = "JobOrderLine")]
        public int? JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }
        public Decimal? CancelQty { get; set; }
        public Decimal? CancelDealQty { get; set; }
        public DateTime? CancelDate { get; set; }
        public Decimal? AmendmentQty { get; set; }
        public Decimal? AmendmentDealQty { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public Decimal? ReceiveQty { get; set; }
        public Decimal? ReceiveDealQty { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public Decimal? InvoiceQty { get; set; }
        public Decimal? InvoiceDealQty { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public Decimal? PaymentQty { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Decimal? ReturnQty { get; set; }
        public Decimal? ReturnDealQty { get; set; }
        public DateTime? ReturnDate { get; set; }
        public Decimal? RateAmendmentRate { get; set; }
        public DateTime? RateAmendmentDate { get; set; }

        [MaxLength(100)]
        public string ExcessJobReceiveReviewBy { get; set; }

    }
}
