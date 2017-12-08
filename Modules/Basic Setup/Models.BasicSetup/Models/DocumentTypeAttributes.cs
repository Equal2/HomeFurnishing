using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class DocumentTypeAttribute : EntityBase, IHistoryLog
    {      
        [Key]       
        public int DocumentTypeAttributeId { get; set; }

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


        [ForeignKey("DocumentType")]
        public int DocumentTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }

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
