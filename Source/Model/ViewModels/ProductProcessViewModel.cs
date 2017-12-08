using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProductProcessViewModel
    {
        public int ProductProcessId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        public int? QAGroupId { get; set; }
        public string QAGroupName { get; set; }

        public int? Dimension1Id { get; set; }

        public int? Dimension2Id { get; set; }

        public int? Dimension3Id { get; set; }

        public int? Dimension4Id { get; set; }

        public string PurchProd { get; set; }

        public int? Sr { get; set; }

        public int? ProductRateGroupId { get; set; }
        public string ProudctRateGroupName { get; set; }
        public string Instructions { get; set; }

    }
}
