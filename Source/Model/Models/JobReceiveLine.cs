using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobReceiveLine : EntityBase, IHistoryLog, IProductUidLastStatus
    {
        public JobReceiveLine()
        {
            JobInvoiceLines = new List<JobInvoiceLine>();
        }

        [Key]
        public int JobReceiveLineId { get; set; }

        [Display(Name = "Job Receive"), Required]
        [ForeignKey("JobReceiveHeader")]
        [Index("IX_JobReceiveLine_Unique", IsUnique = true, Order = 1)]
        public int JobReceiveHeaderId { get; set; }
        public virtual JobReceiveHeader JobReceiveHeader { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        [Index("IX_JobReceiveLine_Unique", IsUnique = true, Order = 3)]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

        public string Specification { get; set; }

        [Display(Name = "Job Order")]
        [ForeignKey("JobOrderLine")]
        [Index("IX_JobReceiveLine_Unique", IsUnique = true, Order = 2)]
        public int? JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }

        [Display(Name = "Qty"), Required]
        public decimal Qty { get; set; }

        [Display(Name = "Pass Qty")]
        public decimal PassQty { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal DealQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Machine")]
        public int? MachineId { get; set; }

        [Display(Name = "Loss %")]
        public decimal LossPer { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal LossQty { get; set; }

        [Display(Name = "Penalty Amount")]
        public Decimal PenaltyAmt { get; set; }       

        [Display(Name = "Penalty Rate")]
        public Decimal PenaltyRate { get; set; }

        [Display(Name = "Incentive Rate")]
        public Decimal IncentiveRate { get; set; }

        [Display(Name = "Incentive Amount")]
        public Decimal? IncentiveAmt { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        [Index("IX_JobReceiveLine_Unique", IsUnique = true, Order = 4)]
        public string LotNo { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public decimal Weight { get; set; }


        [Display(Name = "Stock")]
        [ForeignKey("Stock")]
        public int? StockId { get; set; }
        public virtual Stock Stock { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }


        [Display(Name = "Product UID Header")]
        [ForeignKey("ProductUidHeader")]
        public int? ProductUidHeaderId { get; set; }
        public virtual ProductUidHeader ProductUidHeader { get; set; }


        [Display(Name = "StockProcess")]
        [ForeignKey("StockProcess")]
        public int? StockProcessId { get; set; }
        public virtual StockProcess StockProcess { get; set; }

        [Display(Name = "Penalty Reason")]
        public string PenaltyReason { get; set; }

        public ICollection<JobInvoiceLine> JobInvoiceLines { get; set; }
        public DateTime? MfgDate { get; set; }
        public int? Sr { get; set; }

        [Display(Name = "Bale No."), MaxLength(10)]
        public string BaleNo { get; set; }


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
