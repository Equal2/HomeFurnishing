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
    public class DirectSaleInvoiceHeaderViewModel 
    {
        [Key]
        public int SaleInvoiceHeaderId { get; set; }
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
        public int? AgentId { get; set; }
        public string AgentName { get; set; }

        [Display(Name = "Sale To Buyer")]
        public int SaleToBuyerId { get; set; }
        public string SaleToBuyerName { get; set; }

        public int? SalesTaxGroupPersonId { get; set; }
        public int? GatePassHeaderId { get; set; }

        [Display(Name = "Bill To Buyer")]
        public int BillToBuyerId { get; set; }
        public string BillToBuyerName { get; set; }
        public decimal ? CreditDays { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal ? CreditLimit { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int GodownId { get; set; }
        public string GodownName { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }        

        [Display(Name = "Delivery Terms")]
        public int DeliveryTermsId { get; set; }
        public string DeliveryTermsName { get; set; }       
        public int Status { get; set; }
        public string TermsAndConditions { get; set; }
        public SaleInvoiceSettingsViewModel SaleInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int? SaleDispatchHeaderId { get; set; }
        public string SaleDispatchHeaderDocNo { get; set; }

        public string ShipToPartyAddress { get; set; }

        public string LockReason { get; set; }
        public string Mobile { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public string Address { get; set; }
        public int? CityId { get; set; }

        public int? FinancierId { get; set; }
        public string FinancierName { get; set; }

        public int? SalesExecutiveId { get; set; }
        public string SalesExecutiveName { get; set; }

        [Display(Name = "Ship Method"), Required]
        public int ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public int BuyerDocTypeId { get; set; }
        public int? FinancierDocTypeId { get; set; }
        public List<DocumentTypeHeaderAttributeViewModel> DocumentTypeHeaderAttributes { get; set; }
        public string DocumentInfo { get; set; }

    }
   
    public class DirectSaleInvoiceLineViewModel
    {
        [Key]
        public int SaleInvoiceLineId { get; set; }

        public DateTime? DocDate { get; set; }

        public int? SaleToBuyerId { get; set; }

        [ForeignKey("ProductUid"), Display(Name = "ProductUid")]
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceHeaderDocNo { get; set; }

        [Display(Name = "Sale Dispatch")]
        public int SaleDispatchLineId { get; set; }

        [Display(Name = "Sale Order")]
        public int? SaleOrderLineId { get; set; }
        public string SaleOrderHeaderDocNo { get; set; }


        [Display(Name = "Packing No.")]
        public int? PackingLineId { get; set; }
        public string PackingDocNo { get; set; }

        public string SaleDispatchHeaderDocNo { get; set; }

        public string ProductCode { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public string Specification { get; set; }
        public string LotNo { get; set; }
        public string BaleNo { get; set; }
        public string LockReason { get; set; }

        public int? SalesTaxGroupProductId { get; set; }
        public int? SalesTaxGroupPersonId { get; set; }

        [Display(Name = "Qty"), Required]
        public Decimal Qty { get; set; }
        public Decimal BalanceQty { get; set; }

        public Decimal? FreeQty { get; set; }

        [Display(Name = "Deal Unit")]
        [ForeignKey("DealUnit")]
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        
        [Display(Name = "Delivery Qty")]
        public Decimal DealQty { get; set; }


        [Display(Name = "Reward Points")]
        public Decimal? RewardPoints { get; set; }

        public byte unitDecimalPlaces { get; set; }

        [Display(Name = "Weight")]
        public Decimal? Weight { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal? UnitConversionMultiplier { get; set; }

        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        public int? PromoCodeId { get; set; }
        public string PromoCodeName { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }

        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public int GodownId { get; set; }
        public string GodownName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public List<SaleInvoiceLineCharge> linecharges { get; set; }
        public List<SaleInvoiceHeaderCharge> footercharges { get; set; }
        public SaleInvoiceSettingsViewModel SaleInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
        public int DocTypeId { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public bool ? IsSaleBased { get; set; }
        public int DealunitDecimalPlaces { get; set; }

    }

    public class SaleInvoiceCustomerDetail
    {
        public int SaleInvoiceHeaderId { get; set; }


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

    public class SaleOrderListViewModel
    {
        public int SaleOrderHeaderId { get; set; }
        public int SaleOrderLineId{ get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public decimal BalanceQty { get; set; }
    }

    public class SaleOrderProductHelpList
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }        
        public int SaleOrderLineId { get; set; }
        public string SaleOrderDocNo { get; set; }
        public string Specification { get; set; }
        public int Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public decimal Qty { get; set; }

    }

    public class SaleInvoiceFilterViewModel
    {
        public int SaleInvoiceHeaderId { get; set; }        
        [Display(Name = "Sale Orders")]
        public string SaleOrderHeaderId { get; set; }
        public string PackingHeaderId { get; set; }
        public string SaleDispatchHeaderId { get; set; }

        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }

        public string Dimension1Id { get; set; }

        public string Dimension2Id { get; set; }

        public DateTime UpToDate { get; set; }

        public SaleInvoiceSettingsViewModel SaleInvoiceSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
                
    }

    public class DirectSaleInvoiceListViewModel
    {
        public List<DirectSaleInvoiceLineViewModel> DirectSaleInvoiceLineViewModel { get; set; }
        public SaleInvoiceSettingsViewModel SaleInvoiceSettings { get; set; }
    }


}
