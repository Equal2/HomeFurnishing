using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSamplePhoto : EntityBase, IHistoryLog
    {
        public ProductSamplePhoto()
        {          
        }

        [Key]
        public int ProductSamplePhotoId { get; set; }      
        public string FileName { get; set; }
        public byte[] File { get; set; }

        public virtual ProductSample ProductSample { get; set; }

        [NotMapped]
        public int ProductSampleID { get; set; } 
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
