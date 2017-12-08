
namespace Models.Reports.ViewModels
{
    public class ReportHeaderViewModel
    {
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
