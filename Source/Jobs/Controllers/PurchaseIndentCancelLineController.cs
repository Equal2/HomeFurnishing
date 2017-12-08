using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using AutoMapper;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using PurchaseIndentCancelDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseIndentCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        IPurchaseIndentCancelLineService _PurchaseIndentCancelLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseIndentCancelLineController(IPurchaseIndentCancelLineService PurchaseIndentCancelLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _PurchaseIndentCancelLineService = PurchaseIndentCancelLineService;
            _unitOfWork = unitOfWork;
            _ActivityLogService = aclog;
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
            var p = _PurchaseIndentCancelLineService.GetPurchaseIndentCancelLineListForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _CreateMultiple(int id)
        {
            PurchaseIndentCancelFilterViewModel vm = new PurchaseIndentCancelFilterViewModel();
            vm.PurchaseIndentCancelHeaderId = id;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseIndentCancelFilterViewModel vm)
        {
            List<PurchaseIndentCancelLineViewModel> temp = _PurchaseIndentCancelLineService.GetPurchaseIndentLineForMultiSelect(vm).ToList();
            PurchaseIndentCancelMasterDetailModel svm = new PurchaseIndentCancelMasterDetailModel();
            svm.PurchaseIndentCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseIndentCancelMasterDetailModel vm)
        {

            int Serial = _PurchaseIndentCancelLineService.GetMaxSr(vm.PurchaseIndentCancelViewModels.FirstOrDefault().PurchaseIndentCancelHeaderId);

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseIndentCancelDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseIndentCancelViewModels.FirstOrDefault().PurchaseIndentCancelHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.PurchaseIndentCancelViewModels)
                {
                    decimal balqty = (from p in db.ViewPurchaseIndentBalance
                                      where p.PurchaseIndentLineId == item.PurchaseIndentLineId
                                      select p.BalanceQty).FirstOrDefault();
                    if (balqty < item.Qty)
                    {
                        ModelState.AddModelError("", "Qty Exceeding Balance Qty");
                        return PartialView("_Results", vm);
                    }
                    if (item.Qty > 0)
                    {
                        PurchaseIndentCancelLine line = new PurchaseIndentCancelLine();

                        line.PurchaseIndentCancelHeaderId = item.PurchaseIndentCancelHeaderId;
                        line.PurchaseIndentLineId = item.PurchaseIndentLineId;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.ObjectState = Model.ObjectState.Added;
                        db.PurchaseIndentCancelLine.Add(line);

                        //_PurchaseIndentCancelLineService.Create(line);

                    }
                }

                PurchaseIndentCancelHeader Header = db.PurchaseIndentCancelHeader.Find(vm.PurchaseIndentCancelViewModels.FirstOrDefault().PurchaseIndentCancelHeaderId);

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;

                    Header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentCancelHeader.Add(Header);
                }

                try
                {
                    PurchaseIndentCancelDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseIndentCancelViewModels.FirstOrDefault().PurchaseIndentCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                try
                {
                    PurchaseIndentCancelDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseIndentCancelViewModels.FirstOrDefault().PurchaseIndentCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseIndentCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

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

        public ActionResult _Create(int Id) //Id ==>Prod Order Cancel Header Id
        {
            PurchaseIndentCancelLineViewModel svm = new PurchaseIndentCancelLineViewModel();
            PurchaseIndentCancelHeader H = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(Id);
            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            svm.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);

            svm.PurchaseIndentCancelHeaderId = Id;
            ViewBag.LineMode = "Create";
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseIndentCancelLineViewModel svm)
        {

            if (svm.PurchaseIndentCancelLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            bool BeforeSave = true;

            try
            {

                if (svm.PurchaseIndentLineId <= 0)
                    BeforeSave = PurchaseIndentCancelDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseIndentCancelHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseIndentCancelDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseIndentCancelHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.PurchaseIndentCancelLineId <= 0)
            {
                PurchaseIndentCancelLine s = new PurchaseIndentCancelLine();
                decimal balqty = (from p in db.ViewPurchaseIndentBalance
                                  where p.PurchaseIndentLineId == svm.PurchaseIndentLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (svm.PurchaseIndentLineId <= 0)
                {
                    ModelState.AddModelError("PurchaseIndentLineId", "The Purchase Indent field is required");
                }
                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    s.PurchaseIndentCancelHeaderId = svm.PurchaseIndentCancelHeaderId;
                    s.PurchaseIndentLineId = svm.PurchaseIndentLineId;
                    s.Qty = svm.Qty;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.Sr = _PurchaseIndentCancelLineService.GetMaxSr(s.PurchaseIndentCancelHeaderId);
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    //_PurchaseIndentCancelLineService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseIndentCancelLine.Add(s);

                    PurchaseIndentCancelHeader temp2 = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(s.PurchaseIndentCancelHeaderId);
                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseIndentCancelHeader.Add(temp2);
                        //new PurchaseIndentCancelHeaderService(_unitOfWork).Update(temp2);
                    }

                    try
                    {
                        PurchaseIndentCancelDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentCancelHeaderId, s.PurchaseIndentCancelLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        PurchaseIndentCancelDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentCancelHeaderId, s.PurchaseIndentCancelLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp2.DocTypeId,
                        DocId = temp2.PurchaseIndentCancelHeaderId,
                        DocLineId = s.PurchaseIndentCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));


                    return RedirectToAction("_Create", new { id = s.PurchaseIndentCancelHeaderId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseIndentCancelHeader temp = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(svm.PurchaseIndentCancelHeaderId);


                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                PurchaseIndentCancelLine s = _PurchaseIndentCancelLineService.Find(svm.PurchaseIndentCancelLineId);

                PurchaseIndentCancelLine ExRec = new PurchaseIndentCancelLine();
                ExRec = Mapper.Map<PurchaseIndentCancelLine>(s);

                decimal balqty = (from p in db.ViewPurchaseIndentBalance
                                  where p.PurchaseIndentLineId == svm.PurchaseIndentLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }


                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    if (svm.Qty > 0)
                    {
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                    }

                    s.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentCancelLine.Add(s);

                    //_PurchaseIndentCancelLineService.Update(s);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        //new PurchaseIndentCancelHeaderService(_unitOfWork).Update(temp);
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseIndentCancelHeader.Add(temp);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = s,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseIndentCancelDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentCancelHeaderId, s.PurchaseIndentCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }

                        db.SaveChanges();
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        PurchaseIndentCancelDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentCancelHeaderId, s.PurchaseIndentCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    //SAving the Activity Log::

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = s.PurchaseIndentCancelHeaderId,
                        DocLineId = s.PurchaseIndentCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return Json(new { success = true });
                }
                return PartialView("_Create", svm);
            }

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


        private ActionResult _Modify(int id)
        {
            PurchaseIndentCancelLineViewModel temp = _PurchaseIndentCancelLineService.GetPurchaseIndentCancelLine(id);
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

            PurchaseIndentCancelHeader H = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(temp.PurchaseIndentCancelHeaderId);
            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            PurchaseIndentCancelLineViewModel temp = _PurchaseIndentCancelLineService.GetPurchaseIndentCancelLine(id);
            if (temp == null)
            {
                return HttpNotFound();
            }

            PurchaseIndentCancelHeader H = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(temp.PurchaseIndentCancelHeaderId);
            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);
            return PartialView("_Create", temp);
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
            PurchaseIndentCancelLineViewModel temp = _PurchaseIndentCancelLineService.GetPurchaseIndentCancelLine(id);
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
                ViewBag.LineMode = "Delete";

            PurchaseIndentCancelHeader H = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(temp.PurchaseIndentCancelHeaderId);
            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseIndentCancelLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseIndentCancelDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseIndentCancelHeaderId, vm.PurchaseIndentCancelLineId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";


            if (BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseIndentCancelLine IndentCancelLine = db.PurchaseIndentCancelLine.Find(vm.PurchaseIndentCancelLineId);

                try
                {
                    PurchaseIndentCancelDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(IndentCancelLine.PurchaseIndentCancelHeaderId, IndentCancelLine.PurchaseIndentCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseIndentCancelLine>(IndentCancelLine),
                });

                IndentCancelLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseIndentCancelLine.Remove(IndentCancelLine);

                //_PurchaseIndentCancelLineService.Delete(vm.PurchaseIndentCancelLineId);
                PurchaseIndentCancelHeader header = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(IndentCancelLine.PurchaseIndentCancelHeaderId);
                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    //new PurchaseIndentCancelHeaderService(_unitOfWork).Update(header);
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentCancelHeader.Add(header);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Create", vm);
                }

                try
                {
                    PurchaseIndentCancelDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(IndentCancelLine.PurchaseIndentCancelHeaderId, IndentCancelLine.PurchaseIndentCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = IndentCancelLine.PurchaseIndentCancelHeaderId,
                    DocLineId = IndentCancelLine.PurchaseIndentLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));
              
            }

            return Json(new { success = true });
        }

        public JsonResult GetPendingIndents(int ProductId, int PurchaseIndentCancelHeaderId)
        {
            return Json(_PurchaseIndentCancelLineService.GetPurchaseIndentForProduct(ProductId, PurchaseIndentCancelHeaderId));
        }

        public JsonResult GetBalQtyForPurchaseIndentLineJson(int PurchaseIndentLineId)
        {
            return Json(_PurchaseIndentCancelLineService.GetBalanceQuantity(PurchaseIndentLineId));
        }

        public JsonResult GetPurchaseIndents(int id, string term)//Indent Cancel Header ID
        {
            return Json(new PurchaseOrderLineService(_unitOfWork).GetPendingPurchaseIndentHelpListForIndentCancel(id, term), JsonRequestBehavior.AllowGet);
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
