using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseQuotationLineViewModel
    {
        public int PurchaseQuotationLineId { get; set; }

        [Display(Name = "Purchase Order")]       
        public int PurchaseQuotationHeaderId { get; set; }
        public string PurchaseQuotationHeaderDocNo { get; set; }

        [Display(Name = "Purchase Indent")]        
        public int? PurchaseIndentLineId { get; set; }
        public string PurchaseIndentDocNo { get; set; }

        [Display(Name = "Product"), Required,Range(1,int.MaxValue,ErrorMessage="Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]        
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        [MaxLength(50)]
        public string Specification { get; set; }
        public Decimal IndentBalanceQty { get; set; }
        public int SupplierId { get; set; }

        public bool CalculateDiscountOnRate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Ship Date")]        
        public DateTime? ShipDate { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        public DateTime? DueDate { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int unitDecimalPlaces { get; set; }

        [Display(Name = "Delivery Unit"), Required]       
        public string DealUnitId { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        [Display(Name = "Amount")]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        [Display(Name = "Unit Conversion Multiplier"),Required]
        public Decimal UnitConversionMultiplier { get; set; }
        public string DealUnitName { get; set; }
        public int ProgressPerc { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public bool UnitConversionException { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int ? IndentDocTypeId { get; set; }
        public int ? IndentHeaderId { get; set; }
        public string LockReason { get; set; }
        public PurchaseQuotationSettingsViewModel PurchQuotationSettings { get; set; }
        public List<PurchaseQuotationLineCharge> linecharges { get; set; }
        public List<PurchaseQuotationHeaderCharge> footercharges { get; set; }

    }


    public class PurchaseQuotationLineIndexViewModel
    {
        public int PurchaseQuotationHeaderId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), Required, MaxLength(20)]
        public string DocNo { get; set; }
        public string SupplierName { get; set; }
        public string Remark { get; set; }
    }

    public class PurchaseQuotationLineFilterViewModel
    {
        public int PurchaseQuotationHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Sale Order No")]
        public string PurchaseIndentHeaderId{ get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

        public decimal Rate { get; set; }
        public string DealUnitId { get; set; }
        public PurchaseQuotationSettingsViewModel PurchaseQuotationSettings { get; set; }
    }

    public class PurchaseQuotationMasterDetailModel
    {
        public List<PurchaseQuotationLineViewModel> PurchaseQuotationLineViewModel { get; set; }
        public PurchaseQuotationSettingsViewModel PurchaseQuotationSettings { get; set; }
    }
}
