using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Model.Tasks.ViewModel
{
    public class DARViewModel
    {
        public int DARId { get; set; }
        public DateTime DARDate { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:#.##}")]
        public decimal WorkHours { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int? Priority { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }

        [Display(Name = "For User")]
        public string ForUser { get; set; }
        public DateTime? DueDate { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
    }
}
