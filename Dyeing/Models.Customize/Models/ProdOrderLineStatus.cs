using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class ProdOrderLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("ProdOrderLine")]
        [Display(Name = "ProdOrderLine")]
        public int? ProdOrderLineId { get; set; }
        public virtual ProdOrderLine ProdOrderLine { get; set; }
        public Decimal? CancelQty { get; set; }        
        public DateTime? CancelDate { get; set; }
        public Decimal? AmendmentQty { get; set; }        
        public DateTime? AmendmentDate { get; set; }
        public Decimal? JobOrderQty { get; set; }
        public DateTime? JobOrderDate { get; set; }
        public string ExcessJobOrderReviewBy { get; set; }
    }
}
