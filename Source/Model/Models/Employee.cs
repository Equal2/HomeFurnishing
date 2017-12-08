using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Employee : EntityBase
    {
        public Employee()
        {
           
        }

        [Key]
        [ForeignKey("Person"), Display(Name = "Person")]
        public int PersonID { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("Designation"), Display(Name = "Designation")]
        public int DesignationID { get; set; }
        public virtual Designation Designation { get; set; }


        [ForeignKey("Department"), Display(Name = "Department")]
        public int DepartmentID { get; set; }
        public virtual Department Department { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
