using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanCancelForSaleOrder : EntityBase, IHistoryLog
    {
        [Key]
        public int MaterialPlanCancelForSaleOrderId { get; set; }

        [ForeignKey("MaterialPlanCancelHeader")]
        public int MaterialPlanCancelHeaderId { get; set; }
        public virtual MaterialPlanCancelHeader MaterialPlanCancelHeader { get; set; }

        [ForeignKey("MaterialPlanForSaleOrder")]
        public int MaterialPlanForSaleOrderId { get; set; }
        public virtual MaterialPlanForSaleOrder MaterialPlanForSaleOrder { get; set; }
        public decimal Qty { get; set; }

        [ForeignKey("MaterialPlanCancelLine")]
        public int? MaterialPlanCancelLineId { get; set; }
        public virtual MaterialPlanCancelLine MaterialPlanCancelLine { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
        public int ? Sr { get; set; }
    }
}
