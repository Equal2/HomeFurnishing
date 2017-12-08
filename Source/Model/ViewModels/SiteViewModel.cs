using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    public class SiteViewModel
    {

        public int SiteId { get; set; }
        [MaxLength(50, ErrorMessage = "Site Name cannot exceed 50 characters"), Required]
        public string SiteName { get; set; }
        [MaxLength(250, ErrorMessage = "Site Name cannot exceed 250 characters")]
        public string SiteCode { get; set; }
        [MaxLength(20, ErrorMessage = "Site Code cannot exceed 20 characters")]
        public string Address { get; set; }
        [MaxLength(50, ErrorMessage = "Phone no cannot exceed 50 characters")]
        public string PhoneNo { get; set; }

        [Required]
        public int? CityId { get; set; }
        public string CityName { get; set; }

        public int? PersonId { get; set; }
        public string PersonName { get; set; }
        
        public int ? DefaultGodownId { get; set; }
        public string GodownName { get; set; }

        public int? DefaultDivisionId { get; set; }
        public string DefaultDivisionName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        [MaxLength(25)]
        public string ThemeColour { get; set; }
    }
}
