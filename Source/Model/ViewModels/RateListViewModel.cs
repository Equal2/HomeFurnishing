using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RateListViewModel
    {
        public int ? RateListId { get; set; }

        [Display(Name = "WEF"), Required]
        public DateTime ? WEF { get; set; }

        [Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public string PersonRateGroupName { get; set; }

        [Display(Name = "Document Type")]
        public int? DocTypeId { get; set; }
        public string DocTypeName { get; set; }

        public int? DocId { get; set; }

        [Display(Name = "Product")]
        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        public Decimal WeightageGreaterOrEqual { get; set; }

        public Decimal ? Rate { get; set; }
        
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }
        public string Design { get; set; }
        public int ProductGroupId { get; set; }

        public Decimal ? Loss { get; set; }

        public Decimal ? UnCountedQty { get; set; }

    }
}
