using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSampleShipment : EntityBase, IHistoryLog
    {
        public ProductSampleShipment()
        {          
            ProductSampleApprovals = new List<ProductSampleApproval>();
        }

        [Key]
        public int ProductSampleShipmentId { get; set; }       
 
        [Display(Name = "Ship Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}",ApplyFormatInEditMode=true)]
        public DateTime ShipDate { get; set; }
        public string Specification { get; set; }

        public virtual ProductSamplePhotoApproval ProductSamplePhotoApproval { get; set; }

        public ICollection<ProductSampleApproval> ProductSampleApprovals { get; set; }

        [NotMapped]
        public int ProductSamplePhotoApprovalId { get; set; }       
 
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
