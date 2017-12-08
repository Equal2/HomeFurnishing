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
    public class ChargeGroupSettingsViewModel : EntityBase
    {
        public ChargeGroupSettingsViewModel()
        {
        }

        [Key]
        public int ChargeGroupSettingsId { get; set; }

        public int ChargeTypeId { get; set; }
        public string ChargeTypeName { get; set; }


        public int ChargeGroupPersonId { get; set; }
        public string ChargeGroupPersonName { get; set; }


        public int ProcessId { get; set; }
        public string ProcessName { get; set; }


        public int ChargeGroupProductId { get; set; }
        public string ChargeGroupProductName { get; set; }

        public Decimal ChargePer { get; set; }

        public int? ChargeLedgerAccountId { get; set; }
        public string ChargeLedgerAccountName { get; set; }


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
