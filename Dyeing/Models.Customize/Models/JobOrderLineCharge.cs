using Models.BasicSetup.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderLineCharge : CalculationLineCharge
    {
        [ForeignKey("JobOrderLine")]
        public int LineTableId { get; set; }
        public virtual JobOrderLine JobOrderLine { get; set; }

    }
}
