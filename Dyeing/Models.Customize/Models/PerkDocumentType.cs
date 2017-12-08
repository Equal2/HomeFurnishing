using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class PerkDocumentType : EntityBase, IHistoryLog
    {
        [Key]
        public int PerkDocumentTypeId { get; set; }

        [Display(Name = "Document Type")]
        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        public int SiteId { get; set; }

        public virtual Site Site { get; set; }


        [Display(Name = "Perk Name")]
        [ForeignKey("Perk")]
        public int PerkId { get; set; }
        public virtual Perk Perk { get; set; }


        [Display(Name = "Rate Document Type")]
        [ForeignKey("RateDocType")]
        public int? RateDocTypeId { get; set; }
        public virtual DocumentType RateDocType { get; set; }


        public int? RateDocId { get; set; }

        public decimal Base { get; set; }

        public decimal Worth { get; set; }

        public Decimal CostConversionMultiplier { get; set; }
        public bool IsEditableRate { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
