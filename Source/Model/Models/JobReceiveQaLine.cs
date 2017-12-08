using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveQALine : EntityBase, IHistoryLog
    {

        [Key]
        public int JobReceiveQALineId { get; set; }

        [Display(Name = "QA"), Required]
        [ForeignKey("JobReceiveQAHeader")]
        public int JobReceiveQAHeaderId { get; set; }
        public virtual JobReceiveQAHeader JobReceiveQAHeader { get; set; }

        public int? Sr { get; set; }


        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [Display(Name = "Job Receive"), Required]
        [ForeignKey("JobReceiveLine")]
        public int JobReceiveLineId { get; set; }
        public virtual JobReceiveLine JobReceiveLine { get; set; }

        [Display(Name = "QA Qty")]
        public decimal QAQty { get; set; }

        [Display(Name = "Inspected Qty")]
        public decimal InspectedQty { get; set; }

        [Display(Name = "Pass Qty"), Required]
        public decimal Qty { get; set; }
        [Display(Name = "QAFail Qty")]
        public decimal FailQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Pass Deal Qty"), Required]
        public decimal DealQty { get; set; }

        [Display(Name = "Fail Deal Qty")]
        public decimal FailDealQty { get; set; }

        [Display(Name = "Pass Weight"), Required]
        public decimal Weight { get; set; }

        [Display(Name = "Penalty Rate"), Required]
        public decimal PenaltyRate { get; set; }

        [Display(Name = "Penalty Amt"), Required]
        public decimal PenaltyAmt { get; set; }

        [Range(0,100)]
        [Display(Name = "Marks")]
        public decimal Marks { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }


        [MaxLength(100)]
        public string ImageFolderName { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }


        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }


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

        [MaxLength(50)]
        public string OMSId { get; set; }


    }
}
