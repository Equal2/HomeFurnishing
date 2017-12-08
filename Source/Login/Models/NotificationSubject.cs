using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Models
{
    public class NotificationSubject : EntityBase,IHistoryLog
    {

        [Key]
        public int NotificationSubjectId { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "NotificationSubject Name cannot exceed 50 characters"), Required]
        [Index("IX_NotificationSubject_NotificationSubjectName", IsUnique = true)]
        public string NotificationSubjectName { get; set; }

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
