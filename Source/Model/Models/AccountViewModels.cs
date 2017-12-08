//using System.ComponentModel.DataAnnotations;

//// New namespace imports:
//using Microsoft.AspNet.Identity.EntityFramework;
//using System.Collections.Generic;
//using Surya.India.Model.Models;
//using System;
//using Microsoft.AspNet.Identity;

//namespace Surya.India.Model.Models
//{
//    public class RoleViewModel
//    {
//        public string Id { get; set; }
//        [Required]
//        [Display(Name = "RoleName")]
//        public string Name { get; set; }
//    }

//    public class TempUserStoreViewModel
//    {
//        [Display(Name = "Email Address")]
//        [Required(AllowEmptyStrings=false,ErrorMessage="Provide Email Address")]
//        [EmailAddress(ErrorMessage = "Invalid Email Address")]
//        public string Email { get; set; }   
//    }

//    public class UserRolesViewModel
//    {
//        public string Id { get; set; } 

//        [Display(Name = "User name")]
//        public string UserName { get; set; }      

//        [Display(Name = "Email")]
//        public string Email { get; set; }        
//        public string Roles { get; set; }

//    }

//    // Used to display a single role with a checkbox, within a list structure:
//    public class SelectRoleEditorViewModel
//    {
//        public SelectRoleEditorViewModel() { }
//        public SelectRoleEditorViewModel(IdentityRole role)
//        {
//            this.RoleName = role.Name;
//            this.RoleId = role.Id;
//        }

//         [Required]
//        public string RoleId { get; set; }
//        public bool Selected { get; set; }

//        [Required]
//        public string RoleName { get; set; }
//    }

//    public class SelectUserRolesViewModel
//    {
//        public string UserId { get; set; }

//        public string UserName { get; set; }
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//        public List<SelectRoleEditorViewModel> Roles { get; set; }
//        public SelectUserRolesViewModel()
//        {
//            this.Roles = new List<SelectRoleEditorViewModel>();
//        }


//        // Enable initialization with an instance of ApplicationUser:
//        public SelectUserRolesViewModel(ApplicationUser user)
//            : this()
//        {
//            UserId = user.Id;
//            this.UserName = user.UserName;

//            ApplicationDbContext context = new ApplicationDbContext();

//            // Add all available roles to the list of EditorViewModels:
//            var allRoles = context.Roles;
//            foreach (var role in allRoles)
//            {
//                // An EditorViewModel will be used by Editor Template:
//                var rvm = new SelectRoleEditorViewModel(role);
//                this.Roles.Add(rvm);
//            }

//            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
//            var rolesForUser = UserManager.GetRoles(user.Id);
//            // Set the Selected property to true for those roles for 
//            // which the current user is a member:
//            foreach (var roleName in rolesForUser)
//            {
//                var checkUserRole = this.Roles.Find(r => r.RoleName == roleName);
//                checkUserRole.Selected = true;
//            }
//        }
//    }



//    public class ExternalLoginConfirmationViewModel
//    {
//        [Required]
//        [Display(Name = "User name")]
//        public string UserName { get; set; }

//        [Required]
//        public string LoginProvider { get; set; }
//    }

//    public class ManageUserViewModel
//    {
//        [Required]
//        [DataType(DataType.Password)]
//        [Display(Name = "Current password")]
//        public string OldPassword { get; set; }

//        [Required]
//        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
//        [DataType(DataType.Password)]
//        [Display(Name = "New password")]
//        public string NewPassword { get; set; }

//        [DataType(DataType.Password)]
//        [Display(Name = "Confirm new password")]
//        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
//        public string ConfirmPassword { get; set; }
//    }

//    public class LoginViewModel
//    {
//        [Required(ErrorMessage = "User name required!")]
//        [Display(Name = "User name")]
//        public string UserName { get; set; }

//        [Required(ErrorMessage = "Password required!")]
//        [DataType(DataType.Password)]
//        [Display(Name = "Password")]
//        public string Password { get; set; }

//        [Display(Name = "Remember me?")]
//        public bool RememberMe { get; set; }
//    }

//    public class RegisterViewModel
//    {
//        [Required]
//        [Display(Name = "User name")]
//        public string UserName { get; set; }

//        [Required]
//        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
//        [DataType(DataType.Password)]
//        [Display(Name = "Password")]
//        public string Password { get; set; }

//        [DataType(DataType.Password)]
//        [Display(Name = "Confirm password")]
//        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
//        public string ConfirmPassword { get; set; }

//        public string HomeTown { get; set; }

//        [Required]
//        [EmailAddress(ErrorMessage = "Invalid Email Address")]
//        public string Email { get; set; }
//    }



//}
