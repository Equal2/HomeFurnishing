using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{
    [Table("ViewPurchaseGoodsReceiptHeader")]
    public class ViewPurchaseGoodsReceiptHeader
    {
        [Key]
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal TotalQty { get; set; }
    }
    
    
    [Table("ViewPurchaseGoodsReceiptLine")]
    public class ViewPurchaseGoodsReceiptLine
    {
        [Key]
        public int PurchaseGoodsReceiptLineId { get; set; }
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public decimal Qty { get; set; }
        public int ProductId { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewPurchaseGoodsReceiptBalance")]
    public class ViewPurchaseGoodsReceiptBalance
    {
        [Key]
        public int PurchaseGoodsReceiptLineId { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public int PurchaseOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal BalanceDealQty { get; set; }
        public decimal BalanceDocQty { get; set; }
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public string PurchaseGoodsReceiptNo { get; set; }
        public string PurchaseOrderNo { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }
    [Table("ViewPurchaseInvoiceBalance")]
    public class ViewPurchaseInvoiceBalance
    {
        [Key]
        public int PurchaseInvoiceLineId { get; set; }
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public int PurchaseGoodsReceiptLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int PurchaseInvoiceHeaderId { get; set; }
        public string PurchaseInvoiceNo { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public int PurchaseInvoiceDocTypeId { get; set; }
        public string PurchaseGoodsReceiptNo { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }
}
