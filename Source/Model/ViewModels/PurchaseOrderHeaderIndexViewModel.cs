using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surya.India.Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Surya.India.Model.ViewModels
{
    public class PurchaseOrderHeaderIndexViewModel
    {
         public int PurchaseOrderHeaderId { get; set; }

         [Display(Name = "Order Number"), MaxLength(20)]
         public string DocNo { get; set; }

         [Display(Name="Ship Address")]
         public string ShipAddress { get; set; }

         [Display(Name = "Remark")]
         public string Remark { get; set; }

         [Display(Name = "Order Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
         public DateTime DocDate { get; set; }

         [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
         public DateTime DueDate { get; set; }
         public string Supplier { get; set; }
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

         [Display(Name="Ship Method")]
         public string ShipMethod { get; set; }
         [Display(Name="Delivery Terms")]
         public string DeliveryTerms { get; set; }

         [Display(Name="Terms & Conditions")]
         public string TAndC { get; set; }
         [Display(Name="Credit Days")]
         public int ? CreditDays { get; set; }
 
    }

    public class PurchaseOrderHeaderDocNo
    {
        public string DocNo { get; set; }
        public int PurchaseOrderLineId { get; set; }
        public int PurchaseOrderHeaderId { get; set; }
        public decimal BalanceQty { get; set; }
    }
}
