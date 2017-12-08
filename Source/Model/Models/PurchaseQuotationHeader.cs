using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PurchaseQuotationHeader : EntityBase, IHistoryLog
    {
        public PurchaseQuotationHeader()
        {
            PurchaseQuotationLines = new List<PurchaseQuotationLine>();
        }

        [Key]
        [Display(Name = "Purchase Quotation Id")]
        public int PurchaseQuotationHeaderId { get; set; }
                        
        [Display(Name = "Quotation Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseQuotationHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
                
        [Display(Name = "Quotation Date"),Required ]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Quotation No"),Required,MaxLength(20) ]
        [Index("IX_PurchaseQuotationHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_PurchaseQuotationHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division {get; set;}

        [ForeignKey("Site"), Display(Name = "Site")]
        [Index("IX_PurchaseQuotationHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Supplier"), Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        [ForeignKey("BillingAccount")]
        public int BillingAccountId { get; set; }
        public virtual Supplier BillingAccount { get; set; }

        [ForeignKey("Currency"), Display(Name = "Currency")]
        public int ? CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "Vendor Quotation No"), Required, MaxLength(20)]
        public string VendorQuotationNo { get; set; }

        [Display(Name = "Vendor Quotation Date"), Required]
        public DateTime VendorQuotationDate { get; set; }

        [Display(Name = "Terms & Conditions")]
        public string TermsAndConditions { get; set; }

        public Decimal? ExchangeRate { get; set; }

        public int CreditDays { get; set; }

        [ForeignKey("SalesTaxGroup")]
        public int? SalesTaxGroupId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroup { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For Type")]
        public byte? UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

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

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        public ICollection<PurchaseQuotationLine> PurchaseQuotationLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
