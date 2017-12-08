using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    [Serializable]
    public class DocumentTypeHeaderAttributeViewModel 
    {      
        [Key]       
        public int DocumentTypeHeaderAttributeId { get; set; }

        [Required]
        public string Name { get; set; }

         [Display(Name = "Mandatory")]
        public bool IsMandatory { get; set; }

        [Display(Name = "Data Type")]
        public string DataType { get; set; }

        [Display(Name = "List Item")]
        public string ListItem { get; set; }

        public string Value { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public string PreviousFieldId { get; set; }

        public int DocumentTypeId { get; set; }
    }
}
