using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class LedgerHeader : EntityBase, IHistoryLog
    {
        public LedgerHeader()
        {
            
        }

        [Key]
        [Display(Name = "Ledger Header Id")]
        public int LedgerHeaderId { get; set; }

        public int? DocHeaderId { get; set; }
                        
        [Display(Name = "Doc Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_LedgerHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Doc Date"),Required ]        
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }

        [Display(Name = "Payment For")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PaymentFor { get; set; }

        [Display(Name = "Doc No"),Required,MaxLength(20) ]
        [Index("IX_LedgerHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_LedgerHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_LedgerHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "LedgerAccount")]
        [ForeignKey("LedgerAccount")]
        public int? LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("CostCenter")]
        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }
        public int? CreditDays { get; set; }

        [ForeignKey("Godown")]
        public int ? GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        public string PartyDocNo { get; set; }

        public string  Narration { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int Status { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }




        [MaxLength(20)]
        public string AdjustmentType { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
