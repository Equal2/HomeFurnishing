using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseQuotationSetting : EntityBase, IHistoryLog
    {

        [Key]
        public int PurchaseQuotationSettingId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }      
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleCostCenter { get; set; }
        public bool? isMandatoryCostCenter { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }
        public bool? isMandatoryRate { get; set; }
        public bool? isAllowedWithoutQuotation { get; set; }
        public bool? isEditableRate { get; set; }
        public bool? isVisibleLotNo { get; set; }
        public bool CalculateDiscountOnRate { get; set; }

        public bool? isPostedInStockVirtual { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }


        public bool ? isVisibleForIndent { get; set; }
        public bool ? isVisibleSalesTaxGroup { get; set; }
        public bool ? isVisibleCurrency { get; set; }
        public bool ? isVisibleDeliveryTerms { get; set; }
        public bool ? isVisibleShipMethod { get; set; }

        [ForeignKey("Calculation")]
        public int CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }
        
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
        public string SqlProcGenProductUID { get; set; }


        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For Type")]
        public byte ? UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

        public string TermsAndConditions { get; set; }


        [ForeignKey("OnSubmitMenu")]
        [Display(Name = "OnSubmitMenu")]
        public int? OnSubmitMenuId { get; set; }
        public virtual Menu OnSubmitMenu { get; set; }


        [ForeignKey("OnApproveMenu")]
        [Display(Name = "OnApproveMenu")]
        public int? OnApproveMenuId { get; set; }
        public virtual Menu OnApproveMenu { get; set; }



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
