using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PackingLine : EntityBase, IHistoryLog
    {
        public PackingLine()
        {
        }

        [Key]
        public int PackingLineId { get; set; }

        [Display(Name = "Packing"), Required]
        [ForeignKey("PackingHeader")]
        public int PackingHeaderId { get; set; }
        public virtual PackingHeader PackingHeader { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }


        [Display(Name = "Sale Order")]
        [ForeignKey("SaleOrderLine")]
        public int? SaleOrderLineId { get; set; }
        public virtual SaleOrderLine SaleOrderLine { get; set; }

        [Display(Name = "Sale Delivery Order")]
        [ForeignKey("SaleDeliveryOrderLine")]
        public int? SaleDeliveryOrderLineId { get; set; }
        public virtual SaleDeliveryOrderLine SaleDeliveryOrderLine { get; set; }

        [Display(Name = "Person ProductUid")]
        public int? PersonProductUidId { get; set; }
        public virtual PersonProductUid PersonProductUid { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Loss Qty")]
        public Decimal? LossQty { get; set; }

        [Display(Name = "Pass Qty")]
        public Decimal? PassQty { get; set; }

        [Display(Name = "Free Qty")]
        public Decimal? FreeQty { get; set; }



        [Display(Name = "Bale No."), MaxLength(10)]
        public string BaleNo { get; set; }

        [Display(Name = "Lot No."), MaxLength(50)]
        public string LotNo { get; set; }

        [Display(Name = "Bale Count")]
        public int? BaleCount { get; set; }

        [ForeignKey("FromProcess"), Display(Name = "From Process")]
        public int? FromProcessId { get; set; }
        public virtual Process FromProcess { get; set; }

        [Display(Name = "Party Product Uid"), MaxLength(50)]
        public string PartyProductUid { get; set; }

        [Display(Name = "Deal Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Gross Wt."), Required]
        public Decimal GrossWeight { get; set; }

        [Display(Name = "Net Wt."), Required]
        public Decimal NetWeight { get; set; }

        [Display(Name = "StockIn")]
        [ForeignKey("StockIn")]
        public int? StockInId { get; set; }
        public virtual Stock StockIn { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Seal No")]
        public string SealNo { get; set; }

        [Display(Name = "Rate Remark")]
        public string RateRemark { get; set; }

        public string Specification { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Stock Issue")]
        [ForeignKey("StockIssue")]
        public int? StockIssueId { get; set; }
        public virtual Stock StockIssue { get; set; }

        [Display(Name = "Stock Receive")]
        [ForeignKey("StockReceive")]
        public int? StockReceiveId { get; set; }
        public virtual Stock StockReceive { get; set; }


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
