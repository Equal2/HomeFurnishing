using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobInvoiceHeaderViewModel
    {
        public int JobInvoiceHeaderId { get; set; }

        [Display(Name = "Invoice Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Invoice Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Invoice No"), Required, MaxLength(20)]        
        public string DocNo { get; set; }

        [Display(Name = "JobWorker Doc. No."), MaxLength(20)]
        public string JobWorkerDocNo { get; set; }

        public int? SalesTaxGroupPersonId { get; set; }
        public string SalesTaxGroupPersonName { get; set; }

        public string GovtInvoiceNo { get; set; }

        [Display(Name = "Job Worker Doc Date")]
        public DateTime? JobWorkerDocDate { get; set; }


        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]        
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        public int Status { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }        
        public int? JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }

        public int? FinancierId { get; set; }
        public string FinancierName { get; set; }

        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public string ModifiedBy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Job Receive By")]
        public int? JobReceiveById { get; set; }
        public string JobReceiveByName { get; set; }

        public int GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Job Receive")]
        public int? JobReceiveHeaderId { get; set; }
        public string JobReceiveDocNo { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string LockReason { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? DecimalPlaces { get; set; }
        public List<DocumentTypeHeaderAttributeViewModel> DocumentTypeHeaderAttributes { get; set; }
    }


    public class JobInvoiceLineViewModel
    {
        public string ProductUidName { get; set; }
        public int? ProductUidId { get; set; }

        public int JobInvoiceLineId { get; set; }

        [Display(Name = "Job Invoice"), Required]
        public int JobInvoiceHeaderId { get; set; }
        public virtual JobInvoiceHeader JobInvoiceHeader { get; set; }


        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }
        [Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Job Receive"), Required]
        public int JobReceiveLineId { get; set; }
        public string JobReceiveDocNo { get; set; }
        public string JobOrderDocNo { get; set; }

        [Display(Name = "Job Order")]
        public int? JobOrderLineId { get; set; }

        [Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal DealQty { get; set; }

        public int JobReceiveHeaderId { get; set; }

        public decimal IncentiveAmt { get; set; }
        public decimal IncentiveRate { get; set; }

        public decimal Weight { get; set; }

        [Display(Name = "Rate")]
        public decimal Rate { get; set; }

        [Display(Name = "Amount"), Required]
        public decimal Amount { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int ? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }
        public string ProductNatureName { get; set; }
        public decimal ReceiptBalQty { get; set; }
        public decimal OrderBalanceQty { get; set; }
        public string Specification { get; set; }
        public int? SalesTaxGroupProductId { get; set; }
        public int? SalesTaxGroupPersonId { get; set; }
        public decimal Qty { get; set; }
        [Display(Name = "Job Qty")]
        public decimal JobQty { get; set; }

        [Display(Name = "Pass Qty")]
        public decimal PassQty { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal LossQty { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal ReceiveQty { get; set; }

        [Display(Name = "Penalty Amount")]
        public Decimal PenaltyAmt { get; set; }
        public decimal PenaltyRate { get; set; }
        public Decimal? RateDiscountPer { get; set; }
        public Decimal? RateDiscountAmt { get; set; }
        public DateTime? MfgDate { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        public int? StockId { get; set; }

        public int? StockProcessId { get; set; }

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public List<JobInvoiceLineCharge> linecharges { get; set; }
        public List<JobInvoiceHeaderCharge> footercharges { get; set; }

        public int? CalculationId { get; set; }
        public List<HeaderCharges> RHeaderCharges { get; set; }
        public List<LineCharges> RLineCharges { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string Remark { get; set; }
        public int ? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string InvoiceDocNo { get; set; }
        public DateTime OrderDocDate { get; set; }
        public string LockReason { get; set; }
        public string LineNature { get; set; }
    }


    public class JobInvoiceLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int JobInvoiceLineId { get; set; }
        public string ProductName { get; set; }
        public string ProductGroupName { get; set; }
        public string JobOrderDocNo { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string JobOrderHeaderDocNo { get; set; }
        public string JobReceiveHeaderDocNo { get; set; }
        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Incentive Rate")]
        public Decimal? IncentiveRate { get; set; }

        [Display(Name = "Incentive Amount")]
        public Decimal? IncentiveAmt { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public int? ReceiptHeaderId { get; set; }
        public int? OrderHeaderId { get; set; }
        public int? ReceiptLineId { get; set; }
        public int? OrderLineId { get; set; }
        public int? ReceiptDocTypeId { get; set; }
        public int? OrderDocTypeId { get; set; }

    }

    public class JobInvoiceLineFilterViewModel
    {
        public int JobInvoiceHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string JobReceiveHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string JobOrderHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }

        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        [Display(Name = "Product Category")]
        public string ProductCategoryId { get; set; }
        public DateTime ? ReceiveAsOnDate { get; set; }
        public string JobWorkerIds { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class JobInvoiceMasterDetailModel
    {
        public List<JobInvoiceLineViewModel> JobInvoiceLineViewModel { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
    }
    public class JobInvoiceListViewModel
    {
        public int JobInvoiceHeaderId { get; set; }
        public int JobInvoiceLineId { get; set; }
        public string DocNo { get; set; }
        public string ReceiveDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
    public class JobInvoiceHeaderListViewModel
    {
        public int JobInvoiceHeaderId { get; set; }
        public int JobInvoiceLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public decimal BalanceQty { get; set; }
        public string ProductUidName { get; set; }


    }

    public class JobInvoiceSummaryDetailViewModel
    {
        public List<JobInvoiceSummaryViewModel> JobInvoiceSummaryViewModel { get; set; }
        public int JobInvoiceHeaderId { get; set; }
        public int ProcessId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
    }

    public class JobInvoiceSummaryViewModel
    {
        public int JobInvoiceLineId { get; set; }
        public int? ProductUidId { get; set; }
        public int ProcessId { get; set; }
        public string ProductUidName { get; set; }
        public int CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public Decimal InvoiceAmount { get; set; }
        public Decimal AdvanceAmount { get; set; }
        public Decimal TdsAmount { get; set; }
        public Decimal AdvanceAdjusted { get; set; }
        public Decimal TdsAdjusted { get; set; }
        public Decimal TanaAmount { get; set; }
        public bool ValidationError { get; set; }
    }

    public class PrintViewModel
    {
        public int? HeaderTableId { get; set; }
        public int? DocTypeId { get; set; }
        public string DocNo { get; set; }
        public string DocIdCaption { get; set; }
        public int? SiteId { get; set; }
        public int? DivisionId { get; set; }
        public string DocDate { get; set; }
        public string DocDateCaption { get; set; }
        public string PartyDocNo { get; set; }
        public string PartyDocCaption { get; set; }
        public string PartyDocDate { get; set; }
        public string PartyDocDateCaption { get; set; }
        public int? CreditDays { get; set; }
        public string HeaderRemark { get; set; }
        public string DocumentTypeShortName { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public int? Status { get; set; }
        public string CurrencyName { get; set; }
        public string ReverseChargeYN { get; set; }
        public string CompanyName { get; set; }
        public string PartyName { get; set; }
        public string PartyNameCaption { get; set; }
        public string PartySuffix { get; set; }
        public string PartyPrefix { get; set; }
        public string PartyAddress { get; set; }
        public string PartyStateName { get; set; }
        public string PartyStateCode { get; set; }
        public string PartyMobileNo { get; set; }
        public string PartyTinNo { get; set; }
        public string PartyPanNo { get; set; }
        public string PartyAadharNo { get; set; }
        public string PartyGSTNo { get; set; }
        public string PartCSTNo { get; set; }
        public string ContraDocNo { get; set; }
        public string ContraDocTypeCaption { get; set; }
        public string SignatoryleftCaption { get; set; }
        public string SignatoryMiddleCaption { get; set; }
        public string SignatoryRightCaption { get; set; }
        public string ProductName { get; set; }
        public string ProductCaption { get; set; }
        public string UnitName { get; set; }
        public int? DecimalPlaces { get; set; }
        public string DealUnitName { get; set; }
        public string DealQtyCaption { get; set; }
        public int? DealDecimalPlaces { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? DealQty { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension1Caption { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension2Caption { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension3Caption { get; set; }
        public string Dimension4Name { get; set; }
        public string Dimension4Caption { get; set; }
        public string SpecificationCaption { get; set; }
        public string Specification { get; set; }
        public string LineRemark { get; set; }
        public decimal? DiscountPer { get; set; }
        public decimal? DiscountAmt { get; set; }
        public string SalesTaxProductCodes { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductGroupCaption { get; set; }
        public string ChargeGroupProductName { get; set; }
        public string ProductUidCaption { get; set; }
        public string ProductUidName { get; set; }
        public decimal? LossQty { get; set; }
        public decimal? RecQty { get; set; }
        public string LotNo { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? ReturnAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreaditAmount { get; set; }
        public string ChargeGroupPersonName { get; set; }
        public int ? DealUnitCount { get; set; }
        public string ReportTitle { get; set; }
       public string SalesTaxGroupProductCaption { get; set; } 
       public string SalesTaxProductCodeCaption { get; set; }
       public string PrintedBy { get; set; }
       public string PrintDate { get; set; }
       public List<PrintCalculationViewModel> CalculationHeader { get; set; }
       public List<PrintCompanyViewModel> CompanyDetail { get; set; }
       public List<PrintGSTViewModel> GstDetail { get; set; }
    }

    public class PrintCalculationViewModel
    {
        public int Id { get; set; }
        public int HeaderTableId { get; set; }
        public int ? Sr { get; set; }
        public string ChargeName { get; set; }
        public decimal ? Amount { get; set; }
    }

    public class PrintCompanyViewModel
    {
        public string SiteName { get; set; }
        public string DivisionName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyCity { get; set; }
        public string Phone { get; set; }
        public string CompanyTinNo { get; set; }
        public string CompanyCSTNo { get; set; }
        public string CompanyFaxNo { get; set; }
        public string StateName { get; set; }
        public string LogoFolderName { get; set; }
        public string LogoFileName { get; set; }
        public string ReportHeaderTextRight1 { get; set; }
        public string ReportHeaderTextRight2 { get; set; }
        public string ReportHeaderTextRight3 { get; set; }
        public string ReportHeaderTextRight4 { get; set; }
        public string PanNo { get; set; }
        public string IECNo { get; set; }
        public string LogoBlob { get; set; }
    }

    public class PrintGSTViewModel
    {
        public string ChargeGroupProductName { get; set; }
        public decimal? TaxableAmount { get; set; }
        public decimal? IGST { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? GSTCess { get; set; }
    }

}