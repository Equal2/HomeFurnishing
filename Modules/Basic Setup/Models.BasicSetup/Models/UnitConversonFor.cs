using Model;
using System.ComponentModel.DataAnnotations;

namespace Models.BasicSetup.Models
{
    public class UnitConversionFor : EntityBase
    {
        [Key]
        public byte UnitconversionForId { get; set; }

        [MaxLength(50)]
        public string UnitconversionForName { get; set; }
    }
}
