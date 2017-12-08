using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SampleToProductMapping : EntityBase, IHistoryLog
    {
        public SampleToProductMapping()
        {          
        }

        [Key]
        public int SampleProductMappingId { get; set; }
        public Product Product { get; set; }
        public ProductSampleApproval ProductSampleApproval { get; set; } 

        [NotMapped]
        public int ProductId { get; set; }

        [NotMapped]
        public int ProductSampleApprovalId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
