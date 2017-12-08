using Model;
using Models.BasicSetup.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.Models
{
    public class Company : EntityBase, IHistoryLog
    {


        [Key]
        [Display(Name = "Company Id")]
        public int CompanyId { get; set; }

        [MaxLength(50), Required]
        [Index("IX_Company_Company", IsUnique = true)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set;}

        [MaxLength(250, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [MaxLength(20,ErrorMessage="{0} can't exceed {1} characters")]        
        [Display(Name = "LST No")]
        public string LstNo { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "CST No")]
        public string CstNo { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "Tin No")]
        public string TinNo { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "IEC No")]
        public string IECNo { get; set; }

        [MaxLength(15, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "Phonoe")]
        public string Phone { get; set; }

        [MaxLength(15, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "Fax")]
        public string Fax { get; set; }

        [MaxLength(20, ErrorMessage = "{0} can't exceed {1} characters")]
        [Display(Name = "TitleCase")]
        public string TitleCase { get; set; }

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "Excise Division"), MaxLength(100)]
        public string ExciseDivision { get; set; }

        [Display(Name = "Director Name"), MaxLength(100)]
        public string DirectorName { get; set; }

        [Display(Name = "Bank Name"), MaxLength(100)]
        public string BankName { get; set; }

        [Display(Name = "Bank Branch"), MaxLength(250)]
        public string BankBranch { get; set; }

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
