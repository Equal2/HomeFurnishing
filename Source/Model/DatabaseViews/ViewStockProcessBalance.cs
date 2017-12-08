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

    [Table("ViewStockProcessBalance")]
    public class ViewStockProcessBalance
    {
        [Key]
        public int StockProcessBalanceId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int PersonId { get; set; }
        public int? ProcessId { get; set; }
        public int ProductId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
        public string LotNo { get; set; }
        public Decimal BalanceQty { get; set; }
    }
}
