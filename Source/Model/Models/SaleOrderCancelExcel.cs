using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class SaleOrderCancelExcel 
    {
        public int SaleOrderCancelExcelId { get; set; }

        [ExcelColumn("CancelNo")]
        public string SaleOrderCancelNo { get; set; }

        [ExcelColumn("CancelDate")]
        public DateTime CancelDate { get; set; }

        [ExcelColumn("Product")]
        public string Product { get; set; }

        [ExcelColumn("SaleOrderNo")]
        public string SaleOrderNo { get; set; }

        [ExcelColumn("Qty")]
        public int Quantity { get; set; }

        [ExcelColumn("Remark")]
        public string Remark { get; set; }

    }
}