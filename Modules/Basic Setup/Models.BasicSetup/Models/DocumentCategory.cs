using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.Models
{
    public class DocumentCategory : EntityBase
    {    

        [Key]
        public int DocumentCategoryId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Document Category Name cannot exceed 50 characters"), Required]
        [Index("IX_DocumentCategory_DocumentCategoryName", IsUnique = true)]
        public string DocumentCategoryName { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }
        [MaxLength(50)]
        public string OMSId { get; set; }
     
    }
}
