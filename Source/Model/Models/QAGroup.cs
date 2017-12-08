using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class QAGroup : EntityBase, IHistoryLog
    {

        [Key]        
        public int QAGroupId { get; set; }

        [Display(Name = "Master Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_QAGroup_Name", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }


        [Display(Name = "Name"),Required,MaxLength(50) ]
        [Index("IX_QAGroup_Name", IsUnique = true, Order = 2)]
        public string QaGroupName { get; set; }


        [Display(Name = "Description")]
        public string Description { get; set; }


        [Display(Name = "Status")]
        public int Status { get; set; }                

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }


        [MaxLength(50)]
        public string OMSId { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
    }
}
