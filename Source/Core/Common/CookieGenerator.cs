using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public class CookieGenerator
    {
        public static void CreateNotificationCookie(NotificationTypeConstants notification, string message)
        {
            System.Web.HttpContext.Current.Response.Cookies.Add(new System.Web.HttpCookie(string.Format("Flash.{0}.{1}", notification, Guid.NewGuid()), message) { Path = "/" });            
        }

    }
}
