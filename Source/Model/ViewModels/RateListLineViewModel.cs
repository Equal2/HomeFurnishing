using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RateListLineViewModel
    {
        public int RateListLineId { get; set; }


        [Display(Name = "RateListHeader")]
        public int RateListHeaderId { get; set; }
        public string RateListName { get; set; }

        [Display(Name = "Person Rate Group")]
        public int? PersonRateGroupId { get; set; }
        public string PersonRateGroupName { get; set; }

        [Display(Name = "Product Rate Group")]
        public int? ProductRateGroupId { get; set; }
        public string ProductRateGroupName { get; set; }

        [Display(Name = "Product")]
        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public Decimal Rate { get; set; }

        public Decimal Discount { get; set; }

        public Decimal Incentive { get; set; }

        public Decimal Loss { get; set; }

        public Decimal UnCountedQty { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public decimal WeightageGreaterOrEqual { get; set; }

    }
}
