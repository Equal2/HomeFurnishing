
namespace Model.ViewModels
{
    public class ImportHeaderViewModel
    {
        public int ImportHeaderId { get; set; }
        public string ImportName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string SqlProc { get; set; }
        public string Notes { get; set; }
        public int? ParentImportHeaderId { get; set; }
        public string ImportSQL { get; set; }
    }
}
