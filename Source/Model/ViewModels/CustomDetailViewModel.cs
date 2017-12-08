using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class CustomDetailViewModel 
    {
        public int CustomDetailId { get; set; }

        [Display(Name = "Booking Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_CustomDetail_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Booking Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Booking No"), Required, MaxLength(20)]
        [Index("IX_CustomDetail_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        [Index("IX_CustomDetail_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_CustomDetail_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public string SiteName { get; set; }


        [ForeignKey("SaleInvoiceHeader"), Display(Name = "Invoice No")]
        public int? SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceHeaderDocNo { get; set; }

        [MaxLength(20)]
        public string TRNo { get; set; }

        [Display(Name = "TR Date")]
        public DateTime? TRDate { get; set; }


        [ForeignKey("TRCourier"), Display(Name = "TR Courier Name")]
        public int? TRCourierId { get; set; }
        public string TRCourierName { get; set; }

        [Display(Name = "TR Courier Date")]
        public DateTime? TRCourierDate { get; set; }

        [Display(Name = "TR Courier Ref No"), MaxLength(50)]
        public string TRCourierRefNo { get; set; }

        [Display(Name = "Shipping Bill No"), MaxLength(50)]
        public string ShippingBillNo { get; set; }

        [Display(Name = "Shipping Bill Date")]
        public DateTime? ShippingBillDate { get; set; }

        [Display(Name = "Custom Seal No"), MaxLength(50)]
        public string CustomSealNo { get; set; }

        [Display(Name = "Line Seal No"), MaxLength(50)]
        public string LineSealNo { get; set; }

        [Display(Name = "No Of Packages")]
        public Decimal? NoOfPackages { get; set; }

        [Display(Name = "Actual Weight")]
        public Decimal? ActualWeight { get; set; }

        [Display(Name = "ChargedWeight")]
        public Decimal? ChargedWeight { get; set; }

        [Display(Name = "Container No"), MaxLength(50)]
        public string ContainerNo { get; set; }

        public int Status { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }

    public class CustomDetailViewModelWithLog : CustomDetailViewModel
    {
        [Required, MinLength(20)]
        public string LogReason { get; set; }
    }

}
