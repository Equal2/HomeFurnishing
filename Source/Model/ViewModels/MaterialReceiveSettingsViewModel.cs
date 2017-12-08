using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class MaterialReceiveSettingsViewModel
    {

        public int MaterialReceiveSettingsId { get; set; }

        [Display(Name = "Order Type")]
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

        public bool isVisibleRate { get; set; }
        public bool isMandatoryRate { get; set; }
        public bool isEditableRate { get; set; }
        public bool isVisibleLotNo { get; set; }
        public bool isMandatoryProcessLine { get; set; }
        public bool? isVisibleProcessLine { get; set; }
        public bool isPostedInStockProcess { get; set; }

        [MaxLength(50)]
        public string PersonFieldHeading { get; set; } 

        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterPersonRoles { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }


    }
}
