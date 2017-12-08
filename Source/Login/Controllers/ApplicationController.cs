using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Login.Models;

namespace Login.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult ApplicationSelection()
        {
            return View(db.Application.ToList());
        }

        public ActionResult SelectedApplication(int id)
        {
            var AppRecord = (from p in db.Application
                             where p.ApplicationId == id
                             select p).FirstOrDefault();

            System.Web.HttpContext.Current.Session["DefaultConnectionString"] = AppRecord.ConnectionString;

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