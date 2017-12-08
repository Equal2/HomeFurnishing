using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModel
{
    public class PurchaseGoodsReceiptHeaderViewModel
    {
        public int PurchaseGoodsReceiptHeaderId { get; set; }

        [Display(Name = "Goods Receipt Type"), Required]
        [Index("IX_PurchaseGoodsReceiptHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        
        [Display(Name = "Goods Receipt Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Goods Receipt No"), Required, MaxLength(20)]
        
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]              
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        
        [Display(Name = "Supplier Name"), Required,Range(1,int.MaxValue,ErrorMessage="Supplier field is required")]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        [Display(Name = "Godown"), Required,Range(1,int.MaxValue,ErrorMessage="Godown field is required")]
        public int GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Supplier Doc. No."), MaxLength(20)]
        public string SupplierDocNo { get; set; }

        public int? PurchaseWaybillId { get; set; }
        public string PurchaseWaybillName { get; set; }

        [Display(Name = "Supplier Doc. Date")]
        public DateTime? SupplierDocDate { get; set; }

        [Display(Name = "Gate In No.")]
        public int? GateInId { get; set; }
        public string GateInName{ get; set; }
        public int? RoadPermitFormId { get; set; }
        public string RoadPermitFormName { get; set; }
        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public byte ? UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public PurchaseGoodsReceiptSettingsViewModel PurchaseGoodsReceiptSettings { get; set; }
        public string LockReason { get; set; }
    }


    public class PurchaseGoodsReceiptIndexViewModel
    {

        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocTypeName { get; set; }
        public string SupplierDocNo { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string FirstName { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
    }
    public class PurchaseGoodsReceiptListViewModel
    {
        public int PurchaseGoodsReceiptHeaderId { get; set; }
        public int PurchaseGoodsReceiptLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string ProductUidName { get; set; }
    }

}
