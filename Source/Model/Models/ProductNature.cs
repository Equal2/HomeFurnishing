using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductNature : EntityBase, IHistoryLog
    {
        public ProductNature()
        {
            ProductTypes = new List<ProductType>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int ProductNatureId { get; set; }

        [Display(Name="Product Nature")]
        [MaxLength(50, ErrorMessage = "ProductNature Name cannot exceed 50 characters"), Required]
        [Index("IX_ProductNature_ProductNatureName", IsUnique = true)]
        public string ProductNatureName { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 
        public ICollection<ProductType> ProductTypes { get; set; }
        
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
