using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProcessSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int ProcessSettingsId { get; set; }


        [Index("IX_ProcessSetting_UniqueKey", IsUnique = true, Order = 1)]
        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Index("IX_ProcessSetting_UniqueKey", IsUnique = true, Order = 2)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Index("IX_ProcessSetting_UniqueKey", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        public bool? isApplicable { get; set; }

        [ForeignKey("RateListMenu")]
        [Display(Name = "Rate List Menu")]
        public int? RateListMenuId { get; set; }
        public virtual Menu RateListMenu { get; set; }


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
