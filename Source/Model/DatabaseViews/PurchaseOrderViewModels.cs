using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{
    [Table("ViewPurchaseOrderHeader")]
    public class ViewPurchaseOrderHeader
    {
        [Key]
        public int PurchaseOrderHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public DateTime DueDate { get; set; }
        public int SupplierId { get; set; }
        public int ShipMethodId { get; set; }
        public int DeliveryTermsId { get; set; }
        public int ShipAddressId { get; set; }
        public DateTime SupplierShipDate { get; set; }
        public string SupplierRemark { get; set; }
        public int CreditDays { get; set; }
        public int ProgressPer { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    
    [Table("ViewPurchaseOrderLine")]
    public class ViewPurchaseOrderLine
    {
        [Key]
        public int PurchaseOrderLineId { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public decimal OrderQty { get; set; }
        public decimal OrderDeliveryQty { get; set; }
        public decimal Rate { get; set; }
        public decimal OrderAmount { get; set; }
        public int ProductId { get; set; }
        public string DeliveryUnitId { get; set; }
        public string Remark { get; set; }

    }

    [Table("ViewPurchaseOrderBalance")]
    public class  ViewPurchaseOrderBalance
    {
        [Key]
        public int PurchaseOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public string PurchaseOrderNo { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        //public string DocumentTypeShortName { get; set; }
    }

    [Table("ViewPurchaseOrderBalanceForInvoice")]
    public class ViewPurchaseOrderBalanceForInvoice
    {
        [Key]
        public int PurchaseOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public string PurchaseOrderNo { get; set; }
        public int ProductId { get; set; }
        public int SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
    }
}
