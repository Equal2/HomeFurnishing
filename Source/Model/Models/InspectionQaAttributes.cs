using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class InspectionQaAttributes : EntityBase, IHistoryLog
    {

        [Key]        
        public int InspectionQaAttributesId { get; set; }

        
        //[Display(Name = "Inspection No.")]        
        public int InspectionHeaderId { get; set; }
        //[ForeignKey("InspectionHeaderId")]
        //public virtual InspectionHeader  InspectionHeader { get; set; }

        [Display(Name = "QA Attribute")]        
        public int ProductTypeQaAttributeId { get; set; }
        [ForeignKey("ProductTypeQaAttributeId")]
        public virtual ProductTypeQaAttribute ProductTypeQaRankAttribute { get; set; }
        
        [Display(Name = "Value")]
        public int Value { get; set; }

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
