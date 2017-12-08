using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.DataBaseViews
{
    [Table("_ReportLines")]
    public class _ReportLine : EntityBase
    {
        [Key]
        public int ReportLineId { get; set; }
        public int ReportHeaderId { get; set; }
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string DataType { get; set; }
        public string Type { get; set; }
        public string ListItem { get; set; }
        public string DefaultValue { get; set; }
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
