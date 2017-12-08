using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Login.Models;
using System.Web.Security;
using System;
using Models.Login.Models;
//using Mailer;
//using Mailer.Model;
using System.Configuration;
//using Notifier.Models;
//using Notifier.Execution;
//using Notifier.Core;


namespace Login.Controllers
{
    [Authorize]
    //[RequireHttps]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
            //Comment
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //ViewBag.ReturnUrl = returnUrl;
            //return View();
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.ReturnUrl = returnUrl;
            else
                ViewBag.ReturnUrl = (string)ConfigurationManager.AppSettings["Defaultdomain"];

            string RedirectUrl = ComputeRedirectUrl();

            if (!string.IsNullOrEmpty(RedirectUrl))
                return Redirect(RedirectUrl);

            LoginViewModel model = new LoginViewModel();
            return View("LoginNew", model);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("LoginNew", model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        var user = await UserManager.FindAsync(model.UserName, model.Password);

                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                        //System.Web.HttpContext.Current.Session["ApplicationUserId"] = user.Id;

                        var sessionCookieKey = Response.Cookies.AllKeys.SingleOrDefault(c => c.ToLower() == "asp.net_sessionid");
                        var ReqsessionCookieKey = Request.Cookies.AllKeys.SingleOrDefault(c => c.ToLower() == "asp.net_sessionid");
                        var sessionCookie = Response.Cookies.Get(sessionCookieKey);
                        var ReqsessionCookie = Request.Cookies.Get(ReqsessionCookieKey);
                        if (sessionCookie != null && sessionCookieKey != null && sessionCookie.Expires <= DateTime.Now)
                        {
                            sessionCookie.Expires = DateTime.Now.AddDays(7);
                        }
                        else if (ReqsessionCookie != null && ReqsessionCookieKey != null && ReqsessionCookie.Expires <= DateTime.Now)
                        {
                            ReqsessionCookie.Expires = DateTime.Now.AddDays(7);
                        }

                        if (!string.IsNullOrEmpty(returnUrl))
                        {

                            Application AppRecord = new Application();

                            using (ApplicationDbContext db = new ApplicationDbContext())
                            {
                                AppRecord = (from p in db.Application
                                             where p.ApplicationURL.ToUpper().Trim() == returnUrl.ToUpper().Trim()
                                             select p).FirstOrDefault();
                            }

                            if (AppRecord != null)
                            {

                                System.Web.HttpContext.Current.Session["DefaultConnectionString"] = AppRecord.ConnectionString;
                                System.Web.HttpContext.Current.Session["ApplicationId"] = AppRecord.ApplicationId;
                                //System.Web.HttpContext.Current.Session["TopBarColr"] = AppRecord.TopBarColour;

                                System.Web.HttpContext.Current.Session["TitleCase"] = "UpperCase";

                                if (AppRecord != null && AppRecord.ApplicationDefaultPage != null)
                                {
                                    return Redirect(AppRecord.ApplicationDefaultPage);
                                }
                                else
                                    throw new Exception("Application Default Page is not set in Login Project");
                            }
                            else
                                throw new Exception("Default domain not found");
                        }
                        //else
                        //    return RedirectToAction("ApplicationSelection", "Application");
                        else
                        {

                            //returnUrl = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();

                            Application AppRecord = new Application();
                            string RoleId = "";

                            using (ApplicationDbContext db = new ApplicationDbContext())
                            {

                                var userId = user.Id;
                                var UserAppList = db.UserApplication.Where(m => m.UserId == userId).Select(m => new
                                {

                                    ApplicationId = m.ApplicationId,
                                    ApplicationDescription = m.Application.ApplicationDescription

                                }).ToList();

                                if (UserAppList.Count() == 0)
                                {
                                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                    FormsAuthentication.SignOut();
                                    Session.Abandon();
                                    return View("UserPermissionError");
                                }

                                //var UserRef = db.UserReferral.Where(m => m.ToUser == user.Email).OrderBy(m => m.CreatedDate).ToList().LastOrDefault();
                                //if (UserRef != null)
                                //    RoleId = UserRef.RoleId;

                                System.Web.HttpContext.Current.Session["LoginUserRole"] = RoleId;

                                if (UserAppList.Count() == 1)
                                {
                                    AppRecord = db.Application.Find(UserAppList.FirstOrDefault().ApplicationId);
                                }

                                if (UserAppList.Count() > 1)
                                    return RedirectToAction("UserApplicationSelection", "UserApplication");

                            }


                            if (AppRecord != null)
                            {
                                System.Web.HttpContext.Current.Session["DefaultConnectionString"] = AppRecord.ConnectionString;
                                System.Web.HttpContext.Current.Session["ApplicationId"] = AppRecord.ApplicationId;
                                //System.Web.HttpContext.Current.Session["TopBarColr"] = AppRecord.TopBarColour;

                                if (AppRecord.ApplicationDefaultPage != null)
                                    return Redirect(AppRecord.ApplicationDefaultPage);
                                else
                                    throw new Exception("Application Default Page is not set in Login Project");
                            }
                            else
                                throw new Exception("Default domain not found");


                            //return RedirectToAction("ApplicationSelection", "Application");
                        }

                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                        ViewBag.ReturnUrl = returnUrl;
                        return View("LoginNew", model);
                    }
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public async Task<ActionResult> Register(string RetUrl, int AppId = 0, string ToEmailId = "", string InvitationBy = "",
                DateTime? InvitationDate =  null, string UserRole = "", int SiteId = 0, int DivisionId = 0)
        {
            string ValidationMsg = ValidateInviteParameters(AppId, InvitationBy, ToEmailId);
            if (ValidationMsg != "")
            {
                ViewBag.ErrorMsg = ValidationMsg;
                return View("InviteOnly");
            }

            var ExistingUser = UserManager.FindByEmail(ToEmailId);

            if (ExistingUser != null)
            {
                await AddUserApplication(AppId, ExistingUser.Id, InvitationBy, InvitationDate ?? DateTime.Now, UserRole,SiteId, DivisionId);
                return RedirectToAction("Login");
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                //ViewBag.UserTypeList = db.UserType.ToList();

                RegisterViewModel model = new RegisterViewModel();
                model.ApplicationId = AppId;
                model.Email = ToEmailId;
                model.InvitationBy = InvitationBy;
                model.InvitationDate = InvitationDate ?? DateTime.Now;
                model.UserRole = UserRole;
                model.SiteId = SiteId;
                model.DivisionId = DivisionId;
                model.RetUrl = RetUrl;
                return View(model);
            }

        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //bool InviteValidation = ValidateInviteParameters(model.ApplicationId, model.RefereeId, model.Email);

            if (ModelState.IsValid)
            {
                int AppId = GetApplicationId(model.RetUrl);

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    //LastName = model.LastName,
                    City = "",
                    EmailConfirmed = true,
                    //Company = model.Company,
                    PhoneNumber = model.PhoneNumber,
                    ApplicationId = model.ApplicationId,
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    await AddUserApplication(model.ApplicationId, user.Id, model.InvitationBy, model.InvitationDate, model.UserRole, model.SiteId, model.DivisionId);

                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");


                    #region AutoLogin

                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, change to shouldLockout: true
                    var Loginresult = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, false, shouldLockout: true);

                    if (Loginresult == SignInStatus.Success)
                    {
                        var Loginuser = await UserManager.FindAsync(model.UserName, model.Password);

                        FormsAuthentication.SetAuthCookie(model.UserName, true);

                        var sessionCookieKey = Response.Cookies.AllKeys.SingleOrDefault(c => c.ToLower() == "asp.net_sessionid");
                        var ReqsessionCookieKey = Request.Cookies.AllKeys.SingleOrDefault(c => c.ToLower() == "asp.net_sessionid");
                        var sessionCookie = Response.Cookies.Get(sessionCookieKey);
                        var ReqsessionCookie = Request.Cookies.Get(ReqsessionCookieKey);
                        if (sessionCookie != null && sessionCookieKey != null && sessionCookie.Expires <= DateTime.Now)
                        {
                            sessionCookie.Expires = DateTime.Now.AddDays(7);
                        }
                        else if (ReqsessionCookie != null && ReqsessionCookieKey != null && ReqsessionCookie.Expires <= DateTime.Now)
                        {
                            ReqsessionCookie.Expires = DateTime.Now.AddDays(7);
                        }

                        Application AppRecord = new Application();
                        string Roles = "";

                        using (ApplicationDbContext db = new ApplicationDbContext())
                        {

                            AppRecord = db.Application.Find(user.ApplicationId);
                            Roles = model.UserRole;
                            //var guid = Guid.Parse(model.ReferralId);
                            //var UserRef = db.UserReferral.Where(m => m.ToUser == Loginuser.Email).ToList().LastOrDefault();
                            //if (UserRef != null)
                            //    Roles = UserRef.RoleId;
                        }

                        if (AppRecord != null)
                        {
                            System.Web.HttpContext.Current.Session["DefaultConnectionString"] = AppRecord.ConnectionString;
                            System.Web.HttpContext.Current.Session["ApplicationId"] = AppRecord.ApplicationId;
                            System.Web.HttpContext.Current.Session["LoginUserRole"] = Roles;
                            System.Web.HttpContext.Current.Session["SiteId"] = model.SiteId;
                            System.Web.HttpContext.Current.Session["DivisionId"] = model.DivisionId;

                            if (AppRecord.ApplicationDefaultPage != null)
                                return Redirect(AppRecord.ApplicationDefaultPage);
                            else
                                throw new Exception("Application Default Page is not set in Application Project");
                        }
                    }

                    #endregion

                    //string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account");

                    //ViewBag.Message = "Please check your email and confirm your account, you must be confirmed "
                    //               + "before you can log in.";

                    return RedirectToAction("Login");
                    //return View("Info");
                }
                AddErrors(result);
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                //ViewBag.UserTypeList = db.UserType.ToList();
            }

            //if (InviteValidation)
            //    ModelState.AddModelError("", "Invite Validatoin failed");

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        private async Task AddUserApplication(int ApplicationId, string UserId, string InvitationBy, DateTime InvitationDate, string UserRole,
            int SiteId = 0, int DivisionId = 0)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                UserApplication AlreadyExist = db.UserApplication.Where(m => m.ApplicationId == ApplicationId && m.UserId == UserId).FirstOrDefault();

                if (AlreadyExist == null)
                {
                    UserApplication UApp = new UserApplication();

                    UApp.ApplicationId = ApplicationId;
                    UApp.UserId = UserId;
                    UApp.InvitationBy = InvitationBy;
                    UApp.InvitationDate = InvitationDate;
                    UApp.UserRole = UserRole;
                    UApp.SiteId = SiteId;
                    UApp.DivisionId = DivisionId;

                    db.Entry<UserApplication>(UApp).State = System.Data.Entity.EntityState.Added;



                    await db.SaveChangesAsync();
                }
            }
        }

        private string ValidateInviteParameters(int AppId, string InvitationBy, string ToEmailId)
        {
            string ValidationMsg = "";

            if (string.IsNullOrEmpty(InvitationBy))
                ValidationMsg = "Invitation Send By User is not valid.";
            else
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var InvitationByUser = db.Users.Find(InvitationBy);
                    if (InvitationByUser == null)
                        ValidationMsg = "Invitation Send By User is not valid.";
                }
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var Application = db.Application.Find(AppId);
                if (Application == null)
                    ValidationMsg = "Invitation Send For Application is not valid.";
            }

            return ValidationMsg;
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            try
            {
                var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
                AddErrors(result);
            }
            catch (Exception EX)
            {

                throw;
            }
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject,
               "Please confirm your account by clicking <a href=\"" + callbackUrl + "\"><strong>Here</strong></a>");

            return callbackUrl;
        }

        private int GetApplicationId(string RetUrl)
        {

            int AppId = 0;
            if (!string.IsNullOrEmpty(RetUrl))
            {

                Application AppRecord = new Application();

                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    AppRecord = (from p in db.Application
                                 where p.ApplicationURL.ToUpper().Trim() == RetUrl.ToUpper().Trim()
                                 select p).FirstOrDefault();
                }

                if (AppRecord != null)
                {
                    AppId = AppRecord.ApplicationId;
                }

            }
            else
            {
                string Domain = (string)ConfigurationManager.AppSettings["Defaultdomain"];

                Application AppRecord = new Application();

                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    AppRecord = (from p in db.Application
                                 where p.ApplicationURL.ToUpper().Trim() == Domain.ToUpper().Trim()
                                 select p).FirstOrDefault();
                }

                if (AppRecord != null)
                {
                    AppId = AppRecord.ApplicationId;
                }
            }

            return AppId;

        }

        private string ComputeRedirectUrl()
        {
            string RedirectUrl = "";

            if (!string.IsNullOrEmpty((string)System.Web.HttpContext.Current.Session["DefaultConnectionString"]) && User.Identity.IsAuthenticated)
            {
                if (System.Web.HttpContext.Current.Session["SiteId"] != null && System.Web.HttpContext.Current.Session["DivisionId"] != null)
                {
                    RedirectUrl = ((string)ConfigurationManager.AppSettings["JobsDomain"] + "/Menu/Module");
                }
                else
                {
                    RedirectUrl = ((string)ConfigurationManager.AppSettings["JobsDomain"] + "/SiteSelection/SiteSelection");
                }
            }

            return RedirectUrl;
        }
    }
}