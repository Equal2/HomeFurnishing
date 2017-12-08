using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class DocumentNature : EntityBase
    {
        public DocumentNature()
        {
            //DocumentTypes = new List<DocumentType>();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int DocumentNatureId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "Document Nature Name cannot exceed 50 characters"), Required]
        [Index("IX_DocumentNature_DocumentNatureName", IsUnique = true)]
        public string DocumentNatureName { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }


        [MaxLength(50)]
        public string OMSId { get; set; }
     
    }
}
