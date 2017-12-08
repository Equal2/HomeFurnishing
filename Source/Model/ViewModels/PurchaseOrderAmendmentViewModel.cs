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
    public class PurchaseOrderAmendmentHeaderViewModel
    {
        public int PurchaseOrderAmendmentHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string LockReason { get; set; }
        public int Status { get; set; }
        public PurchaseOrderSettingsViewModel PurchaseOrderSettings { get; set; }
    }


    public class PurchaseOrderRateAmendmentLineViewModel
    {
        [Key]
        public int PurchaseOrderRateAmendmentLineId { get; set; }
        public int PurchaseOrderAmendmentHeaderId { get; set; }        
        public string DocNo { get; set; }
        public int PurchaseOrderLineId { get; set; }
        public string PurchaseOrderDocNo { get; set; }
        public decimal Qty { get; set; }
        public decimal PurchaseOrderRate { get; set; }
        public decimal AmendedRate { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }

        //Support Properties

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string UnitId { get; set; }
        public string DealUnitId { get; set; }
        public string UnitName { get; set; }
        public string DealUnitName { get; set; }
        public decimal DealQty { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int dealUnitDecimalPlaces { get; set; }
        public string SupplierName { get; set; }
        public bool AAmended { get; set; }
        public string LotNo { get; set; }
        public string Specification { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public PurchaseOrderSettingsViewModel PurchaseOrderSettings { get; set; }
        public List<PurchaseOrderRateAmendmentLineCharge> linecharges { get; set; }
        public List<PurchaseOrderAmendmentHeaderCharge> footercharges { get; set; }
    }

    public class PurchaseOrderAmendmentFilterViewModel
    {
        public int PurchaseOrderAmendmentHeaderId { get; set; }
        public int? SupplierId { get; set; }
        [Display(Name = "Sale Order No")]
        public string PurchaseOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }
        public DateTime? UpToDate { get; set; }
        public decimal Rate { get; set; }
    }

    public class PurchaseOrderListViewModel
    {
        public int PurchaseOrderHeaderId { get; set; }
        public int PurchaseOrderLineId { get; set; }
        public string DocNo { get; set; }
        public string RequestDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public decimal BalanceQty { get; set; }


    }


    public class PurchaseOrderAmendmentMasterDetailModel
    {
        public List<PurchaseOrderRateAmendmentLineViewModel> PurchaseOrderRateAmendmentLineViewModel { get; set; }
    }


    [Serializable]
    public class PurchaseOrderAmendmentWizardViewModel
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
        public decimal OldRate { get; set; }
        public decimal Rate { get; set; }
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public int ProdId { get; set; }
        public decimal UnitConversionMultiplier { get; set; }
        public bool Sample { get; set; }
    }


    public class PurchaseOrderRateAmendmentWizardFilterViewModel
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
        public decimal? Rate { get; set; }
        public decimal NewRate { get; set; }
        public decimal? MultiplierGT { get; set; }
        public decimal? MultiplierLT { get; set; }
        public string Sample { get; set; }
    }



}
