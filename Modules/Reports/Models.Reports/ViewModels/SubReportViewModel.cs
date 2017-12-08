using System.ComponentModel.DataAnnotations;

namespace Models.Reports.ViewModels
{
    public class SubReportViewModel
    {
        public int SubReportId { get; set; }
        public string SubReportName { get; set; }
        public string SqlProc { get; set; }
        public int ReportHeaderId { get; set; }
        public string ReportHeaderName { get; set; }
    }
}
