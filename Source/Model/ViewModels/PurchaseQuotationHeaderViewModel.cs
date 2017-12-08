using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    public class PurchaseQuotationHeaderViewModel
    {
        public int PurchaseQuotationHeaderId { get; set; }

        [Display(Name = "Order Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Order Date"), Required]
        public DateTime DocDate { get; set; }
        public string sDocDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }

        [Display(Name = "Order No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Supplier Name"),Range(1,int.MaxValue,ErrorMessage="Supplier field is required")]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        public int BillingAccountId { get; set; }
        public string BillingAccountName { get; set; }
        public bool CalculateDiscountOnRate { get; set; }

        [Display(Name = "Ship Method")]
        public int ? ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }

        [Display(Name = "Delivery Terms")]
        public int? DeliveryTermsId { get; set; }
        public string DeliveryTermsName { get; set; }
        public string TermsAndConditions { get; set; }

        public Decimal? ExchangeRate { get; set; }

        [Display(Name = "Ship Address")]
        public string ShipAddress { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int ? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        
        public int? SalesTaxGroupPersonId { get; set; }
        public string SalesTaxGroupPersonName { get; set; }
        [Display(Name = "Supplier Ship Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SupplierShipDate { get; set; }

        [Display(Name = "Supplier Remark")]
        public string SupplierRemark { get; set; }

        [Display(Name = "CreditDays")]
        public int? CreditDays { get; set; }

        [Display(Name = "Progress %")]
        [Range(0, 100)]
        public int? ProgressPer { get; set; }

        public bool? isUninspected { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        
        public int CalculationId { get; set; }
        public PurchaseQuotationSettingsViewModel PurchaseQuotationSettings { get; set; }
        public byte? UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LockReason { get; set; }
        public decimal? Rate { get; set; }
        public decimal? DiscountPer { get; set; }

        [Display(Name = "Vendor Quotation No"), Required, MaxLength(20)]
        public string VendorQuotationNo { get; set; }

        [Display(Name = "Vendor Quotation Date"), Required]
        public DateTime VendorQuotationDate { get; set; }

        public int? SalesTaxGroupId { get; set; }
        public string SalesTaxGroupName { get; set; }
    }


    public class PurchaseQuotationIndexViewModel
    {
        public int PurchaseQuotationHeaderId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), Required, MaxLength(20)]
        public string DocNo { get; set; }
        public string SupplierName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
    }

    public class PurchaseQuotationLineListViewModel
    {
        public int PurchaseQuotationHeaderId { get; set; }
        public int PurchaseQuotationLineId { get; set; }
        public int PurchaseIndentHeaderId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }

    public class PurchaseQuotationLineFillViewModel : PurchaseQuotationLineListViewModel
    {
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Qty { get; set; }
        public string UnitId { get; set; }
    }


}
