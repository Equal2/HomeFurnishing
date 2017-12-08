using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class JobReceiveQASettingsViewModel
    {
        public int JobReceiveQASettingsId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public int? ImportMenuId { get; set; }
        public string ImportMenuName { get; set; }
        public int? WizardMenuId { get; set; }
        public string WizardMenuName { get; set; }
        public bool isVisibleProductUID { get; set; }
        public bool isMandatoryProductUID { get; set; }
        public bool isVisibleDimension1 { get; set; }
        public bool isVisibleDimension2 { get; set; }
        public bool isVisibleDimension3 { get; set; }
        public bool isVisibleDimension4 { get; set; }


        public bool isVisibleMarks { get; set; }
        public bool isVisibleDealUnit { get; set; }
        public bool IsVisibleInspectedQty { get; set; }
        public bool IsVisiblePenalty { get; set; }
        public bool IsVisibleSpecification { get; set; }
        public bool IsVisibleWeight { get; set; }
        public bool IsVisibleLength { get; set; }
        public bool IsVisibleWidth { get; set; }
        public bool IsVisibleHeight { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterPersonRoles { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

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
        public string DocumentPrint { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string OMSId { get; set; }
    }
}
