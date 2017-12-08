using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Model.Models;
using Service;
using Microsoft.AspNet.Identity;
using Presentation;
using Core.Common;
using Model.ViewModel;
using EmailContents;
using System.Threading.Tasks;
using Data.Models;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class UserInvitationController : System.Web.Mvc.Controller
    {
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();
        private ApplicationDbContext context = new ApplicationDbContext();

        IUserReferralService _UserReferralService;
        IEmployeeService _employeeService;
        IExceptionHandlingService _exception;
        public UserInvitationController(IUserReferralService UserRefService, IExceptionHandlingService exec, IEmployeeService empService)
        {
            _UserReferralService = UserRefService;
            _employeeService = empService;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public ActionResult SendInvites()
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var EmpId = _employeeService.GetEmloyeeForUser(User.Identity.GetUserId());

            if (UserRoles.Contains("Admin") || (EmpId.HasValue && EmpId.Value > 0))
            {
                //ViewBag.UserTypeList = _UserReferralService.GetUserTypes();
                ViewBag.UserRoleList = _UserReferralService.GetUserRolesList();
                ViewBag.SiteList = _UserReferralService.GetSiteList();
                ViewBag.DivisionList = _UserReferralService.GetDivisionList();



                return View();
            }

            return Redirect((string)System.Web.HttpContext.Current.Session[SessionNameConstants.MenuDomain] + "/Menu/Module/").Warning("Must be an employee to send User Invites.");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendInvites(string UserEmails, string UserRole, int SiteId, int DivisionId)
        {

            if (string.IsNullOrEmpty(UserEmails))
            {
                ModelState.AddModelError("UserEmails", "The User email field is required");
                return View();
            }

            int AppId = (int)System.Web.HttpContext.Current.Session["ApplicationId"];
            string InvitationBy = User.Identity.GetUserId();
            string InvitationByEmail = _UserReferralService.GetUserEmail(InvitationBy);
            DateTime InvitationDate = DateTime.Now;

            bool error = false;
            string ErrorMsg = "";
            foreach (var ToEmailId in UserEmails.Trim().Split(','))
            {

                //UserReferral uref = _UserReferralService.Create(User.Identity.Name, user, UserRole);

                
                try
                {
                    await SendRegisterInvitation(AppId, InvitationBy, ToEmailId, InvitationByEmail, InvitationDate, UserRole, SiteId, DivisionId);
                }
                catch (Exception ex)
                {
                    ErrorMsg = ex.Message;
                    error = true;
                }
            }

            if (error)
                return Redirect((string)System.Web.HttpContext.Current.Session[SessionNameConstants.MenuDomain] + "/Menu/Module/").Success(ErrorMsg);

            return Redirect((string)System.Web.HttpContext.Current.Session[SessionNameConstants.MenuDomain] + "/Menu/Module/").Success("Invite sent successfully");
        }

        private async Task SendRegisterInvitation(int AppId, string InvitationBy, string ToEmailId, string InvitationByEmail, DateTime InvitationDate, string UserRole, int SiteId, int DivisionId)
        {
            RegistrationInvitaionEmail riemail = new RegistrationInvitaionEmail();
            await riemail.SendUserRegistrationInvitation(ToEmailId, AppId, InvitationBy, InvitationByEmail, InvitationDate, UserRole, SiteId, DivisionId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _UserReferralService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
