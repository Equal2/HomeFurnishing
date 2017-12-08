using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class LedgerLine : EntityBase, IHistoryLog
    {
        public LedgerLine()
        {
        }

        [Key]        
        public int LedgerLineId { get; set; }

        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int LedgerHeaderId { get; set; }
        public virtual LedgerHeader  LedgerHeader { get; set; }

        [Display(Name = "LedgerAccount"), Required]
        [ForeignKey("LedgerAccount")]
        public int LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }

        public int? ReferenceId { get; set; }


        [Display(Name = "Chq No"), MaxLength(10)]
        public string ChqNo { get; set; }

        [Display(Name = "Chq Date")]
        public DateTime? ChqDate { get; set; }

        [ForeignKey("CostCenter")]
        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }


        [ForeignKey("PaymentMode")]
        [Display(Name = "Payment Mode")]
        public int? PaymentModeId { get; set; }
        public virtual PaymentMode PaymentMode { get; set; }

        [ForeignKey("Agent")]
        [Display(Name = "Agent")]
        public int? AgentId { get; set; }
        public virtual Person Agent { get; set; }

        public decimal BaseValue { get; set; }
        public decimal BaseRate { get; set; }

        public int ? ReferenceDocTypeId { get; set; }
        public int ? ReferenceDocId { get; set; }
        public int ? ReferenceDocLineId { get; set; }
        public Decimal Amount { get; set; }
        
        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "ReferenceLedgerAccount")]
        [ForeignKey("ReferenceLedgerAccount")]
        public int? ReferenceLedgerAccountId { get; set; }
        public virtual LedgerAccount ReferenceLedgerAccount { get; set; }

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
