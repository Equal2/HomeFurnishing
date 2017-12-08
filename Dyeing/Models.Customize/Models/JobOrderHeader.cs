using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
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
        public virtual Person OrderBy { get; set; }

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
        public virtual Product Machine { get; set; }

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


        [MaxLength(50)]
        public string OMSId { get; set; }
        public int ReferentialCheckSum { get; set; }
    }
}
