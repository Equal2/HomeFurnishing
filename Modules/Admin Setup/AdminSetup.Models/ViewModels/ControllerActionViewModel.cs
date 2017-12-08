using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminSetup.Models.ViewModels
{
    public class ControllerActionList
    {
        public int? ControllerId { get; set; }
        public int ActionId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool IsActive { get; set; }
    }

    public class ControllerActionViewModel
    {
        public int ControllerActionId { get; set; }
        public int? ControllerId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string PubModuleName { get; set; }
        public Boolean IsActive { get; set; }
    }
}
