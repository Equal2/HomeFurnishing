using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using Notifier.Hubs;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class NotificationController : System.Web.Mvc.Controller
    { 
    
        public ActionResult GetAllNotifications()
        {

            string UserName = User.Identity.Name;

            var temp = RegisterChanges.GetAllNotifications (UserName);

            return View("~/Views/Shared/Notifications.cshtml",temp);
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
                    System.Web.HttpContext.Current.Session["SiteId"] = SiteId;
                if (DivisionId != 0)
                    System.Web.HttpContext.Current.Session["DivisionId"] = DivisionId;

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
