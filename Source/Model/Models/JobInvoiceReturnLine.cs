using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceReturnLine : EntityBase, IHistoryLog
    {

        [Key]        
        public int JobInvoiceReturnLineId { get; set; }

        [Display(Name = "Job Invoice Return"), Required]
        [ForeignKey("JobInvoiceReturnHeader")]
        public int JobInvoiceReturnHeaderId { get; set; }
        public virtual JobInvoiceReturnHeader  JobInvoiceReturnHeader { get; set; }

        [Display(Name = "Job Invoice"), Required]
        [ForeignKey("JobInvoiceLine")]
        public int JobInvoiceLineId { get; set; }
        public virtual JobInvoiceLine JobInvoiceLine { get; set; }

        [ForeignKey("JobReturnLine")]
        [Display(Name = "Job Return")]
        public int? JobReturnLineId { get; set; }
        public virtual JobReturnLine JobReturnLine { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Deal Qty"), Required]
        public Decimal DealQty { get; set; }


        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }


        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }



        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }

        [ForeignKey("SalesTaxGroupProduct"), Display(Name = "Sales Tax Group Product")]
        public int? SalesTaxGroupProductId { get; set; }
        public virtual ChargeGroupProduct SalesTaxGroupProduct { get; set; }


        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
