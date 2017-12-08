using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class OverTimeApplicationLine : EntityBase, IHistoryLog
    {

        [Key]        
        public int OverTimeApplicationLineId { get; set; }

        
        [ForeignKey("OverTimeApplicationHeader")]
        public int OverTimeApplicationHeaderId { get; set; }
        public virtual OverTimeApplicationHeader  OverTimeApplicationHeader { get; set; }


        [ForeignKey("Employee")]
        [Display(Name = "Employee Name")]
        public int EmployeeId { get; set; }
        public virtual Person Employee { get; set; }


        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime  CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime  ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
