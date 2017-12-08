using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PurchaseInvoiceReturnHeaderCharge : CalculationHeaderCharge 
    {

        [ForeignKey("PurchaseInvoiceReturnHeader")]
        public int HeaderTableId { get; set; }
        public virtual PurchaseInvoiceReturnHeader PurchaseInvoiceReturnHeader { get; set; }


    }
}
