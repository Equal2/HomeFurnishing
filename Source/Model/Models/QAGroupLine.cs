using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class QAGroupLine : EntityBase, IHistoryLog
    {      
        [Key]       
        public int QAGroupLineId { get; set; }

        [Display(Name = "QA Group"), Required]
        [ForeignKey("QAGroup")]        
        public int QAGroupId { get; set; }
        public virtual QAGroup QAGroup { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Mandatory")]
        public bool IsMandatory { get; set; }

        [Display(Name = "Data Type")]
        public string DataType { get; set; }

        [Display(Name = "List Item")]
        public string ListItem { get; set; }

        public string DefaultValue { get; set; }

        public bool IsActive { get; set; }


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
