using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;

namespace Model.ViewModel
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
