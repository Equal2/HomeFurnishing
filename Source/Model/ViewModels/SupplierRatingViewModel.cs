using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModels
{
    public class SupplierPurchaseOrders
    {
        public string SupplierName { get; set; }
        public int TotalPurchaseOrders { get; set; }
    }

    public class SupplierRating
    {
        public int SupplierID { get; set; }

        [Display(Name = "Supplier Name")]
        [Required]
        [MaxLength(50)]
        public string SupplierName { get; set; }

        [Display(Name = "Time")]
        public decimal Time { get; set; }

        [Display(Name = "Quality")]
        public decimal Quality { get; set; }

        [Display(Name = "Information Sharing")]
        public decimal InformationSharing { get; set; }

        [Display(Name = "Creativity")]
        public decimal Creativity { get; set; }

        [Display(Name = "Volume")]
        public decimal Volume { get; set; }

        [Display(Name = "Over All Rating")]
        public decimal OverAllRating { get; set; }
    }
}
