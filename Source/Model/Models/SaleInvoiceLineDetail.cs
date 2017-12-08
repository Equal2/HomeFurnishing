using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleInvoiceLineDetail : EntityBase
    {
        [Key]

        [ForeignKey("SaleInvoiceLine")]
        [Display(Name = "SaleInvoiceLine")]
        public int SaleInvoiceLineId { get; set; }
        public virtual SaleInvoiceLine SaleInvoiceLine { get; set; }

        public Decimal? RewardPoints { get; set; }



    }
}

