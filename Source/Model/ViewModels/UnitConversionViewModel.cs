using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class UnitConversionViewModel
    {
        [Key]
        public int UnitConversionId { get; set; }
        
        [Display(Name = "Product")]        
        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Unit Conversion For")]
        public byte UnitConversionForId { get; set; }
        public string UnitConversionForName { get; set; }

        [Display(Name = "From Qty")]
        public decimal FromQty { get; set; }
        
        [Display(Name = "From Unit")]        
        public string FromUnitId { get; set; }
        public string FromUnitName { get; set; }

        [Display(Name = "To Qty")]
        public decimal ToQty { get; set; }
        
        [Display(Name = "To Unit")]        
        public string ToUnitId { get; set; }
        public string ToUnitName { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }

    }
}
