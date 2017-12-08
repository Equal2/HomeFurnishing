using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseGoodsReceiptLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("PurchaseGoodsReceiptLine")]
        [Display(Name = "PurchaseGoodsReceiptLine")]
        public int? PurchaseGoodsReceiptLineId { get; set; }
        public virtual PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine { get; set; }
        public Decimal? ReturnQty { get; set; }        
        public DateTime? ReturnDate { get; set; }
        public Decimal? InvoiceQty { get; set; }        
        public DateTime? InvoiceDate { get; set; }
        public string ExcessReceiptReviewBy { get; set; }
        public DateTime ? ExcessReceiptReviewDate { get; set; }
    }
}
