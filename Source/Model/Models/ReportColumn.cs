using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ReportColumn : EntityBase, IHistoryLog
    {
        [Key]
        public int ReportColumnId { get; set; }

        [ForeignKey("ReportHeader")]
        public int ReportHeaderId { get; set; }
        public virtual ReportHeader ReportHeader { get; set; }

        [ForeignKey("SubReport")]
        public int SubReportId { get; set; }
        public virtual SubReport SubReport { get; set; }

        [ForeignKey("SubReportHeader")]
        public int ? SubReportHeaderId { get; set; }
        public virtual SubReport SubReportHeader { get; set; }

        [Display(Name="Display Name"), Required]
        public string DisplayName { get; set; }

        [Display(Name = "Field Name"), Required]
        public string FieldName { get; set; }
        public Boolean IsVisible { get; set; }
        public string width { get; set; }
        public string minWidth { get; set; }
        public string AggregateFunc { get; set; }
        public string TestAlignment { get; set; }
        public int Serial { get; set; }
        public bool IsDocNo { get; set; }
        
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
