using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModels;
using Core.Common;

namespace Model.ViewModel
{

    public partial class JobOrderHeaderViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Document Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20)]
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? GatePassHeaderId { get; set; }
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime? GatePassDocDate { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The JobWorker field is required")]
        public int JobWorkerId { get; set; }
        public string JobWorkerName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Billing A/C filed is required")]
        public int BillToPartyId { get; set; }
        public string BillToPartyName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The OrderBy field is required")]
        public int? OrderById { get; set; }
        public string OrderByName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Godown field is required")]
        public int? GodownId { get; set; }
        public string GodownName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Process field is required")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string DealUnitId { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? MachineId { get; set; }
        public string MachineName { get; set; }
        public string ModifiedBy { get; set; }

        public int? CreditDays { get; set; }

        public string TermsAndConditions { get; set; }

        [Display(Name = "Unit Conversion For Type")]
        public byte? UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public List<PerkViewModel> PerkViewModel { get; set; }

        //ForWeavingWizard
        public decimal Rate { get; set; }
        public decimal Loss { get; set; }
        public decimal UnCountedQty { get; set; }
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal? TotalQty { get; set; }
        public int? DecimalPlaces { get; set; }

        public int? CalculationFooterChargeCount { get; set; }



        public int? DeliveryTermsId { get; set; }
        public string DeliveryTermsName { get; set; }
        public int? ShipToAddressId { get; set; }
        public string ShipToAddress { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int? SalesTaxGroupPersonId { get; set; }
        public string SalesTaxGroupPersonName { get; set; }
        public int? ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }
        public int? DocumentShipMethodId { get; set; }
        public string DocumentShipMethodName { get; set; }
        public int? TransporterId { get; set; }
        public string TransporterName { get; set; }
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public int? FinancierId { get; set; }
        public string FinancierName { get; set; }
        public int? SalesExecutiveId { get; set; }
        public string SalesExecutiveName { get; set; }
        public bool IsDoorDelivery { get; set; }

        public List<DocumentTypeHeaderAttributeViewModel> DocumentTypeHeaderAttributes { get; set; }

        public Decimal? PayTermAdvancePer { get; set; }
        public Decimal? PayTermOnDeliveryPer { get; set; }
        public Decimal? PayTermOnDueDatePer { get; set; }
        public Decimal? PayTermCashPer { get; set; }
        public Decimal? PayTermBankPer { get; set; }

    }

    public class JobOrderLineViewModel
    {
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderHeaderDocNo { get; set; }
        public int ProgressPerc { get; set; }
        public int ProgressPercCancelled { get; set; }
        public int? StockId { get; set; }
        public int? StockProcessId { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ProdOrderLineId { get; set; }
        public string ProdOrderDocNo { get; set; }

        public int? OrderDocTypeId { get; set; }
        public int? OrderHeaderId { get; set; }

        public int? StockInId { get; set; }
        public string StockInNo { get; set; }

        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }



        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }



        [MaxLength(50)]
        public string Specification { get; set; }

        public decimal Qty { get; set; }
        public decimal ProdOrderBalanceQty { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(50)]
        public string LotNo { get; set; }

        public int? FromProcessId { get; set; }
        public string FromProcessName { get; set; }

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal DealQty { get; set; }

        [Display(Name = "Non Counted Qty")]
        public decimal NonCountedQty { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal? LossQty { get; set; }

        public decimal Rate { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }

        public decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public Decimal UnitConversionMultiplier { get; set; }
        //public bool IsProdOrderBased { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public List<JobOrderLineCharge> linecharges { get; set; }
        public List<JobOrderHeaderCharge> footercharges { get; set; }
        public int? GodownId { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public bool UnitConversionException { get; set; }

        public int? CalculationId { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public bool IsUidGenerated { get; set; }
        public List<ComboBoxList> BarCodes { get; set; }
        public int? ProductUidHeaderId { get; set; }
        public string LockReason { get; set; }

        //Added for reference in fetching for invoice
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public List<HeaderCharges> RHeaderCharges { get; set; }
        public List<LineCharges> RLineCharges { get; set; }
        public bool AllowRepeatProcess { get; set; }
        public bool IsProcessDone { get; set; }
        public decimal? Incentive { get; set; }
        public decimal? Penalty { get; set; }
        public Decimal? StockInBalanceQty { get; set; }

        public int? SalesTaxGroupProductId { get; set; }
        public string SalesTaxGroupProductName { get; set; }
        public int? SalesTaxGroupPersonId { get; set; }
        public string LineNature { get; set; }
        public Decimal? ExcessReceiveAllowedAgainstOrderQty { get; set; }
    }

    public class JobOrderHeaderListViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public string DocNo { get; set; }
        public string RequestDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public string LotNo { get; set; }

        public decimal BalanceQty { get; set; }


    }

    public class JobOrderLineFilterViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Production Order")]
        public string ProdOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }


        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }

        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }


        public string DealUnitId { get; set; }
        public decimal Rate { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

    }

    public class JobOrderLineFilterForStockInViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobWorkerId { get; set; }
        [Display(Name = "Stock In")]
        public string StockInHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }


        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }

        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }


        public string DealUnitId { get; set; }
        public decimal Rate { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

    }

    public class JobOrderLineListViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }

    public class JobOrderMasterDetailModel
    {
        public List<JobOrderLineViewModel> JobOrderLineViewModel { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }

    public class JobOrderLineProgressViewModel
    {
        public JobOrderLineProgressViewModel()
        {
            JobReceievs = new List<ProgressDetail>();
            JobCancels = new List<ProgressDetail>();
            JobAmendment = new List<ProgressDetail>();
        }
        public List<ProgressDetail> JobReceievs { get; set; }
        public List<ProgressDetail> JobCancels { get; set; }
        public List<ProgressDetail> JobAmendment { get; set; }
    }

    public class ProgressDetail
    {
        public string SKU { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public decimal Qty { get; set; }
        public int DocId { get; set; }
        public int DocTypeId { get; set; }
        public int DocLineId { get; set; }
        public int DecimalPlaces { get; set; }
    }
}
