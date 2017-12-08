using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class AttendanceLine : EntityBase, IHistoryLog
    {

        [Key]
        public int AttendanceLineId { get; set; }


        [ForeignKey("AttendanceHeader")]
        public int AttendanceHeaderId { get; set; }
        public virtual AttendanceHeader AttendanceHeader { get; set; }


        public DateTime DocTime { get; set; }


        [ForeignKey("Person")]
        [Display(Name = "Employee Name")]
        public int EmployeeId { get; set; }
        public virtual Person Person { get; set; }


        [MaxLength(1)]
        public string AttendanceCategory { get; set; }


        [MaxLength(50)]
        public string Remark { get; set; }


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
