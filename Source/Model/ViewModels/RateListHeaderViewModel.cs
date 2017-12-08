using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RateListHeaderViewModel
    {
        public int RateListHeaderId { get; set; }

        [Display(Name = "Effective Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EffectiveDate { get; set; }

        [Display(Name = "Process"), Required]
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Name"), Required, MaxLength(50)]
        public string RateListName { get; set; }

        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }


        /// <summary>
        /// It is a list. (Qty, Deal Qty, Weight)
        /// </summary>
        [MaxLength(20), Required]
        public string CalculateWeightageOn { get; set; }

        public Decimal WeightageGreaterOrEqual { get; set; }
        public string DealUnitId { get; set; }
        public string DealUnitName { get; set; }

        [Display(Name = "Close Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ? CloseDate { get; set; }


        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Minimum Rate")]
        public decimal MinRate { get; set; }

        [Display(Name = "Maximum Rate")]
        public decimal MaxRate { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        public string PersonRateGroup { get; set; }
        public string ProductRateGroup { get; set; }

    }
}
