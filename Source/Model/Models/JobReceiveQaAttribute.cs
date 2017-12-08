using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveQAAttribute : EntityBase, IHistoryLog
    {

        [Key]        
        public int JobReceiveQAAttributeId { get; set; }

        
        [Display(Name = "Job Receive QA No.")]
        [ForeignKey("JobReceiveQALine")]
        public int JobReceiveQALineId { get; set; }        
        public virtual JobReceiveQALine JobReceiveQALine { get; set; }


        [Display(Name = "QA Group")]
        [ForeignKey("QAGroupLine")]
        public int QAGroupLineId { get; set; }
        public virtual QAGroupLine QAGroupLine { get; set; }
        
        [Display(Name = "Value")]
        public string Value { get; set; }

        public string Remark { get; set; }

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
