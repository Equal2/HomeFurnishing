using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ExcessJobOrderReviewViewModel
    {
        public int ProdOrderLineId { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocNo { get; set; }
        public string DocDate { get; set; }
        public string PartyName { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public decimal ? PlanQty { get; set; }
        public decimal ? JobOrderQty { get; set; }
        public decimal ? ExcessQty { get; set; }
        public bool Approved { get; set; }
    }
}
