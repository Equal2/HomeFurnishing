using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RugStencilViewModel
    {
       
       
        public int StencilSizeId { get; set; }
        public string StencilSizeName { get; set; }

        public bool IsActive { get; set; }
        [Key]
        [Display(Name = "Stencil")]
        public int StencilId { get; set; }
        public virtual Product Product { get; set; }
        public string StencilName { get; set; }

        [Display(Name = "Design")]
        public int ProductDesignId { get; set; }
        public virtual ProductDesign ProductDesign { get; set; }
        public string ProductDesignName { get; set; }
        public int DivisionId { get; set; }

        [Display(Name = "Product Size")]
        public int ProductSizeId { get; set; }
        public virtual ProductSize ProductSize { get; set; }
        public string ProductSizeName { get; set; }

        [MaxLength(10)]
        public string FullHalf { get; set; }

    }
}
