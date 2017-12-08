using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PersonCustomGroupHeader : EntityBase, IHistoryLog
    {
        public PersonCustomGroupHeader()
        {
            PersonCustomGroupLine = new List<PersonCustomGroupLine>();
        }

        [Key]
        public int PersonCustomGroupId { get; set; }

        [Display(Name = "Person Custom Group")]
        [MaxLength(100), Required]
        [Index("IX_PersonCustomGroup_PersonCustomGroupName", IsUnique=true) ]
        public string PersonCustomGroupName { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<PersonCustomGroupLine> PersonCustomGroupLine { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
