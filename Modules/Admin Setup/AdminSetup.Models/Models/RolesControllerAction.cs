using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;
using Model;
namespace AdminSetup.Models.Models
{
    public class RolesControllerAction : EntityBase, IHistoryLog
    {

        [Key]
        public int RolesControllerActionId { get; set; }
       
        [Display(Name = "Role"),MaxLength(128)]
        [ForeignKey("Role")]
        public string RoleId { get; set; }        
        public virtual IdentityRole Role { get; set; }


        [Display(Name = "Controller Action")]
        [ForeignKey("ControllerAction")]
        public int ControllerActionId { get; set; }
        public virtual ControllerAction ControllerAction{ get; set; }


        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }
    }
}
