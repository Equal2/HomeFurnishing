using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    [Serializable]
    public class RolesControllerActionViewModel
    {
        public int RolesControllerActionId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int ControllerActionId { get; set; }
        public string ControllerActionName { get; set; }
        public string ControllerName { get; set; }
    }
}
