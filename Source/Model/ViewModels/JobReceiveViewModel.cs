using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    public class JobReceiveHeaderViewModel
    {
        public int JobReceiveHeaderId { get; set; }

        [Display(Name = "Job Receive Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Job Receive Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Job Receive No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Process"),Range(1,int.MaxValue,ErrorMessage="Process field is required")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Job Worker"), Range(1, int.MaxValue, ErrorMessage = "Job Worker field is required")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }

        [Display(Name = "JobWorker Doc. No."), MaxLength(20)]
        public string JobWorkerDocNo { get; set; }

        public DateTime? JobWorkerDocDate { get; set; }

        [Display(Name = "Job Receive By"), Range(1, int.MaxValue, ErrorMessage = "Job Receive field is required")]
        public int? JobReceiveById { get; set; }
        public string JobReceiveByName { get; set; }
        public int Status { get; set; }

        public int GodownId { get; set; }
        public string GodownName { get; set; }       

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int? ReviewCount { get; set; }
        public string ModifiedBy { get; set; }
        public string ReviewBy { get; set; }
        public string LockReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class JobReceiveHeaderListViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public decimal BalanceQty { get; set; }
    }


    public class JobReceiveIndexViewModel
    {

        public int JobReceiveHeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocTypeName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string JobWorkerDocNo { get; set; }
        public DateTime? JobWorkerDocDate { get; set; }
        public string JobWorkerName { get; set; }
        public string ModifiedBy { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public decimal? TotalQty { get; set; }
        public int? DecimalPlaces { get; set; }

    }
    public class JobReceiveListViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobReceiveLineId { get; set; }
        public string DocNo { get; set; }
        public string JobWorkerDocNo { get; set; }
        public DateTime? JobWorkerDocDate { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
    }

    public class JobReceiveProductHelpList
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int JobReceiveLineId { get; set; }
        public int JobOrderLineId { get; set; }
        public string JobReceiveDocNo { get; set; }
        public string Specification { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string JobOrderNo { get; set; }
        public string JobReceiveNo { get; set; }
        public decimal Qty { get; set; }
        public string ProductUidName { get; set; }
        public int ProductType { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int JobWorkerId { get; set; }
    }






    //Line ViewModels

    [Serializable]
    public class JobReceiveLineViewModel
    {

        public string ProductUidName { get; set; }
        public int ? ProductUidId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int JobReceiveLineId { get; set; }

        [Display(Name = "Job Receive"), Required]
        public int JobReceiveHeaderId { get; set; }
        public string JobReceiveHeaderDocNo { get; set; }

        [Display(Name = "Job Order"), Required]
        public int? JobOrderLineId { get; set; }
        public string JobOrderHeaderDocNo { get; set; }
        public int? OrderDocTypeId { get; set; }
        public int? OrderHeaderId { get; set; }
        public int? MachineId { get; set; }
        public string MachineName { get; set; }

        //public decimal OrderQty { get; set; }
        //public decimal OrderDealQty { get; set; }

        [Display(Name = "Qty"), Required]
        public decimal Qty { get; set; }

        [Display(Name = "Job Qty")]
        public decimal DocQty { get; set; }

        [Display(Name = "Pass Qty")]
        public decimal PassQty { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal LossQty { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal ReceiveQty { get; set; }

        [Display(Name = "Penalty Amount")]
        public Decimal PenaltyAmt { get; set; }
        public Decimal PenaltyRate { get; set; }
        public decimal Weight { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        public int? StockId { get; set; }

        public int? SalesTaxGroupProductId { get; set; }
        public string SalesTaxGroupProductName { get; set; }

        public int? StockProcessId { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int ?  Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int ? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }



        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }

        public string Specification { get; set; }
        [Display(Name = "Delivery Unit"), Required]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal UnitConversionMultiplier { get; set; }
        public int JobWorkerId { get; set; }
        public decimal OrderBalanceQty { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public Decimal ? IncentiveAmt { get; set; }
        public Decimal IncentiveRate { get; set; }

        public List<HeaderCharges> RHeaderCharges { get; set; }
        public List<LineCharges> RLineCharges { get; set; }
        //Added for referennce while fetching receipts for invoice
        public int ? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int ? JobOrderUidHeaderId { get; set; }
        public string JobReceiveType { get; set; }
        public int ? ProdOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string LockReason { get; set; }

        public int? JobReceiveQALineId { get; set; }
        public int? JobReceiveQADocTypeId { get; set; }
        public int? QAGroupId { get; set; }
        public Decimal? ExcessReceiveAllowedAgainstOrderQty { get; set; }

    }

    public class HeaderCharges
    {
        public string ChargeCode { get; set; }
        public decimal ? Rate { get; set; }
    }
    public class LineCharges
    {
        public string ChargeCode { get; set; }
        public decimal ? Rate { get; set; }
        public int? LedgerAccountDrId { get; set; }
        public int? LedgerAccountCrId { get; set; }

    }

    [Serializable]
    public class JobReceiveLineFilterViewModel
    {
        public int DocTypeId { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Purchase Order No")]
        public string JobOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string CostCenterId { get; set; }

        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }

        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    [Serializable]
    public class JobReceiveMasterDetailModel
    {
        public List<JobReceiveLineViewModel> JobReceiveLineViewModel { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
    }

    public class JobReceiveSummaryViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Weight { get; set; }
        public decimal Qty { get; set; }
        public decimal DealQty { get; set; }
        public decimal DealQtyPP { get; set; }
        public string UnitName { get; set; }
        public string DealUnitName { get; set; }
        public int MaxDecPlaces { get; set; }
        public int MaxDealUnitDecPlaces { get; set; }
        public string JobOrderNo { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string CostCenterName { get; set; }
        public bool ValidationError { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal Penalty { get; set; }
    }

    public class JobReceiveSummaryDetailViewModel
    {
        public List<JobReceiveSummaryViewModel> JobReceiveSummaryViewModel { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        public DateTime DocData { get; set; }
    }

    public class JobReceiveIAPSummaryViewModel
    {
        public string ProductName { get; set; }
        public string ProductUidName { get; set; }
        public int ProductUidId { get; set; }
        public decimal DealQty { get; set; }
        public string DealUnitName { get; set; }
        public decimal PenalityRate { get; set; }
        public decimal PenalityAmt { get; set; }
        public decimal IncentiveRate { get; set; }
        public decimal IncentiveAmt { get; set; }
        public string Remark { get; set; }
        public int MaxDecPlaces { get; set; }
        public bool IsReturned { get; set; }

    }

    public class JobReceiveIAPSummaryDetailViewModel
    {
        public List<JobReceiveIAPSummaryViewModel> JobReceiveIAPSummaryViewModel { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        public int DocTypeId { get; set; }
        public DateTime DocData { get; set; }
    }

}
