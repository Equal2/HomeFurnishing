using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Service;
using Presentation;
using Components.ExceptionHandlers;
using Models.Reports.ViewModels;

namespace Web
{
    [Authorize]
    public class ReportLineController : System.Web.Mvc.Controller
    {
        List<string> UserRoles = new List<string>();

        IReportLineService _ReportLineService;
        IReportHeaderService _ReportHeaderService;
        IExceptionHandler _exception;
        public ReportLineController(IReportLineService ReportLineService, IExceptionHandler exec, IReportHeaderService ReportHeaderServ)
        {
            _ReportLineService = ReportLineService;
            _ReportHeaderService = ReportHeaderServ;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

        }
        // GET: /ProductMaster/

        public JsonResult Index(int id)
        {
            var ReportLine = _ReportLineService.GetReportLineList(id).ToList();
            return Json(ReportLine, JsonRequestBehavior.AllowGet);
        }

        // GET: /ProductMaster/Create

        public ActionResult _Create(int id)
        {
            ReportLineViewModel line = new ReportLineViewModel();
            line.ReportHeaderId = id;
            return PartialView("_Create", line);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ReportLineViewModel pt)
        {
            if (ModelState.IsValid)
            {

                if (pt.ReportLineId <= 0)
                {
                    try
                    {
                        _ReportLineService.Create(pt, User.Identity.Name);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", pt);
                    }

                    return RedirectToAction("_Create", new { id = pt.ReportHeaderId });

                }
                else
                {


                    try
                    {
                        _ReportLineService.Update(pt, User.Identity.Name);
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", pt);
                    }

                    return Json(new { success = true });
                }
            }

            return PartialView("_Create", pt);
        }

        // GET: /ProductMaster/Edit/5

        public ActionResult _ModifyLine(int id)
        {
            ReportLineViewModel pt = _ReportLineService.GetReportLineViewModel(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", pt);
        }



        [HttpGet]
        public ActionResult Copy(int id)//header id
        {
            ReportCopyViewModel vm = new ReportCopyViewModel();
            vm.ReportHeaderId = id;
            ViewBag.ReporList = _ReportHeaderService.GetReportHeaderListForCopy(id).ToList();
            return PartialView("_Copy", vm);
        }
        [HttpPost]
        public ActionResult Copy(ReportCopyViewModel vm)//header id
        {
            if (ModelState.IsValid)
            {
                _ReportLineService.CopyReport(vm, User.Identity.Name);
                return Json(new { success = true });
            }
            ViewBag.ReporList = _ReportHeaderService.GetReportHeaderList().ToList();
            return PartialView("_Copy", vm);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult _Delete(int id)
        {
            ReportLineViewModel pt = _ReportLineService.GetReportLineViewModel(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", pt);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ReportLineViewModel pt)
        {
            try
            {
                _ReportLineService.Delete(pt, User.Identity.Name);
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return View("_Create", pt);
            }

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ReportLineService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
