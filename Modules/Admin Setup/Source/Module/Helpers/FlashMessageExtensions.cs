
using System.Web;
using System.Web.Mvc;

namespace Presentation
{
    internal static class FlashMessageExtensions
    {
        public static ActionResult Danger(this ActionResult result, string message)
        {
            CreateCookieWithFlashMessage(Notification.Danger, message);
            return result;
        }

        public static ActionResult Danger(this ActionResult result, string message,bool isPermanent)
        {
            CreateCookieWithFlashMessage(Notification.Danger, message, isPermanent);
            return result;
        }

        public static ActionResult Warning(this ActionResult result, string message)
        {
            CreateCookieWithFlashMessage(Notification.Warning, message);
            return result;
        }

        public static ActionResult Warning(this ActionResult result, string message, bool isPermanent)
        {
            CreateCookieWithFlashMessage(Notification.Warning, message, isPermanent);
            return result;
        }

        public static ActionResult Success(this ActionResult result, string message)
        {
            CreateCookieWithFlashMessage(Notification.Success, message);
            return result;
        }

        public static ActionResult Success(this ActionResult result, string message, bool isPermanent)
        {
            CreateCookieWithFlashMessage(Notification.Success, message, isPermanent);
            return result;
        }

        public static ActionResult Information(this ActionResult result, string message)
        {
            CreateCookieWithFlashMessage(Notification.Info, message);
            return result;
        }

        public static ActionResult Information(this ActionResult result, string message, bool isPermanent)
        {
            CreateCookieWithFlashMessage(Notification.Info, message,isPermanent);
            return result;
        }

        private static void CreateCookieWithFlashMessage(Notification notification, string message)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(string.Format("Flash.{0}", notification), message) { Path = "/" });
        }

        private static void CreateCookieWithFlashMessage(Notification notification, string message, bool isPermanent)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(string.Format("Flash.{0}", notification), message + isPermanent.ToString()) { Path = "/" });
        }

        private enum Notification
        {
            Danger,
            Warning,
            Success,
            Info
        }
    }
}