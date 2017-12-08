using System.ComponentModel.DataAnnotations;

namespace Models.Customize.ViewModels
{
    public class JobOrderSettingsViewModel
    {
        public int JobOrderSettingsId { get; set; }        
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
        public bool isMandatoryProductUID { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleLoss { get; set; }
        public bool isVisibleUncountableQty { get; set; }
        public bool isVisibleFromProdOrder { get; set; }
        public string SqlProcConsumption { get; set; }
        public string SqlProcDocumentPrint { get; set; }
        public string DocumentPrint { get; set; }
        public string SqlProcGatePass { get; set; }
        public byte UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }
        public bool isVisibleRate { get; set; }
        public bool isMandatoryRate { get; set; }
        public bool isVisibleGodown { get; set; }
        public bool isMandatoryGodown { get; set; }
        public bool isEditableRate { get; set; }
        public bool isVisibleLotNo { get; set; }
        public bool isMandatoryProcessLine { get; set; }
        public bool isVisibleJobWorkerLine { get; set; }
        public bool isPostedInStock { get; set; }
        public bool isPostedInStockProcess { get; set; }
        public bool isPostedInStockVirtual { get; set; }

        public bool isVisibleProcessLine { get; set; }
        public bool isUniqueCostCenter { get; set; }
        public bool PersonWiseCostCenter { get; set; }

        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }        
        [Range(1,int.MaxValue,ErrorMessage="Process field is required")]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }        
        public int ? CalculationId { get; set; }
        public string CalculationName { get; set; }
        public string SqlProcGenProductUID { get; set; }
        public string Perks { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public int? WizardMenuId { get; set; }
        public string WizardMenuName { get; set; }
        public decimal NonCountedQty { get; set; }
        public decimal? LossQty { get; set; }
        public int? DueDays { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string BarcodeStatusUpdate { get; set; }
        public string Header_Save { get; set; }
        public string Header_Delete { get; set; }
        public string Header_Submit { get; set; }
        public string Header_Approve { get; set; }
        public string Header_Print { get; set; }
        public string Line_Save { get; set; }
        public string Line_Delete { get; set; }
        public int? MaxDays { get; set; }
        public int? AmountRoundOff { get; set; }
        public string JobUnitId { get; set; }
        public string JobUnitName { get; set; }
        public int ExcessQtyAllowedPer { get; set; }

    }
}
