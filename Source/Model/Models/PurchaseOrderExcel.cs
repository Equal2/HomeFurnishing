using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseOrderExcel 
    {
        public int PurchaseOrderExcelID { get; set; }

        [ExcelColumn("OrderNo")]
        public string OrderNumber { get; set; }

        [ExcelColumn("OrderDate")]
        public DateTime OrderDate { get; set; }

        [ExcelColumn("Product")]
        public string Product { get; set; }

        [ExcelColumn("ShipDate")]
        public DateTime ShipDate { get; set; }

        [ExcelColumn("Qty")]
        public int Quantity { get; set; }

        [ExcelColumn("Supplier")]
        public string Supplier { get; set; }

        [ExcelColumn("Rate")]
        public double Rate { get; set; }

    }
}