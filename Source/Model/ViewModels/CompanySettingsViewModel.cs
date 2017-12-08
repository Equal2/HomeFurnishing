using System;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    [Serializable]
    public class CompanySettingsViewModel
    {
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
