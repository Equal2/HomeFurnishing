using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderByProduct : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderByProductId { get; set; }

        [ForeignKey("JobOrderHeader")]
        public int JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }

        [ForeignKey("Product"),Display(Name="Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public decimal Qty { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
