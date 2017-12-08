using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleEnquiryHeader : EntityBase, IHistoryLog
    {
        public SaleEnquiryHeader()
        {
            SaleEnquiryLines = new List<SaleEnquiryLine>();
        }
        [Key]
        public int SaleEnquiryHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Enquiry Type")]
        [Index("IX_SaleEnquiryHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Enquiry Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Enquiry No"), MaxLength(20)]
        [Index("IX_SaleEnquiryHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [ForeignKey("Division"),Display(Name="Division")]
        [Index("IX_SaleEnquiryHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"),Display(Name="Site")]
        [Index("IX_SaleEnquiryHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [MaxLength(20), Display(Name = "Buyer Enquiry Number")]
        public string BuyerEnquiryNo { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ActualDueDate { get; set; }

        [ForeignKey("SaleToBuyer"),Display(Name="Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public virtual Person SaleToBuyer { get; set; }

        [ForeignKey("BillToBuyer"),Display(Name="Bill To Buyer")]
        public int BillToBuyerId { get; set; }
        public virtual Person BillToBuyer { get; set; }

        [ForeignKey("Currency"),Display(Name="Currency")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public int Priority { get; set; }

        [ForeignKey("ShipMethod"),Display(Name="Ship Method"),Required]
        public int ? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        [Display(Name = "Ship Address"), MaxLength(250)]
        public string ShipAddress { get; set; }

        [ForeignKey("DeliveryTerms"),Display(Name="Delivery Terms"),Required]
        public int ? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        public string TermsAndConditions { get; set; }

        public int CreditDays { get; set; }


        [Display(Name = "Ledger Header")]
        [ForeignKey("LedgerHeader")]
        public int? LedgerHeaderId { get; set; }
        public virtual LedgerHeader LedgerHeader { get; set; }

        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }


        public int Status { get; set; }

        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For")]        
        public byte UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }

        public Decimal? Advance { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<SaleEnquiryLine> SaleEnquiryLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}

