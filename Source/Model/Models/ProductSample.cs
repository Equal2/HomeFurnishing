using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Model.Models
{
    public class ProductSample : EntityBase, IHistoryLog
    {
        public ProductSample()
        {
            ProductSamplePhoto = new List<ProductSamplePhoto>();
            ProductSamplePhotoApprovals = new List<ProductSamplePhotoApproval>();
            ProductSampleAttributes = new List<ProductSampleAttributes>();
        }

        [Key]
        public int ProductSampleId { get; set; }        

        [MaxLength(50), Required]
        [Display(Name = "Sample Name")]   
        public string SampleName { get; set; }

        [Display(Name = "Sample Description")]       
        public string SampleDescription { get; set; }   

        [Display(Name = "Email Date") ,DataType(DataType.Date),DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]       
        public DateTime? EmailDate { get; set; }       
        public byte[] ProductPicture { get; set; }
        public Person Supplier { get; set; }
        public Person Employee { get; set; } // Contected person of Surya by Supplier

       // [ForeignKey("ProductType")]
        [Display(Name = "Product Type")]
        [NotMapped]
        public int ProductTypeId { get; set; }
        public virtual ProductType ProductType { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        public virtual ICollection<ProductSamplePhoto> ProductSamplePhoto { get; set; }
        public virtual ICollection<ProductSamplePhotoApproval> ProductSamplePhotoApprovals { get; set; }

        public virtual ICollection<ProductSampleAttributes> ProductSampleAttributes { get; set; }


        [NotMapped]
        public int PersonID { get; set; } // Contected person Id of Surya by Supplier
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
