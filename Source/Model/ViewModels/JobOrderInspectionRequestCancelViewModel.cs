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
    public class JobOrderInspectionRequestCancelHeaderViewModel
    {

        [Key]
        public int JobOrderInspectionRequestCancelHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [MaxLength(10)]
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Process")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "JobWorker Name")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }

        [MaxLength(10)]
        public string RequestBy { get; set; }

        public bool AcceptedYn { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public JobOrderInspectionRequestSettingsViewModel JobOrderInspectionRequestSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

    }

    public class JobOrderInspectionRequestCancelLineViewModel
    {
        public int JobOrderInspectionRequestCancelLineId { get; set; }

        public int JobOrderInspectionRequestCancelHeaderId { get; set; }
        public string JobOrderInspectionRequestCancelHeaderDocNo { get; set; }

        public int? Sr { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int JobOrderInspectionRequestLineId { get; set; }
        public string JobOrderInspectionRequestDocNo { get; set; }

        public decimal Qty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }
        public string LotNo { get; set; }
        public DateTime? DueDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public decimal BalanceQty { get; set; }
        public JobOrderInspectionRequestSettingsViewModel JobOrderInspectionRequestSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string BarCodes { get; set; }
        public string JobInspectionType { get; set; }
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public int ? ProdOrderLineId { get; set; }
    }

    public class JobOrderInspectionRequestCancelFilterViewModel
    {
        public int JobOrderInspectionRequestCancelHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string JobOrderInspectionRequestId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class JobOrderInspectionRequestCancelMasterDetailModel
    {
        public List<JobOrderInspectionRequestCancelLineViewModel> JobOrderInspectionRequestCancelViewModels { get; set; }
        public JobOrderInspectionRequestSettingsViewModel JobOrderInspectionRequestSettings { get; set; }
    }

    public class BarCodeSequenceViewModelForInspectionRequestCancel
    {
        public string ProductName { get; set; }
        public int JobOrderInspectionRequestLineId { get; set; }
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public string JobOrdInspectionRequestLineIds { get; set; }
        public int JobOrderInspectionRequestCancelHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
        public string CostCenterName { get; set; }
        public string JobOrderInspectionRequestCancelType { get; set; }
        public int? ProdOrderLineId { get; set; }
    }

    [Serializable]
    public class BarCodeSequenceListViewModelForInspectionRequestCancel
    {
        public List<BarCodeSequenceViewModelForInspectionRequestCancel> BarCodeSequenceViewModel { get; set; }
        public List<BarCodeSequenceViewModelForInspectionRequestCancel> BarCodeSequenceViewModelPost { get; set; }
    }
}
