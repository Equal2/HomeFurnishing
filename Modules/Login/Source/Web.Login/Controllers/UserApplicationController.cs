using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using Login.Models;

namespace Login.Controllers
{
    [Authorize]
    public class UserApplicationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult UserApplicationSelection()
        {
            var userId = User.Identity.GetUserId();
            ViewBag.UserAppList = db.UserApplication.Where(m => m.UserId == userId).Select(m => new {

                ApplicationId=m.ApplicationId,
                ApplicationDescription=m.Application.ApplicationDescription
            
            }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectedApplication(int id)
        {
            var AppRecord = (from p in db.Application
                             where p.ApplicationId == id
                             select p).FirstOrDefault();

            System.Web.HttpContext.Current.Session["DefaultConnectionString"] = AppRecord.ConnectionString;
            System.Web.HttpContext.Current.Session["ApplicationId"] = AppRecord.ApplicationId;

            return Redirect(AppRecord.ApplicationDefaultPage);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}