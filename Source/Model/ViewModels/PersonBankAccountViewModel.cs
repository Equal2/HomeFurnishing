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
    public class PersonBankAccountViewModel
    {
        [Key]
        public int PersonBankAccountID { get; set; }

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        [MaxLength(200)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [MaxLength(500)]
        [Display(Name = "Bank Branch")]
        public string BankBranch { get; set; }

        [MaxLength(50)]
        [Display(Name = "Bank Code")]
        public string BankCode { get; set; }

        [MaxLength(20)]
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}
