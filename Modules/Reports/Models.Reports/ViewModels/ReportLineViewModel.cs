using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Reports.ViewModels
{
    public class ReportLineViewModel
    {
        public int ReportLineId { get; set; }
        public int ReportHeaderId { get; set; }
        public string ReportHeaderName { get; set; }
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public string Type { get; set; }
        public string ListItem { get; set; }
        public string DefaultValue { get; set; }
        public Boolean IsVisible { get; set; }
        public string ServiceFuncGet { get; set; }
        public string ServiceFuncSet { get; set; }
        public string SqlProcGetSet { get; set; }
        public string SqlProcGet { get; set; }
        public string SqlProcSet { get; set; }
        public string CacheKey { get; set; }
        public int Serial { get; set; }
        public int? NoOfCharToEnter { get; set; }
        public string SqlParameter { get; set; }
        public bool IsCollapse { get; set; }
        public bool IsMandatory { get; set; }
        public string PlaceHolder { get; set; }
        public string ToolTip { get; set; }
    }
}
