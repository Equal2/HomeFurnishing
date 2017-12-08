using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseInvoiceExcel 
    {
        public int PurchaseInvoiceExcelID { get; set; }

        [ExcelColumn("InvoiceNo")]
        public string InvoiceNumber { get; set; }

        [ExcelColumn("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [ExcelColumn("Product")]
        public string Product { get; set; }

        [ExcelColumn("PurchaseOrderNo")]
        public string PurchaseOrderNo { get; set; }

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