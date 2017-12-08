using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PurchaseWaybillViewModel
    {
        public int PurchaseWaybillId { get; set; }
        public int DocTypeId { get; set; }
        public string DocumentTypeName{ get; set; }

        [Display(Name = "Entry No"), MaxLength(20), Required]        
        public string EntryNo { get; set; }

        [DataType(DataType.Date)]        
        [Display(Name = "Waybill Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Waybill No"), MaxLength(20),Required]        
        public string DocNo { get; set; }

        [Display(Name = "Transporter"), Required,Range(1,int.MaxValue,ErrorMessage="The Transporter field is required")]
        public int TransporterId { get; set; }
        public string TransporterName{ get; set; }

        
        [Display(Name = "Consigner"), Required,Range(1,int.MaxValue,ErrorMessage="The Consigner field is required")]
        public int ConsignerId { get; set; }
        public string ConsigneeName { get; set; }
        [MaxLength(30)]
        public string ReferenceDocNo { get; set; }


        [Display(Name = "Division"), Required]               
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]                
        public int SiteId { get; set; }
        public string SiteName { get; set; }


        [Display(Name = "Ship Method")]
        [Required]
        public int ShipMethodId { get; set; }
        public string ShipMethodName { get; set; }
        [MaxLength(100)]
        public string DeliveryPoint { get; set; }

        [Display(Name = "Estd. Delivery Date"), Required]
        public DateTime? EstimatedDeliveryDate { get; set; }

        [Required]
        public string FreightType { get; set; }

        [Required,Range(1,int.MaxValue,ErrorMessage="The FromCity field is required")]
        public int FromCityId { get; set; }
        public string  FromCityName { get; set; }
        [Required,Range(1,int.MaxValue,ErrorMessage="The ToCity field is required")]
        public int ToCityId { get; set; }
        public string ToCityName { get; set; }

        [Display(Name = "Product Description"), MaxLength(100)]
        public string ProductDescription { get; set; }
        [MaxLength(30)]
        public string PrivateMark { get; set; }
        [Display(Name = "Freight Description")]
        public string FreightDescription { get; set; }
        [Required]
        public string NoOfPackages { get; set; }

        [Display(Name = "Actual Weight"), Required,Range(1,int.MaxValue,ErrorMessage="The Actual weight field is required")]
        public Decimal ActualWeight { get; set; }

        [Display(Name = "Charged Weight")]
        public Decimal? ChargedWeight { get; set; }

        [Display(Name = "Container No."), MaxLength(20)]
        public string ContainerNo { get; set; }

        [Display(Name = "Freight Amt")]
        public Decimal? FreightAmt { get; set; }

        public decimal? OtherCharges { get; set; }
        public decimal? ServiceTax { get; set; }
        public decimal? ServiceTaxPer { get; set; }
        public decimal TotalAmount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public bool IsDoorDelivery { get; set; }
        public bool IsPOD { get; set; }

    }
}
