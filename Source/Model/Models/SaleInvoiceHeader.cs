using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleInvoiceHeader : EntityBase, IHistoryLog
    {
        public SaleInvoiceHeader()
        {            
            SaleInvoiceLines = new List<SaleInvoiceLine>();
        }

        [Key]        
        public int SaleInvoiceHeaderId { get; set; }
                        
        [Display(Name = "Invoice Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_SaleInvoiceHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Invoice Date"),Required ]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Invoice No"),Required,MaxLength(20) ]
        [Index("IX_SaleInvoiceHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_SaleInvoiceHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_SaleInvoiceHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }


        [ForeignKey("SaleToBuyer"), Display(Name = "Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public virtual Person SaleToBuyer { get; set; }

        [ForeignKey("BillToBuyer"), Display(Name = "Bill To Buyer")]
        public int BillToBuyerId { get; set; }
        public virtual Buyer BillToBuyer { get; set; }


        [ForeignKey("Agent"), Display(Name = "Agent")]
        public int ? AgentId { get; set; }
        public virtual Person Agent { get; set; }


        [ForeignKey("Currency")]
        [Display(Name = "Currency")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public Decimal? ExchangeRate { get; set; }

        public Decimal? CreditLimit { get; set; }
        public Decimal? CreditDays { get; set; }

        public Decimal? CurrentBalance { get; set; }

        public int Status { get; set; }

        [ForeignKey("Financier")]
        public int? FinancierId { get; set; }
        public virtual Person Financier { get; set; }

        [ForeignKey("SalesExecutive")]
        public int? SalesExecutiveId { get; set; }
        public virtual Person SalesExecutive { get; set; }

        [ForeignKey("SalesTaxGroupPerson")]
        [Display(Name = "SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [ForeignKey("SaleDispatchHeader")]
        public int ? SaleDispatchHeaderId { get; set; }
        public virtual SaleDispatchHeader SaleDispatchHeader { get; set; }
        public bool CalculateDiscountOnRate { get; set; }

        public string TermsAndConditions { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<SaleInvoiceLine> SaleInvoiceLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
