using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductAlias : EntityBase, IHistoryLog
    {
        [Key]
        public int ProductAliasId { get; set; }



        [Display(Name = "DocType")]
        [Index("IX_ProductAlias_ProductAliasName", IsUnique = true, Order = 1)]
        [ForeignKey("DocumentType")]
        public int DocTypeId { get; set; }        
        public virtual DocumentType DocumentType { get; set; }
        
        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "ProductAlias Name cannot exceed 50 characters"), Required]
        [Index("IX_ProductAlias_ProductAliasName", IsUnique = true, Order=2)]
        public string ProductAliasName { get; set; }
                     
        [Display(Name = "Product")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }        
        public virtual Product Product { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
