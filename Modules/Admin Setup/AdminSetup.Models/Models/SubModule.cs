using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSetup.Models.Models
{
    public class MenuSubModule : EntityBase, IHistoryLog
    {      

        [Key]
        public int SubModuleId { get; set; }

        [Display (Name="Sub-Module Name")]
        [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        [Index("IX_SubModule_SubModuleName", IsUnique = true)]
        public string SubModuleName { get; set; }


        [Display(Name = "Icon Name")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        public string IconName { get; set; }


        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date"),DisplayFormat(DataFormatString="{0:dd/MMM/yyyy}")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
