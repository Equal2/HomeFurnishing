using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanForSaleOrder : EntityBase, IHistoryLog
    {
        [Key]
        public int MaterialPlanForSaleOrderId { get; set; }

        [ForeignKey("MaterialPlanHeader")]
        public int MaterialPlanHeaderId { get; set; }
        public virtual MaterialPlanHeader MaterialPlanHeader { get; set; }

        [ForeignKey("SaleOrderLine")]
        public int SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }

        public decimal Qty { get; set; }
        [ForeignKey("MaterialPlanLine")]
        public int? MaterialPlanLineId { get; set; }
        public virtual MaterialPlanLine MaterialPlanLine { get; set; }
        [MaxLength(50)]
        public string OMSId { get; set; }
        public virtual ICollection<MaterialPlanForSaleOrderLine> MaterialPlanForSaleOrderLines { get; set; }

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
