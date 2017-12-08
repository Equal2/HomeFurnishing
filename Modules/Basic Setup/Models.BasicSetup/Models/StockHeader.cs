using Model;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class StockHeader : EntityBase, IHistoryLog
    {
        public StockHeader()
        {
        }

        [Key]
        [Display(Name = "Stock Header Id")]
        public int StockHeaderId { get; set; }

        public int? DocHeaderId { get; set; }
                        
        [Display(Name = "Doc Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_StockHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Doc Date"),Required ]        
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Doc No"),Required,MaxLength(20) ]
        [Index("IX_StockHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_StockHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_StockHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int? CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "Person")]
        [ForeignKey("Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "From Godown")]
        [ForeignKey("FromGodown")]
        public int? FromGodownId { get; set; }
        public virtual Godown FromGodown { get; set; }

        [Display(Name = "Godown")]
        [ForeignKey("Godown")]
        public int? GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [ForeignKey("GatePassHeader")]
        [Display(Name = "Gatepass No.")]
        public int? GatePassHeaderId { get; set; }
        public virtual GatePassHeader GatePassHeader { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [Display(Name = "Machine")]
        public int? MachineId { get; set; }

        [ForeignKey("LedgerHeader")]
        public int ? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        [Display(Name = "ContraLedgerAccount")]
        [ForeignKey("ContraLedgerAccount")]
        public int? ContraLedgerAccountId { get; set; }
        public virtual LedgerAccount ContraLedgerAccount { get; set; }


        [Display(Name = "LedgerAccount")]
        [ForeignKey("LedgerAccount")]
        public int? LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }
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

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }
        public int? ReferenceDocId { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
