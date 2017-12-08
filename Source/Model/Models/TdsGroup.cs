using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class TdsGroup : EntityBase, IHistoryLog
    {
        public TdsGroup()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int TdsGroupId { get; set; }

        [Display(Name="Tds Group")]
        [MaxLength(50), Required]
        [Index("IX_TdsGroup_TdsGroupName", IsUnique = true)]
        public string TdsGroupName { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
