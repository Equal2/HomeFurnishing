using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProductBuyerViewModel
    {
        public int ProductBuyerId { get; set; }
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        [Display(Name = "Buyer")]
        public int BuyerId { get; set; }
        public string BuyerName  { get; set; }

        [MaxLength(50)]
        public string BuyerSku { get; set; }

        [MaxLength(50)]
        public string BuyerProductCode { get; set; }

        [MaxLength(20)]
        public string BuyerUpcCode { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification1 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification2 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification3 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification4 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification5 { get; set; }

        [MaxLength(50)]
        public string BuyerSpecification6 { get; set; }

        public ProductBuyerSettingsViewModel ProductBuyerSettings { get; set; }

    }
}
