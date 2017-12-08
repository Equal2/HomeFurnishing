using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model.Models
{
    
    public class ProductCollection : EntityBase, IHistoryLog
    {
        public ProductCollection()
        {
        }

        [Key]
        public int ProductCollectionId { get; set; }

        [Display(Name = "Product Collection Name")]
        [MaxLength(50, ErrorMessage = "ProductCollection Name cannot exceed 50 characters"), Required]
        [Index("IX_ProductCollection_ProductCollectionName", IsUnique=true) ]
        public string ProductCollectionName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [ForeignKey("ProductType")]
        public int ProductTypeId { get; set; }
        public virtual ProductType ProductType { get; set; }
        

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
