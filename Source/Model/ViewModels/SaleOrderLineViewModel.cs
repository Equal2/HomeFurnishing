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
    public class SaleOrderLineViewModel
    {
        [Key]
        public int SaleOrderLineId { get; set; }

        [ForeignKey("SaleOrderHeader")]
        public int SaleOrderHeaderId { get; set; }
        public virtual SaleOrderHeader SaleOrderHeader { get; set; }
        public string SaleOrderDocNo { get; set; }

        [ForeignKey("Product"), Display(Name = "Product"),Required(ErrorMessage="Please select a Product")]
        public int ? ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set; }

        [MaxLength(50)]
        public string Specification { get; set; }
        [Required]
        public decimal ? Qty { get; set; }

        [Display(Name = "Due Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ? DueDate { get; set; }

        [ForeignKey("DeliveryUnit"), Display(Name = "Delivery Unit")]
        public string DealUnitId { get; set; }
        
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Delivery Qty"),Required]
        public decimal ? DealQty { get; set; }
        [Required]
        public decimal ? Rate { get; set; }
        [Required]
        public decimal ? Amount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public SaleOrderSettingsViewModel SaleOrderSettings { get; set; }
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
        public Decimal ? UnitConversionMultiplier { get; set; }
        public string BuyerSku { get; set; }

        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }

        public int? StockId { get; set; }
    }

    public class SaleOrderLineIndexViewModel : SaleOrderLineViewModel
    {
        public string SaleOrderHeaderDocNo { get; set; }
        public int ProgressPerc { get; set; }
        public int unitDecimalPlaces { get; set; }



    }
    public class SaleOrderLineBalance
    {
        public int SaleOrderLineId { get; set; }
        public string SaleOrderDocNo { get; set; }

    }

    public class PendingSaleOrderFromProc
    {


        public int SaleOrderLineId { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal Rate { get; set; }
        public decimal BalanceAmount { get; set; }
        public int SaleOrderHeaderId { get; set; }
        public string SaleOrderNo { get; set; }
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public int ProductGroupId { get; set; }
        public string UnitName { get; set; }
        public string Specification { get; set; }

        public bool BomDetailExists { get; set; }

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

        public int? Sr { get; set; }

    }
}
