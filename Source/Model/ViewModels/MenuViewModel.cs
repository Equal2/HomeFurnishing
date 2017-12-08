using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class MenuViewModel
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string IconName { get; set; }

        public string Description { get; set; }

        public int ModuleId { get; set; }

        public int SubModuleId { get; set; }

        public int? ControllerActionId { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string RouteId { get; set; }

        public string AreaName { get; set; }
        
        public bool BookMarked { get; set; }
        public bool PermissionAssigned { get; set; }

        public string Srl { get; set; }

        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters")]
        public string URL { get; set; }
        public bool ? IsVisible { get; set; }

    }


    public class MenuModuleViewModelList
    {
        public List<MenuModouleViewModel> MenuModule { get; set; }

        public bool ? RoleModification { get; set; }
        public string RoleId { get; set; }


    }

}
