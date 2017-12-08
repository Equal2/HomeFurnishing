using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleQuotationSettings : EntityBase, IHistoryLog
    {
        public SaleQuotationSettings()
        {
        }

        [Key]
        public int SaleQuotationSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Quotation Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public int Priority { get; set; }

        [ForeignKey("ShipMethod"),Display(Name="Ship Method")]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [ForeignKey("DeliveryTerms"),Display(Name="Delivery Terms")]
        public int DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For"),Range(1,int.MaxValue,ErrorMessage="Unit Conversion for field is required")]        
        public byte UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        public string TermsAndConditions { get; set; }


        public bool? isVisibleCurrency { get; set; }
        public bool? isVisibleShipMethod { get; set; }

        public bool? isVisibleSalesTaxGroupPerson { get; set; }
        public bool? isVisibleSalesTaxGroupProduct { get; set; }
        public bool? isVisibleDoorDelivery { get; set; }
        public bool? isVisibleCreditDays { get; set; }
        public bool? isVisibleCostCenter { get; set; }
        public bool? isVisibleDeliveryTerms { get; set; }
        public bool? isVisiblePriority { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }



        public bool? isVisibleDealUnit { get; set; }
        public bool? isVisibleSpecification { get; set; }
        public bool? isVisibleProductCode { get; set; }
        public bool? isVisibleUnitConversionFor { get; set; }
        public bool? isVisibleFinancier { get; set; }
        public bool? isVisibleSalesExecutive { get; set; }
        public bool? isVisiblePaymentTerms { get; set; }
        public bool? isUniqueCostCenter { get; set; }
        public bool? IsPersonWiseCostCenter { get; set; }
        public bool? isVisibleFromSaleEnquiry { get; set; }
        public bool? isVisibleAgent { get; set; }
        public bool? isVisibleDiscountPer { get; set; }
        public bool? CalculateDiscountOnRate { get; set; }

        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterPersonRoles { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string FilterProductDivision { get; set; }
        public string filterProductCategories { get; set; }

        [MaxLength(100)]
        public string DocumentPrint { get; set; }
        public int? NoOfPrintCopies { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }


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


        [ForeignKey("ImportMenu")]
        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }
        public virtual Menu ImportMenu { get; set; }


        [ForeignKey("WizardMenu")]
        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }
        public virtual Menu WizardMenu { get; set; }



        [ForeignKey("ExportMenu")]
        [Display(Name = "ExportMenu")]
        public int? ExportMenuId { get; set; }
        public virtual Menu ExportMenu { get; set; }


        [ForeignKey("Calculation")]
        public int? CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }

        [ForeignKey("DocumentPrintReportHeader")]
        public int? DocumentPrintReportHeaderId { get; set; }
        public virtual ReportHeader DocumentPrintReportHeader { get; set; }



        


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
