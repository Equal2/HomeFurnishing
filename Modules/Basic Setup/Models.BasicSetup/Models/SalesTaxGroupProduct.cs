using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class SalesTaxGroupProduct : EntityBase, IHistoryLog
    {
        public SalesTaxGroupProduct()
        {
            //Product = new List<Product>();
        }

        [Key]
        public int SalesTaxGroupProductId { get; set; }

        [Display(Name="SalesTaxGroupProduct")]
        [MaxLength(50, ErrorMessage = "SalesTaxGroupProduct Name cannot exceed 50 characters"), Required]
        [Index("IX_SalesTaxGroupProduct_SalesTaxGroupProductName", IsUnique = true)]
        public string SalesTaxGroupProductName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        public ICollection<Product> Product { get; set; }
        
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
