using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class SaleInvoiceHeaderIndexViewModel 
    {
        [Key]
        public int SaleInvoiceHeaderId { get; set; }

        public int SaleDispatchHeaderId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type"),Required(ErrorMessage="Please select a Document type")]
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

        [Display(Name = "Bill To Buyer")]
        public int BillToBuyerId { get; set; }
        public string BillToBuyerName { get; set; }

        [ForeignKey("ShipMethod"), Display(Name = "Ship Method"), Required(ErrorMessage = "The ShipMethod Field is Required")]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        [Display(Name = "Ship To Party Address"), MaxLength(250)]
        public string ShipToPartyAddress { get; set; }

        [ForeignKey("DeliveryTerms"), Display(Name = "Delivery Terms"), Required(ErrorMessage = "The DeliveryTerms Field is Required")]
        public int DeliveryTermsId { get; set; }
        public virtual DeliveryTerms DeliveryTerms { get; set; }

        [Display(Name = "Transporter"), MaxLength(250)]
        public string Transporter { get; set; }

        [Display(Name = "Currency")]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        public Decimal? ExchangeRate { get; set; }

        public int Status { get; set; }

        [Display(Name = "BL No.")]
        [MaxLength(20)]
        public string BlNo { get; set; }

        [Display(Name = "BL Date")]
        public DateTime BlDate { get; set; }

        [Display(Name = "Private Mark")]
        [MaxLength(20)]
        public string PrivateMark { get; set; }

        [Display(Name = "Port Of Loading")]
        [MaxLength(50)]
        public string PortOfLoading { get; set; }

        [Display(Name = "Destination Port")]
        [MaxLength(50)]
        public string DestinationPort { get; set; }

        [Display(Name = "Final Place Of Delivery")]
        [MaxLength(50)]
        public string FinalPlaceOfDelivery { get; set; }

        [Display(Name = "Pre Carriage By")]
        [MaxLength(50)]
        public string PreCarriageBy { get; set; }

        [Display(Name = "Place Of Pre Carriage")]
        [MaxLength(50)]
        public string PlaceOfPreCarriage { get; set; }

        [Display(Name = "Circular No")]
        [MaxLength(50)]
        public string CircularNo { get; set; }

        [Display(Name = "Circular Date")]
        public DateTime CircularDate { get; set; }

        [Display(Name = "Order No")]
        [MaxLength(50)]
        public string OrderNo { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Roll No")]
        public string BaleNoSeries { get; set; }

        [Display(Name = "Description Of Goods")]
        public string DescriptionOfGoods { get; set; }

        [Display(Name = "Packing Material Description")]
        public string PackingMaterialDescription { get; set; }

        [Display(Name = "Kinds Of Packages")]
        public Decimal KindsOfackages { get; set; }

        [Display(Name = "Compositions")]
        public string Compositions { get; set; }

        [Display(Name = "Other Reference")]
        public string OtherRefrence { get; set; }

        [Display(Name = "Terms Of Sale")]
        public string TermsOfSale { get; set; }

        [Display(Name = "Notify Party")]
        public string NotifyParty { get; set; }

        [Display(Name = "Transporter Information")]
        public string TransporterInformation { get; set; }

        public Decimal? GrossWeight { get; set; }

        public Decimal? NetWeight { get; set; }

        public Decimal? InvoiceAmount { get; set; }

        public Decimal? Freight { get; set; }

        [Display(Name = "Freight Remark")]
        public string FreightRemark { get; set; }

        [Display(Name = "Insurance Remark")]
        public string InsuranceRemark { get; set; }
        public Decimal? Insurance { get; set; }

        [Display(Name = "Vehicle No")]
        public string VehicleNo { get; set; }

        public Decimal? Deduction { get; set; }

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
        public decimal? TotalQty { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? DecimalPlaces { get; set; }

    }
    public class SaleInvoiceHeaderIndexViewModelForEdit : SaleInvoiceHeaderIndexViewModel
    {
        [Required,MinLength(20)]
        public string LogReason { get; set; }
    }

    public class SaleInvoiceMasterDetailModel
    {
        public int SaleInvoiceHeaderId { get; set; }
        public SaleInvoiceHeaderIndexViewModel SaleInvoiceHeaderIndexViewModel { get; set; }
        public SaleInvoiceHeaderIndexViewModelForEdit SaleInvoiceHeaderIndexViewModelForEdit { get; set; }
        public List<SaleInvoiceLineViewModel> SaleInvoiceLineViewModel { get; set; }
    }

    public class SaleInvoiceFillProducts 
    {
        public int SaleInvoiceHeaderId { get; set; }

        public int SaleDispatchHeaderId { get; set; }

        public string PackingHeaderIds { get; set; }

        public string DealUnitId { get; set; }
    }

    public class UpdateRates
    {
        public int SaleInvoiceHeaderId { get; set; }

        [Required]
        public int FromBaleNo { get; set; }

        [Required]
        public int ToBaleNo { get; set; }

        [Required]
        public int  ProductInvoiceGroupId { get; set; }

        [Required]
        public Decimal Rate { get; set; }
    }


}
