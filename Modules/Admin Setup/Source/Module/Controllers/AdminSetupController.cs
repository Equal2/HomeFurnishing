using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using Components.ExceptionHandlers;
using Service;
using AdminSetup.Models.ViewModels;
using Models.BasicSetup.ViewModels;

namespace Web
{
    [Authorize]
    public class AdminSetupController : System.Web.Mvc.Controller
    {
        private string ErrorMessage { get; set; }

        IMvcControllerService _MvcControllerService;
        IControllerActionService _ControllerActionService;
        IUserRolesService _UserRolesService;
        IRolesSiteService _RolesSiteService;
        IRolesDivisionService _RolesDivisionService;
        IAdminSetupService _AdminSetupService;
        IExceptionHandler _exception;
        public AdminSetupController(IExceptionHandler exec, IMvcControllerService MvcContrService, IControllerActionService ControllerActionService,
            IUserRolesService UserRolesServ, IRolesSiteService RolesSiteService, IRolesDivisionService RolesDivisionService,
            IAdminSetupService AdminSetupServ)
        {
            _MvcControllerService = MvcContrService;
            _ControllerActionService = ControllerActionService;
            _exception = exec;
            _UserRolesService = UserRolesServ;
            _RolesSiteService = RolesSiteService;
            _RolesDivisionService = RolesDivisionService;
            _AdminSetupService = AdminSetupServ;
        }


        public ActionResult UpdateCA()
        {
            UpdateControllers();
            UpdateActions();
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ViewBag.ErrorMessage = ErrorMessage;
                return View("Error");
            }
            else
            {
                return RedirectToAction("Module", "Menu");
            }

        }



