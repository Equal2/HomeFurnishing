using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class SaleOrderExcel 
    {
        public int SaleOrderExcelID { get; set; }

        [ExcelColumn("OrderNo")]
        public string OrderNumber { get; set; }

        [ExcelColumn("OrderDate")]
        public DateTime OrderDate { get; set; }

        [ExcelColumn("BuyerOrderNo")]
        public string BuyerOrderNo { get; set; }
        [ExcelColumn("SaleToBuyer")]
        public string SaleToBuyer { get; set; }
        [ExcelColumn("BillToBuyer")]
        public string BillToBuyer { get; set; }
        [ExcelColumn("ShipAddress")]
        public string ShipAddress { get; set; }
        [ExcelColumn("Currency")]
        public string Currency { get; set; }
        [ExcelColumn("ShipMethod")]
        public string ShipMethod { get; set; }
        [ExcelColumn("DeliveryTerms")]
        public string DeliveryTerms { get; set; }
        [ExcelColumn("Priority")]
        public string Priority { get; set; }
        [ExcelColumn("Terms&Conditions")]
        public string TermsConditions { get; set; }
        [ExcelColumn("Remark")]
        public DateTime Remark { get; set; }
        [ExcelColumn("DueDate")]
        public DateTime DueDate { get; set; }

        [ExcelColumn("Product")]
        public string Product { get; set; }

        [ExcelColumn("Qty")]
        public int Quantity { get; set; }

        [ExcelColumn("Rate")]
        public double Rate { get; set; }

        [ExcelColumn("BuyerUpcCode")]
        public string BuyerUpcCode { get; set; }


    }
}