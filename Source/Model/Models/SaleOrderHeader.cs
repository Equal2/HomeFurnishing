using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleOrderHeader : EntityBase, IHistoryLog
    {
        public SaleOrderHeader()
        {
            SaleOrderLines = new List<SaleOrderLine>();
        }
        [Key]
        public int SaleOrderHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name="Order Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20)]
        public string DocNo { get; set; }

        [ForeignKey("Division"),Display(Name="Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"),Display(Name="Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [MaxLength(20), Display(Name = "Buyer Order Number")]
        public string BuyerOrderNo { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ActualDueDate { get; set; }

        [ForeignKey("SaleToBuyer"),Display(Name="Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public virtual Person SaleToBuyer { get; set; }

        [ForeignKey("BillToBuyer"),Display(Name="Bill To Buyer")]
        public int BillToBuyerId { get; set; }
        public virtual Buyer BillToBuyer { get; set; }

        [ForeignKey("Currency"),Display(Name="Currency")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public int Priority { get; set; }

        [ForeignKey("ShipMethod"),Display(Name="Ship Method"),Required]
        public int ? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        [ForeignKey("Godown"), Display(Name = "Godown")]
        public int? GodownId { get; set; }
        public virtual Godown Godown { get; set; }


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

        [ForeignKey("Financier")]
        public int? FinancierId { get; set; }
        public virtual Person Financier { get; set; }

        [ForeignKey("SalesExecutive")]
        public int? SalesExecutiveId { get; set; }
        public virtual Person SalesExecutive { get; set; }


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

        public ICollection<SaleOrderLine> SaleOrderLines { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }


        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}

