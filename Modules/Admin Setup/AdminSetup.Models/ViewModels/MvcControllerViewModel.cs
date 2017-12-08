using System;
using System.ComponentModel.DataAnnotations;

namespace AdminSetup.Models.ViewModels
{
    public class MvcControllerViewModel
    {
        public int ControllerId { get; set; }
       
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        public string ControllerName { get; set; }

        [Display(Name = "Parent Controller")]
        public int? ParentControllerId { get; set; }
        public string ParentControllerName { get; set; }

        [MaxLength(30)]
        public string PubModuleName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }
    }
}
