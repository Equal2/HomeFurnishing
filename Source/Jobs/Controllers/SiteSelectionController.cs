using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Service;
using System.Net;
using System.Web.Security;

//using ProjLib.Constants;
//using ProjLib.ViewModels;
//using Models.Company.Models;
//using Models.BasicSetup.ViewModels;
using System;
using System.IO;
using System.Data;
//using Services.BasicSetup;
using Model.ViewModel;
using Model.Models;
using Core.Common;

namespace Module
{
    [Authorize]
    public class SiteSelectionController : Controller
    {
        private readonly ISiteSelectionService _siteSelectionService;
        private readonly IUserRolesService _userRolesService;
        private readonly IModuleService _moduleService;
        private readonly IUserBookMarkService _userBookMarkService;
        private readonly ICompanySettingsService _CompanySettingsService;
        private readonly ICompanyService _CompanyService;
        private readonly IRolesControllerActionService _rolesControllerAcitonService;
        public SiteSelectionController(ISiteSelectionService SiteSelectionServ, ICompanyService CompanyServ, IUserRolesService userRolesService, IModuleService moduleServ, ICompanySettingsService CompanySettingsServ,
            IUserBookMarkService userBookmarkServ, IRolesControllerActionService rolesControllerActServ)
        {
            _siteSelectionService = SiteSelectionServ;
            _userRolesService = userRolesService;
            _moduleService = moduleServ;
            _CompanySettingsService = CompanySettingsServ;
            _CompanyService = CompanyServ;
            _userBookMarkService = userBookmarkServ;
            _rolesControllerAcitonService = rolesControllerActServ;
        }


        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult SiteSelection()
        {
            //var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            string UserId = User.Identity.GetUserId();
            string UserName = User.Identity.GetUserName();

            //using (StreamWriter writer = new StreamWriter(Server.MapPath("/release_notification_emails.txt"), true))
            //{
            //    writer.WriteLine("Tracking STart");
            //    writer.WriteLine("User Id : " + UserId);
            //    writer.WriteLine("User Name : " + UserName);
            //}

            var userInRoles = _userRolesService.GetUserRolesList(UserId);

            if (userInRoles.Count() == 0)
            {
                string URoles = (string)System.Web.HttpContext.Current.Session["LoginUserRole"];

                if (userInRoles.Count() <= 0 && !(_userRolesService.TryInsertUserRole(UserId, UserName, URoles)))
                {
                    AuthenticationManager.SignOut();
                    FormsAuthentication.SignOut();
                    Session.Abandon();
                    return View("NoRoles");
                }



                Session.Remove("LoginUserRole");

            }


            SiteSelectionViewModel vm = new SiteSelectionViewModel();

            AssignSession();

            IEnumerable<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Testing Block

            var temp = _userRolesService.GetRolesList().ToList();

            var RoleIds = string.Join(",", from p in temp
                                           where UserRoles.Contains(p.Name)
                                           select p.Id.ToString());
            //End


            if (UserRoles.Contains("Admin"))
            {

                var SiteList = _siteSelectionService.GetSiteList().ToList();
                ViewBag.SiteList = SiteList;
                var DivList = _siteSelectionService.GetDivisionList().ToList();
                ViewBag.DivisionList = DivList;

                if (SiteList.Count == 1 && DivList.Count == 1)
                {
                    AssignSiteDivModuleSession(SiteList.FirstOrDefault().SiteId, DivList.FirstOrDefault().DivisionId);

                    return RedirectToAction("DefaultGodownSelection");
                }
            }
            else
            {
                var SiteList = _siteSelectionService.GetSiteList(RoleIds).ToList();
                ViewBag.SiteList = SiteList;
                var DivList = _siteSelectionService.GetDivisionList(RoleIds).ToList();
                ViewBag.DivisionList = DivList;
                if (SiteList.Count == 0 || DivList.Count == 0)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
                else if (SiteList.Count == 1 && DivList.Count == 1)
                {
                    AssignSiteDivModuleSession(SiteList.FirstOrDefault().SiteId, DivList.FirstOrDefault().DivisionId);

                    return RedirectToAction("DefaultGodownSelection");
                }
            }
            if (System.Web.HttpContext.Current.Session["DivisionId"] != null && System.Web.HttpContext.Current.Session["SiteId"] != null)
            {
                vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            }

            return View(vm);
        }


