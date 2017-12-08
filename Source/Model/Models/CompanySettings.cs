using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class CompanySettings : EntityBase, IHistoryLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        [Display(Name = "CompanySettingsId")]
        public int CompanySettingsId { get; set; }
        public int CompanyId { get; set; }
        public bool? isVisibleMessage { get; set; }
        public bool? isVisibleTask { get; set; }
        public bool? isVisibleNotification { get; set; }
        public bool? isVisibleGodownSelection { get; set; }
        public bool? isVisibleCompanyName { get; set; }
        public string SiteCaption { get; set; }
        public string DivisionCaption { get; set; }
        public string GodownCaption { get; set; }

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
