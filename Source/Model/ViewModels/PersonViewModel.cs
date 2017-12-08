using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class PersonViewModel 
    {
        public PersonViewModel()
        {
            PersonContacts = new List<PersonContact>();
        }

        public int PersonID { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        
        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Name { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters"), Required]
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

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }
        public string CityName { get; set; }

        [MaxLength(6)]
        public string Zipcode { get; set; }

        public int? PersonRateGroupId { get; set; }


        [Display(Name = "PAN No")]
        [MaxLength(40)]
        public string PanNo { get; set; }

        [Display(Name = "GST No")]
        [MaxLength(40)]
        public string GstNo { get; set; }

        [Display(Name = "CST No")]
        [MaxLength(40)]
        public string CstNo { get; set; }

        [Display(Name = "TIN No")]
        [MaxLength(40)]
        public string TinNo { get; set; }

        [Display(Name = "Aadhar No")]
        [MaxLength(40)]
        public string AadharNo { get; set; }

        public int? ParentId { get; set; }
        public string ParentName { get; set; }

        public int? GuarantorId { get; set; }
        public string GuarantorName { get; set; }

        public int? TdsCategoryId { get; set; }
        public string TdsCategoryName { get; set; }

        public int? TdsGroupId { get; set; }
        public string TdsGroupName { get; set; }

        [Display(Name = "Is Sister Concern ?")]
        public Boolean IsSisterConcern { get; set; }
        public string DivisionIds { get; set; }
        public string SiteIds { get; set; }
        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }

        public int? SalesTaxGroupPartyId { get; set; }
        public string SalesTaxGroupPartyName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        public int? CreaditDays { get; set; }

        public Decimal? CreaditLimit { get; set; }

        public int PersonAddressID { get; set; }

        public int AccountId { get; set; }
        public string Tags { get; set; }
        public int? PersonRegistrationPanNoID { get; set; }
        public int? PersonRegistrationCstNoID { get; set; }
        public int? PersonRegistrationGstNoID { get; set; }
        public int? PersonRegistrationTinNoID { get; set; }
        public int? PersonRegistrationAadharNoID { get; set; }

        public int? ProcessId { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }




        public string PersonType { get; set; }
        

        
        public bool Active { get; set; }

        public virtual ICollection<PersonContact> PersonContacts { get; set; }



        public PersonSettingsViewModel PersonSettings { get; set; }



        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
    }

    public class PersonIndexViewModel
    {
        public int PersonId { get; set; }
        public int DocTypeId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Suffix { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

    }
}
