using ProjLib.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace PermissionValidator
{
    public static class ValidateData
    {
        public static bool ValidateUserPermission(string ActionName, string ControllerName)
        {
            bool Temp = false;
            if (System.Web.HttpContext.Current.Session["CAPermissionsCacheKeyHint"] != null)
            {
                var CacheData = (List<RolesControllerActionViewModel>)System.Web.HttpContext.Current.Session["CAPermissionsCacheKeyHint"];

                Temp = (from p in CacheData
                        where p.ControllerName == ControllerName && p.ControllerActionName == ActionName
                        select p).Any();
            }
            return Temp;
        }
    }
}
