using Model;
using Models.BasicSetup.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.Models
{
    public class Site : EntityBase, IHistoryLog
    {

        [Key]
        public int SiteId { get; set; }


        [ForeignKey("DocType")]
        public int? DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [MaxLength(50, ErrorMessage = "Site Name cannot exceed 50 characters"), Required]


        public string SiteName { get; set; }
        [MaxLength(250, ErrorMessage = "Site Name cannot exceed 250 characters")]
        public string SiteCode { get; set; }
        [MaxLength(20, ErrorMessage = "Site Code cannot exceed 20 characters")]
        public string Address { get; set; }
        [MaxLength(50, ErrorMessage = "Phone no cannot exceed 50 characters")]
        public string PhoneNo { get; set; }

        [ForeignKey("City"), Range(1,int.MaxValue)]
        public int? CityId { get; set; }
        public virtual City City { get; set; }

        [ForeignKey("Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("DefaultGodown")]
        public int ? DefaultGodownId { get; set; }
        public virtual Godown DefaultGodown { get; set; }

        [ForeignKey("DefaultDivision")]
        public int? DefaultDivisionId { get; set; }
        public virtual Division DefaultDivision { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        [MaxLength(25)]
        public string ThemeColour { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        [InverseProperty("Site")]
        public ICollection<Godown> Godown { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
