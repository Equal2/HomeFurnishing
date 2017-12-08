using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseInvoiceReturnHeaderViewModel
    {

        public int PurchaseInvoiceReturnHeaderId { get; set; }
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

        [Display(Name = "Currency"), Required]
        public int CurrencyId { get; set; }
        public string CurrencyName{ get; set; }
      
        public int? SalesTaxGroupId { get; set; }
        public string SalesTaxGroupName { get; set; }

        
        [Display(Name = "Supplier Name"),Range(1,int.MaxValue,ErrorMessage="The Supplier field is required"),Required]
        public int SupplierId { get; set; }
        public string SupplierName{ get; set; }

        public bool CalculateDiscountOnRate { get; set; }        

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public int ? PurchaseGoodsReturnHeaderId { get; set; }
        
        public int GodownId { get; set; }
        public PurchaseInvoiceSettingsViewModel PurchaseInvoiceSettings { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime? GatePassDocDate { get; set; }
        public int? GatePassHeaderId { get; set; }
        public string LockReason { get; set; }

    }

    public class PurchaseInvoiceReturnLineViewModel
    {
        public int PurchaseInvoiceReturnLineId { get; set; }
        [Required,Range(1,int.MaxValue,ErrorMessage="The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        [MaxLength(10)]
        public string LotNo { get; set; }
        public decimal InvoiceBalQty { get; set; }
        public decimal Qty { get; set; }
        public int SupplierId { get; set; }

        [Display(Name = "Purchase Invoice"), Required]        
        public int PurchaseInvoiceReturnHeaderId { get; set; }
        public string PurchaseInvoiceReturnHeaderDocNo { get; set; }

        [Display(Name = "Receipt"), Required]
        public int PurchaseInvoiceLineId { get; set; }
        public string PurchaseInvoiceHeaderDocNo{ get; set; }

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

        

        public bool CalculateDiscountOnRate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string UnitId { get; set; }
        public PurchaseInvoiceSettingsViewModel PurchInvoiceSettings { get; set; }
        public List<PurchaseInvoiceReturnLineCharge> linecharges { get; set; }
        public List<PurchaseInvoiceReturnHeaderCharge> footercharges { get; set; }
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
        public string LockReason { get; set; }
    }
    public class PurchaseInvoiceReturnLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int PurchaseInvoiceReturnLineId { get; set; }
        public string ProductName { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string PurchaseInvoiceHeaderDocNo { get; set; }
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
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
    }

    public class PurchaseInvoiceReturnLineFilterViewModel
    {
        public int PurchaseInvoiceReturnHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string PurchaseGoodsReceiptHeaderId { get; set; }

        public string PurchaseInvoiceHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class PurchaseInvoiceReturnMasterDetailModel
    {
        public List<PurchaseInvoiceReturnLineViewModel> PurchaseInvoiceReturnLineViewModel { get; set; }
    }

}
