using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModels
{
    public class AttendanceLinesViewModel
    {
        public int AttendanceLineId { get; set; }
        public int AttendanceHeaderId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocTime { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string AttendanceCategory { get; set; }
        public string Remark { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string OMSId { get; set; }
    }
}
