using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProductHelpListViewModel
    {

        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public string Specification { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public int HeaderId { get; set; }
        public int LineId { get; set; }
        public decimal BalanceQty { get; set; }
        public string HeaderDocNo { get; set; }
        public string HeaderDocNo2 { get; set; }

    }

    public class ProductCustomDetailViewModel
    {
        public int ProductId { get; set; }
        public string BuyerSku { get; set; }
        public string BuyerSpecification { get; set; }
        public string BuyerSpecification1 { get; set; }
        public string BuyerSpecification2 { get; set; }
        public string BuyerSpecification3 { get; set; }
    }
}
