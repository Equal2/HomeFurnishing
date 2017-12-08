using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class AttendanceHeaderViewModel
    {
        public int AttendanceHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public int? SiteId { get; set; }
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }

        public int Status { get; set; }


        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public bool? Reviewed { get; set; }
        public string FirstName { get; set; }
    }
}
