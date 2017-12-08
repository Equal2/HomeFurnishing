using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleInvoiceHeaderDetail : SaleInvoiceHeader
    {


        [Display(Name = "BL No.")]
        [MaxLength(20)]
        public string BLNo { get; set; }

        [Display(Name = "BL Date")]
        public DateTime BLDate { get; set; }

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

        [Display(Name = "Invoice Amount")]
        public Decimal? InvoiceAmount { get; set; }

        [Display(Name = "Freight")]
        public Decimal? Freight { get; set; }

        [Display(Name = "Freight Remark")]
        public string FreightRemark { get; set; }

        [Display(Name = "Insurance")]
        public Decimal? Insurance { get; set; }

        [Display(Name = "Insurance Remark")]
        public string InsuranceRemark { get; set; }

        [Display(Name = "Vehicle No")]
        public string VehicleNo { get; set; }
        public Decimal? Deduction { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
