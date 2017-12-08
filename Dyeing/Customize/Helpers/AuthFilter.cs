using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace Customize
{
    public class AuthenticationFilter : IAuthenticationFilter
    {
        void IAuthenticationFilter.OnAuthentication(AuthenticationContext filterContext)
        {
        }
        void IAuthenticationFilter.OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            string Name = filterContext.HttpContext.User.Identity.Name;

            var user = filterContext.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {

                if (System.Configuration.ConfigurationManager.AppSettings["LoginDomain"] == null)
                {
                    throw new Exception("Login domain is not configured in Jobs Project");
                }

                if (System.Configuration.ConfigurationManager.AppSettings["DefaultDomain"] == null)
                {
                    throw new Exception("Default domain is not configured in Jobs Project");
                }


                //filterContext.Result = new HttpUnauthorizedResult();
                string loginURL = (string)System.Configuration.ConfigurationManager.AppSettings["LoginDomain"] + "?returnUrl=";

                string ReturnDomain = (string)System.Configuration.ConfigurationManager.AppSettings["DefaultDomain"];
                filterContext.Result = new RedirectResult(loginURL + ReturnDomain);
            }
            else
            {
                string controllerName = filterContext.RouteData.Values["controller"].ToString();
                string actionName = filterContext.RouteData.Values["action"].ToString();

                // Authentication challenge trace centralyzed
                //ApplicationTracer.ApplicationTrace(System.Diagnostics.TraceLevel.Info, this.GetType(), controllerName, actionName, "OnAuthenticationChallenge", null);
            }
        }
    }
    public class RequestLogFilter : IActionFilter
    {
        protected static string TempDate = "";
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {

            // var temp = filterContext.HttpContext.Request.UrlReferrer.ToString();

            //TempDate.Substring((TempDate.IndexOf("MenuId")+7));
            //if (temp.IndexOf("MenuId")!=-1)
            //TempDate = temp.Substring((temp.IndexOf("MenuId") + 7));

            string ActionName = filterContext.ActionDescriptor.ActionName;
            string ControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            string User = filterContext.HttpContext.User.Identity.GetUserId();
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            try
            {
                if (User != null && !UserRoles.Contains("Admin") && ControllerName != "Account" && ControllerName != "Menu")
                {
                    //if (!AdminSetupController.ValidateData.ValidateUserPermission(ActionName, ControllerName))
                    //{
                    //    return;
                    //}
                    //else
                    //{

                    //    throw new Exception("Permission Denied");
                                     
                    //}
                }
            }
            catch (Exception ex)
            {
                var model = new HandleErrorInfo(ex, ControllerName, ActionName);
                if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {

                    filterContext.Result = new ViewResult
                    {
                        ViewName = "AjaxError",
                        ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                        TempData = filterContext.Controller.TempData

                    };
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Error",
                        ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                        TempData = filterContext.Controller.TempData
                    };
                }
            }

            // Don't show filter multiple times when using Html.RenderAction or Html.Action.
            if (filterContext.IsChildAction == true)
                return;

            // Action trace centralyzed
            //ApplicationTracer.ApplicationTrace(System.Diagnostics.TraceLevel.Info, this.GetType(), filterContext.Controller.ToString(), filterContext.ActionDescriptor.ActionName, "Child action", null);


        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //NameValueCollection QS = filterContext.HttpContext.Request.QueryString;
            //QS = (NameValueCollection)filterContext.HttpContext.Request.GetType().GetField("MenuId", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(filterContext);
            //PropertyInfo readOnlyInfo = QS.GetType().GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            //readOnlyInfo.SetValue(QS, false, null);

            //if(!string.IsNullOrEmpty(TempDate))
            //QS.Add("MenuId", TempDate);
            //filterContext.HttpContext.Request.QueryString.Add("MenuId", TempDate);
            //readOnlyInfo.SetValue(QS, true, null);


        }
    }

    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        //private readonly ILog _logger;

        public CustomHandleErrorAttribute()
        {
            // _logger = LogManager.GetLogger("MyLogger");
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                //((Controller)filterContext.Controller).ModelState.AddModelError("No Permision", filterContext.Exception.Message);
                // filterContext.ExceptionHandled = true;
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }


            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];
            var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);



            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {

                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        error = true,
                        message = filterContext.Exception.Message
                    }
                };
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
            }



            //ApplicationTracer.SignalExceptionToElmahAndTrace(filterContext.Exception, this.GetType(), controllerName, actionName);
            // log the error using log4net.
            //   _logger.Error(filterContext.Exception.Message, filterContext.Exception);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

    }

}