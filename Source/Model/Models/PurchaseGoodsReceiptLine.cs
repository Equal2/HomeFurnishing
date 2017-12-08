using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseGoodsReceiptLine : EntityBase, IHistoryLog, IProductUidLastStatus
    {
        public PurchaseGoodsReceiptLine()
        {
            //PurchaseInvoiceLines = new List<PurchaseInvoiceLine>();
        }

        [Key]
        public int PurchaseGoodsReceiptLineId { get; set; }

        [Display(Name = "Purchase Goods Receipt"), Required]
        [ForeignKey("PurchaseGoodsReceiptHeader")]
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public virtual PurchaseGoodsReceiptHeader  PurchaseGoodsReceiptHeader { get; set; }



        [Display(Name = "Purchase Order")]
        [ForeignKey("PurchaseOrderLine")]
        public int? PurchaseOrderLineId { get; set; }
        public virtual PurchaseOrderLine PurchaseOrderLine { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("PurchaseIndentLine")]
        public int? PurchaseIndentLineId { get; set; }
        public virtual PurchaseIndentLine PurchaseIndentLine { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int ? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [Display(Name = "Lot No."), MaxLength(20)]
        public string LotNo { get; set; }

        [Display(Name = "Doc. Qty")]
        public Decimal DocQty { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "Debit Note Amount"), Required]
        public Decimal DebitNoteAmount { get; set; }

        [Display(Name = "Debit Note Reason"), MaxLength(50)]
        public string DebitNoteReason { get; set; }


        public bool? isUninspected { get; set; }
        

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Delivery Unit"), Required]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }

        [Display(Name = "Bale No."), MaxLength(10)]
        public string BaleNo { get; set; }

        [Display(Name = "Stock")]
        [ForeignKey("Stock")]
        public int? StockId { get; set; }
        public virtual Stock Stock { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocLineId { get; set; }


        public int? Sr { get; set; }
       
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public string Remark { get; set; }
        [MaxLength(50)]
        public string Specification { get; set; }

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
