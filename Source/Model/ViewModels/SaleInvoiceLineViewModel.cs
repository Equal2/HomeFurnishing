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
    public class SaleInvoiceLineViewModel
    {
        [Key]
        public int SaleInvoiceLineId { get; set; }

        public int SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceHeaderDocNo { get; set; }
        public string SaleOrderHeaderDocNo { get; set; }

        public int SaleDispatchLineId { get; set; }

        public int SaleDispatchHeaderId { get; set; }

        public string SaleDispatchDocNo { get; set; }
        public string ProductUidIdName { get; set; }

        public int? ProductUidId { get; set; }

        public int PackingLineId { get; set; }

        public int? SaleOrderLineId { get; set; }
        public string Specification { get; set; }
        public string UnitId { get; set; }
        public Decimal DealQty { get; set; }

        public string SaleOrderDealUnitId { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public int DealUnitDecimalPlaces { get; set; }
        public int UnitDecimalPlaces { get; set; }

        [Display(Name = "Unit Conversion Multiplier")]
        public Decimal? UnitConversionMultiplier { get; set; }

        public string LotNo { get; set; }

        [Display(Name = "Product"), Required(ErrorMessage = "Please select a Product")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }


        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }


        [Display(Name = "Godown"),Required(ErrorMessage="Please select a Godown")]
        public int GodownId { get; set; }
        public string GodownName { get; set; }


        public string DescriptionOfGoodsName { get; set; }

        public string FaceContentName { get; set; }

        public int? ProductInvoiceGroupId { get; set; }
        public string ProductInvoiceGroupName { get; set; }

        public int? SalesTaxGroupProductId { get; set; }
        public string SalesTaxGroupProductName { get; set; }

        public string BaleNo { get; set; }

        [Display(Name = "Qty")]
        public Decimal Qty { get; set; }

        [Display(Name = "Rate")]
        public Decimal Rate { get; set; }

        [Display(Name = "Sale Order Rate")]
        public Decimal SaleOrderRate { get; set; }


        [Display(Name = "Amount"), Required]
        public Decimal Amount { get; set; }

        [Display(Name = "Discount %")]
        public Decimal? DiscountPer { get; set; }

        [Display(Name = "Discount Amount")]
        public Decimal? DiscountAmount { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Rate Remark")]
        public string RateRemark { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }   
}
