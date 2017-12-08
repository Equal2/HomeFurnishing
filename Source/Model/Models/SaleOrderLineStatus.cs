using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleOrderLineStatus :EntityBase
    {
        [Key]
        [ForeignKey("SaleOrderLine")]
        [Display(Name = "SaleOrderLine")]
        public int? SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }

        public Decimal? CancelQty { get; set; }

        public DateTime? CancelDate { get; set; }

        public Decimal? AmendmentQty { get; set; }

        public DateTime? AmendmentDate { get; set; }

        public Decimal? ShipQty { get; set; }

        public DateTime? ShipDate { get; set; }

        public Decimal? ReturnQty { get; set; }

        public DateTime? ReturnDate { get; set; }

        public Decimal? InvoiceQty { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public Decimal? PaymentQty { get; set; }
        public DateTime? PaymentDate { get; set; }

        [MaxLength(100)]
        public string ExcessSaleDispatchReviewBy { get; set; }
        

    }
}
