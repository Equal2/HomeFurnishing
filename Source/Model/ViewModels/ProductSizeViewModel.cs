using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class ProductSizeViewModel 
    {
        [Key]
        public int ProductSizeId { get; set; }

        [ForeignKey("ProductSizeType")]
        [Display(Name = "ProductSizeType")]
        public int ProductSizeTypeId { get; set; }
        public virtual ProductSizeType ProductSizeType { get; set; }
        public string ProductSizeTypeName { get; set; }

        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string ProductName { get; set; }

        [ForeignKey("Size")]
        [Display(Name = "Size")]
        public int SizeId { get; set; }
        public virtual Size Size { get; set; }
        public string SizeName { get; set; }
    }

    public class ProductSizeIndexViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductStencilId { get; set; }
        public string ProductStencilSizeName { get; set; }
        public int ProductMapSizeId { get; set; }
        public string ProductMapSizeName { get; set; }
        public int ProductStandardSizeId { get; set; }
        public string ProductStandardSizeName { get; set; }
        public int ProductManufacturingSizeId { get; set; }
        public string ProductManufacturingSizeName { get; set; }
        public int FinishingSizeId { get; set; }
        public string FinishingSizeName { get; set; }
        public string ShapeName { get; set; }
        public decimal ? StandardWeight { get; set; }
    }
}
