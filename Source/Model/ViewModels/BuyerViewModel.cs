using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class BuyerViewModel
    {
        public int PersonId { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Name { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Suffix { get; set; }

        [Index("IX_Person_Code", IsUnique = true)]
        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Code { get; set; }

        [MaxLength(11, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Phone { get; set; }

        [MaxLength(10, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Mobile { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Email { get; set; }

        [Display(Name = "Address"), Required]
        public string Address { get; set; }

        [Display(Name = "City"), Required]
        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        public string CityName { get; set; }

        [MaxLength(6), Required]
        public string Zipcode { get; set; }

        [Display(Name = "Cst No")]
        [MaxLength(40)]
        public string CstNo { get; set; }

        [Display(Name = "Tin No")]
        [MaxLength(40)]
        public string TinNo { get; set; }


        [Display(Name = "PAN No")]
        [MaxLength(40)]
        public string PANNo { get; set; }

        [Display(Name = "Is Sister Concern ?")]
        public Boolean IsSisterConcern { get; set; }

        [ForeignKey("PersonRateGroup")]
        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public virtual PersonRateGroup PersonRateGroup { get; set; }

        public int? CreaditDays { get; set; }

        public Decimal? CreaditLimit { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [ForeignKey("SalesTaxGroupParty"), Display(Name = "Sales Tax Group Party")]
        public int? SalesTaxGroupPartyId { get; set; }
        public string SalesTaxGroupPartyName { get; set; }

        [ForeignKey("LedgerAccountGroup"), Display(Name = "Account Group"), Required]
        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        public int PersonAddressID { get; set; }
        public int AccountId { get; set; }
        public int PersonRegistrationCstNoID { get; set; }
        public int PersonRegistrationTinNoID { get; set; }
        public int PersonRegistrationPANNoID { get; set; }
        public string SiteIds { get; set; }
        public string Tags { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public int? GuarantorId { get; set; }
        public string GuarantorName { get; set; }
    }

    public class BuyerIndexViewModel
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Suffix { get; set; }
    }
}
