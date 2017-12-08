using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Company.Models;

namespace Models.BasicSetup.Models
{
    public class PersonRegistration : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonRegistrationID { get; set; }        

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        
        [MaxLength(30)]
        [Display(Name = "Registration Type")]
        public string RegistrationType { get; set; }

        [MaxLength(50)]
        [Display(Name = "Registration No")]
        public string RegistrationNo { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegistrationDate { get; set; }

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

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
