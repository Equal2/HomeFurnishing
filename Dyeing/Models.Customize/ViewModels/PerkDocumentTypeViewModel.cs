using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Customize.ViewModels
{
    public class PerkDocumentTypeViewModel
    {
        [Key]
        public int PerkDocumentTypeId { get; set; }

        [Display(Name = "Document Type")]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Perk Name")]
        public int PerkId { get; set; }
        public string PerkName { get; set; }

        [Display(Name = "Rate Document Type")]
        public int? RateDocTypeId { get; set; }
        public string RateDocTypeName { get; set; }
        public int? RateDocId { get; set; }
        public decimal Base { get; set; }
        public decimal Worth { get; set; }
        public Decimal CostConversionMultiplier { get; set; }
        public bool IsEditableRate { get; set; }
    }
}
