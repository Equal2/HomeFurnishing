using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{
    [Table("StockVirtual")]
    public class StockVirtual
    {
        [Key]
        public int DocHeaderId { get; set; }
        public int DocLineId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public int ProductId { get; set; }
        public decimal Qty_Iss { get; set; }
        public decimal Qty_Rec { get; set; }
    }
}
