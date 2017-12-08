using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{

    [Table("ViewMaterialPlanBalance")]
    public class ViewMaterialPlanBalance
    {
        [Key]
        public int MaterialPlanLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        public string MaterialPlanNo { get; set; }
        public int ProductId { get; set; }
        public DateTime MaterialPlanDate { get; set; }
        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
        //public string DocTypeName { get; set; }
    }

    [Table("ViewMaterialPlanForProdOrderLineBalance")]
    public class ViewMaterialPlanForProdOrderLineBalance
    {
        [Key]
        public int MaterialPlanForProdOrderLineId { get; set; }
        public int MaterialPlanForProdOrderId { get; set; }
        public int ProductId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public decimal BalanceQty { get; set; }
        public int? ProcessId { get; set; }
        public int? MaterialPlanLineId { get; set; }
    }

    [Table("ViewMaterialPlanForProdOrderBalance")]
    public class ViewMaterialPlanForProdOrderBalance
    {
        [Key]
        public int MaterialPlanForProdOrderId { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        public int ProdOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
    }

    [Table("ViewMaterialPlanForSaleOrderBalance")]
    public class ViewMaterialPlanForSaleOrderBalance
    {
        [Key]
        public int MaterialPlanForSaleOrderId { get; set; }
        public int MaterialPlanHeaderId { get; set; }
        public int SaleOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int? MaterialPlanLineId { get; set; }
    }
}
