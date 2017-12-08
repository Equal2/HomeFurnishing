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
    public class MaterialPlanSettingsViewModel
    {
        public int MaterialPlanSettingsId { get; set; }

        [Display(Name = "Order Type")]
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

        public bool isVisibleMachine { get; set; }
        public bool isMandatoryMachine { get; set; }
        public bool isMandatoryProcessLine { get; set; }
        public bool isVisiblePurchPlanQty { get; set; }
        public bool isVisibleProdPlanQty { get; set; }
        public string SqlProcConsumption { get; set; }
        public string filterProcesses { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterPersonRoles { get; set; }
        public string SqlProcDocumentPrint { get; set; }
        public string PendingProdOrderList { get; set; }
        public int ? DocTypePurchaseIndentId { get; set; }
        public string DocTypePurchaseIndentName { get; set; }        
        public int ? DocTypeProductionOrderId { get; set; }
        public string DocTypeProductOrderName { get; set; }



        public int? WizardMenuId { get; set; }

        [Required]
        public string PlanType { get; set; }
    }
}
