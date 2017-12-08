using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class SaleOrderPrintViewModel
    {
        [Key]
        public int SaleOrderHeaderId { get; set; }
        
        [Display(Name = "Document Type")]
        public string DocumentTypeName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Order Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Order No")]
        public string DocNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Buyer Order No")]
        public string BuyerOrderNo { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Sale To Buyer")]
        public string SaleToBuyer { get; set; }

        [Display(Name = "Bill To Buyer")]
        public string BillToBuyer { get; set; }

        [Display(Name = "Ship Address")]
        public string ShipAddress { get; set; }

        [Display(Name = "Currency")]
        public string Currency { get; set; }

        [Display(Name = "Ship Method")]
        public string ShipMethodName { get; set; }

        [Display(Name = "Delivery Terms")]
        public string DeliveryTermsName { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; }

        public int? CreditDays { get; set; }

        [Display(Name = "Term & Conditions")]
        public string TermAndConditions { get; set; }

        public int? SaleOrderLineId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        public Decimal?  Qty { get; set; }

        public Decimal? DeliveryQty { get; set; }

        public Decimal? AreaInSqYard { get; set; }

        public Decimal? RatePerPcs { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Due Date")]
        public DateTime? LineDueDate { get; set; }

        public Decimal? Rate { get; set; }

        public Decimal? Amount { get; set; }
             

        [Display(Name = "Remark")]
        public string LineRemark { get; set; }

        [Display(Name = "Unit")]
        public string UnitName { get; set; }

        public byte? DecimalPlaces { get; set; }

        [Display(Name = "Delivery Unit")]
        public string DeliveryUnit { get; set; }

        public byte? DeliveryUnitDecimalPlace { get; set; }

        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "CurrentStatus")]
        public string CurrentStatus { get; set; }

        [Display(Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "ApproveBy")]
        public string ApproveBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "ApproveDate")]
        public DateTime? ApproveDate { get; set; }

    }
}
