using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSampleAttributes : EntityBase, IHistoryLog
    {
        public ProductSampleAttributes()
        { 
         
        }

        [Key]
        public int ProductSampleAttributeId { get; set; }
        public int ProductSampleID { get; set; }

        [ForeignKey("ProductTypeAttribute")]
        public int ProductTypeAttributeId { get; set; }
        public virtual ProductTypeAttribute ProductTypeAttribute { get; set; }

        public string AttributeValue { get; set; }

        
       // public virtual ProductSample ProductSample { get; set; }

       // [NotMapped]
       
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
