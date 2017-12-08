using Models.Reports.Models;
using System.Collections.Generic;

namespace Models.Reports.ViewModels
{
    public class ReportMasterViewModel
    {
        public int ReportHeaderId { get; set; }
        public bool closeOnSelect { get; set; }
        public ReportHeader ReportHeader { get; set; }
        public List<ReportLineViewModel> ReportLine { get; set; }

    }
}
