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
    [Table("ViewSaleDeliveryOrderBalance")]
    public class ViewSaleDeliveryOrderBalance
    {
        [Key]
        public int SaleDeliveryOrderLineId { get; set; }
        public decimal OrderQty { get; set; }
        public decimal DispatchQty { get; set; }
        public decimal BalanceQty { get; set; }
        public int SaleOrderLineId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int SaleDeliveryOrderHeaderId { get; set; }
        public string SaleDeliveryOrderNo { get; set; }
        public int BuyerId { get; set; }
        public DateTime DueDate { get; set; }
        public int Priority { get; set; }
    }
}
