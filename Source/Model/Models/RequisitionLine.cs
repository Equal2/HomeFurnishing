using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RequisitionLine : EntityBase, IHistoryLog
    {
        public RequisitionLine()
        {
        }

        [Key]        
        public int RequisitionLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("RequisitionHeader")]
        public int RequisitionHeaderId { get; set; }
        public virtual RequisitionHeader  RequisitionHeader { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        
        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }




        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }


        [MaxLength(50)]
        public string Specification { get; set; }
        public DateTime? DueDate { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

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
