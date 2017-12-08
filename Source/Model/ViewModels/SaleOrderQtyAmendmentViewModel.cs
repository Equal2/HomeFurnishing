using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SaleOrderQtyAmendmentLineViewModel
    {

        public int SaleOrderQtyAmendmentLineId { get; set; }
        
        public int SaleOrderAmendmentHeaderId { get; set; }
        public string SaleOrderAmendmentHeaderDocNo { get; set; }
        
        public int SaleOrderLineId { get; set; }
        public string SaleOrderDocNo { get; set; }

        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public decimal CurrentQty { get; set; }
        public decimal Qty { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string UnitId { get; set; }
        public string LockReason { get; set; }
    }
}
