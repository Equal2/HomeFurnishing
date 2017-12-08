using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class SaleQuotationLineViewModel
    {
        [Key]
        public int SaleQuotationLineId { get; set; }

        [ForeignKey("SaleQuotationHeader")]
        public int SaleQuotationHeaderId { get; set; }
        public virtual SaleQuotationHeader SaleQuotationHeader { get; set; }
        public string SaleQuotationDocNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product"),Required(ErrorMessage="Please select a Product")]
        public int  ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set; }

        [MaxLength(50)]
        public string Specification { get; set; }
        [Required]
        public decimal  Qty { get; set; }

        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Delivery Qty"),Required]
        public decimal  DealQty { get; set; }
        [Required]
        public decimal  Rate { get; set; }
        [Required]
        public decimal  Amount { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public SaleQuotationSettingsViewModel SaleQuotationSettings { get; set; }
        public DocumentTypeSettingsViewModel DocumentTypeSettings { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Unit")]
        public string UnitId { get; set; }
        public string UnitName { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal  UnitConversionMultiplier { get; set; }

        public byte UnitDecimalPlaces { get; set; }
        public byte DealUnitDecimalPlaces { get; set; }
        public string BuyerSku { get; set; }

        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }

        public int? SaleEnquiryLineId { get; set; }
        public string SaleEnquiryDocNo { get; set; }

        public string LockReason { get; set; }

        public int Sr { get; set; }

        public int? EnquiryDocTypeId { get; set; }
        public int? EnquiryHeaderId { get; set; }

        public Decimal? SaleEnquiryBalanceQty { get; set; }
        public List<SaleQuotationLineCharge> linecharges { get; set; }
        public List<SaleQuotationHeaderCharge> footercharges { get; set; }

        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site"), Required]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public bool IsRefBased { get; set; }
        public bool UnitConversionException { get; set; }

        public int? SalesTaxGroupProductId { get; set; }
        public int? SalesTaxGroupPersonId { get; set; }


    }

    public class SaleQuotationLineIndexViewModel : SaleQuotationLineViewModel
    {
        public string SaleQuotationHeaderDocNo { get; set; }
        public int ProgressPerc { get; set; }
        public int unitDecimalPlaces { get; set; }



    }


    [Serializable]
    public class SaleEnquiryHeaderListViewModel
    {
        public int SaleEnquiryHeaderId { get; set; }
        public int SaleEnquiryLineId { get; set; }
        public string DocNo { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string Dimension3Name { get; set; }
        public string Dimension4Name { get; set; }
    }

}
