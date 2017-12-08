using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using Model.ViewModels;

namespace Model.ViewModel
{
    public class SaleDeliveryOrderCancelHeaderViewModel
    {
        public int SaleDeliveryOrderCancelHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int OrderById { get; set; }
        public string OrderByName { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public SaleDeliveryOrderSettingsViewModel SaleDeliveryOrderSettings { get; set; }

    }    

    public class SaleDeliveryOrderCancelFilterViewModel
    {
        public int SaleDeliveryOrderCancelHeaderId { get; set; }
        public int BuyerId { get; set; }
        [Display(Name = "Sale Delivery Order No")]
        public string SaleDeliveryOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class SaleDeliveryOrderCancelMasterDetailModel
    {
        public List<SaleDeliveryOrderCancelLineViewModel> SaleDeliveryOrderCancelViewModels { get; set; }
    }




    public class SaleDeliveryOrderCancelLineViewModel
    {
        public int SaleDeliveryOrderCancelLineId { get; set; }
        public int SaleDeliveryOrderCancelHeaderId { get; set; }
        public string SaleDeliveryOrderCancelHeaderDocNo { get; set; }
        public int? Sr { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int SaleDeliveryOrderLineId { get; set; }
        public string SaleDeliveryOrderDocNo { get; set; }
        public int ? Dimension1Id { get; set; }
        public int ? Dimension2Id { get; set; }
        public int ? ProductId { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public decimal Qty { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }
        public string UnitId { get; set; }
        public decimal BalanceQty { get; set; }
        public int unitDecimalPlaces { get; set; }
        public int DealunitDecimalPlaces { get; set; }
        public SaleDeliveryOrderSettingsViewModel SaleDeliveryOrderSettings { get; set; }

        [Display(Name = "OrderDealQty"), Required]
        public Decimal OrderDealQty { get; set; }

        [Display(Name = "OrderQty"), Required]
        public Decimal OrderQty { get; set; }
        public string DealUnitId { get; set; }
        public Decimal DealQty { get; set; }
        public Decimal ? UnitConversionMultiplier { get; set; }
        public Decimal? Rate { get; set; }
    }

}
