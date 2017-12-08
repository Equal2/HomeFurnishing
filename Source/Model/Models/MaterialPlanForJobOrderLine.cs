using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanForJobOrderLine : EntityBase
    {
        [Key]
        public int MaterialPlanForJobOrderLineId { get; set; }

        [ForeignKey("MaterialPlanForJobOrder")]
        public int MaterialPlanForJobOrderId { get; set; }
        public virtual MaterialPlanForJobOrder MaterialPlanForJobOrder { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        [MaxLength(50)]
        public string Specification { get; set; }



        public decimal Qty { get; set; } 

        [Display(Name = "Remark")]
        public string Remark { get; set; }



        [ForeignKey("MaterialPlanLine")]
        public int? MaterialPlanLineId { get; set; }
        public virtual MaterialPlanLine MaterialPlanLine { get; set; }


        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
