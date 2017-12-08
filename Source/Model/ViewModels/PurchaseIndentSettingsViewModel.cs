using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseIndentSettingsViewModel
    {
        public int PurchaseIndentSettingId { get; set; }        
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public bool isVisibleCostCenter { get; set; }
        public bool isMandatoryCostCenter { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleDimension3 { get; set; }
        public bool isVisibleDimension4 { get; set; }

        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterSubmit { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterApprove { get; set; }
    }
}
