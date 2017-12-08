using Model;
using Models.BasicSetup.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
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
