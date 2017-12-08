using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleOrderCancelHeader : EntityBase, IHistoryLog
    {
        public SaleOrderCancelHeader()
        {
            SaleOrderCancelLines = new List<SaleOrderCancelLine>();
        }

        [Key]
        public int SaleOrderCancelHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Cancel Type")]
        [Index("IX_SaleOrderCancelHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Cancel Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [MaxLength(20)]
        [Index("IX_SaleOrderCancelHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [ForeignKey("Division")]
        [Index("IX_SaleOrderCancelHeader_DocID", IsUnique = true, Order = 3)]
        public int? DivisionId { get; set; }
        public virtual Division Division { get; set; }
        
        [ForeignKey("Buyer")]
        public int BuyerId { get; set; }
        public virtual Buyer Buyer { get; set; }

        [ForeignKey("Site")]
        [Index("IX_SaleOrderCancelHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public int Status { get; set; }

        [MinLength(30),Required]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<SaleOrderCancelLine> SaleOrderCancelLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
