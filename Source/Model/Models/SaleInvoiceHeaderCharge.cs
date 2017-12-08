using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleInvoiceHeaderCharge : CalculationHeaderCharge 
    {

        [ForeignKey("SaleInvoiceHeader")]
        public int HeaderTableId { get; set; }
        public virtual SaleInvoiceHeader SaleInvoiceHeader { get; set; }


    }
}
