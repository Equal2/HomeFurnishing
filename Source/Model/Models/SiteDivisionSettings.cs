using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SiteDivisionSettings : EntityBase, IHistoryLog
    {
        public SiteDivisionSettings()
        {
            IsApplicableVAT = false;
            IsApplicableGST = true;
        }

        [Key]
        public int SiteDivisionSettingsId { get; set; }
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsApplicableVAT { get; set; }
        public bool IsApplicableGST { get; set; }
        public string ReportHeaderTextLeft { get; set; }
        public string ReportHeaderTextRight { get; set; }

        [MaxLength(50)]
        public string SalesTaxProductCodeCaption { get; set; }
        [MaxLength(50)]
        public string SalesTaxCaption { get; set; }
        [MaxLength(50)]
        public string SalesTaxGroupProductCaption { get; set; }
        [MaxLength(50)]
        public string SalesTaxGroupPersonCaption { get; set; }
        [MaxLength(50)]
        public string SalesTaxRegistrationCaption { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}
