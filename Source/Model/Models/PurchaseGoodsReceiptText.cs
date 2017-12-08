using Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseGoodsReceiptText 
    {
        public int PurchaseGoodsReceiptTextID { get; set; }

        [ExcelColumn("Product Uid")]
        public string ProductUid { get; set; }
    }
}