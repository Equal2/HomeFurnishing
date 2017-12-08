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
    public class JobReceiveQAHeaderViewModel
    {
        [Key]
        public int JobReceiveQAHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public int? QAById { get; set; }
        public string QAByName { get; set; }
        public string Remark { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public string ReferenceDocTypeName { get; set; }
        public int? ReferenceDocId { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public JobReceiveQASettingsViewModel JobReceiveQASettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string LockReason { get; set; }
       
    }

    public class JobReceiveQALineViewModel
    {
        [Key]
        public int JobReceiveQALineId { get; set; }
        public int JobReceiveQAHeaderId { get; set; }
        public string JobReceiveQADocNo { get; set; }
        public int? Sr { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }     
        public int JobReceiveLineId { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public string JobReceiveDocNo { get; set; }
        public int JobWorkerId { get; set; }
        public decimal InspectedQty { get; set; }
        public decimal Qty { get; set; }
        public decimal QAQty { get; set; }
        public decimal FailQty { get; set; }
        public decimal FailDealQty { get; set; }

        [Range(0, 100)]
        public decimal Marks { get; set; }
        public string Remark { get; set; }
        public string ImageFolderName { get; set; }
        public string ImageFileName { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public string ReferenceDocTypeName { get; set; }
        public int? ReferenceDocLineId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }
        public JobReceiveQASettingsViewModel JobReceiveQASettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string JobReceiveQAType { get; set; }
        public int? Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }

        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public decimal DealQty { get; set; }
        public decimal Weight { get; set; }
        public decimal PenaltyRate { get; set; }
        public decimal PenaltyAmt { get; set; }
        public string JobInspectionType { get; set; }

        public int? ReceiveDocTypeId { get; set; }
        public int? ReceiveHeaderId { get; set; }

    }

    public class JobReceiveQALineFilterViewModel
    {
        public int DocTypeId { get; set; }
        public int JobReceiveQAHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Purchase Order No")]
        public string JobReceiveQARequestHeaderId { get; set; }
        public string JobReceiveHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class JobReceiveQAMasterDetailModel
    {
        public List<JobReceiveQALineViewModel> JobReceiveQALineViewModel { get; set; }
        public JobReceiveQASettingsViewModel JobReceiveQASettings { get; set; }
    }


    public class BarCodeSequenceViewModelForQA
    {
        public string ProductName { get; set; }
        public int JobReceiveLineId { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public string JobRecLineIds { get; set; }
        public string JobRecQARequestLineIds { get; set; }
        public int JobReceiveQARequestHeaderId { get; set; }
        public int JobReceiveQAHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
        public string CostCenterName { get; set; }
        public string JobReceiveQARequestType { get; set; }
        public int? ProdOrderLineId { get; set; }
        public string JobReceiveQAType { get; set; }
    }

    [Serializable]
    public class BarCodeSequenceListViewModelForQA
    {
        public List<BarCodeSequenceViewModelForQA> BarCodeSequenceViewModel { get; set; }
        public List<BarCodeSequenceViewModelForQA> BarCodeSequenceViewModelPost { get; set; }
    }



    [Serializable]
    public class JobReceiveQAWizardViewModel
    {
        public DateTime OrderDate { get; set; }
        public string SOrderDate { get; set; }
        public string OrderNo { get; set; }
        public int JobWorkerId { get; set; }
        public int JobReceiveLineId { get; set; }
        public string JobWorkerName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Qty { get; set; }
        public decimal InspectionQty { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string ProductUidName { get; set; }
        public int ? ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ? ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public int ProdId { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public bool Sample { get; set; }
        public int DocTypeId { get; set; }
        public string Remark { get; set; }
    }


    public class JobReceiveQAWizardFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string JobReceiveHeaderId { get; set; }
        public string JobWorkerId { get; set; }
        public int DocTypeId { get; set; }
        public string ProductId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductCategoryId { get; set; }
        public decimal? BalanceQty { get; set; }
        public decimal InspectionQty { get; set; }
        public decimal? MultiplierGT { get; set; }
        public decimal? MultiplierLT { get; set; }
        public string Sample { get; set; }
    }




    public class JobReceiveQAAttributeViewModel
    {
        public int JobReceiveQAAttributeId { get; set; }
        public int JobReceiveQALineId { get; set; }
        public int JobReceiveQAHeaderId { get; set; }

        public int DocTypeId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public int SiteId { get; set; }
        public int ProcessId { get; set; }
        public int JobWorkerId { get; set; }
        public int? QAById { get; set; }
        public int JobReceiveLineId { get; set; }

        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal QAQty { get; set; }
        public decimal InspectedQty { get; set; }
        public decimal Qty { get; set; }
        public decimal FailQty { get; set; }

        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }

        public string UnitId { get; set; }
        public string DealUnitId { get; set; }
        public Decimal UnitConversionMultiplier { get; set; }
        public decimal DealQty { get; set; }
        public decimal FailDealQty { get; set; }
        public decimal Weight { get; set; }
        public decimal PenaltyRate { get; set; }
        public decimal PenaltyAmt { get; set; }
        public decimal Marks { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<QAGroupLineLineViewModel> QAGroupLine { get; set; }

        public JobReceiveQASettingsViewModel JobReceiveQASettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }

        public int? DimensionUnitDecimalPlaces { get; set; }
        public string OMSId { get; set; }

    }

    public class JobReceiveQAPenaltyViewModel
    {
        public int JobReceiveQAPenaltyId { get; set; }
        public int Sr { get; set; }

        public int DocTypeId { get; set; }
        public int JobReceiveQALineId { get; set; }
        public int JobReceiveQAHeaderId { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }
        public string OMSId { get; set; }
    }

    public class JobReceivePendingToQAIndex
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public string JobWorkerName { get; set; }
        public string ProductName { get; set; }
        public string ProductUidName { get; set; }
    }
}
