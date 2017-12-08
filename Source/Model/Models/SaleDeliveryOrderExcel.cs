using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class SaleDeliveryOrderExcel 
    {
        [ExcelColumn("Product")]
        public string Product { get; set; }

        [ExcelColumn("Qty")]
        public int Qty { get; set; }
    }
}