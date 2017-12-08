using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class Notification : EntityBase, IHistoryLog
    {

        [Key]
        public int NotificationId { get; set; }

        [ForeignKey("NotificationSubject")]
        public int NotificationSubjectId { get; set; }
        public virtual NotificationSubject NotificationSubject { get; set; }


        [Display (Name="NotificationText"), Required]        
        public string NotificationText { get; set; }

        [Display(Name = "NotificationUrl")]
        public string NotificationUrl { get; set; }

        [Display(Name = "UrlKey")]
        public string UrlKey { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public DateTime? SeenDate { get; set; }

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
