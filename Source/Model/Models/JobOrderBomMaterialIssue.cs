using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderBomMaterialIssue : EntityBase
    {
        [Key]
        public int JobOrderBomMaterialIssueId { get; set; }


        [ForeignKey("JobOrderBom")]
        public int? JobOrderBomId { get; set; }
        public virtual JobOrderBom JobOrderBom { get; set; }

        [ForeignKey("StockLine"),Display(Name= "StockLine")]
        public int StockLineId { get; set; }
        public virtual StockLine StockLine { get; set; }
        public decimal IssueForQty { get; set; }
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
