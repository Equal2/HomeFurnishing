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
    public class TransporterViewModel
    {
        public int PersonId { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Name { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Suffix { get; set; }

        [Index("IX_Person_Code", IsUnique = true)]
        [MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters"), Required]
        public string Code { get; set; }

        [Display(Name = "Address"), Required]
        public string Address { get; set; }

        [Display(Name = "City"), Required]
        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [MaxLength(6), Required]
        public string Zipcode { get; set; }

        [MaxLength(11, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Phone { get; set; }

        [MaxLength(10, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Mobile { get; set; }

        [MaxLength(100, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Email { get; set; }

        [ForeignKey("LedgerAccountGroup"), Display(Name = "Account Group"), Required]
        public int LedgerAccountGroupId { get; set; }
        //public virtual AccountGroup AccountGroup { get; set; }

        [Display(Name = "PAN No")]
        [MaxLength(40)]
        public string PanNo { get; set; }



        [ForeignKey("TdsCategory"), Display(Name = "Tds Category")]
        public int? TdsCategoryId { get; set; }
        public virtual TdsCategory TdsCategory { get; set; }

        [ForeignKey("TdsGroup"), Display(Name = "Tds Group")]
        public int? TdsGroupId { get; set; }
        public virtual TdsGroup TdsGroup { get; set; }

        [Display(Name = "Service Tax No")]
        [MaxLength(40)]
        public string ServiceTaxNo { get; set; }

        [ForeignKey("ServiceTaxCategory"), Display(Name = "Service Tax Category")]
        public int? ServiceTaxCategoryId { get; set; }
        public virtual ServiceTaxCategory ServiceTaxCategory { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        public int PersonAddressID { get; set; }

        public int AccountId { get; set; }

        public int PersonRegistrationPanNoID { get; set; }

        public int PersonRegistrationServiceTaxNoID { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

    }

    public class TransporterIndexViewModel
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
    }
}
