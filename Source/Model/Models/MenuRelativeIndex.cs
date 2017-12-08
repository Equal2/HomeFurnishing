using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class MenuRelativeIndex : EntityBase, IHistoryLog
    {

        [Key]
        public int MenuRelativeIndexId { get; set; }

        [Required, MaxLength(5)]
        public string Srl { get; set; }


        [ForeignKey("Menu")]
        [Display(Name = "Menu")]
        [Index("IX_MenuRelativeIndex_MenuName", IsUnique = true, Order = 2)]
        public int MenuId { get; set; }
        public virtual Menu Menu { get; set; }


        [ForeignKey("RelativeMenu")]
        [Display(Name = "RelativeMenu")]
        [Index("IX_MenuRelativeIndex_RelativeMenuName", IsUnique = true, Order = 2)]
        public int RelativeMenuId { get; set; }
        public virtual Menu RelativeMenu { get; set; }


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
