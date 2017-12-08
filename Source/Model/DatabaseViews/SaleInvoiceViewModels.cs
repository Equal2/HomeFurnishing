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
    [Table("ViewSaleInvoiceHeader")]
    public class ViewSaleInvoiceHeader
    {
        [Key]
        public int SaleInvoiceHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public int BillToBuyerId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public string BaleNoStr { get; set; }
        public string Remark { get; set; }
        public string LrNo { get; set; }
        public DateTime LrDate { get; set; }
        public string PrivateMark { get; set; }
        public string PortOfLoading { get; set; }
        public string DestinationPort { get; set; }
        public string FinalPlaceOfDelivery { get; set; }
        public string PreCarriageBy { get; set; }
        public string PlaceOfPreCarriage { get; set; }
        public string CircularNo { get; set; }
        public DateTime CircularDate { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string RollNo { get; set; }
        public string DescriptionOfGoods { get; set; }
        public Decimal KindsOfackages { get; set; }
        public string Compositions { get; set; }
        public string OtherRefrence { get; set; }
        public string TermsOfSale { get; set; }
        public string NotifyParty { get; set; }
        public string TransporterInformation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Decimal TotalQty { get; set; }
        public Decimal TotalAmount { get; set; }
    }
    
    
    [Table("ViewSaleInvoiceLineX")]
    public class ViewSaleInvoiceLineX
    {
        [Key]
        public int SaleInvoiceHeaderId { get; set; }
        public int SaleOrderLineId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public int BillToBuyerId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public string BaleNoStr { get; set; }
        public string Remark { get; set; }
        public string LrNo { get; set; }
        public DateTime LrDate { get; set; }
        public string PrivateMark { get; set; }
        public string PortOfLoading { get; set; }
        public string DestinationPort { get; set; }
        public string FinalPlaceOfDelivery { get; set; }
        public string PreCarriageBy { get; set; }
        public string PlaceOfPreCarriage { get; set; }
        public string CircularNo { get; set; }
        public DateTime CircularDate { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string RollNo { get; set; }
        public string DescriptionOfGoods { get; set; }
        public Decimal KindsOfackages { get; set; }
        public string Compositions { get; set; }
        public string OtherRefrence { get; set; }
        public string TermsOfSale { get; set; }
        public string NotifyParty { get; set; }
        public string TransporterInformation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Decimal Qty { get; set; }
        public Decimal Amount { get; set; }
    }
    [Table("ViewSaleInvoiceLine")]
    public class ViewSaleInvoiceLine
    {
        [Key]
        public int SaleInvoiceHeaderId { get; set; }
        public int SaleInvoiceLineId { get; set; }
        public int ? ProductUidId { get; set; }
        public int ProductID { get; set; }

        public int? Dimension1Id { get; set; }

        public int? Dimension2Id { get; set; }
        public string Specification { get; set; }
        public int SaleOrderLineId { get; set; }
        public decimal Qty { get; set; }
        public string BaleNo { get; set; }
        public int? ProductInvoiceGroupId { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal NetWeight { get; set; }
        public Decimal Rate { get; set; }
        public Decimal Amount { get; set; }

        public int Sr { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewSaleDispatchBalance")]
    public class ViewSaleDispatchBalance
    {
        [Key]
        public int SaleDispatchLineId { get; set; }
        public int SaleOrderHeaderId { get; set; }
        public int SaleOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal BalanceDealQty { get; set; }
        public decimal BalanceDocQty { get; set; }
        public int SaleDispatchHeaderId { get; set; }
        public string SaleDispatchNo { get; set; }
        public string SaleOrderNo { get; set; }
        public int ProductId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public Decimal? Rate { get; set; }

        public int? Sr { get; set; }
    }

    [Table("ViewSaleInvoiceBalance")]
    public class ViewSaleInvoiceBalance
    {
        [Key]
        public int SaleInvoiceLineId { get; set; }
        public int SaleDispatchHeaderId { get; set; }
        public int SaleDispatchLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public int SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceNo { get; set; }
        public string BaleNo { get; set; }
        public int ProductId { get; set; }
        public int SaleToBuyerId { get; set; }
        public DateTime OrderDate { get; set; }
        public int SaleInvoiceDocTypeId { get; set; }
        public string SaleDispatchNo { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }

    [Table("ViewSaleInvoiceBalanceForDelivery")]
    public class ViewSaleInvoiceBalanceForDelivery
    {
        [Key]
        public int SaleInvoiceLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleInvoiceHeaderId { get; set; }
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string SaleInvoiceNo { get; set; }
        public int SaleToBuyerId { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
    }

}
