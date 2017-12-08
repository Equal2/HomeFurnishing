using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    [Serializable]
    public class JobReceiveSettingsViewModel
    {
        public int JobReceiveSettingsId { get; set; }        
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public bool isVisibleMachine { get; set; }
        public bool isMandatoryMachine { get; set; }
        public bool isVisibleCostCenter { get; set; }
        public bool isMandatoryCostCenter { get; set; }
        public bool isVisibleProductUID { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleDimension3 { get; set; }
        public bool isVisibleDimension4 { get; set; }

        public bool isVisibleLoss { get; set; }
        public bool isVisibleSpecification { get; set; }
        public bool isVisibleDealUnit { get; set; }
        public bool isVisibleUncountableQty { get; set; }
        public bool IsVisibleForOrderMultiple { get; set; }
        public bool IsVisibleWeight { get; set; }
        public bool IsMandatoryWeight { get; set; }
        public string SqlProcConsumption { get; set; }
        public string SqlProcDocumentPrint { get; set; }
        public string SqlProcProductUidHelpList { get; set; }
        public string DocumentPrint { get; set; }
        public byte UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }
        public bool isVisibleRate { get; set; }
        public bool isMandatoryRate { get; set; }
        public bool isEditableRate { get; set; }
        public bool isVisibleLotNo { get; set; }
        public bool IsVisibleIncentive { get; set; }
        public bool IsVisiblePenalty { get; set; }
        public bool IsVisiblePassQty { get; set; }
        public bool isVisibleProcessHeader { get; set; }
        public bool isVisibleConsumptionDetail { get; set; }
        public bool isVisibleByProductDetail { get; set; }
        public bool isMandatoryProcessLine { get; set; }
        public bool isPostedInStock { get; set; }
        public bool isPostedInStockProcess { get; set; }
        public bool isPostedInStockVirtual { get; set; }

        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }

        public string filterPersonRoles { get; set; }

        [Range(1,int.MaxValue,ErrorMessage="Process field is required")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }        
        public int ? CalculationId { get; set; }
        public string CalculationName { get; set; }
        public string SqlProcGenProductUID { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string SqlProcGatePass { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterSubmit { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterApprove { get; set; }

        [MaxLength(20)]
        public string StockQty { get; set; }
        public Decimal? LossPer { get; set; }


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
        public bool isVisibleConsumptionDimension1 { get; set; }
        public bool isVisibleConsumptionDimension2 { get; set; }
        public bool isVisibleConsumptionDimension3 { get; set; }
        public bool isVisibleConsumptionDimension4 { get; set; }
        public bool isVisibleByProductDimension1 { get; set; }
        public bool isVisibleByProductDimension2 { get; set; }
        public bool isVisibleByProductDimension3 { get; set; }
        public bool isVisibleByProductDimension4 { get; set; }

        public int? WizardMenuId { get; set; }
        public string WizardMenuName { get; set; }
        public int? ImportMenuId { get; set; }
        public string ImportMenuName { get; set; }

    }
}
