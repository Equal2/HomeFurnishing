using System.Collections.Generic;

namespace AdminSetup.Models.ViewModels
{
    public class SubModuleViewModel
    {
        public int ModuleId { get; set; }
        public int ModuleName { get; set; }
        public int SubModuleId { get; set; }
        public string SubModuleName { get; set; }
        public string SubModuleIconName { get; set; }
        public string Srl { get; set; }
        public List<MenuViewModel> MenuViewModel { get; set; }
    }
}
