using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Service;
using AdminSetup.Models.ViewModels;

namespace Presentation.Controllers
{
    [Authorize]
    public class MenuController : Controller
    {
        IModuleService _ModuleService;
        ISubModuleService _SubModuleService;
        IMenuService _menuService;
        public MenuController(IModuleService mService, ISubModuleService serv, IMenuService menuService)
        {
            _ModuleService = mService;
            _SubModuleService = serv;
            _menuService = menuService;
        }

        [Authorize]
        public ActionResult Module()
        {
            var Moduleslist = _ModuleService.GetModules();

            if (Moduleslist.MenuModule.Count() == 1)
            {
                return RedirectToAction("SubModule", new { id = Moduleslist.MenuModule.FirstOrDefault().ModuleId });
            }
            else
                return View("Module", Moduleslist);
        }


        public ActionResult UserPermissions(string RoleId)
        {
            var UserPermModules = _ModuleService.GetUserPermission(RoleId);
            return View("Module", UserPermModules);
        }

        [Authorize]
        public ActionResult SubModule(int id, bool? RolePerm)//ModuleId
        {
            var SubModulelist = _SubModuleService.GetSubModule(id, RolePerm, User.Identity.Name);

            var tem = _ModuleService.Find(id);
            ViewBag.MName = tem.ModuleName;
            ViewBag.IconName = tem.IconName;
            ViewBag.RolePermissions = RolePerm ?? false;

            return View("SubModule", SubModulelist);
        }

        [Authorize]
        public ActionResult MenuSelection(int id)//Controller ActionId
        {

            var menuviewmodel = _menuService.GetMenu(id);

            var ma = (List<MenuModouleViewModel>)System.Web.HttpContext.Current.Session["UserModuleList"];
            System.Web.HttpContext.Current.Session.Remove("UserModuleList");

            UpdateSession(ref ma, menuviewmodel.ModuleId, menuviewmodel.SubModuleId);

            if (menuviewmodel == null)
            {
                return View("~/Views/Shared/UnderImplementation.cshtml");
            }
            else if (!string.IsNullOrEmpty(menuviewmodel.URL))
            {
                return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
            }
            else
            {
                return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
            }

        }

        [Authorize]
        public ActionResult DropDown(int id)//Menu Id
        {
            MenuViewModel menuviewmodel = _menuService.GetMenu(id);

            if (menuviewmodel == null)
            {
                return View("~/Views/Shared/UnderImplementation.cshtml");
            }
            else if (!string.IsNullOrEmpty(menuviewmodel.URL))
            {
                return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
            }
            else
            {
                return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
            }

        }

        [Authorize]
        public JsonResult SetSelectedSubModule(int ModuleId, int SubModuleId)
        {

            var ma = (List<MenuModouleViewModel>)System.Web.HttpContext.Current.Session["UserModuleList"];
            System.Web.HttpContext.Current.Session.Remove("UserModuleList");

            UpdateSession(ref ma, ModuleId, SubModuleId);

            return Json(new { Success = true });
        }

        private void UpdateSession(ref List<MenuModouleViewModel> ma, int ModuleId, int SubModuleId)
        {
            if (ma != null)
            {
                foreach (MenuModouleViewModel item in ma)
                {
                    if (item.ModuleId == ModuleId)
                    {
                        item.SelectedSubModuleId = SubModuleId;
                        break;
                    }
                }
            }

            System.Web.HttpContext.Current.Session["UserModuleList"] = ma;
        }

    }
}