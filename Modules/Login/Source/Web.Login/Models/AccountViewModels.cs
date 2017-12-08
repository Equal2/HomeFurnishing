using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User Name")]        
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        //[Required]
        //public string UserType { get; set; }

        [Required(ErrorMessage = "* Full name is required."), MaxLength(25, ErrorMessage = "* Full name cannot exceed 25 characters")]
        public string FirstName { get; set; }
        //[Required(ErrorMessage = "* Last name is required."), MaxLength(25, ErrorMessage = "* Last name cannot exceed 25 characters")]
        public string LastName { get; set; }

        public string Company { get; set; }
        //[Required(ErrorMessage = "* City is required.")]
        public string City { get; set; }
        [Required(ErrorMessage = "* Mobile number is required."),
        StringLength(10, MinimumLength = 10, ErrorMessage = "* Mobile number should contain 10 digits.")
        , Phone(ErrorMessage = "* Please enter a valid mobile number.")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "* User name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "* Email is required.")]
        [EmailAddress(ErrorMessage = "* Please enter a valid email address.")]
        [Display(Name = "Email"), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string RetUrl { get; set; }


        //Invite Referals fields
        public string InvitationBy { get; set; }
        public DateTime InvitationDate { get; set; }
        public string UserRole { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int ApplicationId { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
