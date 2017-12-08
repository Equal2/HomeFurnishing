using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseIndentLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("PurchaseIndentLine")]
        [Display(Name = "PurchaseIndentLine")]
        public int? PurchaseIndentLineId { get; set; }
        public virtual PurchaseIndentLine PurchaseIndentLine { get; set; }
        public Decimal? CancelQty { get; set; }
        public Decimal? CancelDealQty { get; set; }
        public DateTime? CancelDate { get; set; }
        public Decimal? AmendmentQty { get; set; }
        public Decimal? AmendmentDealQty { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public Decimal? OrderQty { get; set; }
        public DateTime? OrderDate { get; set; }
        [MaxLength(100)]
        public string ExcessPurchaseOrderReviewBy { get; set; }
    }
}
