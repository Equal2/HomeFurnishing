using Model;
using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class CostCenter : EntityBase, IHistoryLog
    {

        [Key]
        public int CostCenterId { get; set; }

        [Display(Name = "Name")]
        [MaxLength(50), Required]
        [Index("IX_CostCenter_CostCenterName", IsUnique = true)]
        public string CostCenterName { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int? DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int? DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int? SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int Status { get; set; }

        [ForeignKey("LedgerAccount"), Display(Name = "Ledger Account")]
        public int? LedgerAccountId { get; set; }
        public virtual LedgerAccount LedgerAccount { get; set; }


        [ForeignKey("ParentCostCenter"), Display(Name = "Parent Cost Center")]
        public int? ParentCostCenterId { get; set; }
        public virtual CostCenter ParentCostCenter { get; set; }

        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Is System Define ?")]
        public Boolean IsSystemDefine { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "Order Type")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        [Display(Name = "Document Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Document Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? CloseDate { get; set; }

        public int? ReferenceDocId { get; set; }
        public string ReferenceDocNo { get; set; }

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

        [InverseProperty("CostCenter")]
        public ICollection<Process> ProcessCollection { get; set; }


    }
}
