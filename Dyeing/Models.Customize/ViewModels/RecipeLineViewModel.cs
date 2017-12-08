using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.BasicSetup.ViewModels;
using Models.Customize.Models;

namespace Models.Customize.ViewModels
{
    public class RecipeLineViewModel
    {
        public int StockLineId { get; set; }
        public int StockHeaderId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string StockHeaderDocNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public Decimal? DyeingRatio { get; set; }
        public Decimal? TestingQty { get; set; }
        public Decimal? DocQty { get; set; }
        public Decimal? ExcessQty { get; set; }
        public Decimal? HeaderTestingQty { get; set; }
        public Decimal? HeaderQty { get; set; }
        public Decimal Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte? UnitDecimalPlaces { get; set; }
        public string LockReason { get; set; }
        public int? StockId { get; set; }
        public int? StockProcessId { get; set; }
    }
}
