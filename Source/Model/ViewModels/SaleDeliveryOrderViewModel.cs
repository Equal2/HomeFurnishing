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
    public class SaleDeliveryOrderHeaderViewModel
    {
        public int SaleDeliveryOrderHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20)]
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int Priority { get; set; }

        [ Display(Name = "Ship Method"), Required]
        public int? ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }
        [Display(Name = "Ship Address"), MaxLength(250)]
        public string ShipAddress { get; set; }
        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public SaleDeliveryOrderSettingsViewModel SaleDeliveryOrderSettings { get; set; }

    }

    public class SaleDeliveryOrderHeaderIndexViewModel
    {

        public int SaleDeliveryOrderHeaderId { get; set; }        
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20)]
        public string DocNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime DueDate { get; set; }
        public string BuyerName { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public string ModifiedBy { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }

    }
    

    public class SaleDeliveryOrderLineViewModel
    {

        public int SaleDeliveryOrderLineId { get; set; }
        public int SaleOrderLineId { get; set; }
        public string SaleOrderDocNo { get; set; }
        public int SaleDeliveryOrderHeaderId { get; set; }
        public string SaleDeliveryOrderHeaderName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Qty { get; set; }
        public decimal BalanceQty { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ? DueDate { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int UnitDecimalPlaces { get; set; }
        public string UnitName { get; set; }
        public string LockReason { get; set; }
        public SaleDeliveryOrderSettingsViewModel SaleDeliveryOrderSettings { get; set; }

    }

    public class SaleDeliveryOrderFilterViewModel
    {
        public int SaleDeliveryOrderHeaderId { get; set; }
        public int BuyerId { get; set; }
        [Display(Name = "Sale Order No")]
        public string SaleOrderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }

    public class SaleDeliveryMasterDetailModel
    {
        public int SaleDeliveryOrderHeaderId { get; set; }
        public List<SaleDeliveryOrderLineViewModel> SaleDeliveryOrderLineViewModel { get; set; }
    }

    public class SaleDeliveryProductHelpList
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SaleOrderLineId { get; set; }
        public string SaleOrderDocNo { get; set; }
        public string Specification { get; set; }
        public decimal Qty { get; set; }
    }

    public class SaleDeliveryOrderLineListViewModel
    {
        public int SaleDeliveryOrderHeaderId { get; set; }
        public int SaleDeliveryOrderLineId { get; set; }
        public int SaleDeliveryIndentHeaderId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }

}
