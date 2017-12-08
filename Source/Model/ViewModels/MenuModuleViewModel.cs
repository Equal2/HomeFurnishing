using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    [Serializable]
    public class MenuModouleViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public int Srl { get; set; }

        [Display(Name = "Icon Name")]
        public string IconName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "URL")]
        public string URL { get; set; }
        public int SelectedSubModuleId { get; set; }
    }
}
