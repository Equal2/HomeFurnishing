using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReturnLine : EntityBase, IHistoryLog, IProductUidLastStatus
    {
        [Key]
        public int JobReturnLineId { get; set; }

        public int JobReturnHeaderId { get; set; }

        [ForeignKey("JobReceiveLine")]
        public int JobReceiveLineId { get; set; }
        public virtual JobReceiveLine JobReceiveLine { get; set; }

        public decimal Qty { get; set; }

        [Display(Name = "Loss %")]
        public decimal LossPer { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal LossQty { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }


        [Display(Name = "Deal Qty")]
        public decimal DealQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public decimal Weight { get; set; }

        public int? Sr { get; set; }

        [Display(Name = "Stock")]
        [ForeignKey("Stock")]
        public int? StockId { get; set; }
        public virtual Stock Stock { get; set; }

        [Display(Name = "StockProcess")]
        [ForeignKey("StockProcess")]
        public int? StockProcessId { get; set; }
        public virtual StockProcess StockProcess { get; set; }



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

        /// <summary>
        /// ///////////////For Maintaining Product Uid Last Values///////////////////////////
        /// </summary>
        public int? ProductUidLastTransactionDocId { get; set; }

        public string ProductUidLastTransactionDocNo { get; set; }

        [ForeignKey("ProductUidLastTransactionDocType"), Display(Name = "Document Type")]
        public int? ProductUidLastTransactionDocTypeId { get; set; }
        public virtual DocumentType ProductUidLastTransactionDocType { get; set; }

        public DateTime? ProductUidLastTransactionDocDate { get; set; }

        [ForeignKey("ProductUidLastTransactionPerson"), Display(Name = "Last Transaction Person")]
        public int? ProductUidLastTransactionPersonId { get; set; }
        public virtual Person ProductUidLastTransactionPerson { get; set; }

        [ForeignKey("ProductUidCurrentGodown"), Display(Name = "Current Godown")]
        public int? ProductUidCurrentGodownId { get; set; }
        public virtual Godown ProductUidCurrentGodown { get; set; }

        [ForeignKey("ProductUidCurrentProcess"), Display(Name = "Current Process")]
        public int? ProductUidCurrentProcessId { get; set; }
        public virtual Process ProductUidCurrentProcess { get; set; }

        [MaxLength(10)]
        public string ProductUidStatus { get; set; }
    }
}
