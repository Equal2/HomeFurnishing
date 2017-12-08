using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class DispatchWaybillHeaderViewModel 
    {
        [Key]
        public int DispatchWaybillHeaderId { get; set; }

        [Display(Name = "Dispatch Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Sale Dispatch Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Sale Dispatch No"), Required, MaxLength(20)]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [ForeignKey("Consignee"), Display(Name = "Consignee"), Required]
        public int ConsigneeId { get; set; }
        public string ConsigneeName { get; set; }

        [Display(Name = "Ship Method"), Required]
        [ForeignKey("ShipMethod")]
        public int ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }

        [ForeignKey("SaleInvoiceHeader"), Display(Name = "Invoice No"), Required]
        public int SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceHeaderDocNo { get; set; }

        [ForeignKey("Transporter"), Display(Name = "Transporter"), Required]
        public int TransporterId { get; set; }
        public string TransporterName { get; set; }

        [Display(Name = "Delivery Office"), MaxLength(100)]
        public string DeliveryOffice { get; set; }

        [Display(Name = "Waybill No"), MaxLength(50), Required]
        public string WaybillNo { get; set; }

        [Display(Name = "Waybill Date"), Required]
        public DateTime WaybillDate { get; set; }

        [Display(Name = "Estd. Delivery Date"), Required]
        public DateTime EstimatedDeliveryDate { get; set; }

        [Display(Name = "Delivery Office"), MaxLength(20), Required]
        public string PaymentType { get; set; }

        [ForeignKey("FromCity"), Display(Name = "From City"), Required]
        public int FromCityId { get; set; }
        public string FromCityName { get; set; }

        [ForeignKey("ToCity"), Display(Name = "To City"), Required]
        public int ToCityId { get; set; }
        public string ToCityName { get; set; }

        [ForeignKey("Route"), Display(Name = "Route"), Required]
        public int RouteId { get; set; }
        public string RouteName { get; set; }

        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        [Display(Name = "Private Mark"), MaxLength(100)]
        public string PrivateMark { get; set; }

        [Display(Name = "No.of Packages"), MaxLength(50)]
        public string NoOfPackages { get; set; }

        [Display(Name = "Actual Weight")]
        public Decimal? ActualWeight { get; set; }

        [Display(Name = "Charged Weight")]
        public Decimal? ChargedWeight { get; set; }

        [Display(Name = "Container No"), MaxLength(50)]
        public string ContainerNo { get; set; }

        [Display(Name = "Freight")]
        public Decimal? Freight { get; set; }

        [Display(Name = "Other Charges")]
        public Decimal? OtherCharges { get; set; }

        [Display(Name = "Service Tax Per")]
        public Decimal? ServiceTaxPer { get; set; }

        [Display(Name = "Service Tax Amount")]
        public Decimal? ServiceTaxAmount { get; set; }

        [Display(Name = "Total Amount")]
        public Decimal? TotalAmount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public Boolean IsPreCarriage { get; set; }

        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public int? ReviewCount { get; set; }
    }

    public class DispatchWaybillHeaderViewModelWithLog : DispatchWaybillHeaderViewModel
    {
        [Required, MinLength(20)]
        public string LogReason { get; set; }
    }


    public class DispatchWaybillLineViewModel 
    {
        [Key]
        public int DispatchWaybillLineId { get; set; }

        [Display(Name = "DispatchWaybill"), Required]
        public int DispatchWaybillHeaderId { get; set; }

        [Display(Name = "City"), Required]
        public int CityId { get; set; }
        public string CityName { get; set; }

        [Display(Name = "Receive Date")]
        public DateTime? ReceiveDateTime { get; set; }

        [Display(Name = "Receive Remark")]
        public string ReceiveRemark { get; set; }

        [Display(Name = "Forwarding Date")]
        public DateTime? ForwardingDateTime { get; set; }

        [Display(Name = "Forwarded By"), MaxLength(250)]
        public string ForwardedBy { get; set; }

        [Display(Name = "Forwarding Remark")]
        public string ForwardingRemark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }


    public class DispatchWaybillHeaderMasterDetailModel
    {
        public int DispatchWaybillHeaderId { get; set; }
        public DispatchWaybillHeaderViewModel DispatchWaybillHeaderViewModel { get; set; }
        public List<PackingLineViewModel> PackingLineViewModel { get; set; }
        public DispatchWaybillHeaderViewModelWithLog DispatchWaybillHeaderViewModelWithLog { get; set; }
    }
}
