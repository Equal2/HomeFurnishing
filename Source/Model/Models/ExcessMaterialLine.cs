using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ExcessMaterialLine : EntityBase, IHistoryLog, IProductUidLastStatus
    {
        public ExcessMaterialLine()
        {
          
        }

        [Key]        
        public int ExcessMaterialLineId { get; set; }

        [Display(Name = "Purchase Indent")]
        [ForeignKey("ExcessMaterialHeader")]
        public int ExcessMaterialHeaderId { get; set; }
        public virtual ExcessMaterialHeader  ExcessMaterialHeader { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public virtual ProductUid ProductUid { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int ? ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name="Remark")]
        public string Remark { get; set; }    

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

        public int? Sr { get; set; }

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
