using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDeliveryOrderCancelHeader : EntityBase, IHistoryLog
    {

        [Key]
        public int SaleDeliveryOrderCancelHeaderId { get; set; }

        [ForeignKey("DocType")]
        [Index("IX_DeliveryOrderHeader_Unique", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime DocDate { get; set; }


        [Index("IX_DeliveryOrderHeader_Unique", IsUnique = true, Order = 2)]
        [MaxLength(10)]
        public string DocNo { get; set; }

        [Index("IX_DeliveryOrderHeader_Unique", IsUnique = true, Order = 3)]
        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Index("IX_DeliveryOrderHeader_Unique", IsUnique = true, Order = 4)]
        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [ForeignKey("Buyer")]
        [Display(Name = "Buyer Name")]
        public int BuyerId { get; set; }
        public virtual Buyer Buyer { get; set; }

        [ForeignKey("OrderBy"), Display(Name = "Order By")]
        public int OrderById { get; set; }
        public virtual Employee OrderBy { get; set; }


        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
    }
}
