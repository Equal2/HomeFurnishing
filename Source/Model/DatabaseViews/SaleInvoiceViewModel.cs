using Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{
    [Table("ViewSaleInvoicePendingForCustomDetail")]
    public class ViewSaleInvoicePendingForCustomDetail
    {
        [Key]
        public int SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceNo { get; set; }
        public DateTime DocDate { get; set; }
    }


}
