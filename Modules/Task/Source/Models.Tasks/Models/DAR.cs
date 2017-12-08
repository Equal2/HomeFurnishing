using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Tasks.Models
{
    public class DAR : EntityBase, IHistoryLog
    {


        [Key]
        public int DARId { get; set; }

        public DateTime DARDate { get; set; }

        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:#.##}")]
        public decimal WorkHours { get; set; }


        [ForeignKey("Task"), Display(Name = "Task")]
        public int TaskId { get; set; }
        public virtual Tasks Task { get; set; }

        public string Description { get; set; }


        public int? Priority { get; set; }

        [MaxLength(20)]        
        public string Status { get; set; }

        [Display(Name = "For User")]
        public string ForUser { get; set; }

        public DateTime? DueDate { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
