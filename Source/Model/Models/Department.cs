using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Department : EntityBase, IHistoryLog
    {
        public Department()
        {
            Employee = new List<Employee>();
        }

        [Key]
        public int DepartmentId { get; set; }

        [Display(Name="Department Name")]
        [MaxLength(50), Required]
        [Index("IX_Department_DepartmentName", IsUnique = true)]
        public string DepartmentName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }


        public ICollection<Employee> Employee { get; set; }
        
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
