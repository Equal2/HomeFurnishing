using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class DocumentCategory : EntityBase
    {
        public DocumentCategory()
        {
            //DocumentTypes = new List<DocumentType>();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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

        //public ICollection<DocumentType > DocumentTypes { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
     
    }
}
