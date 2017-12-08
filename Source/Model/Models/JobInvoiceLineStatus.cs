using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class JobInvoiceLineStatus : EntityBase
    {
        [Key]
        [ForeignKey("JobInvoiceLine")]
        [Display(Name = "JobInvoiceLine")]
        public int? JobInvoiceLineId { get; set; }
        public virtual JobInvoiceLine JobInvoiceLine { get; set; }


        public Decimal? ReturnQty { get; set; }
        public Decimal? ReturnDealQty { get; set; }
        public Decimal? ReturnWeight { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}

