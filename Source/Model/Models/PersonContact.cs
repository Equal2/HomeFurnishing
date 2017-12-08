using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PersonContact : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonContactID { get; set; }

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("PersonContactType"), Display(Name = "Contact Type")]
        public int PersonContactTypeId { get; set; }
        public virtual PersonContactType PersonContactType { get; set; }

        [Display(Name = "Contact Name")]
        public int ContactId { get; set; }
        [ForeignKey("ContactId")]
        public virtual Person Contact { get; set; }

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