        public void UpdateControllers()
        {

            List<ControllerActionList> Controllers = new List<ControllerActionList>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var controllers = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t)).Select(t => t);

            #region ☺ControllerUpdateCode☺

            foreach (Type controller in controllers)
            {
                Controllers.Add(new ControllerActionList { ControllerName = controller.Name.Replace("Controller", "") });
            }

            try
            {
                _MvcControllerService.SyncControllersList(Controllers, User.Identity.Name);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ErrorMessage = message;
            }

            #endregion

        }

        public void UpdateActions()
        {
            List<ControllerActionList> Actions = new List<ControllerActionList>();

            #region ☻ActionUpdateCode☻

            var controllers = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t)).Select(t => t);

            foreach (Type controller in controllers)
            {

                var actions = controller.GetMethods().Where(t => t.Name != "Dispose" && !t.IsSpecialName && t.DeclaringType.IsSubclassOf(typeof(ControllerBase)) && t.IsPublic && !t.IsStatic).ToList();

                foreach (var action in actions)
                {
                    var myAttributes = action.GetCustomAttributes(false);
                    for (int j = 0; j < myAttributes.Length; j++)
                        if (myAttributes.All(m => (m is HttpGetAttribute)))
                        {
                            Actions.Add(new ControllerActionList
                            {
                                ActionName = action.Name,
                                ControllerName = controller.Name.Replace("Controller", "")
                            });
                            break;
                        }
                }
            }


            try
            {
                _ControllerActionService.SyncActionList(Actions, User.Identity.Name);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ErrorMessage = message;
            }




            #endregion

        }


        public ActionResult AssignPermissions()
        {

            var RolesList = _UserRolesService.GetRolesList();

            var Roles = (from p in RolesList
                         orderby p.Name
                         select new RolesViewModel
                         {
                             RoleId = p.Id,
                             RoleName = p.Name,
                         });
            return View("RolesList", Roles);
        }

        [HttpGet]
        public ActionResult Roles(string id)
        {
            RoleSitePermissionViewModel Vm = new RoleSitePermissionViewModel();

            Vm.DivisionId = string.Join(",", (_RolesDivisionService.GetRolesDivisionList(id).Select(m => m.DivisionId.ToString())));


            Vm.SiteId = string.Join(",", (_RolesSiteService.GetRolesSiteList(id).Select(m => m.SiteId.ToString())));

            Vm.RoleId = id;

            return View("RolesSiteAndDivision", Vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Roles(RoleSitePermissionViewModel vm)
        {

            List<int> NewDivisionIds = new List<int>();
            List<int> NewSiteIds = new List<int>();

            if (!string.IsNullOrEmpty(vm.DivisionId))
                NewDivisionIds = vm.DivisionId.Split(',').Select(Int32.Parse).ToList();

            if (!string.IsNullOrEmpty(vm.SiteId))
                NewSiteIds = vm.SiteId.Split(',').Select(Int32.Parse).ToList();

            var NewDivisionForRoles = from p in NewDivisionIds
                                      select new RolesDivisionViewModel
                                      {
                                          DivisionId = p,
                                          RoleId = vm.RoleId
                                      };

            var NewSiteForRoles = from p in NewSiteIds
                                  select new RolesSiteViewModel
                                  {
                                      SiteId = p,
                                      RoleId = vm.RoleId
                                  };


            var ExistingDivisionsForRoles = _RolesDivisionService.GetRolesDivisionList(vm.RoleId).ToList();

            var ExistingSiteForRoles = _RolesSiteService.GetRolesSiteList(vm.RoleId).ToList();

            var DivisionPendingToUpdate = (from p in NewDivisionForRoles
                                           join t in ExistingDivisionsForRoles on new { x = p.RoleId, y = p.DivisionId } equals new { x = t.RoleId, y = t.DivisionId } into table
                                           from left in table.DefaultIfEmpty()
                                           where left == null
                                           select p.DivisionId).ToList();

            var DivisionPendingToDelete = (from p in ExistingDivisionsForRoles
                                           join t in NewDivisionForRoles on new { x = p.RoleId, y = p.DivisionId } equals new { x = t.RoleId, y = t.DivisionId } into table
                                           from right in table.DefaultIfEmpty()
                                           where right == null
                                           select p).ToList();

            List<RolesDivisionViewModel> vmRolesDiv = new List<RolesDivisionViewModel>();

            foreach (int item in DivisionPendingToUpdate)
            {
                RolesDivisionViewModel temp = new RolesDivisionViewModel();
                temp.RoleId = vm.RoleId;
                temp.DivisionId = item;
                vmRolesDiv.Add(temp);
            }

            foreach (var item in DivisionPendingToDelete)
            {
                _RolesDivisionService.Delete(item.RolesDivisionId);
            }

            _RolesDivisionService.CreateRange(vmRolesDiv, User.Identity.Name);


            var SitePendingToUpdate = (from p in NewSiteForRoles
                                       join t in ExistingSiteForRoles on new { x = p.RoleId, y = p.SiteId } equals new { x = t.RoleId, y = t.SiteId } into table
                                       from left in table.DefaultIfEmpty()
                                       where left == null
                                       select p.SiteId).ToList();

            var SitePendingToDelete = (from p in ExistingSiteForRoles
                                       join t in NewSiteForRoles on new { x = p.RoleId, y = p.SiteId } equals new { x = t.RoleId, y = t.SiteId } into table
                                       from right in table.DefaultIfEmpty()
                                       where right == null
                                       select p).ToList();


            List<RolesSiteViewModel> vmRolesSite = new List<RolesSiteViewModel>();

            foreach (int item in SitePendingToUpdate)
            {
                RolesSiteViewModel temp = new RolesSiteViewModel();
                temp.RoleId = vm.RoleId;
                temp.SiteId = item;
                vmRolesSite.Add(temp);
            }

            _RolesSiteService.CreateRange(vmRolesSite, User.Identity.Name);

            foreach (var item in SitePendingToDelete)
            {
                _RolesSiteService.Delete(item.RolesSiteId);
            }


            try
            {
                _RolesSiteService.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return View("RolesSiteAndDivision", vm);

            }

            //TempData["Validation"] = "Valid";
            //return RedirectToAction("UserPermissions", "Menu", new { RoleId = vm.RoleId });

            return RedirectToAction("SiteDivisionSummary", new { SiteId = vm.SiteId, DivisionId = vm.DivisionId, RoleId = vm.RoleId });
        }


        public ActionResult SiteDivisionSummary(string SiteId, string DivisionId, string RoleId)
        {
            return View(_AdminSetupService.GetSiteDivisionSummary(SiteId, DivisionId, RoleId));
        }

        public ActionResult SelectedSiteDivision(int SiteId, int DivisionId, string Id)
        {
            System.Web.HttpContext.Current.Session["UserPermissionDivisionId"] = DivisionId;
            System.Web.HttpContext.Current.Session["UserPermissionSiteId"] = SiteId;

            System.Web.HttpContext.Current.Session["UserPermissionSiteColour"] = _AdminSetupService.FindSiteViewModel(SiteId).ThemeColour;

            TempData["Validation"] = "Valid";
            return RedirectToAction("UserPermissions", "Menu", new { RoleId = Id });
        }


        public ActionResult GetActionsForMenu(int MenuId)
        {
            return Json(_AdminSetupService.GetMenuActions(MenuId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLineActionsForMenu(int MenuId)
        {


            return Json(_AdminSetupService.GetMenuLineActions(MenuId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CopyPermissions()
        {
            CopyRolesViewModel vm = new CopyRolesViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CopyPermissions(CopyRolesViewModel vm)
        {
            if (ModelState.IsValid && vm.FromRoleId != vm.ToRoleId)
            {

                try
                {
                    _AdminSetupService.CopyPermission(vm, User.Identity.Name);
                }
                catch (Exception ex)
                {
                    throw;
                }
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["MenuDomain"] + "/Menu/Module/");

            }

            return View(vm);
        }

        public JsonResult GetRoles(string term)
        {

            var temp = _UserRolesService.GetRolesList().ToList();

            return Json(temp, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _AdminSetupService.Dispose();
            }
            base.Dispose(disposing);
        }

    }


}
