using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Model.Tasks.ViewModel
{
    public class TasksViewModel
    {
        public int TaskId { get; set; }

        [Display(Name = "Title")]
        [MaxLength(250, ErrorMessage = "Task Name cannot exceed 250 characters"), Required]
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public int ? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? Priority { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }

        [Display(Name = "For User")]
        public string ForUser { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
