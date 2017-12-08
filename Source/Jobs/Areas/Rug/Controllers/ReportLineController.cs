using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Model.ViewModels;
using Model.ViewModel;
using System.Xml.Linq;
using Core.Common;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ReportLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IReportLineService _ReportLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ReportLineController(IReportLineService ReportLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ReportLineService = ReportLineService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
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
            ReportLine line = new ReportLine();
            line.ReportHeaderId = id;
            return PartialView("_Create", line);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ReportLine pt)
        {
            if (ModelState.IsValid)
            {

                if (pt.ReportLineId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ReportLineService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                        DocId = pt.ReportHeaderId,
                        DocLineId = pt.ReportLineId,
                        ActivityType = (int)ActivityTypeContants.Added,                        
                    }));


                    return RedirectToAction("_Create", new { id = pt.ReportHeaderId });

                }
                else
                {

                    pt.ModifiedDate = DateTime.Now;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Modified;
                    _ReportLineService.Update(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                        DocId = pt.ReportHeaderId,
                        DocLineId = pt.ReportLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                    }));

                    return Json(new { success = true });
                }
            }

            return PartialView("_Create", pt);
        }

        // GET: /ProductMaster/Edit/5

        public ActionResult _ModifyLine(int id)
        {
            ReportLine pt = _ReportLineService.GetReportLine(id);
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
            ViewBag.ReporList = new ReportHeaderService(_unitOfWork).GetReportHeaderListForCopy(id).ToList();
            return View(vm);
        }
        [HttpPost]
        public ActionResult Copy(ReportCopyViewModel vm)//header id
        {
            if (ModelState.IsValid)
            {
                List<ReportLine> temp = _ReportLineService.GetReportLineList(vm.CopyHeaderId).ToList();

                foreach (var item in temp)
                {
                    ReportLine line = new ReportLine();
                    line.ReportHeaderId = vm.ReportHeaderId;
                    line.DataType = item.DataType;
                    line.Type = item.Type;
                    line.ServiceFuncGet = item.ServiceFuncGet;
                    line.ServiceFuncSet = item.ServiceFuncSet;
                    line.CacheKey = item.CacheKey;
                    line.Serial = item.Serial;
                    line.FieldName = item.FieldName;
                    line.DisplayName = item.DisplayName;
                    line.NoOfCharToEnter = item.NoOfCharToEnter;
                    line.CreatedBy = User.Identity.Name;
                    line.CreatedDate = DateTime.Now;
                    line.ModifiedBy = User.Identity.Name;
                    line.ModifiedDate = DateTime.Now;

                    _ReportLineService.Create(line);
                    _unitOfWork.Save();

                }
                return RedirectToAction("Index", new { id = vm.ReportHeaderId }).Success("Date Copied Successfully");
            }
            ViewBag.ReporList = new ReportHeaderService(_unitOfWork).GetReportHeaderList().ToList();
            return View(vm);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult _Delete(int id)
        {
            ReportLine pt = _ReportLineService.GetReportLine(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", pt);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ReportLine pt)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            LogList.Add(new LogTypeViewModel
            {
                ExObj = pt,
            });

            _ReportLineService.Delete(pt);
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return View("_Create", pt);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.Report).DocumentTypeId,
                DocId = pt.ReportHeaderId,
                DocLineId = pt.ReportLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                xEModifications = Modifications,                
            }));

            return Json(new { success = true });

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
