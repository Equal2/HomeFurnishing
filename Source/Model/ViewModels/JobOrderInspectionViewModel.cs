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
    public class JobOrderInspectionHeaderViewModel
    {
        [Key]
        public int JobOrderInspectionHeaderId { get; set; }
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
        public int InspectionById { get; set; }
        public string InspectionByName { get; set; }
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
        public JobOrderInspectionSettingsViewModel JobOrderInspectionSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string LockReason { get; set; }
       
    }

    public class JobOrderInspectionLineViewModel
    {
        [Key]
        public int JobOrderInspectionLineId { get; set; }
        public int JobOrderInspectionHeaderId { get; set; }
        public string JobOrderInspectionDocNo { get; set; }
        public int? Sr { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int? JobOrderInspectionRequestLineId { get; set; }
        public string JobOrderInspectionRequestDocNo { get; set; }
        public int JobOrderInspectionRequestHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderDocNo { get; set; }
        public int JobWorkerId { get; set; }
        public decimal InspectedQty { get; set; }
        public decimal Qty { get; set; }

        //[Range(0, 100)]
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
        public JobOrderInspectionSettingsViewModel JobOrderInspectionSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string JobOrderInspectionType { get; set; }
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
        public bool InsReq { get; set; }
        public string JobInspectionType { get; set; }

    }

    public class JobOrderInspectionLineFilterViewModel
    {
        public int DocTypeId { get; set; }
        public int JobOrderInspectionHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Purchase Order No")]
        public string JobOrderInspectionRequestHeaderId { get; set; }
        public string JobOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class JobOrderInspectionMasterDetailModel
    {
        public List<JobOrderInspectionLineViewModel> JobOrderInspectionLineViewModel { get; set; }
        public JobOrderInspectionSettingsViewModel JobOrderInspectionSettings { get; set; }
    }


    [Serializable]
    public class JobOrderInspectionWizardViewModel
    {
        public DateTime OrderDate { get; set; }
        public string SOrderDate { get; set; }
        public string OrderNo { get; set; }
        public int JobWorkerId { get; set; }
        public int JobOrderLineId { get; set; }
        public string JobWorkerName { get; set; }
        public int JobRequestLineId { get; set; }
        public string RequestNo { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Qty { get; set; }
        public decimal InspectionQty { get; set; }
        public string ProductUidName { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int JobOrderHeaderId { get; set; }
        public int JobRequestHeaderId { get; set; }
        public int ProdId { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public int DocTypeId { get; set; }
        public string Remark { get; set; }
    }


    public class JobOrderInspectionWizardFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string JobOrderHeaderId { get; set; }
        public string JobRequestHeaderId { get; set; }
        public string JobWorkerId { get; set; }
        public int DocTypeId { get; set; }
        public string ProductId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductCategoryId { get; set; }
        public decimal? BalanceQty { get; set; }
        public decimal InspectionQty { get; set; }
        public string Sample { get; set; }
    }

}
