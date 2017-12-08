using Model;
using Models.BasicSetup.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobWorker : EntityBase
    {
        public JobWorker()
        {
        
        }

        [Key]
        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonID { get; set; }
        public virtual Person Person { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
