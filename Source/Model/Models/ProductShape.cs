using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model.Models
{
    
    public class ProductShape : EntityBase, IHistoryLog
    {
        public ProductShape()
        {
            Sizes = new List<Size>();
        }

        [Key]
        public int ProductShapeId { get; set; }

        [Display(Name = "Product Shape Name")]
        [MaxLength(50, ErrorMessage = "ProductShape Name cannot exceed 50 characters"), Required]
        [Index("IX_ProductShape_ProductShapeName", IsUnique=true) ]
        public string ProductShapeName { get; set; }

        public string ProductShapeShortName { get; set; }

        public ICollection<Size> Sizes { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 
        
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
