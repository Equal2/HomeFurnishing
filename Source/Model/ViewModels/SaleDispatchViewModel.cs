using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class SaleDispatchHeaderViewModel 
    {
        [Key]
        public int SaleDispatchHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        [Display(Name = "Order Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}"),Required(ErrorMessage="Please select Order Date")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20),Required(ErrorMessage="The OrderNo Field is Required")]
        public string DocNo { get; set; }

        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public string SaleToBuyerName { get; set; }

        public int? GatePassHeaderId { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int GodownId { get; set; }
        public string GodownName { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }


        [Display(Name = "Delivery Terms")]
        public int DeliveryTermsId { get; set; }
        public string DeliveryTermsName { get; set; }       
        public int Status { get; set; }
        public SaleDispatchSettingsViewModel SaleDispatchSettings { get; set; }

        [Display(Name = "Ship Method"), Required]
        public int ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string  GatePassDocNo { get; set; }
        public DateTime? GatePassDocDate { get; set; }


    }
   
    public class SaleDispatchLineViewModel
    {
      
        public int SaleDispatchLineId { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int SaleDispatchHeaderId { get; set; }
        public string SaleDispatchHeaderDocNo { get; set; }

        public int ? PackingLineId { get; set; }

        [Display(Name = "Sale Order")]
        public int? SaleOrderLineId { get; set; }
        public string SaleOrderHeaderDocNo { get; set; }

        public int? StockInId { get; set; }
        public string StockInNo { get; set; }

        public string ProductCode { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int? DealUnitDecimalPlaces { get; set; }


        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }
        public string BaleNo { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }


        [Display(Name = "Loss Qty")]
        public Decimal? LossQty { get; set; }

        [Display(Name = "Pass Qty")]
        public Decimal? PassQty { get; set; }


        [Display(Name = "Free Qty")]
        public Decimal? FreeQty { get; set; }



        public Decimal BalanceQty { get; set; }
        public Decimal StockInBalanceQty { get; set; }

        public Decimal? Rate { get; set; }

        [Display(Name = "Deal Unit")]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        
        [Display(Name = "Delivery Qty")]
        public Decimal DealQty { get; set; }


        [Display(Name = "Weight")]
        public Decimal? Weight { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal? UnitConversionMultiplier { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int GodownId { get; set; }
        public string GodownName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public SaleDispatchSettingsViewModel SaleDispatchSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public bool ? IsSaleBased { get; set; }
        public int unitDecimalPlaces { get; set; }

    }

    public class SaleDispatchCustomerDetail
    {
        public int SaleDispatchHeaderId { get; set; }


        public int PersonId { get; set; }
        [MaxLength(10)]
        public string Mobile { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }
        
        [MaxLength(100)]
        public string Name { get; set; }

        public string Address { get; set; }
        public int? CityId { get; set; }

        public string CityName { get; set; }
    }

    public class SaleDispatchFilterViewModel
    {
        public int SaleDispatchHeaderId { get; set; }        
        [Display(Name = "Sale Orders")]
        public string SaleOrderHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }


        public string Dimension1Id { get; set; }

        public string Dimension2Id { get; set; }

        public SaleDispatchSettingsViewModel SaleDispatchSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
                
    }

    public class SaleDispatchListViewModel
    {
        public int SaleDispatchHeaderId { get; set; }
        public int SaleDispatchLineId { get; set; }

        public List<SaleDispatchLineViewModel> SaleDispatchLineViewModel { get; set; }

        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
    }


    public class SaleDispatchHeaderIndexViewModel
    {
        [Key]
        public int SaleDispatchHeaderId { get; set; }


        [ForeignKey("DocType"), Display(Name = "Order Type"), Required(ErrorMessage = "Please select a Document type")]
        public int DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        [Display(Name = "Order Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}"), Required(ErrorMessage = "Please select Order Date")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20), Required(ErrorMessage = "The OrderNo Field is Required")]
        public string DocNo { get; set; }

        [Display(Name = "Division")]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public string SaleToBuyerName { get; set; }


        [ForeignKey("ShipMethod"), Display(Name = "Ship Method"), Required(ErrorMessage = "The ShipMethod Field is Required")]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }


        [ForeignKey("DeliveryTerms"), Display(Name = "Delivery Terms"), Required(ErrorMessage = "The DeliveryTerms Field is Required")]
        public int DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        public int Status { get; set; }



        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
        public string LockReason { get; set; }

        public int? GatePassHeaderId { get; set; }
        public string GatePassDocNo { get; set; }
        public int GatePassStatus { get; set; }
        public DateTime? GatePassDocDate { get; set; }
    }

    public class SaleDispatchHeaderIndexViewModelForEdit : SaleDispatchHeaderIndexViewModel
    {
        [Required, MinLength(20)]
        public string LogReason { get; set; }
    }


}
