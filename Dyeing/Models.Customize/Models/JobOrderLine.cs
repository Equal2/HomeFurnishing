using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Customize.Models
{
    public class JobOrderLine : EntityBase, IHistoryLog, IProductUidLastStatus
    {
        [Key]
        public int JobOrderLineId { get; set; }

        [ForeignKey("JobOrderHeader")]
        public int JobOrderHeaderId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int ? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [ForeignKey("Product"), Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [ForeignKey("ProdOrderLine"), Display(Name = "ProdOrder")]
        public int ? ProdOrderLineId { get; set; }
        public virtual ProdOrderLine ProdOrderLine { get; set; }

        [ForeignKey("Dimension1"),Display(Name="Dimension1")]
        public int ? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"),Display(Name="Dimension2")]
        public int ? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [MaxLength(50)]
        public string Specification { get; set; }

        public decimal Qty { get; set; }

        public DateTime ? DueDate { get; set; }

        [MaxLength(50)]
        public string  LotNo { get; set; }

        [ForeignKey("FromProcess"), Display(Name = "From Process")]
        public int ? FromProcessId { get; set; }
        public virtual Process FromProcess { get; set; }

        [ForeignKey("Unit"), Display(Name = "Unit")]
        public string UnitId { get; set; }
        public virtual Unit Unit { get; set; }

        [ForeignKey("DealUnit"), Display(Name = "Deal Unit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Deal Qty")]
        public decimal DealQty { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal UnitConversionMultiplier { get; set; }

        [Display(Name = "Non Counted Qty")]
        public decimal NonCountedQty { get; set; }

        [Display(Name = "Loss Qty")]
        public decimal? LossQty { get; set; }

        public decimal Rate { get; set; }
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Stock")]
        [ForeignKey("Stock")]
        public int? StockId { get; set; }
        public virtual Stock Stock { get; set; }

        [Display(Name = "Stock Process")]
        [ForeignKey("StockProcess")]
        public int? StockProcessId { get; set; }
        public virtual StockProcess StockProcess { get; set; }

        [Display(Name = "Product UID Header")]
        [ForeignKey("ProductUidHeader")]
        public int? ProductUidHeaderId { get; set; }
        public virtual ProductUidHeader ProductUidHeader { get; set; }

        public int? Sr { get; set; }


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
