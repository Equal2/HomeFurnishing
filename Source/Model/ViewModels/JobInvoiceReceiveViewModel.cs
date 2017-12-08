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
    public class JobInvoiceReceiveHeaderViewModel
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

        [Display(Name = "Job Worker Doc Date"), Required]
        public DateTime? JobWorkerDocDate { get; set; }

        [Display(Name = "Job Receive By"), Range(1, int.MaxValue, ErrorMessage = "Job Receive field is required")]
        public int JobReceiveById { get; set; }
        public string JobReceiveByName { get; set; }
        public int Status { get; set; }

        public int GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }

    }

    public class JobInvoiceReceiveHeaderListViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobReceiveLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
    }


    public class JobInvoiceReceiveIndexViewModel
    {

        public int JobReceiveHeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocTypeName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }        
        public string JobWorkerDocNo { get; set; }
        public string JobWorkerName { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class JobInvoiceReceiveListViewModel
    {
        public int JobReceiveHeaderId { get; set; }
        public int JobOrderLineId { get; set; }
        public int JobReceiveLineId { get; set; }
        public string DocNo { get; set; }
        public string JobWorkerDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
    }

    public class JobInvoiceReceiveProductHelpList
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int JobReceiveLineId { get; set; }
        public string JobReceiveDocNo { get; set; }
        public string Specification { get; set; }
        public string  Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string JobOrderNo { get; set; }
        public string JobReceiveNo { get; set; }
        public decimal Qty { get; set; }
    }






    //Line ViewModels


    public class JobInvoiceReceiveLineViewModel
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

        [Display(Name = "Qty"), Required]
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

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        public int? StockId { get; set; }

        public int? StockProcessId { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int ?  Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int ? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
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
        public JobInvoiceSettingsViewModel JobInvoiceSettings { get; set; }
        public List<JobInvoiceLineCharge> linecharges { get; set; }
        public List<JobInvoiceHeaderCharge> footercharges { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

    }

    public class JobInvoiceReceiveMasterDetailModel
    {
        public List<JobInvoiceReceiveLineViewModel> JobReceiveLineViewModel { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; } 
    } 

}
