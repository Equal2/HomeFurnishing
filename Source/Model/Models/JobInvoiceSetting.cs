using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceSettings : EntityBase, IHistoryLog
    {
        public JobInvoiceSettings()
        {
            IsVisiblePassQty = true;
            IsVisibleRate = true;
        }

        [Key]
        public int JobInvoiceSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }      

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleMachine { get; set; }
        public bool? isMandatoryMachine { get; set; }
        public bool? isMandatoryJobOrder { get; set; }
        public bool? isMandatoryJobReceive { get; set; }
        public bool? isVisibleProductUID { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }
        public bool? isVisibleLotNo { get; set; }
        
        public bool? isVisibleSpecification { get; set; }
        public bool? isVisibleDealUnit { get; set; }
        public bool? isVisibleWeight { get; set; }
        public bool? isVisibleCostCenter { get; set; }
        public bool? isVisibleHeaderJobWorker { get; set; }
        public bool? isVisibleSalesTaxGroupPerson { get; set; }
        public bool? isVisibleSalesTaxGroupProduct { get; set; }
        public bool? isVisibleJobOrder { get; set; }
        public bool? isVisibleJobReceive { get; set; }
        public bool? isVisibleProcessHeader { get; set; }
        public bool? isVisibleGovtInvoiceNo { get; set; }

        public bool? isVisibleGodown { get; set; }
        public bool? isVisibleJobReceiveBy { get; set; }


        public bool? isVisibleIncentive { get; set; }
        public bool? isVisiblePenalty { get; set; }

        public bool? isVisibleRateDiscountPer { get; set; }
        public bool? isVisibleFinancier { get; set; }
        public bool? isVisibleMfgDate { get; set; }


        public bool? IsVisibleDocQty { get; set; }
        public bool? isVisibleLoss { get; set; }
        public bool? IsVisibleReceiveQty { get; set; }
        public bool IsVisiblePassQty { get; set; }
        public bool IsVisibleRate { get; set; }

        public bool? IsVisibleAdditionalCharges { get; set; }



        public bool? isPostedInStock { get; set; }
        public bool? isPostedInStockProcess { get; set; }
        public bool? isPostedInStockVirtual { get; set; }
        public bool? isAutoCreateJobReceive { get; set; }
        public bool? isLedgerPostingLineWise { get; set; }
        public bool? isGenerateProductUid { get; set; }

        [ForeignKey("DocumentPrintReportHeader")]
        public int? DocumentPrintReportHeaderId { get; set; }
        public virtual ReportHeader DocumentPrintReportHeader { get; set; }

        /// <summary>
        /// DocId will be passed as a parameter in specified procedure.
        /// Procedure should have only one parameter of type int.
        /// </summary>
        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }

        /// <summary>
        /// DocId will be passed as a parameter in specified procedure.
        /// Procedure should have only one parameter of type int.
        /// </summary>
        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterSubmit { get; set; }

        /// <summary>
        /// DocId will be passed as a parameter in specified procedure.
        /// Procedure should have only one parameter of type int.
        /// </summary>
        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterApprove { get; set; }

        [MaxLength(100)]
        public string DocumentPrint { get; set; }
        
        [MaxLength (100)]
        public string SqlProcConsumption { get; set; }

        [MaxLength(100)]
        public string SqlProcGatePass { get; set; }
        public string SqlProcGenProductUID { get; set; }
        [MaxLength(100)]
        public string SqlProcProductUidHelpList { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
       
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterPersonRoles { get; set; }

        [ForeignKey("SalesTaxGroupPerson")]
        [Display(Name = "SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }


        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("ImportMenu")]
        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }
        public virtual Menu ImportMenu { get; set; }

        [ForeignKey("ExportMenu")]
        [Display(Name = "ExportMenu")]
        public int? ExportMenuId { get; set; }
        public virtual Menu ExportMenu { get; set; }


        [ForeignKey("WizardMenu")]
        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }
        public virtual Menu WizardMenu { get; set; }


        [ForeignKey("Calculation")]
        public int ? CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }



        [ForeignKey("JobReceiveDocType"), Display(Name = "Order Type")]
        public int ? JobReceiveDocTypeId { get; set; }
        public virtual DocumentType JobReceiveDocType { get; set; }
        public int? AmountRoundOff { get; set; }

        [ForeignKey("ReturnDocType")]
        public int ? JobReturnDocTypeId { get; set; }
        public virtual DocumentType ReturnDocType { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }
        public int? NoOfPrintCopies { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }


    }
}
