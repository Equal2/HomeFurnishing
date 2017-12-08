using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;
using Model;
using Models.Company.Models;
namespace AdminSetup.Models.Models
{
    public class RolesDivision : EntityBase, IHistoryLog
    {

        [Key]
        public int RolesDivisionId { get; set; }
       
        [Display(Name = "Role"),MaxLength(128)]
        [ForeignKey("Role")]
        public string RoleId { get; set; }        
        public virtual IdentityRole Role { get; set; }

        [Display(Name = "Division")]
        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }


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
