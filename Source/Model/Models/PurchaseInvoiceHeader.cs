using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseInvoiceHeader : EntityBase, IHistoryLog
    {
        public PurchaseInvoiceHeader()
        {            
            PurchaseInvoiceLines = new List<PurchaseInvoiceLine>();
        }

        [Key]        
        public int PurchaseInvoiceHeaderId { get; set; }
                        
        [Display(Name = "Invoice Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseInvoiceHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Invoice Date"),Required ]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Invoice No"),Required,MaxLength(20) ]
        [Index("IX_PurchaseInvoiceHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_PurchaseInvoiceHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }


        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        public Decimal? ExchangeRate { get; set; }


        [Display(Name = "Currency"), Required]
        [ForeignKey("Currency")]        
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [MaxLength(20), Display(Name = "Supplier Doc No")]
        public string SupplierDocNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime SupplierDocDate { get; set; }

        public int CreditDays { get; set; }


        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_PurchaseInvoiceHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        //public int PurchaseGoodsReceiptHeaderId { get; set; }

        [ForeignKey("Supplier")]
        [Display(Name = "Supplier Name")]
        public int SupplierId { get; set; }        
        public virtual Supplier Supplier { get; set; }

        [ForeignKey("BillingAccount")]
        public int BillingAccountId { get; set; }
        public virtual Supplier BillingAccount { get; set; }


        [ForeignKey("SalesTaxGroup")]
        public int? SalesTaxGroupId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroup { get; set; }

        [ForeignKey("SalesTaxGroupParty")]
        public int? SalesTaxGroupPartyId { get; set; }
        public virtual SalesTaxGroupParty SalesTaxGroupParty { get; set; }


        public string TermsAndConditions { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For Type")]
        public byte ? UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }


        [ForeignKey("PurchaseGoodsReceiptHeader")]
        [Display(Name = "Purchase Goods Receipt No.")]
        public int? PurchaseGoodsReceiptHeaderId { get; set; }
        public virtual PurchaseGoodsReceiptHeader PurchaseGoodsReceiptHeader { get; set; }

        [Display(Name = "Delivery Terms")]
        [ForeignKey("DeliveryTerms")]
        public int? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        [Display(Name = "Ship Method")]
        [ForeignKey("ShipMethod")]
        public int? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        public bool CalculateDiscountOnRate { get; set; }        

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }



        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<PurchaseInvoiceLine> PurchaseInvoiceLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
