using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SubReport : EntityBase, IHistoryLog
    {       
        [Key]
        public int SubReportId { get; set; }
        public string SubReportName { get; set; }
        public string SqlProc { get; set; }

        [ForeignKey("ReportHeader")]
        public int ReportHeaderId { get; set; }
        public virtual ReportHeader ReportHeader { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

    }
}
