using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Surya.India.Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Surya.India.Model.ViewModels
{
    public class PurchaseOrderLineIndexViewModel
    {
        [Key]
        public int PurchaseOrderLineId { get; set; }
        
        public int PurchaseOrderHeaderId { get; set; }

        [Display(Name = "Product")]
        public string Product { get; set; }

        [DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ? DueDate { get; set; }

        public decimal Qty { get; set; }        

        public decimal ? Amount { get; set; }
        public decimal DeliveryQty { get; set; }
        public string DeliveryUnit { get; set; }
        public decimal ? Rate { get; set; }
        public string PurchaseOrderHeaderDocNo { get; set; }
        public string Remark { get; set; }

    }
}
