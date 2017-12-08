using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderHeader : EntityBase, IHistoryLog
    {
        public JobOrderHeader()
        {
            JobOrderLines = new List<JobOrderLine>();
        }
        [Key]
        public int JobOrderHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Document Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20)]
        public string DocNo { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ActualDueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ActualDocDate { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "Job Worker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [ForeignKey("BillToParty"), Display(Name = "Job Worker")]
        public int BillToPartyId { get; set; }
        public virtual JobWorker BillToParty { get; set; }

        [ForeignKey("OrderBy"), Display(Name = "Order By")]
        public int? OrderById { get; set; }
        public virtual Employee OrderBy { get; set; }

        [ForeignKey("Godown"), Display(Name = "Godown")]
        public int? GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [ForeignKey("Process"), Display(Name = "Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [ForeignKey("Machine"), Display(Name = "Machine")]
        public int? MachineId { get; set; }
        public virtual ProductUid Machine { get; set; }


        [ForeignKey("Financier")]
        public int? FinancierId { get; set; }
        public virtual Person Financier { get; set; }

        [ForeignKey("SalesExecutive")]
        public int? SalesExecutiveId { get; set; }
        public virtual Person SalesExecutive { get; set; }



        public string TermsAndConditions { get; set; }

        [Display(Name = "CreditDays")]
        public int? CreditDays { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }


        [ForeignKey("GatePassHeader")]
        [Display(Name = "Gatepass No.")]
        public int? GatePassHeaderId { get; set; }
        public virtual GatePassHeader GatePassHeader { get; set; }


        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }


        public int Status { get; set; }
        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For Type")]
        public byte? UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<JobOrderLine> JobOrderLines { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        public int Priority { get; set; }

        public bool ? IsGatePassPrinted { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
        public int ReferentialCheckSum { get; set; }


        [Display(Name = "Delivery Terms")]
        [ForeignKey("DeliveryTerms")]
        public int? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        [Display(Name = "Shipe Address")]
        [ForeignKey("ShipToAddress")]
        public int? ShipToAddressId { get; set; }
        public virtual PersonAddress ShipToAddress { get; set; }

        [ForeignKey("Currency"), Display(Name = "Currency")]
        public int? CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }



        [ForeignKey("SalesTaxGroupPerson")]
        public int? SalesTaxGroupPersonId { get; set; }
        public virtual ChargeGroupPerson SalesTaxGroupPerson { get; set; }


        [Display(Name = "Ship Method")]
        [ForeignKey("ShipMethod")]
        public int? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }


        [Display(Name = "Document Ship Method")]
        [ForeignKey("DocumentShipMethod")]
        public int? DocumentShipMethodId { get; set; }
        public virtual DocumentShipMethod DocumentShipMethod { get; set; }




        [Display(Name = "Transporter")]
        [ForeignKey("Transporter")]
        public int? TransporterId { get; set; }
        public virtual Person Transporter { get; set; }

        public bool? IsDoorDelivery { get; set; }


        [Display(Name = "Agent")]
        [ForeignKey("Agent")]
        public int? AgentId { get; set; }
        public virtual Person Agent { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }

        public Decimal? PayTermAdvancePer { get; set; }
        public Decimal? PayTermOnDeliveryPer { get; set; }
        public Decimal? PayTermOnDueDatePer { get; set; }
        public Decimal? PayTermCashPer { get; set; }
        public Decimal? PayTermBankPer { get; set; }
    }
}
