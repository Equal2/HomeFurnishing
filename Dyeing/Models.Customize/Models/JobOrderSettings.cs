using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int JobOrderSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }      

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleMachine { get; set; }
        public bool? isMandatoryMachine { get; set; }
        public bool? isVisibleCostCenter { get; set; }
        public bool? isMandatoryCostCenter { get; set; }
        public bool? isVisibleProductUID { get; set; }
        public bool? isMandatoryProductUID { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleRate { get; set; }
        public bool? isMandatoryRate { get; set; }
        public bool? isVisibleGodown { get; set; }
        public bool? isMandatoryGodown { get; set; }
        public bool? isEditableRate { get; set; }  
        public bool? isVisibleLotNo { get; set; }
        public bool? isVisibleLoss { get; set; }
        public bool? isVisibleUncountableQty { get; set; }
        public bool? isMandatoryProcessLine { get; set; }
        public bool? isVisibleProcessLine { get; set; }
        public bool? isVisibleJobWorkerLine { get; set; }
        public bool? isUniqueCostCenter { get; set; }
        public bool? PersonWiseCostCenter { get; set; }
        public bool? isPostedInStock { get; set; }
        public bool? isPostedInStockProcess { get; set; }
        public bool? isPostedInStockVirtual { get; set; }

        public int? RetensionCostCenter { get; set; }
        public bool? isVisibleFromProdOrder { get; set; }

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
        
        [MaxLength (100)]
        public string SqlProcConsumption { get; set; }
        [MaxLength(100)]
        public string SqlProcGenProductUID { get; set; }

        [MaxLength(100)]
        public string DocumentPrint { get; set; }

        [MaxLength(100)]
        public string SqlProcGatePass { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
       
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For")]
        public byte UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

        public int? DocTypeProductionOrderId { get; set; } 

        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("Calculation")]
        public int ? CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }

        /// <summary>
        /// Multiple perks ids will be stored as comma saperated values
        /// </summary>
        public string Perks { get; set; }

        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }

        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }

        [ForeignKey("JobUnit"), Display(Name = "Job Unit")]
        public string JobUnitId { get; set; }
        public virtual Unit JobUnit { get; set; }

        [Display(Name = "OnSubmitMenu")]
        public int? OnSubmitMenuId { get; set; }

        [Display(Name = "OnApproveMenu")]
        public int? OnApproveMenuId { get; set; }
        public decimal NonCountedQty { get; set; }

        public decimal? LossQty { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }
        public int ? DueDays { get; set; }

        public int ? AmountRoundOff { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }

        public string FilterProductDivision { get; set; }

        public string Event_OnHeaderSave { get; set; }
        public string Event_OnHeaderDelete { get; set; }
        public string Event_OnHeaderSubmit { get; set; }
        public string Event_OnHeaderApprove { get; set; }
        public string Event_OnHeaderPrint { get; set; }
        public string Event_OnLineSave { get; set; }
        public string Event_OnLineDelete { get; set; }


        public string Event_AfterHeaderSave { get; set; }
        public string Event_AfterHeaderDelete { get; set; }
        public string Event_AfterHeaderSubmit { get; set; }
        public string Event_AfterHeaderApprove { get; set; }
        public string Event_AfterLineSave { get; set; }
        public string Event_AfterLineDelete { get; set; }

        public int ? MaxDays { get; set; }
        public int ExcessQtyAllowedPer { get; set; }

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
