using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleOrderAmendmentHeader : EntityBase, IHistoryLog
    {
        [Key]
        public int SaleOrderAmendmentHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Amendment Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public DateTime DocDate { get; set; }

        [MaxLength(10)]
        public string DocNo { get; set; }

        [ForeignKey("Division")]
        public int? DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        //[ForeignKey("Buyer")]
        //public int BuyerId { get; set; }
        //public virtual Buyer Buyer { get; set; }

        [ForeignKey("Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [ForeignKey("Buyer")]
        public int BuyerId { get; set; }
        public virtual Buyer Buyer { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

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
