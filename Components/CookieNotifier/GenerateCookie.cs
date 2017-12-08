using System;

namespace CookieNotifier
{
    public enum NotificationTypeConstants
    {
        Danger,
        Warning,
        Success,
        Info
    }

    public static class GenerateCookie
    {
        public static void CreateNotificationCookie(NotificationTypeConstants notification, string message)
        {
            System.Web.HttpContext.Current.Response.Cookies.Add(new System.Web.HttpCookie(string.Format("Flash.{0}.{1}", notification, Guid.NewGuid()), message) { Path = "/" });
        }
    }
}
