
// New namespace imports:
using System;

namespace Models.Login.ViewModels
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
