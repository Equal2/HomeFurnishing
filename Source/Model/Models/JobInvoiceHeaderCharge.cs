using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceHeaderCharge : CalculationHeaderCharge 
    {

        [ForeignKey("JobInvoiceHeader")]
        public int HeaderTableId { get; set; }
        public virtual JobInvoiceHeader JobInvoiceHeader { get; set; }


    }
}
