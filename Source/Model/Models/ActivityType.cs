using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ActivityType : EntityBase, IHistoryLog
    {
        [Key]
        public int ActivityTypeId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "ActivityType Name cannot exceed 50 characters"), Required]
        [Index("IX_ActivityType_ActivityTypeName", IsUnique = true)]
        public string ActivityTypeName { get; set; }

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
