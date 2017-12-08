using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.ViewModels
{
    public class JobWorkerViewModel
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
        public string CityName { get; set; }

        [MaxLength(6), Required]
        public string Zipcode { get; set; }

        [Display(Name = "PAN No")]
        [MaxLength(40)]
        public string PanNo { get; set; }

        [Display(Name = "Guarantor")]
        public int? GuarantorId { get; set; }
        public string GuarantorName { get; set; }

        [Display(Name = "Tds Category")]
        public int? TdsCategoryId { get; set; }
        public string TdsCategoryName { get; set; }

        [Display(Name = "Tds Group")]
        public int? TdsGroupId { get; set; }
        public string TdsGroupName { get; set; }

        [Display(Name = "Is Sister Concern ?")]
        public Boolean IsSisterConcern { get; set; }

        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public string PersonRateGroupName { get; set; }
        public int? CreaditDays { get; set; }
        public Decimal? CreaditLimit { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Account Group"), Required]
        public int LedgerAccountGroupId { get; set; }
        public string LedgerAccountGroupName { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        public int PersonAddressID { get; set; }
        public int AccountId { get; set; }
        public string DivisionIds { get; set; }
        public string SiteIds { get; set; }
        public string ProcessIds { get; set; }
        public int PersonRegistrationPanNoID { get; set; }
        public string Tags { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

    }

    public class JobWorkerIndexViewModel
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Mobile { get; set; }
        public string PanNo { get; set; }
    }
}
