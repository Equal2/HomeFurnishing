using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Model.Tasks.ViewModel
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }
        [Display(Name = "Name")]
        [MaxLength(50, ErrorMessage = "Project Name cannot exceed 50 characters"), Required]
        public string ProjectName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
    }
}
