
// New namespace imports:
using System;

namespace Model.Models
{
    public class NotificationContentViewModel
    {
        public int NotificationSubjectId { get; set; }
        public string NotificationText { get; set; }
        public string NotificationUrl { get; set; }
        public string UrlKey { get; set; }
        public string UserNameList { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
