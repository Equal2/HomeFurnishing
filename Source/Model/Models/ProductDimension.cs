using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surya.India.Model.Models
{
    public class ProductDimension : Product
    {
        [MaxLength(2), Required]
        public string Shape { get; set; }

        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Thickness { get; set; }
        public decimal? Area { get; set; }
        public decimal? CBM { get; set; }
    }
}
