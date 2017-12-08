using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int JobReceiveSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        [Index("IX_JobReceiveSettings_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Index("IX_JobReceiveSettings_DocID", IsUnique = true, Order = 2)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        
        [Index("IX_JobReceiveSettings_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleMachine { get; set; }
        public bool? isMandatoryMachine { get; set; }
        public bool? isVisibleProductUID { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }
        public bool? isVisibleLotNo { get; set; }
        public bool? isVisibleLoss { get; set; }
        public bool? isVisibleSpecification { get; set; }
        public bool? isVisibleDealUnit { get; set; }
        public bool ? IsVisibleWeight { get; set; }
        public bool ? IsMandatoryWeight { get; set; }
        public bool? IsVisibleForOrderMultiple { get; set; }
        public bool? IsVisibleIncentive { get; set; }
        public bool? IsVisiblePenalty { get; set; }
        public bool? IsVisiblePassQty { get; set; }
        public bool? isVisibleCostCenter { get; set; }
        public bool? isVisibleConsumptionDetail { get; set; }
        public bool? isVisibleProcessHeader { get; set; }
        public bool? isVisibleByProductDetail { get; set; }
        public bool? isPostedInStock { get; set; }
        public bool? isPostedInStockProcess { get; set; }
        public bool? isPostedInStockVirtual { get; set; }
        

        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterPersonRoles { get; set; }


        [MaxLength(100)]
        public string SqlProcGatePass { get; set; }



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
        public string SqlProcProductUidHelpList { get; set; }

        [MaxLength(100)]
        public string DocumentPrint { get; set; }
        
        [MaxLength (100)]
        public string SqlProcConsumption { get; set; }


        public string SqlProcGenProductUID { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
       
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }

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

        [ForeignKey("OnSubmitMenu")]
        [Display(Name = "OnSubmitMenu")]
        public int? OnSubmitMenuId { get; set; }
        public virtual Menu OnSubmitMenu { get; set; }


        [ForeignKey("OnApproveMenu")]
        [Display(Name = "OnApproveMenu")]
        public int? OnApproveMenuId { get; set; }
        public virtual Menu OnApproveMenu { get; set; }


        [ForeignKey("Calculation")]
        public int ? CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }

        [MaxLength(20)]
        public string StockQty { get; set; }

        public Decimal? LossPer { get; set; }

        [ForeignKey("DocumentPrintReportHeader")]
        public int? DocumentPrintReportHeaderId { get; set; }
        public virtual ReportHeader DocumentPrintReportHeader { get; set; }

        [MaxLength(50)]
        public string ConsumptionProductCaption { get; set; }

        [MaxLength(50)]
        public string ConsumptionDimension1Caption { get; set; }

        [MaxLength(50)]
        public string ConsumptionDimension2Caption { get; set; }
        [MaxLength(50)]
        public string ConsumptionDimension3Caption { get; set; }

        [MaxLength(50)]
        public string ConsumptionDimension4Caption { get; set; }
        
        [MaxLength(50)]
        public string ByProductCaption { get; set; }

        [MaxLength(50)]
        public string ByProductDimension1Caption { get; set; }

        [MaxLength(50)]
        public string ByProductDimension2Caption { get; set; }
        [MaxLength(50)]
        public string ByProductDimension3Caption { get; set; }
        [MaxLength(50)]
        public string ByProductDimension4Caption { get; set; }
        public bool? isVisibleConsumptionDimension1 { get; set; }
        public bool? isVisibleConsumptionDimension2 { get; set; }
        public bool? isVisibleConsumptionDimension3 { get; set; }
        public bool? isVisibleConsumptionDimension4 { get; set; }
        public bool? isVisibleByProductDimension1 { get; set; }
        public bool? isVisibleByProductDimension2 { get; set; }
        public bool? isVisibleByProductDimension3 { get; set; }
        public bool? isVisibleByProductDimension4 { get; set; }


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
