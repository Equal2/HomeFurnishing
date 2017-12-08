using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class SaleInvoicePrintViewModel
    {
        [Key]
        public int? BaleNoChangeGrp { get; set; }
        public Decimal? Prev_BaleNoToShort { get; set; }
        public int SaleInvoiceHeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public string BillToBuyerName { get; set; }
        public string BillToBuyerAddress { get; set; }
        public string BIllToPartyCity { get; set; }
        public string BillToPartyCountry { get; set; }
        public string CurrencyName { get; set; }
        public string BLNo { get; set; }
        public DateTime? BLDate { get; set; }
        public string PrivateMark { get; set; }
        public string PortOfLoading { get; set; }
        public string DestinationPort { get; set; }
        public string FinalPlaceOfDelivery { get; set; }
        public string PreCarriageBy { get; set; }
        public string PlaceOfPreCarriage { get; set; }
        public string CircularNo { get; set; }
        public DateTime? CircularDate { get; set; }
        public string OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string BaleNoSeries { get; set; }
        public string DescriptionOfGoods { get; set; }
        public string DescriptionOfGoodsWithoutCRLF { get; set; }
        public string PackingMaterialDescription { get; set; }
        public Decimal? KindsOfackages { get; set; }
        public string Compositions { get; set; }
        public string OtherRefrence { get; set; }
        public string TermsOfSale { get; set; }
        public string NotifyParty { get; set; }
        public string TransporterInformation { get; set; }
        public string ShipMethodName { get; set; }
        public string DeliveryTermsName { get; set; }
        public string ProductName { get; set; }
        public string ProductSpecification { get; set; }
        public string ProductDesignName{ get; set; }
        public string ProductColourName{ get; set; }
        public string ProductTypeName { get; set; }
        public string ProductSizeName { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductInvoiceGroupName { get; set; }
        public string SaleOrderNo { get; set; }
        public string ItcHsCode { get; set; }
        public Decimal? Knots { get; set; }
        public string BaleNo { get; set; }
        public string DealUnitName { get; set; }
        public Decimal? BaleNoToSort { get; set; }
        public Decimal? Qty { get; set; }
        public Decimal? Rate { get; set; }
        public Decimal? Amount { get; set; }
        public Decimal? SqFeetArea { get; set; }
        public Decimal? SqMeterArea { get; set; }
        public Decimal? TotalSqFeetArea { get; set; }
        public Decimal? GrossWeight { get; set; }
        public Decimal? NetWeight { get; set; }
        public Decimal? TotalRugGrossWeight { get; set; }
        public Decimal? TotalFinishedProductGrossWeight { get; set; }
        public Decimal? TotalGrossWeight { get; set; }
        public Decimal? TotalRugNetWeight { get; set; }
        public Decimal? TotalFinishedProductNetWeight { get; set; }
        public Decimal? TotalNetWeight { get; set; }
        public Decimal? TotalRugQty { get; set; }
        public Decimal? TotalFinishedProductQty { get; set; }
        public Decimal? InvoiceAmount { get; set; }
        public int? TotalRugRolls { get; set; }
        public int? TotalFinishedProductRolls { get; set; }
        public int? TotalRolls { get; set; }
        public string TotalRoleText { get; set; }
        public string TotalRoleUnitText { get; set; }
        public string ShipToPartyAddress { get; set; }
        public string TotalQtyText { get; set; }
        public string TotalGrossWeightText { get; set; }
        public string TotalNetWeightText { get; set; }
        public string ProductCollectionName { get; set; }
        public string VehicleNo { get; set; }
        public Decimal? Freight { get; set; }
        public string CourierName { get; set; }
        public string WeightText { get; set; }
        public int SeparateWeightInInvoice { get; set; }
        public Decimal TotalQty { get; set; }
        public int InvoiceOnProductUnit { get; set; }
    }

    public class MasterKeyPrintViewModel
    {
        public string SaleOrderNo { get; set; }
        public string ProductName { get; set; }
        public string CurrencyName { get; set; }
        public Decimal? Qty { get; set; }
        public Decimal? RatePerPcs { get; set; }
        public Decimal? Amount { get; set; }
        public string BuyerSku { get; set; }
        public string Design { get; set; }
        public string Size { get; set; }
        public string BuyerSpecification { get; set; }
        public string BaleNoList { get; set; }
    }
}
