using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.ViewModels
{
    [Serializable]
    public class DocumentTypeAttributeViewModel 
    {      
        [Key]       
        public int DocumentTypeAttributeId { get; set; }
        public int DocumentAttributeId { get; set; }

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

        [NotMapped]
        public string PreviousFieldId { get; set; }

        //[ForeignKey("DocumentType")]
        public int DocumentTypeId { get; set; }
        //public virtual DocumentType DocumentType { get; set; }

        //[Display(Name = "Created By")]
        //public string CreatedBy { get; set; }
        //[Display(Name = "Modified By")]
        //public string ModifiedBy { get; set; }
        //[Display(Name = "Created Date")]
        //public DateTime CreatedDate { get; set; }
        //[Display(Name = "Modified Date")]
        //public DateTime ModifiedDate { get; set; }

    }
}
