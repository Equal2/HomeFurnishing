using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using Model.ViewModels;

namespace Model.ViewModel
{
    public class SaleInvoiceReturnHeaderViewModel
    {

        public int SaleInvoiceReturnHeaderId { get; set; }
        [Required]
        public int DocTypeId { get; set; }
        public string DocTypeName{ get; set; }

        [Display(Name = "Invoice Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Invoice No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]        
        public int SiteId { get; set; }
        public string SiteName{ get; set; }

        //[Display(Name = "Currency"), Required]
        //public int CurrencyId { get; set; }
        //public string CurrencyName{ get; set; }
      
        //public int? SalesTaxGroupId { get; set; }
        //public string SalesTaxGroupName { get; set; }

        
        [Display(Name = "Buyer Name"),Range(1,int.MaxValue,ErrorMessage="The Buyer field is required"),Required]
        public int BuyerId { get; set; }
        public string BuyerName{ get; set; }

        public bool CalculateDiscountOnRate { get; set; }        

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        public int? ProcessId { get; set; }

        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public int ? SaleDispatchReturnHeaderId { get; set; }

        public string SaleInvoiceDocNo { get; set; }

        public int? GodownId { get; set; }
        public SaleInvoiceSettingsViewModel SaleInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FirstName { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string Nature { get; set; }
    }

    public class SaleInvoiceReturnLineViewModel
    {
        public int SaleInvoiceReturnLineId { get; set; }
        [Required,Range(1,int.MaxValue,ErrorMessage="The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SaleOrderDocNo { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        [MaxLength(10)]
        public string LotNo { get; set; }

        public string BaleNo { get; set; }
        public decimal InvoiceBalQty { get; set; }
        public decimal Qty { get; set; }
        public int BuyerId { get; set; }

        [Display(Name = "Sale Invoice"), Required]        
        public int SaleInvoiceReturnHeaderId { get; set; }
        public string SaleInvoiceReturnHeaderDocNo { get; set; }

        [Display(Name = "Receipt"), Required]
        public int SaleInvoiceLineId { get; set; }
        public string SaleInvoiceHeaderDocNo{ get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        public Decimal RateAfterDiscount { get; set; }

        public Decimal? Weight { get; set; }
        public int? GodownId { get; set; }

        public bool CalculateDiscountOnRate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }
        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public string UnitId { get; set; }
        public SaleInvoiceSettingsViewModel SaleInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public List<SaleInvoiceReturnLineCharge> linecharges { get; set; }
        public List<SaleInvoiceReturnHeaderCharge> footercharges { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public string Nature { get; set; }
    }
    public class SaleInvoiceReturnLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int SaleInvoiceReturnLineId { get; set; }
        public string ProductName { get; set; }
        public string SaleOrderDocNo { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string SaleInvoiceHeaderDocNo { get; set; }
        public string SaleDispatchHeaderDocNo { get; set; }      
        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
    }

    public class SaleInvoiceReturnLineFilterViewModel
    {
        public int SaleInvoiceReturnHeaderId { get; set; }
        public int BuyerId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string SaleDispatchHeaderId { get; set; }

        public string SaleInvoiceHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class SaleInvoiceReturnMasterDetailModel
    {
        public List<SaleInvoiceReturnLineViewModel> SaleInvoiceReturnLineViewModel { get; set; }
    }

    public class SaleInvoiceListViewModel
    {
        public int SaleInvoiceHeaderId { get; set; }
        public int SaleInvoiceLineId { get; set; }

        public List<DirectSaleInvoiceLineViewModel> DirectSaleInvoiceLineViewModel { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
    }
}
