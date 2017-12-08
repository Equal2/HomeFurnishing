using System.ComponentModel.DataAnnotations;
using System;
using System.Web;
// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web.Security;
namespace AdminSetup.Controllers
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }

    public class TempUserStoreViewModel
    {
        [Display(Name = "Email Address")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Provide Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    public class UserRolesViewModel
    {
        public string Id { get; set; }

        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
        public string Roles { get; set; }

    }

    // Used to display a single role with a checkbox, within a list structure:
    public class SelectRoleEditorViewModel
    {
        public SelectRoleEditorViewModel() { }
        public SelectRoleEditorViewModel(IdentityRole role)
        {
            this.RoleName = role.Name;
            this.RoleId = role.Id;            
        }

        [Required]
        public string RoleId { get; set; }
        public bool Selected { get; set; }

        [Required]
        public string RoleName { get; set; }
    }

    public class SelectUserRolesViewModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<SelectRoleEditorViewModel> Roles { get; set; }
        public SelectUserRolesViewModel()
        {
            this.Roles = new List<SelectRoleEditorViewModel>();
        }


        // Enable initialization with an instance of ApplicationUser:
        //public SelectUserRolesViewModel(IdentityUser user)
        //    : this()
        //{
        //    UserId = user.Id;
        //    this.UserName = user.UserName;
            
        //    ApplicationDbContext context = new ApplicationDbContext();

        //    // Add all available roles to the list of EditorViewModels:
        //    var allRoles = context.Roles;
        //    foreach (var role in allRoles)
        //    {
        //        // An EditorViewModel will be used by Editor Template:
        //        var rvm = new SelectRoleEditorViewModel(role);
        //        this.Roles.Add(rvm);
        //    }

        //    var UserManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(context));
        //    var rolesForUser = UserManager.GetRoles(user.Id);
        //    // Set the Selected property to true for those roles for 
        //    // which the current user is a member:
        //    foreach (var roleName in rolesForUser)
        //    {
        //        var checkUserRole = this.Roles.Find(r => r.RoleName == roleName);
        //        checkUserRole.Selected = true;
        //    }
        //}
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]

        public string ConfirmPassword { get; set; }

        public string HomeTown { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    [Authorize]
    public class AccountController : Controller
    {
        public UserManager<IdentityUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            string HomeUrl = System.Configuration.ConfigurationManager.AppSettings["LoginDomain"];

            if (string.IsNullOrEmpty(HomeUrl))
            {
                throw new Exception("Login Domain is not set in Modules Project");
            }

            AuthenticationManager.SignOut();
            FormsAuthentication.SignOut();

            Session.Abandon();            
            return Redirect(HomeUrl);
        }
      

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
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
                return System.Web.HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(IdentityUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion


    }

}
