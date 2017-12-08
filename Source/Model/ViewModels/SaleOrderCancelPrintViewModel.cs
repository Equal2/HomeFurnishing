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
    public class SaleOrderCancelPrintViewModel
    {
        [Key]
        public int SaleOrderCancelHeaderId { get; set; }
        
        [Display(Name = "Document Type")]
        public string DocumentTypeName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Cancel Date"), Required]
        public DateTime DocDate { get; set; }

        [Display(Name = "Cancel No")]
        public string DocNo { get; set; }

        [Display(Name = "Reason")]
        public string ReasonName { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        public int? SaleOrderCancelLineId { get; set; }

        [Display(Name = "Sale Order No")]
        public string SaleOrderNo { get; set; }

        [Display(Name = "Buyer")]
        public string BuyerName { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        public Decimal?  Qty { get; set; }

        [Display(Name = "Line Remark")]
        public string LineRemark { get; set; }

        [Display(Name = "UnitName")]
        public string UnitName { get; set; }

        public byte? DecimalPlaces { get; set; }

        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "CreatedDate")]
        public DateTime? CreatedDate { get; set; }

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
