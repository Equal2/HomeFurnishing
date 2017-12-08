using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderHeaderExtended : EntityBase
    {
        [Key]
        [ForeignKey("JobOrderHeader")]
        public int JobOrderHeaderId { get; set; }
        public JobOrderHeader JobOrderHeader { get; set; }

        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
    }
}
