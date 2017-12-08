using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RequisitionLineStatus :EntityBase
    {
        [Key]
        [ForeignKey("RequisitionLine")]
        [Display(Name = "RequisitionLine")]
        public int? RequisitionLineId { get; set; }
        public virtual RequisitionLine RequisitionLine { get; set; }

        public Decimal? CancelQty { get; set; }

        public DateTime? CancelDate { get; set; }

        public Decimal? AmendmentQty { get; set; }

        public DateTime? AmendmentDate { get; set; }

        public Decimal? IssueQty { get; set; }

        public Decimal? ReceiveQty { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ReceiveDate { get; set; }


    }
}
