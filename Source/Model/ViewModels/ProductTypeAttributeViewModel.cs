using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    [Serializable]
    public class ProductTypeAttributeViewModel 
    {      
        [Key]       
        public int ProductTypeAttributeId { get; set; }
        public int ProductAttributeId { get; set; }

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

        //[ForeignKey("ProductType")]
        public int ProductType_ProductTypeId { get; set; }
        //public virtual ProductType ProductType { get; set; }

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
