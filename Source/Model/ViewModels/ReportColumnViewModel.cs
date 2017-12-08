using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ReportColumnViewModel
    {
        public int ReportColumnId { get; set; }
        public int ReportHeaderId { get; set; }
        public string ReportHeaderName { get; set; }
        public int SubReportId { get; set; }
        public string SubReportName { get; set; }

        public int? SubReportHeaderId { get; set; }
        public string SubReportHeaderName { get; set; }

        [Display(Name = "Display Name"), Required]
        public string DisplayName { get; set; }

        [Display(Name = "Field Name"), Required]
        public string FieldName { get; set; }
        public bool IsVisible { get; set; }
        public int ? width { get; set; }
        public int ? minWidth { get; set; }
        public string AggregateFunc { get; set; }
        public string TestAlignment { get; set; }
        public string field { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public bool hasTotal { get; set; }
        public bool hasTotalName { get; set; }
        public string cssClass { get; set; }
        public string headerCssClass { get; set; }
        public int Sr { get; set; }
        public bool IsDocNo { get; set; }
    }
}
