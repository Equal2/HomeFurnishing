using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surya.India.Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Surya.India.Model.ViewModels
{
    public class SaleOrderHeaderIndexViewModel
    {
         [Key]
        public int SaleOrderHeaderId { get; set; }

         [Display(Name = "Order Number"), MaxLength(20)]
         public string DocNo { get; set; }

         [Display(Name = "Sale To Buyer")]
         public string SaleToBuyer { get; set; }
        [Display(Name="Buyer Order No.")]
         public string BuyerOrderNumber { get; set; }
        [Display(Name="Bill To Buyer")]
         public string BillToBuyer { get; set; }
        [Display(Name="Ship Address")]
         public string ShipAddress { get; set; }

        [Display(Name = "Remark")]
         public string Remark { get; set; }

         [Display(Name = "Order Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
         public DateTime DocDate { get; set; }

         [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
         public DateTime DueDate { get; set; }

         public int Status { get; set; }
         public string DivisionName { get; set; }
         public string SiteName { get; set; }
         public string CreatedBy { get; set; }
         [Display(Name = "Created Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
         public DateTime CreatedDate { get; set; }
         [Display(Name = "Modified By")]
         public string ModifiedBy { get; set; }
         [Display(Name = "Modified Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
         public DateTime ModifiedDate { get; set; }
        [Display(Name="Entry Type")]
         public string DocumentTypeName { get; set; }
        [Display(Name="Currency")]
         public string CurrencyName { get; set; }
        [Display(Name="Ship Method")]
         public string ShipMethod { get; set; }
        [Display(Name="Delivery Terms")]
         public string DeliveryTerms { get; set; }

         public int PriorityId { get; set; }
         public string Priority { get; set; }
        [Required,MinLength(20)]
         public string LogReason { get; set; }
        [Display(Name="Terms & Conditions")]
        public string TAndC { get; set; }
        [Display(Name="Credit Days")]
        public int CreditDays { get; set; }

    }
}
