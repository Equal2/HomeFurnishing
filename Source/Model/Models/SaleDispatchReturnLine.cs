using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDispatchReturnLine : EntityBase, IHistoryLog
    {
        [Key]        
        public int SaleDispatchReturnLineId { get; set; }

        [Display(Name = "Purchase Goods Return"), Required]
        [ForeignKey("SaleDispatchReturnHeader")]
        public int SaleDispatchReturnHeaderId { get; set; }
        public virtual SaleDispatchReturnHeader  SaleDispatchReturnHeader { get; set; }

        [Display(Name = "Purchase Goods Receipt"), Required]
        [ForeignKey("SaleDispatchLine")]
        public int SaleDispatchLineId { get; set; }
        public virtual SaleDispatchLine SaleDispatchLine { get; set; }


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

        public Decimal? Weight { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Stock")]
        [ForeignKey("Stock")]
        public int? StockId { get; set; }
        public virtual Stock Stock { get; set; }

        [Display(Name = "Godown")]
        [ForeignKey("Godown")]
        public int? GodownId { get; set; }
        public virtual Godown Godown { get; set; }
        public int ? Sr { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Lock Reason Delete")]
        public string LockReasonDelete { get; set; }


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
