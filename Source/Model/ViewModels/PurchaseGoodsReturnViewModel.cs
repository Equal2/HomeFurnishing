using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseGoodsReturnHeaderViewModel
    {

        public int PurchaseGoodsReturnHeaderId { get; set; }

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
        
        [Display(Name = "Supplier Name")]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        [Display(Name = "Remark"), Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public PurchaseGoodsReceiptSettingsViewModel PurchaseGoodsReceiptSettings { get; set; }
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
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime? GatePassDocDate { get; set; }
        public string LockReason { get; set; }

    }

    public class PurchaseGoodsReturnLineViewModel
    {
        public int PurchaseGoodsReturnLineId { get; set; }

        [Display(Name = "Purchase Goods Return"), Required]        
        public int PurchaseGoodsReturnHeaderId { get; set; }
        public string PurchaseGoodsReturnHeaderDocNo { get; set; }

        [Display(Name = "Purchase Goods Receipt"), Required]        
        public int PurchaseGoodsReceiptLineId { get; set; }
        public string PurchaseGoodsReceiptHeaderDocNo { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public int SupplierId { get; set; }

        [Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        public string SupplierName { get; set; }

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
        public PurchaseGoodsReceiptSettingsViewModel PurchGoodsReceiptSettings { get; set; }
        
        public int? PurchaseInvoiceReturnLineId { get; set; }

        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public string LockReason { get; set; }
    }
    public class PurchaseGoodsReturnLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int PurchaseGoodsReturnLineId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }        
        public string PurchaseGoodsRecieptHeaderDocNo { get; set; }
        public Decimal? Weight { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }

        public int StockId { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
    }

    public class PurchaseGoodsReturnLineFilterViewModel
    {
        public int PurchaseGoodsReturnHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string PurchaseGoodsReceiptHeaderId { get; set; }

        public string PurchaseOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class PurchaseGoodsReturnMasterDetailModel
    {
        public List<PurchaseGoodsReturnLineViewModel> PurchaseGoodsReturnLineViewModel { get; set; }
    }
}
