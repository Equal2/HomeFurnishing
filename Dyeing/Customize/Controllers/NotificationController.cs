using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Notifier.Hubs;
using ProjLib.Constants;
using Services.Customize;

namespace Customize.Controllers
{
    [Authorize]
    public class NotificationController : System.Web.Mvc.Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationServ)
        {
            _notificationService = notificationServ;
        }

        public ActionResult GetAllNotifications()
        {

            string UserName = User.Identity.Name;

            var temp = RegisterChanges.GetAllNotifications(UserName);

            return View("~/Views/Shared/Notifications.cshtml", temp);
        }

        public ActionResult NotificationRequest(int id)//NotificationId
        {

            var DefaultUrl = HttpContext.Request.UrlReferrer.ToString();

            string RetUrl = RegisterChanges.SetReadDate(id);

            if (string.IsNullOrEmpty(RetUrl))
            { return Redirect(DefaultUrl); }

            Uri rU = new Uri(RetUrl);

            string[] QueryParameters = HttpUtility.ParseQueryString(rU.Query).AllKeys;

            if (QueryParameters.Contains("SiteId") && QueryParameters.Contains("DivisionId"))
            {
                int SiteId = Int32.Parse(HttpUtility.ParseQueryString(rU.Query).Get("SiteId"));
                int DivisionId = Int32.Parse(HttpUtility.ParseQueryString(rU.Query).Get("DivisionId"));


                if (SiteId != 0)
                {
                    System.Web.HttpContext.Current.Session["SiteId"] = SiteId;
                    System.Web.HttpContext.Current.Session[SessionNameConstants.SiteShortName] = _notificationService.GetSite(SiteId).SiteCode;
                }
                if (DivisionId != 0)
                {
                    System.Web.HttpContext.Current.Session["DivisionId"] = DivisionId;
                    System.Web.HttpContext.Current.Session[SessionNameConstants.DivisionName] = _notificationService.GetDivision(DivisionId).DivisionName;
                }

                var ParamCollection = HttpUtility.ParseQueryString(rU.Query);
                ParamCollection.Remove("SiteId");
                ParamCollection.Remove("DivisionId");

                UriBuilder r = new UriBuilder(RetUrl);
                r.Query = ParamCollection.ToString();

                RetUrl = r.ToString();
            }

            return Redirect(RetUrl);

        }

        public ActionResult UpdateNotificationSessionCount(int Count)
        {
            System.Web.HttpContext.Current.Session[SessionNameConstants.UserNotificationCount] = Count;

            return Json(true);
        }


    }
}
