using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceLine : EntityBase, IHistoryLog
    {
        public JobInvoiceLine()
        {
            
        }

        [Key]
        public int JobInvoiceLineId { get; set; }

        [Display(Name = "Job Invoice"), Required]
        [ForeignKey("JobInvoiceHeader")]
        [Index("IX_JobInvoiceLine_Unique", IsUnique = true, Order = 1)]
        public int JobInvoiceHeaderId { get; set; }
        public virtual JobInvoiceHeader JobInvoiceHeader { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "Job Worker")]
        public int JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [Display(Name = "Job Receive"), Required]
        [ForeignKey("JobReceiveLine")]
        [Index("IX_JobInvoiceLine_Unique", IsUnique = true, Order = 2)]
        public int JobReceiveLineId { get; set; }
        public virtual JobReceiveLine JobReceiveLine { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Qty")]
        public decimal Qty { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal DealQty { get; set; }
        public decimal? RateDiscountPer { get; set; }
        public decimal? RateDiscountAmt { get; set; }
        
        
        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Incentive Rate")]
        public Decimal ? IncentiveRate { get; set; }

        [Display(Name = "Incentive Amount")]
        public Decimal ? IncentiveAmt { get; set; }
        public string Remark { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }



        [ForeignKey("SalesTaxGroupProduct"), Display(Name = "Sales Tax Group Product")]
        public int? SalesTaxGroupProductId { get; set; }
        public virtual ChargeGroupProduct SalesTaxGroupProduct { get; set; }


        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }



        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
