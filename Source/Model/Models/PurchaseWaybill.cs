using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseWaybill : EntityBase, IHistoryLog
    {
        public PurchaseWaybill()
        {
        }

        [Key]
        public int PurchaseWaybillId { get; set; }

        [Display(Name = "Waybill Type")]
        [ForeignKey("DocType")]
        [Index("IX_PurchaseWaybill_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Entry No"), MaxLength(20),Required]
        [Index("IX_PurchaseWaybill_EntryNo", IsUnique = true, Order = 2)]
        public string EntryNo { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Waybill Date"),Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Waybill No"), MaxLength(20)]
        [Index("IX_PurchaseWaybill_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [ForeignKey("Transporter"), Display(Name = "Transporter"), Required]
        public int TransporterId { get; set; }
        public virtual Transporter Transporter { get; set; }

        [ForeignKey("Consignee")]
        [Display(Name = "Consigner"),Required]
        public int ConsignerId { get; set; }
        public virtual Person Consignee { get; set; }
        [MaxLength(30)]
        public string ReferenceDocNo { get; set; }


        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_PurchaseWaybill_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_PurchaseTransportGR_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
       

        [Display(Name = "Ship Method")]
        [ForeignKey("ShipMethod"),Required]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }
        [MaxLength(100)]
        public string DeliveryPoint { get; set; }

        //[Display(Name = "Road Permit")]
        //[ForeignKey("RoadPermit")]
        //public int ? RoadPermitId { get; set; }
        //public virtual Product RoadPermit { get; set; }

        //[Display(Name = "Road Permit No")]
        //[ForeignKey("RoadPermitNo")]
        //public int ? RoadPermitNoId { get; set; }
        //public virtual ProductUid RoadPermitNo { get; set; }

        [Display(Name = "Estd. Delivery Date"),Required]
        public DateTime ?  EstimatedDeliveryDate { get; set; }
        
        [Required]
        public string FreightType { get; set; }

        [ForeignKey("FromCity"),Required]
        public int FromCityId { get; set; }
        public virtual City FromCity { get; set; }
        [ForeignKey("ToCity"),Required]
        public int ToCityId { get; set; }
        public virtual City ToCity { get; set; }

        [Display(Name = "Product Description"),MaxLength(100)]
        public string ProductDescription { get; set; }
        [MaxLength(30)]
        public string PrivateMark { get; set; }
        [Required]
        public string NoOfPackages { get; set; }

        [Display(Name = "Actual Weight"),Required]
        public Decimal ActualWeight { get; set; }

        [Display(Name = "Charged Weight")]
        public Decimal? ChargedWeight { get; set; }

        [Display(Name = "Container No."), MaxLength(20)]
        public string ContainerNo { get; set; }   

        [Display(Name = "Freight Amt")]
        public Decimal ? FreightAmt { get; set; }

        public decimal? OtherCharges { get; set; }
        public decimal? ServiceTax { get; set; }
        public decimal? ServiceTaxPer { get; set; }
        public decimal TotalAmount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Freight Description")]
        public string FreightDescription { get; set; }


        public bool IsDoorDelivery { get; set; }
        public bool IsPOD { get; set; }

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
