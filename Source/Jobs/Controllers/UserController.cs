using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Jobs.Controllers
{
    [Authorize]
    public class UserController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public UserController(IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /User/

        public ActionResult Index()
        {
            IQueryable<IdentityUser> p = db.Users.OrderBy(m => m.Id);
            return View(p);
        }


        // GET: /User/Create

        public ActionResult Create()
        {
            IdentityUser vm = new IdentityUser();
            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(IdentityUser svm)
        {
            if (ModelState.IsValid)
            { 
            }
            return View("Create", svm);
        }


        // GET: /User/Edit/5
        public ActionResult Edit(string id)
        {
            IdentityUser s = db.Users.Find(id);
            if (s == null)
            {
                return HttpNotFound();
            }
            return View("Create", s);
        }


        // GET: /PurchaseOrderHeader/Delete/5

        public ActionResult Delete(string id)
        {

            return PartialView("_Reason");

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            { 
            }
            return PartialView("_Reason", vm);
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
