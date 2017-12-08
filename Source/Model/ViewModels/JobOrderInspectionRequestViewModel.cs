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
    public class JobOrderInspectionRequestHeaderViewModel
    {

        [Key]
        public int JobOrderInspectionRequestHeaderId { get; set; }
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

    public class JobOrderInspectionRequestLineViewModel
    {
        public int JobOrderInspectionRequestLineId { get; set; }

        public int JobOrderInspectionRequestHeaderId { get; set; }
        public string JobOrderInspectionRequestHeaderDocNo { get; set; }

        public int? Sr { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        public int JobOrderLineId { get; set; }
        public string JobOrderDocNo { get; set; }

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
        public int JobOrderHeaderId { get; set; }
        public int ? ProdOrderLineId { get; set; }
        public string DealUnitId { get; set; }
        public decimal DealQty { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public string DealUnitName { get; set; }        
    }

    public class JobOrderInspectionRequestFilterViewModel
    {
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string JobOrderId { get; set; }
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
    public class JobOrderInspectionRequestMasterDetailModel
    {
        public List<JobOrderInspectionRequestLineViewModel> JobOrderInspectionRequestViewModels { get; set; }
        public JobOrderInspectionRequestSettingsViewModel JobOrderInspectionRequestSettings { get; set; }
    }

    public class BarCodeSequenceViewModelForInspection
    {
        public string ProductName { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrdLineIds { get; set; }
        public string JobOrdInspectionRequestLineIds { get; set; }
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public int JobOrderInspectionHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
        public string CostCenterName { get; set; }
        public string JobOrderInspectionRequestType { get; set; }
        public int? ProdOrderLineId { get; set; }
        public string JobOrderInspectionType { get; set; }
    }

    [Serializable]
    public class BarCodeSequenceListViewModelForInspection
    {
        public bool InsReq { get; set; }
        public List<BarCodeSequenceViewModelForInspection> BarCodeSequenceViewModel { get; set; }
        public List<BarCodeSequenceViewModelForInspection> BarCodeSequenceViewModelPost { get; set; }
    }





    [Serializable]
    public class JobOrderInspectionRequestWizardViewModel
    {
        public DateTime OrderDate { get; set; }
        public string SOrderDate { get; set; }
        public string OrderNo { get; set; }
        public int JobWorkerId { get; set; }
        public int JobOrderLineId { get; set; }
        public string JobWorkerName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Qty { get; set; }
        public string ProductUidName { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int JobOrderHeaderId { get; set; }
        public int ProdId { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public bool Sample { get; set; }
        public int DocTypeId { get; set; }
        public string Remark { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }


    public class JobOrderInspectionRequestWizardFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string JobOrderHeaderId { get; set; }
        public string JobWorkerId { get; set; }
        public int DocTypeId { get; set; }
        public string ProductId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductCategoryId { get; set; }
        public decimal? BalanceQty { get; set; }
        public decimal Qty { get; set; }        
        public string Sample { get; set; }
    }



}
