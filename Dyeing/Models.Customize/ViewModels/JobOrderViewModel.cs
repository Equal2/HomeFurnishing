using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.BasicSetup.ViewModels;
using Models.Customize.Models;

namespace Models.Customize.ViewModels
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
        public decimal ? TotalQty { get; set; }
        public int ? DecimalPlaces { get; set; }
    }

    public class JobOrderLineViewModel
    {
        public int JobOrderLineId { get; set; }
        public int JobOrderHeaderId { get; set; }
        public string JobOrderHeaderDocNo { get; set; }
        public int ProgressPerc { get; set; }
        public int? StockId { get; set; }
        public int? StockProcessId { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "The Product field is required")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ProdOrderLineId { get; set; }
        public string ProdOrderDocNo { get; set; }

        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

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

        public decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public Decimal UnitConversionMultiplier { get; set; }
        public bool IsProdOrderBased { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }
        public List<JobOrderLineCharge> linecharges { get; set; }
        public List<JobOrderHeaderCharge> footercharges { get; set; }
        public int? GodownId { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public bool UnitConversionException { get; set; }

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
    }

    public class JobOrderHeaderListViewModel
    {
        public int JobOrderHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public string DocNo { get; set; }
        public string RequestDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
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

        public string DealUnitId { get; set; }
        public decimal Rate { get; set; }
        public JobOrderSettingsViewModel JobOrderSettings { get; set; }

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
    }

}
