using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderLineRefData : EntityBase
    {
        [Key]
        public int JobOrderLineRefDataId { get; set; }

        [ForeignKey("JobOrderLine")]
        [Display(Name = "JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }
        public decimal CancelQty { get; set; }
        public DateTime CancelFirstDate { get; set; }
        public DateTime CancelLastDate { get; set; }
        public int CancelRecCount { get; set; }
        public decimal AmendmentQty { get; set; }
        public DateTime AmendmentQtyFirstDate { get; set; }
        public DateTime AmendmentQtyLastDate { get; set; }
        public int AmendmentQtyRecCount { get; set; }
        public decimal AmendmentRate { get; set; }
        public DateTime AmendmentRateFirstDate { get; set; }
        public DateTime AmendmentRateLastDate { get; set; }
        public int AmendmentRateRecCount { get; set; }
        public decimal ReceiveQty { get; set; }
        public DateTime ReceiveFirstDate { get; set; }
        public DateTime ReceiveLastDate { get; set; }
        public int ReceiveRecCount { get; set; }
        public decimal InvoiceQty { get; set; }
        public decimal InvoiceAmt { get; set; }
        public DateTime InvoiceFirstDate { get; set; }
        public DateTime InvoiceLastDate { get; set; }
        public int InvoiceRecCount { get; set; }
        public decimal PaymentAmt { get; set; }
        public DateTime PaymentFirstDate { get; set; }
        public DateTime PaymentLastDate { get; set; }
        public int PaymentRecCount { get; set; }
        public int CheckSum { get; set; }        

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
