using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tasks.Models
{
    public class Tasks : EntityBase, IHistoryLog
    {
        [Key]
        public int TaskId { get; set; }

        [Display (Name="Title")]
        [MaxLength(250, ErrorMessage = "Task Name cannot exceed 250 characters"), Required]
        public string TaskTitle { get; set; }

        public string TaskDescription { get; set; }

        [ForeignKey("Project"), Display(Name = "Project")]
        public int ? ProjectId { get; set; }
        public virtual Project Project { get; set; }

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
