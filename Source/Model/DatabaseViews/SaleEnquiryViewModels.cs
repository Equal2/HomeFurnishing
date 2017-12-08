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
    [Table("ViewSaleEnquiryHeader")]
    public class ViewSaleEnquiryHeader
    {
        [Key]
        public int SaleEnquiryHeaderId { get; set; }
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
    
    
    [Table("ViewSaleEnquiryLine")]
    public class ViewSaleEnquiryLine
    {
        [Key]
        public int SaleEnquiryLineId { get; set; }
        public int SaleEnquiryHeaderId { get; set; }
        public decimal OrderQty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
        public string Specification { get; set; }
        public int DeliveryUnitId { get; set; }
        public string Remark { get; set; }
    }

    [Table("ViewSaleEnquiryBalance")]
    public class ViewSaleEnquiryBalance
    {
        [Key]
        public int SaleEnquiryLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleEnquiryHeaderId { get; set; }
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string SaleEnquiryNo { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
    }

    [Table("ViewSaleEnquiryBalanceForQuotation")]
    public class ViewSaleEnquiryBalanceForQuotation
    {
        [Key]
        public int SaleEnquiryLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleEnquiryHeaderId { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }

        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }


        public int? DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string SaleEnquiryNo { get; set; }
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public DateTime EnquiryDate { get; set; }
    }


}
