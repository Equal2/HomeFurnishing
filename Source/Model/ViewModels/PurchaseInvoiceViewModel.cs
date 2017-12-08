using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseInvoiceHeaderViewModel
    {

        public int PurchaseInvoiceHeaderId { get; set; }
        [Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Invoice Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Invoice No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Currency"), Required]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        [Display(Name = "Purchase Goods Receipt Header")]
        public int? PurchaseGoodsReceiptHeaderId { get; set; }
        public string PurchaseGoodsReceiptHeaderDocNo { get; set; }

        [Display(Name = "Godown")]
        public int GodownId { get; set; }
        public string GodownName { get; set; }

        [MaxLength(20), Display(Name = "Supplier Doc No"), Required]
        public string SupplierDocNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime SupplierDocDate { get; set; }
        [Required(ErrorMessage = "The SalesTaxGroup field is required.")]
        public int? SalesTaxGroupId { get; set; }
        public string SalesTaxGroupName { get; set; }

        public string TermsAndConditions { get; set; }

        public int CreditDays { get; set; }

        [Display(Name = "Supplier Name"), Range(1, int.MaxValue, ErrorMessage = "The Supplier field is required"), Required]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int BillingAccountId { get; set; }
        public string BillingAccountName { get; set; }
        public bool CalculateDiscountOnRate { get; set; }
        public int? DeliveryTermsId { get; set; }
        public string DeliveryTermsName { get; set; }
        public int? ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public byte? UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }
        public PurchaseInvoiceSettingsViewModel PurchaseInvoiceSettings { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }

        public string sDocDate { get; set; }
        public decimal Rate { get; set; }
        public decimal? DiscountPer { get; set; }

    }

    public class PurchaseInvoiceLineViewModel
    {
        public int PurchaseInvoiceLineId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public string ProductUidName { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        [MaxLength(10)]
        public string LotNo { get; set; }
        public decimal ReceiptBalQty { get; set; }
        public decimal ReceiptBalDocQty { get; set; }
        public decimal ReceiptBalDealQty { get; set; }
        public decimal IndentBalQty { get; set; }
        public decimal Qty { get; set; }
        [Display(Name = "Purchase Invoice"), Required]
        public int PurchaseInvoiceHeaderId { get; set; }
        public string PurchaseInvoiceHeaderDocNo { get; set; }

        [Display(Name = "Receipt"), Required]
        public int PurchaseGoodsReceiptLineId { get; set; }
        public string PurchaseGoodsRecieptHeaderDocNo { get; set; }

        [Display(Name = "Receipt")]
        public int? PurchaseIndentLineId { get; set; }

        [Display(Name = "Purchase Invoice"), Required]
        public int PurchaseIndentHeaderId { get; set; }
        public string PurchaseIndentHeaderDocNo { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }
        public int SupplierId { get; set; }

        public bool CalculateDiscountOnRate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }


        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        public Decimal RateAfterDiscount { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string PurchaseGoodsReceiptHeaderDocNo { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public List<PurchaseInvoiceLineCharge> linecharges { get; set; }
        public List<PurchaseInvoiceHeaderCharge> footercharges { get; set; }
        public PurchaseInvoiceSettingsViewModel PurchInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public bool UnitConversionException { get; set; }
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
        public decimal DocQty { get; set; }
        public decimal AdjShortQty { get; set; }
        public decimal ShortQty { get; set; }
        public List<HeaderCharges> RHeaderCharges { get; set; }
        public List<LineCharges> RLineCharges { get; set; }
        public string LockReason { get; set; }
    }
    public class PurchaseInvoiceLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int PurchaseInvoiceLineId { get; set; }
        public string ProductName { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public string PurchaseIndentDocNo { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public string PurchaseOrderHeaderDocNo { get; set; }
        public string PurchaseGoodsRecieptHeaderDocNo { get; set; }
        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitName { get; set; }

        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public decimal ShortQty { get; set; }
        public int? ReceiptHeaderId { get; set; }
        public int? OrderHeaderId { get; set; }
        public int? ReceiptLineId { get; set; }
        public int? OrderLineId { get; set; }
        public int? ReceiptDocTypeId { get; set; }
        public int? OrderDocTypeId { get; set; }
    }

    public class PurchaseInvoiceLineFilterViewModel
    {
        public int PurchaseInvoiceHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string PurchaseGoodsReceiptHeaderId { get; set; }

        public string PurchaseOrderHeaderId { get; set; }
        public string PurchaseIndentHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

        public string DealUnitId { get; set; }
        public decimal Rate { get; set; }
        public PurchaseInvoiceSettingsViewModel PurchaseInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class PurchaseInvoiceMasterDetailModel
    {
        public List<PurchaseInvoiceLineViewModel> PurchaseInvoiceLineViewModel { get; set; }
        public PurchaseInvoiceSettingsViewModel PurchaseInvoiceSettings { get; set; }
    }
    public class PurchaseInvoiceListViewModel
    {
        public int PurchaseInvoiceHeaderId { get; set; }
        public int PurchaseInvoiceLineId { get; set; }
        public string DocNo { get; set; }
        public string GoodsReceiptDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public decimal BalanceQty { get; set; }
    }

}
