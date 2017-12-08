using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
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
