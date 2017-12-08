using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class DispatchWaybillHeader : EntityBase, IHistoryLog
    {
        public DispatchWaybillHeader()
        {
        }

        [Key]
        public int DispatchWaybillHeaderId { get; set; }
                        
        [Display(Name = "Dispatch Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]                
        [Display(Name = "Sale Dispatch Date"),Required ]
        public DateTime DocDate { get; set; }
        
        [Display(Name = "Sale Dispatch No"),Required,MaxLength(20) ]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_DispatchWaybillHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Consignee"), Display(Name = "Consignee"),Required]
        public int ConsigneeId { get; set; }
        public virtual Buyer Consignee { get; set; }

        [Display(Name = "Ship Method"),Required]
        [ForeignKey("ShipMethod")]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }

        [ForeignKey("SaleInvoiceHeader"), Display(Name = "Invoice No"),Required]
        public int SaleInvoiceHeaderId { get; set; }
        public virtual SaleInvoiceHeader SaleInvoiceHeader { get; set; }

        [ForeignKey("Transporter"), Display(Name = "Transporter"),Required]
        public int TransporterId { get; set; }
        public virtual Transporter Transporter { get; set; }

        [Display(Name = "Delivery Office"),MaxLength(100)]
        public string DeliveryOffice { get; set; }

        [Display(Name = "Waybill No"), MaxLength(50),Required]
        public string WaybillNo { get; set; }

        [Display(Name = "Waybill Date"), Required]
        public DateTime WaybillDate { get; set; }

        [Display(Name = "Estd. Delivery Date"), Required]
        public DateTime EstimatedDeliveryDate { get; set; }

        [Display(Name = "Delivery Office"), MaxLength(20),Required]
        public string PaymentType { get; set; }

        [ForeignKey("FromCity"), Display(Name = "From City"), Required]
        public int FromCityId { get; set; }
        public virtual City FromCity { get; set; }

        [ForeignKey("ToCity"), Display(Name = "To City"), Required]
        public int ToCityId { get; set; }
        public virtual City ToCity { get; set; }

        [ForeignKey("Route"), Display(Name = "Route"), Required]
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        [Display(Name = "Private Mark"),MaxLength(100)]
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
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
