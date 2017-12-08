using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class DivisionSetting : EntityBase, IHistoryLog
    {
        [Key]
        public int DivisionSettingId { get; set; }
        [ForeignKey("Division"), Display(Name = "Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        [MaxLength(50)]
        public string Dimension1Caption { get; set; }

        [MaxLength(50)]
        public string Dimension2Caption { get; set; }

        [MaxLength(50)]
        public string Dimension3Caption { get; set; }

        [MaxLength(50)]
        public string Dimension4Caption { get; set; }
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
