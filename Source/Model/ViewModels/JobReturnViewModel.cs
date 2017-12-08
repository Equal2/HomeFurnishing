using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobReturnHeaderViewModel
    {

        public int JobReturnHeaderId { get; set; }

        [Display(Name = "Return Type"), Required]       
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Return Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Return No"), Required, MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters")]        
        public string DocNo { get; set; }

        public int ProcessId { get; set; }

        public int ReasonId { get; set; }
        public string ReasonName { get; set; }

        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]        
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        
        [Display(Name = "JobWorker Name")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }


        [Display(Name = "Order By Name")]
        public int OrderById { get; set; }
        public string OrderByName { get; set; }

        [Display(Name = "Remark"), Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int? GatePassHeaderId { get; set; }
        public string GatePassHeaderDocNo { get; set; }        
        public int GodownId { get; set; }
        public string GodownName { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime? GatePassDocDate { get; set; }
        public string LockReason { get; set; }
        public decimal? TotalQty { get; set; }
        public int? DecimalPlaces { get; set; }
    }

    public class JobReturnLineViewModel
    {
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        public int JobReturnLineId { get; set; }

        [Display(Name = "Job Return"), Required]        
        public int JobReturnHeaderId { get; set; }
        public string JobReturnHeaderDocNo { get; set; }

        [Display(Name = "Job Receive"), Required]
        public int JobReceiveLineId { get; set; }
        public string JobReceiveHeaderDocNo { get; set; }
        public string JobOrderDocNo { get; set; }
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }
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
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        
        public int? PurchaseInvoiceReturnLineId { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public int ? GodownId { get; set; }
        public string LockReason { get; set; }
    }
    public class JobReturnLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int JobReturnLineId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public string JobReceiveHeaderDocNo { get; set; }
        public Decimal? Weight { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }

        public int StockId { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
    }

    public class JobReturnLineFilterViewModel
    {
        public int JobReturnHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string JobReceiveHeaderId { get; set; }

        public string JobOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class JobReturnMasterDetailModel
    {
        public List<JobReturnLineViewModel> JobReturnLineViewModel { get; set; }
    }

    public class GatePassGeneratedViewModel
    {
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public decimal Qty { get; set; }
        public string UnitId { get; set; }
    }

    public class JobReturnBarCodeSequenceViewModel
    {
        public string ProductName { get; set; }
        public int JobReceiveLineId { get; set; }
        public string SJobRecLineIds { get; set; }
        public int JobReturnHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
    }

    public class JobReturnBarCodeSequenceListViewModel
    {
        public List<JobReturnBarCodeSequenceViewModel> JobReturnBarCodeSequenceViewModel { get; set; }
        public List<JobReturnBarCodeSequenceViewModel> JobReturnBarCodeSequenceViewModelPost { get; set; }
    }

}
