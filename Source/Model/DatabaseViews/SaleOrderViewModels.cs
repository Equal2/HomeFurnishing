using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{
    [Table("ViewSaleOrderHeader")]
    public class ViewSaleOrderHeader
    {
        [Key]
        public int SaleOrderHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public DateTime DueDate { get; set; }
        public int SaleToBuyerId { get; set; }
        public int BillToBuyerId { get; set; }
        public int CurrencyId { get; set; }
        public int Priority { get; set; }
        public int ShipMethodId { get; set; }
        public string ShipAddress { get; set; }
        public int DeliveryTermsId { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public string BuyerOrderNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Decimal TotalQty { get; set; }
        public Decimal TotalAmount { get; set; }
    }
    
    
    [Table("ViewSaleOrderLine")]
    public class ViewSaleOrderLine
    {
        [Key]
        public int SaleOrderLineId { get; set; }
        public int SaleOrderHeaderId { get; set; }
        public decimal OrderQty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
        public string Specification { get; set; }
        public int DeliveryUnitId { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewSaleOrderBalance")]
    public class ViewSaleOrderBalance
    {
        [Key]
        public int SaleOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleOrderHeaderId { get; set; }
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string SaleOrderNo { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
    }

    [Table("ViewSaleOrderBalanceForCancellation")]
    public class ViewSaleOrderBalanceForCancellation
    {
        [Key]
        public int SaleOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleOrderHeaderId { get; set; }
        public string SaleOrderNo { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int SiteId { get; set; }
        public int DivisionId { get; set; }

        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
    }

  
    [Table("ViewSaleOrderLineTheir")]
    public class ViewSaleOrderLineTheir
    {
        [Key]
        public int SaleOrderLineId { get; set; }
        public int SaleOrderHeaderId { get; set; }
        public Decimal OrderQty { get; set; }
        public Decimal DeliveryQty { get; set; }
        public Decimal Rate { get; set; }
        public Decimal OrderAmount { get; set; }
        public int ProductId { get; set; }
        public DateTime DueDate { get; set; }
        public string Specification { get; set; }
        public string DeliveryUnitId { get; set; }
        public string Remark { get; set; }
        public int CancelCount { get; set; }
        public int AmendmentCount { get; set; }
        public int RateAmdCount { get; set; }
    }
}
