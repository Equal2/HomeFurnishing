using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Models
{
    public class NotificationViewModel
    {        
        public int NotificationId { get; set; }

        public int NotificationSubjectId { get; set; }
        public string NotificationSubjectName { get; set; }

        public string NotificationText { get; set; }

        public string NotificationUrl { get; set; }

        public string UrlKey { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public DateTime? SeenDate { get; set; }

        [MaxLength(128)]
        public string ApplicationUserId { get; set; }        

        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        public DateTime CreatedDate { get; set; }        

        public string IconName { get; set; }
        public string UserName { get; set; }
        public string ForUser { get; set; }
        public int NotificationCount { get; set; }

    }


    public class NotificationListViewModel
    {
        public List<NotificationViewModel> NotificationViewModel { get; set; }
        public string Count { get; set; }
    }
}
