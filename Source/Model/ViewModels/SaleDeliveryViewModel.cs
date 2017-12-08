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
    public class SaleDeliveryHeaderViewModel 
    {
        [Key]
        public int SaleDeliveryHeaderId { get; set; }
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


        public string DeliverToPerson { get; set; }
        public string DeliverToPersonReference { get; set; }

        public string ShipToPartyAddress { get; set; }

        public int? GatePassHeaderId { get; set; }

        public string GatePassDocNo { get; set; }
        public DateTime? GatePassDocDate { get; set; }



        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }


   
        public int Status { get; set; }
        public SaleDeliverySettingsViewModel SaleDeliverySettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

    }
   
    public class SaleDeliveryLineViewModel
    {
      
        public int SaleDeliveryLineId { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int SaleDeliveryHeaderId { get; set; }
        public string SaleDeliveryHeaderDocNo { get; set; }

        public int SaleInvoiceLineId { get; set; }
        public string SaleInvoiceHeaderDocNo { get; set; }




        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int? DealUnitDecimalPlaces { get; set; }


        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }






        public Decimal BalanceQty { get; set; }

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

        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public SaleDeliverySettingsViewModel SaleDeliverySettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }

        public int unitDecimalPlaces { get; set; }

    }


    public class SaleDeliveryFilterViewModel
    {
        public int SaleDeliveryHeaderId { get; set; }        
        [Display(Name = "Sale Invoice")]
        public string SaleInvoiceHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }


        public string Dimension1Id { get; set; }

        public string Dimension2Id { get; set; }

        public SaleDeliverySettingsViewModel SaleDeliverySettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
                
    }

    public class SaleDeliveryListViewModel
    {
        public int SaleDeliveryHeaderId { get; set; }
        public int SaleDeliveryLineId { get; set; }

        public List<SaleDeliveryLineViewModel> SaleDeliveryLineViewModel { get; set; }

        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }


    }


    public class SaleDeliveryHeaderIndexViewModel
    {
        [Key]
        public int SaleDeliveryHeaderId { get; set; }


        public int DocTypeId { get; set; }
        public string DocumentTypeName { get; set; }

        public DateTime DocDate { get; set; }

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

    public class SaleDeliveryHeaderIndexViewModelForEdit : SaleDeliveryHeaderIndexViewModel
    {
        [Required, MinLength(20)]
        public string LogReason { get; set; }
    }

    [Serializable]
    public class SaleDeliveryWizardViewModel
    {
        public int DocTypeId { get; set; }
        public string DocNo { get; set; }
        public string Date { get; set; }
        public int? SaleToBuyerId { get; set; }
        public string SaleToBuyerName { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Qty { get; set; }
        public int SaleInvoiceLineId { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleDeliveryWizardMasterDetailModel
    {
        public List<SaleDeliveryWizardViewModel> SaleDeliveryWizardViewModel { get; set; }
    }


}
