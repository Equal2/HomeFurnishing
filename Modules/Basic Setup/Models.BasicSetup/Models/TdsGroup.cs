using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.BasicSetup.Models
{
    public class TdsGroup : EntityBase, IHistoryLog
    {
        public TdsGroup()
        {
        }

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
