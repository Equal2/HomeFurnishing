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
    public class SaleOrderHeaderIndexViewModel 
    {
        [Key]
        public int SaleOrderHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type"),Required(ErrorMessage="Please select a Document type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Order Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}"),Required(ErrorMessage="Please select Order Date")]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No"), MaxLength(20, ErrorMessage = "Order No. can not exceed 20 characters"), Required(ErrorMessage = "The OrderNo Field is Required")]
        public string DocNo { get; set; }

        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [MaxLength(20), Display(Name = "Buyer Order Number")]
        public string BuyerOrderNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}"),Required(ErrorMessage="Please select Due Date")]
        public DateTime DueDate { get; set; }

        [ForeignKey("SaleToBuyer"), Display(Name = "Sale To Buyer"),Required(ErrorMessage="Please select Sale To Buyer"),Range(1,int.MaxValue,ErrorMessage="Sale To Buyer field is required")]
        public int SaleToBuyerId { get; set; }
        public virtual Buyer SaleToBuyer { get; set; }

        [ForeignKey("BillToBuyer"), Display(Name = "Bill To Buyer"),Required(ErrorMessage="Please select Bill To Buyer"),Range(1,int.MaxValue,ErrorMessage="Bill To Buyer field is required")]
        public int BillToBuyerId { get; set; }
        public virtual Buyer BillToBuyer { get; set; }

        [ForeignKey("Currency"), Display(Name = "Currency"),Required(ErrorMessage="The Currency Field is Required"),Range(1,int.MaxValue,ErrorMessage="Currency field is required")]
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }
        [Required(ErrorMessage="Please select a Priority")]
        public int Priority { get; set; }

        [ForeignKey("ShipMethod"), Display(Name = "Ship Method"), Required(ErrorMessage = "The ShipMethod Field is Required")]
        public int ? ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        public int? GodownId { get; set; }
        public string GodownName { get; set; }


        [Display(Name = "Ship Address"), MaxLength(250)]
        public string ShipAddress { get; set; }

        [ForeignKey("DeliveryTerms"), Display(Name = "Delivery Terms"), Required(ErrorMessage = "The DeliveryTerms Field is Required")]
        public int ? DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        public string TermsAndConditions { get; set; }

        public int CreditDays { get; set; }

        public int? ProcessId { get; set; }

        public Decimal? Advance { get; set; }

        [ForeignKey("Financier")]
        public int? FinancierId { get; set; }
        public virtual Person Financier { get; set; }

        [ForeignKey("SalesExecutive")]
        public int? SalesExecutiveId { get; set; }
        public virtual Person SalesExecutive { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public SaleOrderSettingsViewModel SaleOrderSettings { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

         [Display(Name = "Sale To Buyer")]
         public string SaleToBuyerName { get; set; }

         [Display(Name="Bill To Buyer")]
         public string BillToBuyerName { get; set; }

         public string DivisionName { get; set; }
         public string SiteName { get; set; }
        
         [Display(Name="Entry Type")]
         public string DocumentTypeName { get; set; }
         [Display(Name="Currency")]
         public string CurrencyName { get; set; }

         [Display(Name="Delivery Terms")]
         public string DeliveryTermsName { get; set; }
         public string PriorityName { get; set; }
         public string ShipMethodName { get; set; }
         public byte UnitConversionForId { get; set; }
         public string UnitConversionForName { get; set; }
         public string ReviewBy { get; set; }
         public bool? Reviewed { get; set; }
         public int? ReviewCount { get; set; }

       public IEnumerable<SaleOrderLine>  SaleOrderLine {get;set;}

    }
    public class SaleOrderHeaderIndexViewModelForEdit : SaleOrderHeaderIndexViewModel
    {
        [Required,MinLength(20)]
        public string LogReason { get; set; }
    }

    public class SaleOrderMasterDetailModel
    {
        public int SaleOrderHeaderId { get; set; }
        public SaleOrderHeaderIndexViewModel SaleOrderHeaderIndexViewModel { get; set; }
        public SaleOrderHeaderIndexViewModelForEdit SaleOrderHeaderIndexViewModelForEdit { get; set; } 
        public List<SaleOrderLineIndexViewModel> SaleOrderLineIndexViewModel { get; set; }
    }

    public class SaleOrderLineListViewModel
    {
        public int SaleOrderHeaderId { get; set; }
        public int SaleOrderLineId { get; set; }
        public string DocNo { get; set; }
    }
}
