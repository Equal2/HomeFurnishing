using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    public class PurchaseGoodsReceiptLineViewModel
    {

        public int PurchaseGoodsReceiptLineId { get; set; }

        [Display(Name = "Purchase Goods Receipt"), Required]
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public string  PurchaseGoodsReceiptHeaderDocNo { get; set; }

        [Display(Name = "Purchase Order")]        
        public int? PurchaseOrderLineId { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public int ? OrderDocTypeId { get; set; }
        public int ? OrderHeaderId { get; set; }
        
        public int? StockId { get; set; }

        [Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }

        [Display(Name = "Product"), Required]        
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SupplierId { get; set; }

        [Display(Name = "Dimension1")]
        public int ? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]        
        public int ? Dimension2Id { get; set; }
        public string Dimension2Name{ get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }

        [Display(Name = "OrderDealQty"), Required]
        public Decimal OrderDealQty { get; set; }

        [Display(Name = "OrderQty"), Required]
        public Decimal OrderQty { get; set; }

        [Display(Name = "Doc. Qty")]
        public Decimal DocQty { get; set; }

        public bool? isUninspected { get; set; }      

        [Display(Name = "Delivery Unit"), Required]       
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Delivery Qty"), Required]
        public Decimal DealQty { get; set; }

        public string Remark { get; set; }
        [Display(Name = "Unit")]
        public string UnitId { get; set; }
        public string UnitName { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal UnitConversionMultiplier { get; set; }
        [Display(Name = "Bale No."), MaxLength(10)]
        public string BaleNo { get; set; }
        [MaxLength(50)]
        public string Specification { get; set; }
        public decimal OrderBalanceQty { get; set; }

        public Decimal? DiscountPer { get; set; }
        public decimal ? Rate { get; set; }
        public decimal ? Amount { get; set; }
        public int PurchaseInvoiceHeaderId { get; set; }
        public int GodownId { get; set; }

        [Display(Name = "Debit Note Amount"), Required]
        public Decimal DebitNoteAmount { get; set; }

        [Display(Name = "Debit Note Reason"), MaxLength(50)]
        public string DebitNoteReason { get; set; }
        public PurchaseGoodsReceiptSettingsViewModel PurchGoodsReceiptSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public string LockReason { get; set; }
    }

    public class PurchaseGoodsReceiptLineFilterViewModel
    {
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Purchase Order No")]
        public string PurchaseOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
    public class PurchaseGoodsReceiptMasterDetailModel
    {
        public List<PurchaseGoodsReceiptLineViewModel> PurchaseGoodsReceiptLineViewModel { get; set; }
        public PurchaseGoodsReceiptSettingsViewModel PurchGoodsReceiptSettings { get; set; }
    }

}
