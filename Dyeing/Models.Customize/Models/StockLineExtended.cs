using Model;
using Models.BasicSetup.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class StockLineExtended : EntityBase
    {
        [Key]
        [ForeignKey("StockLine")]
        public int StockLineId { get; set; }
        public StockLine StockLine { get; set; }
        public Decimal? DyeingRatio { get; set; }
        public Decimal? TestingQty { get; set; }
        public Decimal? DocQty { get; set; }
        public Decimal? ExcessQty { get; set; }

    }
}
