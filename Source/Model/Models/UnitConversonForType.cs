using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class UnitConversionFor
    {
        [Key]
        public byte UnitconversionForId { get; set; }

        [MaxLength(50)]
        public string UnitconversionForName { get; set; }
    }
}
