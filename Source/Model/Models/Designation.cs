using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Designation : EntityBase, IHistoryLog
    {
        public Designation()
        {
            Employee = new List<Employee>();
        }

        [Key]
        public int DesignationId { get; set; }

        [Display(Name="Designation Name")]
        [MaxLength(50), Required]
        [Index("IX_Designation_DesignationName", IsUnique = true)]
        public string DesignationName { get; set; }

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
