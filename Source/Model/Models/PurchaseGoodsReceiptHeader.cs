using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseGoodsReceiptHeader : EntityBase, IHistoryLog
    {
        public PurchaseGoodsReceiptHeader()
        {
            PurchaseGoodsReceiptLines = new List<PurchaseGoodsReceiptLine>();
        }

        [Key]        
        public int PurchaseGoodsReceiptHeaderId { get; set; }

                        
        [Display(Name = "Goods Receipt Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseGoodsReceiptHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]                
        [Display(Name = "Goods Receipt Date"),Required ]
        public DateTime DocDate { get; set; }
        
        [Display(Name = "Goods Receipt No"),Required,MaxLength(20) ]
        [Index("IX_PurchaseGoodsReceiptHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_PurchaseGoodsReceiptHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_PurchaseGoodsReceiptHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Supplier")]
        [Display(Name = "Supplier Name"),Required]
        public int SupplierId { get; set; }        
        public virtual Person Supplier { get; set; }

        [Display(Name = "Godown"),Required]
        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [Display(Name = "Supplier Doc. No."), MaxLength(20)]
        public string SupplierDocNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Supplier Doc. Date")]
        public DateTime? SupplierDocDate { get; set; }


        [Display(Name = "Way Bill No.")]
        [ForeignKey("PurchaseWaybill")]
        public int? PurchaseWaybillId { get; set; }
        public virtual PurchaseWaybill PurchaseWaybill { get; set; }

        [Display(Name = "Gate In No.")]
        [ForeignKey("GateInNo")]
        public int ? GateInId { get; set; }
        public virtual GateIn GateInNo { get; set; }


        [Display(Name = "Road Permit Form")]
        [ForeignKey("RoadPermitForm")]
        public int? RoadPermitFormId { get; set; }
        public virtual ProductUid RoadPermitForm { get; set; }


        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }






        public int Status { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }


        [ForeignKey("UnitConversionFor")]
        [Display(Name = "Unit Conversion For Type")]
        public byte ? UnitConversionForId { get; set; }
        public virtual UnitConversionFor UnitConversionFor { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<PurchaseGoodsReceiptLine> PurchaseGoodsReceiptLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
