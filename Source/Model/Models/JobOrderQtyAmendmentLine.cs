using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderQtyAmendmentLine : EntityBase, IHistoryLog
    {
        [Key]
        public int JobOrderQtyAmendmentLineId { get; set; }

        [ForeignKey("JobOrderAmendmentHeader")]
        public int JobOrderAmendmentHeaderId { get; set; }
        public virtual JobOrderAmendmentHeader JobOrderAmendmentHeader { get; set; }

        [ForeignKey("JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }       

        public decimal Qty { get; set; }

        public int? Sr { get; set; }

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
