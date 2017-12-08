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
    public class SaleEnquiryLineViewModel
    {
        [Key]
        public int SaleEnquiryLineId { get; set; }

        [ForeignKey("SaleEnquiryHeader")]
        public int SaleEnquiryHeaderId { get; set; }
        public virtual SaleEnquiryHeader SaleEnquiryHeader { get; set; }
        public string SaleEnquiryDocNo { get; set; }

        public int? SaleToBuyerId { get; set; }
        public string SaleToBuyerName { get; set; }


        [ForeignKey("Product"), Display(Name = "Product")]
        public int ? ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set; }

        [MaxLength(50)]
        public string Specification { get; set; }
        public decimal Qty { get; set; }

        [Display(Name = "Due Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ? DueDate { get; set; }


        [ForeignKey("DeliveryUnit"), Display(Name = "Delivery Unit")]
        public string DealUnitId { get; set; }
        
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Delivery Qty")]
        public decimal ? DealQty { get; set; }
        public decimal ? Rate { get; set; }
        public decimal ? Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public SaleEnquirySettingsViewModel SaleEnquirySettings { get; set; }
        public ProductBuyerSettingsViewModel ProductBuyerSettings { get; set; }

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
        public Decimal ? UnitConversionMultiplier { get; set; }
        public string BuyerSku { get; set; }
        public string BuyerUpcCode { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }


        public string BuyerSpecification { get; set; }
        public string BuyerSpecification1 { get; set; }
        public string BuyerSpecification2 { get; set; }
        public string BuyerSpecification3 { get; set; }
        public string LockReason { get; set; }
    }

    public class SaleEnquiryLineIndexViewModel : SaleEnquiryLineViewModel
    {
        public string SaleEnquiryHeaderDocNo { get; set; }
        public DateTime SaleEnquiryHeaderDocDate { get; set; }
        public int ProgressPerc { get; set; }
        public int unitDecimalPlaces { get; set; }


    }
    public class SaleEnquiryLineBalance
    {
        public int SaleEnquiryLineId { get; set; }
        public string SaleEnquiryDocNo { get; set; }

    }

    public class PendingSaleEnquiryFromProc
    {

        public int SaleEnquiryLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleEnquiryHeaderId { get; set; }
        public string SaleEnquiryNo { get; set; }
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public int ProductGroupId { get; set; }
        public string UnitName { get; set; }
        public string Specification { get; set; }

        [Display(Name = "Dimension1")]
        [ForeignKey("Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        [ForeignKey("Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        public string Dimension2Name { get; set; }



    }
}