        private void AssignSession()
        {
            var UserId = User.Identity.GetUserId();

            #region Roles
            IEnumerable<string> UserRoles = _userRolesService.GetUserRolesForSession(UserId);

            System.Web.HttpContext.Current.Session["Roles"] = UserRoles;

            #endregion

            #region CompanyDetails
            System.Web.HttpContext.Current.Session["CompanyId"] = 1;
            System.Web.HttpContext.Current.Session["CompanyName"] = "SURYA CARPET PVT. LTD.";


            #endregion

            #region BookMarks
            Dictionary<int, string> bookmarks = new Dictionary<int, string>();
            var temp = _userBookMarkService.GetUserBookMarkListForUser(User.Identity.Name);
            foreach (var item in temp)
            {
                bookmarks.Add(item.MenuId, item.MenuName);
            }


            List<UserBookMarkViewModel> vm = new List<UserBookMarkViewModel>();
            foreach (var item in temp)
            {
                vm.Add(new UserBookMarkViewModel()
                {
                    IconName = item.IconName,
                    MenuId = item.MenuId,
                    MenuName = item.MenuName,
                });
            }

            System.Web.HttpContext.Current.Session["BookMarks"] = vm;
            #endregion

            #region CompanySettings
            //var vm = new Compa _userBookMarkService.GetUserBookMarkListForUser(User.Identity.Name);
            var CompanySettings = _CompanySettingsService.GetCompanySettingsForCompany((int)System.Web.HttpContext.Current.Session["CompanyId"]);
            
            System.Web.HttpContext.Current.Session["CompanySettings"] = CompanySettings;
            #endregion


            #region Permissions
            if (!UserRoles.Contains("Admin"))
            {
                List<RolesControllerActionViewModel> Temp = _rolesControllerAcitonService.GetRolesControllerActionsForRoles(UserRoles.ToList()).ToList();

                System.Web.HttpContext.Current.Session["CAPermissionsCacheKeyHint"] = Temp.ToList();
            }

            #endregion

            #region NotificationCount

            System.Web.HttpContext.Current.Session[SessionNameConstants.UserNotificationCount] = _siteSelectionService.GetNotificationCount(User.Identity.Name);

            #endregion

            #region Menu&LoginDomains
            System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDomain] = (string)System.Configuration.ConfigurationManager.AppSettings["LoginDomain"];
            System.Web.HttpContext.Current.Session[SessionNameConstants.MenuDomain] = (string)System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"];
            #endregion

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SiteSelection(SiteSelectionViewModel vm)
        {
            AssignSiteDivModuleSession(vm.SiteId, vm.DivisionId);

            return RedirectToAction("DefaultGodownSelection");
        }


        public ActionResult DefaultGodownSelection()
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var GodownList = _siteSelectionService.GetGodownList(SiteId);

            var CompanySettings = _CompanySettingsService.GetCompanySettingsForCompany((int)System.Web.HttpContext.Current.Session["CompanyId"]);
            if ((CompanySettings.isVisibleGodownSelection ?? true) == false)
            {
                return RedirectToAction("Module", "Menu");
            }

            if (GodownList.Count() == 1 || GodownList.Count() == 0)
            {
                if (GodownList.Count() == 1)
                    System.Web.HttpContext.Current.Session["DefaultGodownId"] = GodownList.FirstOrDefault().GodownId;

                return RedirectToAction("Module", "Menu");
            }

            ViewBag.GodownList = GodownList;
            ViewBag.GodownId = (int?)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            return View();
        }

        [HttpPost]
        public ActionResult DefaultGodownSelection(int? DefaultGodownId)
        {
            if (DefaultGodownId.HasValue && DefaultGodownId.Value > 0)
                System.Web.HttpContext.Current.Session["DefaultGodownId"] = DefaultGodownId;
            return RedirectToAction("Module", "Menu");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _siteSelectionService.Dispose();
            }
            base.Dispose(disposing);
        }


        private void AssignSiteDivModuleSession(int SiteId, int DivisionId)
        {
            System.Web.HttpContext.Current.Session["DivisionId"] = DivisionId;
            System.Web.HttpContext.Current.Session["SiteId"] = SiteId;

            Site S = _siteSelectionService.GetSite(SiteId);
            Division D = _siteSelectionService.GetDivision(DivisionId);
            Company C = _CompanyService.Find((int)D.CompanyId);

            System.Web.HttpContext.Current.Session["CompanyId"] = C.CompanyId;


            System.Web.HttpContext.Current.Session[SessionNameConstants.LoginDivisionId] = DivisionId;
            System.Web.HttpContext.Current.Session[SessionNameConstants.LoginSiteId] = SiteId;
            System.Web.HttpContext.Current.Session[SessionNameConstants.CompanyName] = C.CompanyName;
            System.Web.HttpContext.Current.Session[SessionNameConstants.SiteName] = S.SiteName;
            System.Web.HttpContext.Current.Session[SessionNameConstants.SiteShortName] = S.SiteCode;
            System.Web.HttpContext.Current.Session[SessionNameConstants.SiteAddress] = S.Address;
            System.Web.HttpContext.Current.Session[SessionNameConstants.SiteCityName] = S.City.CityName;
            System.Web.HttpContext.Current.Session[SessionNameConstants.DivisionName] = D.DivisionName;

            System.Web.HttpContext.Current.Session["TitleCase"] = "UpperCase";

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            if (UserRoles.Contains("Admin"))
            {
                System.Web.HttpContext.Current.Session["UserModuleList"] = _moduleService.GetModuleList().ToList();
            }
            else
            {
                System.Web.HttpContext.Current.Session["UserModuleList"] = _moduleService.GetModuleListForUser(UserRoles.ToList(), SiteId, DivisionId).ToList();
            }

        }
    }

}