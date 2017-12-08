using Models.BasicSetup.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderHeaderCharge : CalculationHeaderCharge 
    {

        [ForeignKey("JobOrderHeader")]
        public int HeaderTableId { get; set; }
        public virtual JobOrderHeader JobOrderHeader { get; set; }

    }
}
