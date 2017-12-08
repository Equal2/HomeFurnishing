using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.DataBaseViews
{
    [Table("_ReportHeaders")]
    public class _ReportHeader : EntityBase
    {
        [Key]
        public int ReportHeaderId { get; set; }
        public string ReportName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string SqlProc { get; set; }
        public string Notes { get; set; }
        public int? ParentReportHeaderId { get; set; }
        public string ReportSQL { get; set; }
    }
}
