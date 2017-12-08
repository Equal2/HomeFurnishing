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
    public class SaleDispatchReturnHeaderViewModel
    {

        public int SaleDispatchReturnHeaderId { get; set; }

        [Display(Name = "Return Type"), Required]       
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Return Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Return No"), Required, MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters")]        
        public string DocNo { get; set; }
        
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }

        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]        
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Buyer Name")]
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }

        [Display(Name = "Remark"), Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public SaleDispatchSettingsViewModel SaleDispatchSettings { get; set; }
        public int? GatePassHeaderId { get; set; }
        public string GatePassHeaderDocNo { get; set; }        
        public int GodownId { get; set; }
        public string GodownName { get; set; }
        
        [Display(Name = "PurchaseInvoiceReturn")]
        public int? PurchaseInvoiceReturnHeaderId { get; set; }
        public string PurchaseInvoiceReturnDocNo { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string LockReason { get; set; }
    }

    public class SaleDispatchReturnLineViewModel
    {
        public int SaleDispatchReturnLineId { get; set; }

        [Display(Name = "Purchase Goods Return"), Required]        
        public int SaleDispatchReturnHeaderId { get; set; }
        public string SaleDispatchReturnHeaderDocNo { get; set; }

        [Display(Name = "Purchase Goods Receipt"), Required]        
        public int SaleDispatchLineId { get; set; }
        public string SaleDispatchHeaderDocNo { get; set; }
        public string SaleOrderDocNo { get; set; }
        public int BuyerId { get; set; }

        [Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        public string BuyerName { get; set; }

        public int GodownId { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public Decimal GoodsReceiptBalQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }

        public Decimal? Weight { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public SaleDispatchSettingsViewModel SaleDispatchSettings { get; set; }
        
        public int? PurchaseInvoiceReturnLineId { get; set; }

        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public string LockReason { get; set; }
    }
    public class SaleDispatchReturnLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int SaleDispatchReturnLineId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }        
        public string SaleDispatchHeaderDocNo { get; set; }
        public Decimal? Weight { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }

        public int StockId { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
    }

    public class SaleDispatchReturnLineFilterViewModel
    {
        public int SaleDispatchReturnHeaderId { get; set; }
        public int BuyerId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string SaleDispatchHeaderId { get; set; }

        public string SaleOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class SaleDispatchReturnMasterDetailModel
    {
        public List<SaleDispatchReturnLineViewModel> SaleDispatchReturnLineViewModel { get; set; }
    }

    
}
