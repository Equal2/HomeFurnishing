using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductSampleApproval : EntityBase, IHistoryLog
    {
        public ProductSampleApproval()
        {          
        }

        [Key]
        public int ProductSampleApprovalId { get; set; }

        [Display(Name = "Response Date")]
        public DateTime ResponseDate { get; set; }
        public bool IsApproved { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public virtual ProductSampleShipment ProductSampleShipment { get; set; }

        [NotMapped]
        public int ProductSampleShipmentId { get; set; }

        [ForeignKey("Product"),Display(Name="Product")]
        public int ? Product_ProductId { get; set; }
        public virtual Product Product { get; set; } 
    
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
