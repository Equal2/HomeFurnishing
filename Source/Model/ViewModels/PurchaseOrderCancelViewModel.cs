using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseOrderCancelHeaderViewModel
    {
        [Key]
        public int PurchaseOrderCancelHeaderId { get; set; }

        [Display(Name = "Cancel Type"), Required]
        public int DocTypeId { get; set; }
        public string DocTypeName{ get; set; }

        [Display(Name = "Cancel Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Cancel No"), Required, MaxLength(20, ErrorMessage = "{0} can not exceed {1} characters")]        
        public string DocNo { get; set; }

        [Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public string ReasonName{ get; set; }

        [Display(Name = "Division"), Required]        
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]        
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        
        [Display(Name = "Supplier Name")]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }


        [Display(Name = "Remark"),Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public PurchaseOrderSettingsViewModel PurchaseOrderSettings { get; set; }

    }

    public class PurchaseOrderCancelHeaderIndexViewModel
    {
        public int PurchaseOrderCancelHeaderId { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string SupplierName { get; set; }
        public string ReasonName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string FirstName { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
    }

    public class PurchaseOrderCancelFilterViewModel
    {
        public int PurchaseOrderCancelHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Sale Order No")]
        public string PurchaseOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class PurchaseOrderCancelMasterDetailModel
    {
        public List<PurchaseOrderCancelLineViewModel> PurchaseOrderCancelViewModels { get; set; }
    }




    public class PurchaseOrderCancelLineViewModel
    {

        [Key]
        public int PurchaseOrderCancelLineId { get; set; }

        [Display(Name = "Purchase Order Cancel"), Required]
        public int PurchaseOrderCancelHeaderId { get; set; }
        public string PurchaseOrderCancelHeaderDocNo { get; set; }

        [Display(Name = "Purchase Order"), Required]
        public int PurchaseOrderLineId { get; set; }
        public string PurchaseOrderDocNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public decimal OrderQty { get; set; }
        public decimal BalanceQty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        [MaxLength(50)]
        public string Specification { get; set; }
        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }
        public DateTime? DueDate { get; set; }
        public string UnitId { get; set; }
        public PurchaseOrderSettingsViewModel PurchOrderSettings { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public int ? OrderHeaderId { get; set; }
        public int ? OrderDocTypeId { get; set; }
        public string LockReason { get; set; }
    }




    [Serializable]
    public class PurchaseOrderCancelWizardViewModel
    {
        public DateTime OrderDate { get; set; }
        public string SOrderDate { get; set; }
        public string OrderNo { get; set; }
        public int SupplierId { get; set; }
        public int PurchaseOrderLineId { get; set; }
        public string SupplierName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal CancelQty { get; set; }
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public int ProdId { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public bool Sample { get; set; }
    }


    public class PurchaseOrderCancelWizardFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string PurchaseOrderHeaderId { get; set; }
        public string SupplierId { get; set; }
        public int DocTypeId { get; set; }
        public string ProductId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public string ProductGroupId { get; set; }
        public string ProductCategoryId { get; set; }
        public decimal? BalanceQty { get; set; }
        public decimal CancelQty { get; set; }
        public decimal? MultiplierGT { get; set; }
        public decimal? MultiplierLT { get; set; }
        public string Sample { get; set; }
    }


}
