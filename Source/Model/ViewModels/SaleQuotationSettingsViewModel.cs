using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    public class SaleQuotationSettingsViewModel : EntityBase, IHistoryLog
    {
        public SaleQuotationSettingsViewModel()
        {
        }

        [Key]
        public int SaleQuotationSettingsId { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public int Priority { get; set; }

        public int ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        public int DeliveryTermsId { get; set; }
        public string DeliveryTermsName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public byte UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }





        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public string TermsAndConditions { get; set; }


        public bool isVisibleCurrency { get; set; }
        public bool isVisibleShipMethod { get; set; }
        public bool isVisibleSalesTaxGroupPerson { get; set; }
        public bool isVisibleSalesTaxGroupProduct { get; set; }
        public bool isVisibleDoorDelivery { get; set; }
        public bool isVisibleCreditDays { get; set; }
        public bool isVisibleCostCenter { get; set; }
        public bool isVisibleDeliveryTerms { get; set; }
        public bool isVisiblePriority { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleDimension3 { get; set; }
        public bool isVisibleDimension4 { get; set; }

        public bool isVisibleDealUnit { get; set; }
        public bool isVisibleSpecification { get; set; }
        public bool isVisibleProductCode { get; set; }
        public bool isVisibleUnitConversionFor { get; set; }
        public bool isVisibleFinancier { get; set; }
        public bool isVisibleSalesExecutive { get; set; }
        public bool isVisiblePaymentTerms { get; set; }
        public bool isUniqueCostCenter { get; set; }
        public bool IsPersonWiseCostCenter { get; set; }



        
        public bool isVisibleFromSaleEnquiry { get; set; }
        public bool isVisibleAgent { get; set; }
        public bool isVisibleDiscountPer { get; set; }
        public bool CalculateDiscountOnRate { get; set; }
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

        public string DocumentPrint { get; set; }
        public int? NoOfPrintCopies { get; set; }

        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }


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



        public int? ImportMenuId { get; set; }
        public string ImportMenuName { get; set; }

        public int? WizardMenuId { get; set; }
        public string WizardMenuName { get; set; }


        public int? ExportMenuId { get; set; }
        public string ExportMenuName { get; set; }


        public int? CalculationId { get; set; }
        public string CalculationName { get; set; }




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
