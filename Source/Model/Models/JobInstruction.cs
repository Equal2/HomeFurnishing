using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInstruction : EntityBase, IHistoryLog
    {
        public JobInstruction()
        {
        }

        [Key]
        public int JobInstructionId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50), Required]
        [Index("IX_JobInstruction_JobInstructionDescription", IsUnique = true)]
        public string JobInstructionDescription { get; set; }

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
