using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class ChargeGroupPersonCalculationViewModel : EntityBase
    {
        public ChargeGroupPersonCalculationViewModel()
        {
        }

        [Key]
        public int ChargeGroupPersonCalculationId { get; set; }


        public int? SiteId { get; set; }
        public string SiteName { get; set; }

        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }


        public int ChargeGroupPersonId { get; set; }
        public string ChargeGroupPersonName { get; set; }


        public int CalculationId { get; set; }
        public string CalculationName { get; set; }




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
