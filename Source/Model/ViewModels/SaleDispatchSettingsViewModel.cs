using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SaleDispatchSettingsViewModel
    {

        [Key]
        public int SaleDispatchSettingId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleDimension3 { get; set; }
        public bool isVisibleDimension4 { get; set; }
        public bool isVisibleFreeQty { get; set; }

        public bool isVisibleProductUID { get; set; }
        public bool isVisibleLotNo { get; set; }
        public bool isVisibleStockIn { get; set; }
        public bool CalculateDiscountOnRate { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterPersonRoles { get; set; }
        public int? DocTypePackingHeaderId { get; set; }
        public string DocTypePackingHeaderName { get; set; }
        public int? SaleDispatchDocTypeId { get; set; }
        public string SaleDispatchDocTypeName { get; set; }



        public bool isVisibleAgent { get; set; }
        public bool isVisibleCurrency { get; set; }
        public bool isVisibleDeliveryTerms { get; set; }
        public bool isVisibleDealUnit { get; set; }
        public bool isVisibleShipMethod { get; set; }
        public bool isVisibleSpecification { get; set; }
        public bool isVisibleSalesTaxGroup { get; set; }

        public bool isVisibleProductCode { get; set; }
        public bool isVisibleBaleNo { get; set; }
        public bool isVisibleDiscountPer { get; set; }
        public bool isVisibleForSaleOrder { get; set; }
        public bool isVisibleWeight { get; set; }

        public bool IsMandatoryStockIn { get; set; }



        public int? CurrencyId { get; set; }
        public int? DeliveryTermsId { get; set; }
        public int? ShipMethodId { get; set; }
        public int? SalesTaxGroupId { get; set; }

        public int? GodownId { get; set; }

        public int? ProcessId { get; set; }


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
        public string SqlProcGatePass { get; set; }

        [Display(Name = "Unit Conversion For Type")]
        public byte? UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }
        public int? DocTypeDispatchReturnId { get; set; }
        public string DocTypeDispatchReturnName { get; set; }


        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }
        public string ImportMenuName { get; set; }

    }
}
