using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobOrderLineExtended : EntityBase
    {
        [Key]
        [ForeignKey("JobOrderLine")]
        [Display(Name = "JobOrderLine")]
        public int? JobOrderLineId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }

        [ForeignKey("OtherUnit"), Display(Name = "OtherUnit")]
        public string OtherUnitId { get; set; }
        public virtual Unit OtherUnit { get; set; }

        [Display(Name = "Unit Conversion Multiplier"), Required]
        public Decimal OtherUnitConversionMultiplier { get; set; }
        public decimal OtherRate { get; set; }

        [DataType(DataType.Currency)]
        public decimal OtherAmount { get; set; }
        public decimal OtherQty { get; set; }

    }
}
