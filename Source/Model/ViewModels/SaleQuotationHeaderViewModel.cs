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
    public class SaleQuotationHeaderViewModel 
    {
        [Key]
        public int SaleQuotationHeaderId { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public DateTime DocDate { get; set; }

        public string DocNo { get; set; }

        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }

        public DateTime DueDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public int SaleToBuyerId { get; set; }
        public string SaleToBuyerName { get; set; }

        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int Priority { get; set; }

        public int ? ShipMethodId { get; set; }
        public virtual string ShipMethodName { get; set; }

        public int? CostCenterId { get; set; }
        public virtual string CostCenterName { get; set; }



        public int? CalculationFooterChargeCount { get; set; }


        [Display(Name = "Ship Address"), MaxLength(250)]
        public string ShipAddress { get; set; }

        [ForeignKey("DeliveryTerms"), Display(Name = "Delivery Terms"), Required(ErrorMessage = "The DeliveryTerms Field is Required")]
        public int ? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        public string TermsAndConditions { get; set; }

        public int CreditDays { get; set; }

        public int ProcessId { get; set; }
        public int? FinancierId { get; set; }
        public string FinancierName { get; set; }

        public int? AgentId { get; set; }
        public string AgentName { get; set; }

        public int? SalesExecutiveId { get; set; }
        public string SalesExecutiveName { get; set; }

        public int? SalesTaxGroupPersonId { get; set; }
        public string SalesTaxGroupPersonName { get; set; }

        public bool IsDoorDelivery { get; set; }

        public int BuyerDocTypeId { get; set; }


        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public SaleQuotationSettingsViewModel SaleQuotationSettings { get; set; }

        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }



        
        
         [Display(Name="Delivery Terms")]
         public string DeliveryTermsName { get; set; }
         public string PriorityName { get; set; }
         public byte UnitConversionForId { get; set; }
         public string UnitConversionForName { get; set; }
         public string ReviewBy { get; set; }
         public bool? Reviewed { get; set; }
         public int? ReviewCount { get; set; }

         public string LockReason { get; set; }

         public Decimal? PayTermAdvancePer { get; set; }
         public Decimal? PayTermOnDeliveryPer { get; set; }
         public Decimal? PayTermOnDueDatePer { get; set; }
         public Decimal? PayTermCashPer { get; set; }
         public Decimal? PayTermBankPer { get; set; }
         public string PayTermDescription { get; set; }

       public IEnumerable<SaleQuotationLine>  SaleQuotationLine {get;set;}
       public List<DocumentTypeHeaderAttributeViewModel> DocumentTypeHeaderAttributes { get; set; }

    }
    public class SaleQuotationHeaderListViewModel
    {
        public int SaleQuotationHeaderId { get; set; }
        public int SaleQuotationLineId { get; set; }
        public string DocNo { get; set; }
        public string RequestDocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
        public string DocumentTypeName { get; set; }
        public string ProductName { get; set; }
        public decimal BalanceQty { get; set; }


    }

    public class SaleQuotationLineFilterViewModel
    {
        public int SaleQuotationHeaderId { get; set; }
        public int SaleToBuyerUd { get; set; }
        [Display(Name = "Sale Enquiry")]
        public string SaleEnquiryHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }


        public string Dimension1Id { get; set; }
        public string Dimension2Id { get; set; }

        public string Dimension3Id { get; set; }
        public string Dimension4Id { get; set; }


        public string DealUnitId { get; set; }
        public decimal Rate { get; set; }
        public SaleQuotationSettingsViewModel SaleQuotationSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

    }

    public class SaleQuotationLineListViewModel
    {
        public int SaleQuotationHeaderId { get; set; }
        public int SaleQuotationLineId { get; set; }
        public string DocNo { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string DocumentTypeName { get; set; }
    }

    public class SaleQuotationMasterDetailModel
    {
        public List<SaleQuotationLineViewModel> SaleQuotationLineViewModel { get; set; }
        public SaleQuotationSettingsViewModel SaleQuotationSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }
    }
}
