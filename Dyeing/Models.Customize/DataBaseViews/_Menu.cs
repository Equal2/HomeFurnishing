using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.DataBaseViews
{
    [Table("_Menus")]
    public class _Menu : EntityBase
    {
        [Key]
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string Srl { get; set; }
        public string IconName { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public int ModuleId { get; set; }
        public int SubModuleId { get; set; }
        public int ControllerActionId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool? IsVisible { get; set; }
        public bool? isDivisionBased { get; set; }
        public bool? isSiteBased { get; set; }
        public string RouteId { get; set; }
        public bool IsActive { get; set; }
    }
}
