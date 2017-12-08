using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProdOrderSettingsViewModel
    {
        public int ProdOrderSettingsId { get; set; }

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
        public bool isVisibleBuyer { get; set; }
        public bool isVisibleLineDueDate { get; set; }
        public bool isMandatoryProcessLine { get; set; }
        public string filterProcesses { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterPersonRoles { get; set; }
        public string SqlProcDocumentPrint { get; set; }

        public int? ImportMenuId { get; set; }
        public string ImportMenuName { get; set; }

        public int? ExportMenuId { get; set; }
        public string ExportMenuName { get; set; }
    }
}
