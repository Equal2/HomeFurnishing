using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanCancelForProdOrderLine : EntityBase
    {
        [Key]
        public int MaterialPlanCancelForProdOrderLineId { get; set; }

        [ForeignKey("MaterialPlanCancelForProdOrder")]
        public int MaterialPlanCancelForProdOrderId { get; set; }
        public virtual MaterialPlanCancelForProdOrder MaterialPlanCancelForProdOrder { get; set; }
        public decimal Qty { get; set; } 

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        
        [ForeignKey("MaterialPlanCancelLine")]
        public int? MaterialPlanCancelLineId { get; set; }
        public virtual MaterialPlanCancelLine MaterialPlanCancelLine { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
        public int ? Sr { get; set; }
    }
}
