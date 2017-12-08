using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSetup.Models.Models
{
    public class Menu : EntityBase, IHistoryLog
    {

        [Key]
        public int MenuId { get; set; }
        [Display (Name="Menu Name")]
        [MaxLength(50, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        [Index("IX_Menu_MenuName", IsUnique = true,Order=1)]
        public string MenuName { get; set; }


        [Required, MaxLength(5)]
        public string Srl { get; set; }

        [Display(Name = "Icon Name")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters"), Required]        
        public string IconName { get; set; }

        [Display(Name = "Description")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters"), Required]
        public string Description { get; set; }

        [Display(Name = "URL")]
        [MaxLength(100, ErrorMessage = "{0} cannot exceed {1} characters")]
        public string URL { get; set; }


        [ForeignKey("Module")]
        [Display(Name = "Module")]
        [Index("IX_Menu_MenuName", IsUnique = true, Order = 2)]
        public int ModuleId { get; set; }
        public virtual MenuModule Module { get; set; }


        [ForeignKey("SubModule")]
        [Display(Name = "Sub-Module")]
        [Index("IX_Menu_MenuName", IsUnique = true, Order = 3)]
        public int SubModuleId { get; set; }
        public virtual MenuSubModule SubModule { get; set; }


        [ForeignKey("ControllerAction")]
        [Display(Name = "Controller\\Action")]
        public int ControllerActionId { get; set; }
        public virtual ControllerAction ControllerAction { get; set; }

        public bool ? IsVisible { get; set; }

        public bool ? isDivisionBased { get; set; }
        public bool? isSiteBased { get; set; }

        public string RouteId { get; set; }

        [MaxLength(50)]
        public string AreaName { get; set; }

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}
