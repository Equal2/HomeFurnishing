using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanLine : EntityBase, IHistoryLog
    {
        [Key]
        public int MaterialPlanLineId { get; set; }

        [ForeignKey("MaterialPlanHeader")]
        public int MaterialPlanHeaderId { get; set; }
        public virtual MaterialPlanHeader MaterialPlanHeader { get; set; }

        [MaxLength(10)]
        public string GeneratedFor { get; set; }

        [ForeignKey("Product"),Display(Name="Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public decimal RequiredQty { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal ExcessStockQty { get; set; }

        public decimal StockPlanQty { get; set; }

        public decimal ProdPlanQty { get; set; }

        public decimal PurchPlanQty { get; set; }

        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }



        [Display(Name = "Dimension3")]
        [ForeignKey("Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [Display(Name = "Dimension4")]
        [ForeignKey("Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }


        public string Specification { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

        public int ? Sr { get; set; }
    }
}
