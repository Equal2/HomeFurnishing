using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSamplePhotoApproval : EntityBase, IHistoryLog
    {
        public ProductSamplePhotoApproval()
        {          
        }

        [Key]
        public int ProductSamplePhotoApprovalId { get; set; }       
 
        [Display(Name = "Response Date") ,DataType(DataType.Date),DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]      
        public DateTime ResponseDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsModificationRequired { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }


        public virtual ProductSample ProductSample { get; set; } 
    
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
