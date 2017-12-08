using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ProductBuyerExcel 
    {
        public int ProductBuyerExcelID { get; set; }



        [ExcelColumn("ProductName")]
        public string ProductName { get; set; }
        [ExcelColumn("BuyerName")]
        public string BuyerName { get; set; }
        [ExcelColumn("BuyerSKU")]
        public string BuyerSKU { get; set; }
        [ExcelColumn("BuyerUPC")]
        public string BuyerUPC { get; set; }




    }
}