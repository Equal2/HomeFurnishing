using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Service;
using Presentation;
using Infrastructure.IO;
using Components.ExceptionHandlers;
using Models.Reports.ViewModels;
using Models.BasicSetup.ViewModels;

namespace Web
{
    [Authorize]
    public class ReportHeaderController : System.Web.Mvc.Controller
    {
        List<string> UserRoles = new List<string>();

        IReportHeaderService _ReportHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandler _exception;
        public ReportHeaderController(IReportHeaderService ReportHeaderService, IUnitOfWork unitOfWork, IExceptionHandler exec)
        {
            _ReportHeaderService = ReportHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }
        // GET: /ProductMaster/

        public ActionResult Index()
        {
            var ReportHeader = _ReportHeaderService.GetReportHeaderList().ToList();
            return View(ReportHeader);
        }

        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            ReportHeaderViewModel vm = new ReportHeaderViewModel();
            return View("Create",vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(ReportHeaderViewModel pt)
        {
            if (ModelState.IsValid)
            {
                if (pt.ReportHeaderId <= 0)
                {
                    try
                    {
                        _ReportHeaderService.Create(pt, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", pt);
                    }

                    return RedirectToAction("Edit", new { id = pt.ReportHeaderId }).Success("Data saved successfully");
                }
                else
                {
                    try
                    {
                        _ReportHeaderService.Update(pt, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", pt);
                    }

                    return RedirectToAction("Index").Success("Data saved successfully");
                }
            }
            return View("Create", pt);
        }

        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ReportHeaderViewModel pt = _ReportHeaderService.GetReportHeaderViewModel(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReportHeaderViewModel ReportHeader = _ReportHeaderService.GetReportHeaderViewModel(id);
            if (ReportHeader == null)
            {
                return HttpNotFound();
            }
            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _ReportHeaderService.Delete(vm, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ReportHeaderService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
