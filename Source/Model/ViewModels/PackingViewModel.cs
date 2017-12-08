using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class PackingHeaderViewModel 
    {
        [Key]
        public int PackingHeaderId { get; set; }
                        
        [Display(Name = "Packing Type"),Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Required(ErrorMessage = "Please select Packing Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }
        
        [Display(Name = "Packing No"),Required,MaxLength(20) ]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        public int DivisionId { get; set; }
        [Display(Name = "Division")]
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        [Display(Name = "Site")]
        public string SiteName { get; set; }

        [Display(Name = "Buyer")]
        public int BuyerId { get; set; }
        [Display(Name = "Buyer")]
        public string BuyerName { get; set; }

        [Display(Name = "Job Worker")]
        public int? JobWorkerId { get; set; }
        [Display(Name = "Job Worker")]
        public string JobWorkerName { get; set; }

        [Display(Name = "Godown")]
        public int GodownId { get; set; }
        [Display(Name = "Godown")]
        public string GodownName { get; set; }

        [Display(Name = "Ship Method"), Required]
        public int ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }

        [Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        [Display(Name = "Deal Unit")]
        public string DealUnitName { get; set; }

        [Display(Name = "Bale No Pattern")]
        public Byte BaleNoPattern { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int Status { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public decimal? TotalQty { get; set; }
        public int? DecimalPlaces { get; set; }

        public PackingSettingsViewModel PackingSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class PackingLineViewModel 
    {
        [Key]
        public int PackingLineId { get; set; }

        [Display(Name = "Packing No"), Required]
        public int PackingHeaderId { get; set; }

        [Display(Name = "Product UID")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }

        [Display(Name = "Is Show All Products ?")]
        public Boolean IsShowAllProducts { get; set; }

        [Display(Name = "Person ProductUid")]
        public int? PersonProductUidId { get; set; }
        public string PersonProductUid { get; set; }

        public int GodownId { get; set; }
        public string GodownName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        [Display(Name = "Sale Order")]
        public int? SaleOrderLineId { get; set; }
        public string SaleOrderNo { get; set; }

        public Decimal? Weight { get; set; }

        public int DocTypeId { get; set; }

        [Display(Name = "Sale Delivery Order")]
        public int? SaleDeliveryOrderLineId { get; set; }
        public string SaleDeliveryOrderNo { get; set; }

        [Display(Name = "Product Invoice Group")]
        public int? ProductInvoiceGroupId { get; set; }
        public string ProductInvoiceGroupName { get; set; }


        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public int unitDecimalPlaces { get; set; }

        public Decimal? BalanceQty { get; set; }

        [Display(Name = "Bale No."), MaxLength(10)]
        public string BaleNo { get; set; }

        public int? BaleCount { get; set; }

        public Decimal? StockInBalanceQty { get; set; }

        [Display(Name = "From Process")]
        public int? FromProcessId { get; set; }
        public string FromProcess { get; set; }

        [Display(Name = "Party Product Uid"), MaxLength(50)]
        public string PartyProductUid { get; set; }

        [Display(Name = "Delivery Unit"), Required]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }
        public int? DealUnitDecimalPlaces { get; set; }

        [Display(Name = "Gross Wt."), Required]
        public Decimal GrossWeight { get; set; }

        [Display(Name = "Net Wt."), Required]
        public Decimal NetWeight { get; set; }

        [Display(Name = "Unit")]
        public string UnitId { get; set; }
        public string UnitName { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal? UnitConversionMultiplier { get; set; }

        public string ImageFolderName { get; set; }

        public string ImageFileName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Seal No")]
        public string SealNo { get; set; }

        [Display(Name = "Lot No")]
        public string LotNo { get; set; }

        [Display(Name = "Rate Remark")]
        public string RateRemark { get; set; }
        public string Specification { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }

        public bool? IsSaleBased { get; set; }
        public bool IsSample { get; set; }

        public bool CreateExtraSaleOrder { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        public int? PackingShipMethodId { get; set; }

        public int? DeliveryOrderShipMethodId { get; set; }

        public int? OtherBuyerDeliveryOrders { get; set; }

        public string BuyerSku { get; set; }
        public string BuyerSpecification { get; set; }
        public string BuyerSpecification1 { get; set; }
        public string BuyerSpecification2 { get; set; }
        public string BuyerSpecification3 { get; set; }
        public ProductBuyerSettingsViewModel ProductBuyerSettings { get; set; }

        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }

        public int? DimensionUnitDecimalPlaces { get; set; }
        public PackingSettingsViewModel PackingSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public int? StockInId { get; set; }
        public string StockInNo { get; set; }



    }

    public class PackingHeaderViewModelWithLog : PackingHeaderViewModel
    {
        [Required, MinLength(20)]
        public string LogReason { get; set; }
    }


    public class PackingMasterDetailModel
    {
        public int PackingHeaderId { get; set; }
        public PackingHeaderViewModel PackingHeaderViewModel { get; set; }
        public List<PackingLineViewModel> PackingLineViewModel { get; set; }
        public PackingHeaderViewModelWithLog PackingHeaderViewModelWithLog { get; set; }
    }

    public class PackingSegmentationViewModel
    {
        [Display(Name = "From Packing No"), Required]
        public int FromPackingHeaderId { get; set; }
        public string FromPackingNo { get; set; }

        [Display(Name = "To Packing No"), Required]
        public int ToPackingHeaderId { get; set; }
        public string ToPackingNo { get; set; }

        [Display(Name = "From Bale No"), Required]
        public int FromBaleNo { get; set; }

        [Display(Name = "To Bale No"), Required]
        public int ToBaleNo { get; set; }
    }

    public class PackingHeaderIndexViewModel
    {
        [Key]
        public int PackingHeaderId { get; set; }

        public int DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        [Display(Name = "Doc Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}"), Required(ErrorMessage = "Please select Packing Date")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20), Required(ErrorMessage = "The OrderNo Field is Required")]
        public string DocNo { get; set; }

        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Buyer")]
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }


        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }
    }

    public class PackingFilterViewModel
    {
        public int PackingHeaderId { get; set; }
        [Display(Name = "Sale Orders")]
        public string SaleOrderHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }


        public string Dimension1Id { get; set; }

        public string Dimension2Id { get; set; }

        public PackingSettingsViewModel PackingSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

    }

    public class PackingListViewModel
    {
        public int PackingHeaderId { get; set; }
        public int PackingLineId { get; set; }

        public List<PackingLineViewModel> PackingLineViewModel { get; set; }

        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
    }
}
