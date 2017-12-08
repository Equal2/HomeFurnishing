using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class AttendanceHeader : EntityBase, IHistoryLog
    {

        [Key]
        [Display(Name = "Attendance Id")]
        public int AttendanceHeaderId { get; set; }

        [Display(Name = "Attendance Type"), Required]
        [ForeignKey("DocType")]
        [Index("IX_AttendanceHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Attendance Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocDate { get; set; }

        [Display(Name = "Attendance No"), Required, MaxLength(20)]
        [Index("IX_AttendanceHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [ForeignKey("Department")]
        [Display(Name = "Department Name")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public int ShiftId { get; set; }
        public Shift Shift { get; set; }

        [ForeignKey("Person")]
        [Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }


        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
    }
}
