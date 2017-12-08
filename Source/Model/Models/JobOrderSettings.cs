using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
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

        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }
        public bool? isVisibleRate { get; set; }
        public bool? isMandatoryRate { get; set; }
        public bool? isVisibleGodown { get; set; }
        public bool? isMandatoryGodown { get; set; }
        public bool? isEditableRate { get; set; }  
        public bool? isVisibleLotNo { get; set; }
        public bool? isVisibleLoss { get; set; }
        public bool? isVisibleDealUnit { get; set; }
        public bool? isVisibleLineDueDate { get; set; }
        public bool? isVisibleBillToParty { get; set; }
        public bool? isVisibleUncountableQty { get; set; }
        public bool? isVisibleUnitConversionFor { get; set; }
        public bool? isVisibleSpecification { get; set; }
        public bool? isVisibleCreditDays { get; set; }
        public bool? isMandatoryProcessLine { get; set; }
        public bool? isVisibleProcessLine { get; set; }
        public bool? isVisibleProcessHeader { get; set; }
        public bool? isVisibleJobWorkerLine { get; set; }
        public bool? isVisibleDiscountPer { get; set; }
        public bool? isUniqueCostCenter { get; set; }
        public bool? PersonWiseCostCenter { get; set; }
        public bool? isPostedInStock { get; set; }
        public bool? isPostedInStockProcess { get; set; }
        public bool? isPostedInStockVirtual { get; set; }



        public bool? isVisibleStockIn { get; set; }
        public bool? IsMandatoryStockIn { get; set; }

        public bool? isVisibleDeliveryTerms { get; set; }
        public bool? isVisibleShipToAddress { get; set; }
        public bool? isVisibleCurrency { get; set; }
        public bool? isVisibleSalesTaxGroupPerson { get; set; }
        public bool? isVisibleSalesTaxGroupProduct { get; set; }
        public bool? isVisibleShipMethod { get; set; }
        public bool? isVisibleDocumentShipMethod { get; set; }
        public bool? isVisibleTransporter { get; set; }
        public bool? isVisibleAgent { get; set; }
        public bool? isVisibleDoorDelivery { get; set; }
        public bool? isVisiblePaymentTerms { get; set; }
        public bool? isVisibleFinancier { get; set; }
        public bool? isVisibleSalesExecutive { get; set; }

        public bool? CalculateDiscountOnRate { get; set; }

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
        [MaxLength(100)]
        public string SqlProcProductUidHelpList { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
       
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProductCategories { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterPersonRoles { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For")]
        public byte UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

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

        [ForeignKey("JobUnit"), Display(Name = "Job Unit")]
        public string JobUnitId { get; set; }
        public virtual Unit JobUnit { get; set; }

        [ForeignKey("OnSubmitMenu")]
        [Display(Name = "OnSubmitMenu")]
        public int? OnSubmitMenuId { get; set; }
        public virtual Menu OnSubmitMenu { get; set; }


        [ForeignKey("OnApproveMenu")]
        [Display(Name = "OnApproveMenu")]
        public int? OnApproveMenuId { get; set; }
        public virtual Menu OnApproveMenu { get; set; }
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
        public int ? NoOfPrintCopies { get; set; }
        
        [ForeignKey("DocumentPrintReportHeader")]
        public int? DocumentPrintReportHeaderId { get; set; }
        public virtual ReportHeader DocumentPrintReportHeader { get; set; }

        [MaxLength(50)]
        public string NonCountedQtyCaption { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public string TermsAndConditions { get; set; }

    }
}
