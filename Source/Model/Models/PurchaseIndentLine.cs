using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseIndentLine : EntityBase, IHistoryLog
    {
        public PurchaseIndentLine()
        {
            PurchaseIndentCancelLines = new List<PurchaseIndentCancelLine>();
            PurchaseOrderLines = new List<PurchaseOrderLine>();
        }

        [Key]        
        public int PurchaseIndentLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("PurchaseIndentHeader")]
        public int PurchaseIndentHeaderId { get; set; }
        public virtual PurchaseIndentHeader  PurchaseIndentHeader { get; set; }

        [ForeignKey("MaterialPlanLine")]
        public int ? MaterialPlanLineId { get; set; }
        public virtual MaterialPlanLine MaterialPlanLine { get; set; }

        public DateTime ? DueDate { get; set; }
        public string Specification { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [Display(Name = "Dimension3")]
        [ForeignKey("Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [Display(Name = "Dimension4")]
        [ForeignKey("Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }
        
        [Display(Name = "Qty"), Required]
        public decimal Qty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<PurchaseIndentCancelLine> PurchaseIndentCancelLines { get; set; }
        public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
