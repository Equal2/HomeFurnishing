using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PersonContactType : EntityBase, IHistoryLog
    {
        public PersonContactType()
        {
            PersonContacts = new List<PersonContact>();
        }

        [Key]
        public int PersonContactTypeId { get; set; }

        [MaxLength(50, ErrorMessage = "ContactType Name cannot exceed 50 characters"), Required]
        [Display(Name = "Contact Type")]
        [Index("IX_PersonContactType_PersonContactTypeName", IsUnique = true)]
        public string PersonContactTypeName { get; set; }        

        public ICollection<PersonContact> PersonContacts { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 

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
