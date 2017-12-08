using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProcessSequenceLine : EntityBase, IHistoryLog
    {
        public ProcessSequenceLine()
        {
        }

        [Key]
        public int ProcessSequenceLineId { get; set; }

        [Display(Name = "ProcessSequence"), Required]
        [ForeignKey("ProcessSequenceHeader")]
        public int ProcessSequenceHeaderId { get; set; }
        public virtual ProcessSequenceHeader ProcessSequenceHeader { get; set; }

        [Display(Name = "Process"), Required]
        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Sequence"), Required]
        public int Sequence { get; set; }

        [Display(Name = "Days"), Required]
        public int Days { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Rate Group")]
        [ForeignKey("ProductRateGroup")]
        public int? ProductRateGroupId { get; set; }
        public virtual ProductRateGroup ProductRateGroup { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
