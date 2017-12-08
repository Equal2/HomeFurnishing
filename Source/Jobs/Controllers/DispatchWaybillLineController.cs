using System;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Reports.Controllers;
using Model.ViewModel;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Jobs.Controllers
{

    [Authorize]
    public class DispatchWaybillLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IDispatchWaybillLineService _DispatchWaybillLineService;
        IStockService _StockService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public DispatchWaybillLineController(IDispatchWaybillLineService DispatchWaybillLineService, IStockService StockService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _DispatchWaybillLineService = DispatchWaybillLineService;
            _StockService = StockService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _DispatchWaybillLineService.GetDispatchWaybillLineViewModelForHeaderId(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(DispatchWaybillLineViewModel s)
        {
            if (s == null)
            {
                ViewBag.CityId = new SelectList(new CityService(_unitOfWork).GetCityList(), "CityId", "CityName");
            }
            else
            {
                ViewBag.CityId = new SelectList(new CityService(_unitOfWork).GetCityList(), "CityId", "CityName", s.CityId);
            }
        }

        [HttpGet]
        public ActionResult CreateLine(int id)
        {
            return _Create(id);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id)
        {
            return _Create(id);
        }


        public ActionResult _Create(int Id) //Id ==>Sale Invoice Header Id
        {
            DispatchWaybillHeader H = new DispatchWaybillHeaderService(_unitOfWork).GetDispatchWaybillHeader(Id);
            DispatchWaybillLineViewModel s = new DispatchWaybillLineViewModel();

            s.DispatchWaybillHeaderId = H.DispatchWaybillHeaderId;

            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            PrepareViewBag(null);
            ViewBag.LineMode = "Create";

            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(DispatchWaybillLineViewModel svm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            DispatchWaybillHeader DispatchWaybillheader = new DispatchWaybillHeaderService(_unitOfWork).Find(svm.DispatchWaybillHeaderId);

            string DataValidationMsg = DataValidation(svm);

            if (DataValidationMsg != "")
            {
                PrepareViewBag(svm);
                TempData["CSEXCL"] += DataValidationMsg;
                return PartialView("_Create", svm);
            }

            if (svm.DispatchWaybillLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }


            if (ModelState.IsValid)
            {
                if (svm.DispatchWaybillLineId == 0)
                {
                    DispatchWaybillLine DispatchWaybillline = Mapper.Map<DispatchWaybillLineViewModel, DispatchWaybillLine>(svm);

                    DispatchWaybillline.CreatedDate = DateTime.Now;
                    DispatchWaybillline.ModifiedDate = DateTime.Now;
                    DispatchWaybillline.CreatedBy = User.Identity.Name;
                    DispatchWaybillline.ModifiedBy = User.Identity.Name;
                    DispatchWaybillline.ObjectState = Model.ObjectState.Added;
                    _DispatchWaybillLineService.Create(DispatchWaybillline);

                    if (DispatchWaybillheader.Status != (int)StatusConstants.Drafted)
                    {
                        DispatchWaybillheader.Status = (int)StatusConstants.Modified;
                        new DispatchWaybillHeaderService(_unitOfWork).Update(DispatchWaybillheader);
                    }

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DispatchWaybillheader.DocTypeId,
                        DocId = DispatchWaybillheader.DispatchWaybillHeaderId,
                        DocLineId = DispatchWaybillline.DispatchWaybillLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = DispatchWaybillheader.DocNo,
                        DocDate = DispatchWaybillheader.DocDate,
                        DocStatus = DispatchWaybillheader.Status,
                    }));

                    return RedirectToAction("_Create", new { id = DispatchWaybillline.DispatchWaybillHeaderId });
                }
                else
                {
                    DispatchWaybillLine DispatchWaybillline = _DispatchWaybillLineService.GetDispatchWaybillLineForLineId(svm.DispatchWaybillLineId);                    
                    int status = DispatchWaybillheader.Status;

                    DispatchWaybillLine ExTempLine = new DispatchWaybillLine();
                    ExTempLine = Mapper.Map<DispatchWaybillLine>(DispatchWaybillline);


                    DispatchWaybillline.ReceiveDateTime = svm.ReceiveDateTime;
                    DispatchWaybillline.ReceiveRemark = svm.ReceiveRemark;
                    DispatchWaybillline.ForwardingDateTime = svm.ForwardingDateTime;
                    DispatchWaybillline.ForwardedBy = svm.ForwardedBy;
                    DispatchWaybillline.ForwardingRemark = svm.ForwardingRemark;
                    DispatchWaybillline.ModifiedDate = DateTime.Now;
                    DispatchWaybillline.ModifiedBy = User.Identity.Name;
                    _DispatchWaybillLineService.Update(DispatchWaybillline);


                    DispatchWaybillheader.Status = (int)StatusConstants.Modified;
                    new DispatchWaybillHeaderService(_unitOfWork).Update(DispatchWaybillheader);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExTempLine,
                        Obj = DispatchWaybillline
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DispatchWaybillheader.DocTypeId,
                        DocId = DispatchWaybillheader.DispatchWaybillHeaderId,
                        DocLineId = DispatchWaybillline.DispatchWaybillLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = DispatchWaybillheader.DocNo,
                        xEModifications = Modifications,
                        DocDate = DispatchWaybillheader.DocDate,
                        DocStatus = DispatchWaybillheader.Status,
                    }));                 

                    return Json(new { success = true });
                }
            }

            ViewBag.Status = DispatchWaybillheader.Status;
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }



        [HttpGet]
        public ActionResult _ModifyLine(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        public ActionResult _ModifyLineAfterSubmit(int id)
        {
            return _Modify(id);
        }



        [HttpGet]
        private ActionResult _Modify(int id)
        {
            DispatchWaybillLine temp = _DispatchWaybillLineService.GetDispatchWaybillLineForLineId(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = temp.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Edit";

            DispatchWaybillHeader H = new DispatchWaybillHeaderService(_unitOfWork).GetDispatchWaybillHeader(temp.DispatchWaybillHeaderId);
            ViewBag.DocNo = H.DocNo;
            DispatchWaybillLineViewModel s = _DispatchWaybillLineService.GetDispatchWaybillLineViewModelForLineId(id);
            PrepareViewBag(s);


            return PartialView("_Create", s);
        }


        [HttpGet]
        public ActionResult _DeleteLine(int id)
        {
            return _Delete(id);
        }
        [HttpGet]
        public ActionResult _DeleteLine_AfterSubmit(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DispatchWaybillLine DispatchWaybillLine = _DispatchWaybillLineService.GetDispatchWaybillLineForLineId(id);
            if (DispatchWaybillLine == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = DispatchWaybillLine.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXCL"] += ExceptionMsg;
            #endregion

            if ((TimePlanValidation || Continue))
                ViewBag.LineMode = "Delete";

            return View("_Create", DispatchWaybillLine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(DispatchWaybillLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            DispatchWaybillLine DispatchWaybillLine = _DispatchWaybillLineService.GetDispatchWaybillLineForLineId(vm.DispatchWaybillLineId);

            LogList.Add(new LogTypeViewModel
            {
                Obj = Mapper.Map<JobOrderLine>(DispatchWaybillLine),
            });

            _DispatchWaybillLineService.Delete(vm.DispatchWaybillLineId);
            DispatchWaybillHeader DispatchWaybillheader = new DispatchWaybillHeaderService(_unitOfWork).Find(DispatchWaybillLine.DispatchWaybillHeaderId);
            if (DispatchWaybillheader.Status != (int)StatusConstants.Drafted)
            {
                DispatchWaybillheader.Status = (int)StatusConstants.Modified;
                new DispatchWaybillHeaderService(_unitOfWork).Update(DispatchWaybillheader);
            }

            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

            try
            {
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                ViewBag.LineMode = "Delete";
                return PartialView("_Create", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = DispatchWaybillheader.DocTypeId,
                DocId = DispatchWaybillheader.DispatchWaybillHeaderId,
                DocLineId = DispatchWaybillLine.DispatchWaybillLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                DocNo = DispatchWaybillheader.DocNo,
                xEModifications = Modifications,
                DocDate = DispatchWaybillheader.DocDate,
                DocStatus = DispatchWaybillheader.Status,
            }));

            return Json(new { success = true });
        }

        public string DataValidation(DispatchWaybillLineViewModel svm)
        {
            string ValidationMsg = "";

            if (svm.ReceiveDateTime > svm.ForwardingDateTime)
            {
                ValidationMsg = "Forwarding Datetime cannot be less than Receive Datetime";
            }

            return ValidationMsg;
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
