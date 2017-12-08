using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model.Models
{
    
    public class ProductSizeType : EntityBase, IHistoryLog
    {
        public ProductSizeType()
        {
        }

        [Key]
        public int ProductSizeTypeId { get; set; }

        [Display(Name = "ProductSizeType Name")]
        [MaxLength(50, ErrorMessage = "ProductSizeType Name cannot exceed 50 characters"), Required]
        [Index("IX_ProductSizeType_ProductSizeTypeName", IsUnique=true) ]
        public string ProductSizeTypeName { get; set; }

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
