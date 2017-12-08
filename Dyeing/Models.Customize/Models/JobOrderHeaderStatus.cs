using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderHeaderStatus : EntityBase
    {
        [Key]
        [ForeignKey("JobOrderHeader")]
        [Display(Name = "JobOrderHeader")]
        public int? JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }
        public Decimal? BOMQty { get; set; }
        public bool ? IsTimeIncentiveProcessed { get; set; }
        public Decimal? ReceiveQty { get; set; }
        public Decimal? ReceiveDealQty { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public Decimal? CancelQty { get; set; }
        public Decimal? CancelDealQty { get; set; }
        public DateTime? CancelDate { get; set; }
        public bool ? IsTimePenaltyProcessed { get; set; }
        public bool? IsSmallChunkPenaltyProcessed { get; set; }
        public int ? TimePenaltyCount { get; set; }
        public int ? JobReceiveCount { get; set; }
        public int ? TimeIncentiveId { get; set; }
        public int Status { get; set; }
    }
}
