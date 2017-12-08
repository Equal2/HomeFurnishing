using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class ProductType : EntityBase, IHistoryLog
    {
        public ProductType()
        {
            //ProductGroups = new List<ProductGroup>();
            //ProductCategories = new List<ProductCategory>();
            //ProductDesigns = new List<ProductDesign>();
            //ProductTypeAttributes = new List<ProductTypeAttribute>();
        }

        [Key]
        public int ProductTypeId { get; set; }

        [Display(Name="Product Type")]
        [MaxLength(50, ErrorMessage = "ProductType Name cannot exceed 50 characters"), Required]
        [Index("IX_ProductType_ProductTypeName", IsUnique = true)]
        public string ProductTypeName { get; set; }


        [ForeignKey("ProductNature")]
        [Display(Name = "Product Nature")]
        public int ProductNatureId { get; set; }
        public virtual ProductNature ProductNature { get; set; }

        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

        public bool? IsCustomUI { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is PostedInStock")]
        public Boolean IsPostedInStock { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; } 


        [ForeignKey("Dimension1Type")]
        [Display(Name = "Dimension1 Type")]
        public int? Dimension1TypeId { get; set; }
        public virtual Dimension1Types Dimension1Type { get; set; }

        [ForeignKey("Dimension2Type")]
        [Display(Name = "Dimension2 Type")]
        public int? Dimension2TypeId { get; set; }
        public virtual Dimension2Types Dimension2Type { get; set; }


        //public ICollection<ProductGroup> ProductGroups { get; set; }
        //public ICollection<ProductCategory> ProductCategories { get; set; }
        //public ICollection<ProductDesign> ProductDesigns { get; set; }
        //public List<ProductTypeAttribute> ProductTypeAttributes { get; set; }
        

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
