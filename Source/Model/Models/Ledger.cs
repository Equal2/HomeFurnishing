using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class Ledger : EntityBase
    {
        public Ledger()
        {
        }

        [Key]        
        public int LedgerId { get; set; }

        [Display(Name = "Ledger Header"), Required]
        [ForeignKey("LedgerHeader")]
        public int LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        [Display(Name = "Ledger Line")]
        [ForeignKey("LedgerLine")]
        public int? LedgerLineId { get; set; }
        public virtual LedgerLine LedgerLine { get; set; }


        [Display(Name = "LedgerAccount"), Required]
        [ForeignKey("LedgerAccount")]
        public int LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }

        [Display(Name = "Contra Ledger Account")]
        [ForeignKey("ContraLedgerAccount")]
        public int? ContraLedgerAccountId { get; set; }
        public virtual LedgerAccount ContraLedgerAccount { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [Display(Name = "AmtDr")]
        public Decimal AmtDr { get; set; }

        [Display(Name = "AmtCr")]
        public Decimal AmtCr { get; set; }

        [MaxLength(250)]
        public string Narration { get; set; }

        public DateTime? BankDate { get; set; }

        public string ContraText { get; set; }
        [Display(Name = "Chq No"), MaxLength(10)]
        public string ChqNo { get; set; }
        public DateTime? ChqDate { get; set; }

        [Display(Name = "DueDate")]
        public DateTime ? DueDate { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }



        [MaxLength(50)]
        public string OMSId { get; set; }

        
    }
}
