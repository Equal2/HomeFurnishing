using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobInvoiceReturnHeaderViewModel
    {

        public int JobInvoiceReturnHeaderId { get; set; }
        [Required]
        public int DocTypeId { get; set; }
        public string DocTypeName{ get; set; }

        [Display(Name = "Invoice Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Invoice No"), Required, MaxLength(20)]        
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]        
        public int SiteId { get; set; }
        public string SiteName{ get; set; }
        
        [Display(Name = "JobWorker Name"),Range(1,int.MaxValue,ErrorMessage="The JobWorker field is required"),Required]
        public int JobWorkerId { get; set; }
        public string JobWorkerName{ get; set; }

        public int? OrderById { get; set; }
        public string OrderByName { get; set; }

        public int? SalesTaxGroupPersonId { get; set; }
        public string SalesTaxGroupPersonName { get; set; }

        public bool CalculateDiscountOnRate { get; set; }        

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public int ? JobReturnHeaderId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public int? GodownId { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
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
        public int? GatePassHeaderId { get; set; }
        public string LockReason { get; set; }
        public string Nature { get; set; }


    }

    public class JobInvoiceReturnLineViewModel
    {
        public int JobInvoiceReturnLineId { get; set; }
        [Required,Range(1,int.MaxValue,ErrorMessage="The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string JobOrderDocNo { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        public int? Dimension3Id { get; set; }
        public int? Dimension4Id { get; set; }

        [MaxLength(10)]
        public string LotNo { get; set; }
        public decimal InvoiceBalQty { get; set; }
        public decimal Qty { get; set; }
        public int JobWorkerId { get; set; }

        [Display(Name = "Job Invoice"), Required]        
        public int JobInvoiceReturnHeaderId { get; set; }
        public string JobInvoiceReturnHeaderDocNo { get; set; }

        [Display(Name = "Receipt"), Required]
        public int JobInvoiceLineId { get; set; }
        public string JobInvoiceHeaderDocNo{ get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        public Decimal RateAfterDiscount { get; set; }


        public int? SalesTaxGroupProductId { get; set; }
        public int? SalesTaxGroupPersonId { get; set; }

        public bool CalculateDiscountOnRate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string UnitId { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public List<JobInvoiceReturnLineCharge> linecharges { get; set; }
        public List<JobInvoiceReturnHeaderCharge> footercharges { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public string LockReason { get; set; }
        public decimal Weight { get; set; }
        public string Nature { get; set; }
        public int? CalculationId { get; set; }
    }
    public class JobInvoiceReturnLineIndexViewModel
    {
        public string ProductUidName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }

        public int JobInvoiceReturnLineId { get; set; }
        public string ProductName { get; set; }
        public string JobOrderDocNo { get; set; }
        public decimal Qty { get; set; }
        public Decimal DealQty { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string JobInvoiceHeaderDocNo { get; set; }
        public string JobGoodsRecieptHeaderDocNo { get; set; }      
        [Display(Name = "Rate"), Required]
        public Decimal Rate { get; set; }

        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }

        public int? JobInvoiceDocTypeId { get; set; }
        public int? JobInvoiceHeaderId { get; set; }
    }

    public class JobInvoiceReturnLineFilterViewModel
    {
        public int JobInvoiceReturnHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Goods Receipt")]
        public string JobReceiveHeaderId { get; set; }

        public string JobInvoiceHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class JobInvoiceReturnMasterDetailModel
    {
        public List<JobInvoiceReturnLineViewModel> JobInvoiceReturnLineViewModel { get; set; }
    }


    public class JobInvoiceReturnBarCodeSequenceViewModel
    {
        public string ProductName { get; set; }
        public int JobInvoiceLineId { get; set; }
        public string SJobInvLineIds { get; set; }
        public int JobInvoiceReturnHeaderId { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }
        public int ProductUidId { get; set; }
        public string ProductUidIds { get; set; }
        public string FirstBarCode { get; set; }
        public string ProductUidIdName { get; set; }
    }

    public class JobInvoiceReturnBarCodeSequenceListViewModel
    {
        public List<JobInvoiceReturnBarCodeSequenceViewModel> JobInvoiceReturnBarCodeSequenceViewModel { get; set; }
        public List<JobInvoiceReturnBarCodeSequenceViewModel> JobInvoiceReturnBarCodeSequenceViewModelPost { get; set; }
        public int GodownId { get; set; }
    }

}
