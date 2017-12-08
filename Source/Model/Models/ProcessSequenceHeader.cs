using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProcessSequenceHeader : EntityBase, IHistoryLog
    {
        public ProcessSequenceHeader()
        {
        }

        [Key]
        public int ProcessSequenceHeaderId { get; set; }

        [Display(Name="Process Sequence")]
        [MaxLength(50), Required]
        [Index("IX_ProcessSequence_ProcessSequenceHeaderName", IsUnique = true)]
        public string ProcessSequenceHeaderName { get; set; }


        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Doc Type")]
        [ForeignKey("DocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public int? ReferenceDocId { get; set; }

        public string CheckSum { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
