using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobInvoiceSettingsViewModel
    {

        public int JobInvoiceSettingsId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public bool isVisibleMachine { get; set; }
        public bool isMandatoryMachine { get; set; }
        public bool isMandatoryJobOrder { get; set; }
        public bool isMandatoryJobReceive { get; set; }
        public bool isVisibleProductUID { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleDimension3 { get; set; }
        public bool isVisibleDimension4 { get; set; }

        public bool isVisibleProcessHeader { get; set; }

        public bool IsVisibleDocQty { get; set; }
        public bool isVisibleLoss { get; set; }
        public bool IsVisibleReceiveQty { get; set; }
        public bool IsVisiblePassQty { get; set; }
        public bool IsVisibleRate { get; set; }
        public bool IsVisibleAdditionalCharges { get; set; }

        public bool isVisibleLotNo { get; set; }
        public bool isVisibleSpecification { get; set; }
        public bool isVisibleDealUnit { get; set; }
        public bool isVisibleWeight { get; set; }
        public bool isVisibleCostCenter { get; set; }
        public bool isVisibleSalesTaxGroupPerson { get; set; }
        public bool isVisibleGovtInvoiceNo { get; set; }
        public bool isVisibleSalesTaxGroupProduct { get; set; }
        public bool isVisibleHeaderJobWorker { get; set; }
        public bool isVisibleIncentive { get; set; }
        public bool isVisiblePenalty { get; set; }
        public bool isVisibleRateDiscountPer { get; set; }
        public bool isVisibleFinancier { get; set; }
        public bool isVisibleJobOrder { get; set; }
        public bool isVisibleJobReceive { get; set; }
        public bool isVisibleMfgDate { get; set; }
        public bool isVisibleGodown { get; set; }
        public bool isVisibleJobReceiveBy { get; set; }
        public bool isPostedInStock { get; set; }
        public bool isPostedInStockProcess { get; set; }
        public bool isPostedInStockVirtual { get; set; }
        public bool isAutoCreateJobReceive { get; set; }
        public bool isGenerateProductUid { get; set; }

        [MaxLength(100)]
        public string SqlProcConsumption { get; set; }
        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }
        public string SqlProcProductUidHelpList { get; set; }

        [MaxLength(100)]
        public string DocumentPrint { get; set; }

        public string SqlProcGenProductUID { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }

        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterPersonRoles { get; set; }


        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }
        public string ImportMenuName { get; set; }
        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }
        public string WizardMenuName { get; set; }

        public int ? CalculationId { get; set; }
        public string CalculationName { get; set; }


        public int? SalesTaxGroupPersonId { get; set; }
        public string SalesTaxGroupPersonName { get; set; }

        public int? JobReceiveDocTypeId { get; set; }
        public string JobReceiveDocTypeName { get; set; }
        public int? AmountRoundOff { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }
        public int ? JobReturnDocTypeId { get; set; }
        public string SqlProcGatePass { get; set; }
        public int? NoOfPrintCopies { get; set; }
    }
}
