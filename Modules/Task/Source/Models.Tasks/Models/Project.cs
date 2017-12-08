using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tasks.Models
{
    public class Project : EntityBase, IHistoryLog
    {
        public Project()
        {
        }

        [Key]
        public int ProjectId { get; set; }
        [Display(Name = "Name")]
        [MaxLength(50, ErrorMessage = "Project Name cannot exceed 50 characters"), Required]
        [Index("IX_Project_ProjectName", IsUnique = true)]
        public string ProjectName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
