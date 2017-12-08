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

    [Table("ViewStockInBalance")]
    public class ViewStockInBalance
    {
        [Key]
        public int StockInId { get; set; }
        public string StockInNo { get; set; }
        public DateTime StockInDate { get; set; }
        public int? PersonId { get; set; }
        public int ProductId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
        public string LotNo { get; set; }
        public Decimal BalanceQty { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
    }
}
