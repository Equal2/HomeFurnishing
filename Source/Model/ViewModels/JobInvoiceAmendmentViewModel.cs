using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobInvoiceAmendmentHeaderViewModel
    {
        public int JobInvoiceAmendmentHeaderId { get; set; }        
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }

        [MaxLength(10)]
        public string DocNo { get; set; }        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }        
        public int SiteId { get; set; }
        public string SiteName { get; set; }        
        public int ? JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public int Status { get; set; }

        [Display(Name = "Order By")]
        public int OrderById { get; set; }
        public string OrderByName { get; set; }

        [Display(Name = "Process")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class JobInvoiceAmendmentHeaderIndexViewModel
    {
        public int JobInvoiceAmendmentHeaderId { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string JobWorkerName { get; set; }
        public string OrderByName { get; set; }
        public string ProcessName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
    }

    public class JobInvoiceAmendmentFilterViewModel
    {
        public int JobInvoiceAmendmentHeaderId { get; set; }
        public int ? JobWorkerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string JobInvoiceId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }
        public DateTime ? UpToDate { get; set; }
        public decimal OldRate { get; set; }
        public decimal Rate { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class JobInvoiceAmendmentMasterDetailModel
    {
        public List<JobInvoiceRateAmendmentLineViewModel> JobInvoiceRateAmendmentLineViewModel { get; set; }
    }



    public class JobInvoiceRateAmendmentLineViewModel
    {
        public string ProductUidName { get; set; }
        public int JobInvoiceRateAmendmentLineId { get; set; }
        public int JobInvoiceAmendmentHeaderId { get; set; }
        public string JobInvoiceAmendmentHeaderDocNo { get; set; }
        public string JobInvoiceDocNo { get; set; }
        public DateTime JobInvoiceDocDate { get; set; }
        public int JobInvoiceLineId { get; set; }
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        public decimal Qty { get; set; }
        public decimal JobInvoiceRate { get; set; }
        public decimal AmendedRate { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public Decimal UnitConversionMultiplier { get; set; }
        public decimal DealQty { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public List<JobInvoiceRateAmendmentLineCharge> linecharges { get; set; }
        public List<JobInvoiceAmendmentHeaderCharge> footercharges { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public bool AAmended { get; set; }
        public string LockReason { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int InvoiceDocTypeId { get; set; }
        public int InvoiceHeaderId { get; set; }
    }

    [Serializable]
    public class JobInvoiceAmendmentWizardViewModel
    {
        public DateTime InvoiceDate { get; set; }
        public string SInvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public int JobWorkerId { get; set; }
        public int JobInvoiceLineId { get; set; }
        public string JobWorkerName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public decimal OldRate { get; set; }
        public decimal Rate { get; set; }
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int JobInvoiceHeaderId { get; set; }
        public int ProdId { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public bool Sample { get; set; }
    }

    public class JobInvoiceRateAmendmentWizardFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string JobInvoiceHeaderId { get; set; }
        public string JobWorkerId { get; set; }
        public int DocTypeId { get; set; }
        public int ProcessId { get; set; }
        public string ProductId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductCategoryId { get; set; }
        public decimal? Rate { get; set; }
        public decimal NewRate { get; set; }
        public decimal? MultiplierGT { get; set; }
        public decimal? MultiplierLT { get; set; }
        public string Sample { get; set; }
    }


}
