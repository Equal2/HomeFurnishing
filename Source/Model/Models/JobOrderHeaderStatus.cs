using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderHeaderStatus : EntityBase
    {
        [Key]
        [ForeignKey("JobOrderHeader")]
        [Display(Name = "JobOrderHeader")]
        public int? JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }

        //public Decimal? MaterialIssueQty { get; set; }

        //public DateTime MaterialIssueDate { get; set; }

        //public Decimal? MaterialReturnQty { get; set; }

        //public DateTime MaterialReturnDate { get; set; }

        public Decimal? BOMQty { get; set; }

        public bool ? IsTimeIncentiveProcessed { get; set; }

        public Decimal? ReceiveQty { get; set; }
        public Decimal? ReceiveDealQty { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public Decimal? CancelQty { get; set; }
        public Decimal? CancelDealQty { get; set; }
        public DateTime? CancelDate { get; set; }
        public int? JobCancelCount { get; set; }

        public bool ? IsTimePenaltyProcessed { get; set; }
        public bool? IsSmallChunkPenaltyProcessed { get; set; }
        public int ? TimePenaltyCount { get; set; }
        public int ? JobReceiveCount { get; set; }
        public int ? TimeIncentiveId { get; set; }

        public int Status { get; set; }
    }
}
