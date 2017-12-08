using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Reports.DatabaseViews
{
    [Table("_Menus")]
    public class _Menus : EntityBase
    {
        [Key]
        public int MenuId { get; set; }
        public string MenuName { get; set; }
    }
}
