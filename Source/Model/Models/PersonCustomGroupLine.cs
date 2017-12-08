using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PersonCustomGroupLine : EntityBase, IHistoryLog
    {
        public PersonCustomGroupLine()
        {
        }

        [Key]
        public int PersonCustomGroupLineId { get; set; }

        [Display(Name = "Person Custom Group")]
        [ForeignKey("PersonCustomGroupHeader")]
        public int PersonCustomGroupHeaderId { get; set; }
        public virtual PersonCustomGroupHeader PersonCustomGroupHeader { get; set; }

        [Display(Name = "Person"), Required]
        [ForeignKey("Person")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

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
