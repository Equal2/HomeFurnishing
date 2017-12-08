using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MaterialPlanSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int MaterialPlanSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        public bool? isVisibleMachine { get; set; }
        public bool? isMandatoryMachine { get; set; }

        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }
        public bool? isMandatoryProcessLine { get; set; }
        public bool? isVisiblePurchPlanQty { get; set; }
        public bool? isVisibleProdPlanQty { get; set; }
        public string SqlProcConsumption { get; set; }
        public string SqlProcConsumptionSummary { get; set; }
        public string PendingProdOrderList { get; set; }
        public string filterProcesses { get; set; }
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterProductTypesConsumption { get; set; }
        public string filterPersonRoles { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }

        [ForeignKey("DocTypePurchaseIndent"), Display(Name = "Purchase Indent Type")]
        public int ? DocTypePurchaseIndentId { get; set; }
        public virtual DocumentType DocTypePurchaseIndent { get; set; }

        [ForeignKey("DocTypeProductionOrder"), Display(Name = "Production Order Type")]
        public int ? DocTypeProductionOrderId { get; set; }
        public virtual DocumentType DocTypeProductionOrder { get; set; }

        [ForeignKey("Godown"), Display(Name = "Godown")]
        public int? GodownId { get; set; }
        public virtual Godown Godown { get; set; }


        [Required]
        public string PlanType { get; set; }

        [ForeignKey("WizardMenu")]
        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }
        public virtual Menu WizardMenu { get; set; }

        [ForeignKey("ImportMenu")]
        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }
        public virtual Menu ImportMenu { get; set; }


        [ForeignKey("ExportMenu")]
        [Display(Name = "ExportMenu")]
        public int? ExportMenuId { get; set; }
        public virtual Menu ExportMenu { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        
        public int ? DocumentPrintReportHeaderId { get; set; }



    }
}
