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
using DocumentEvents;
using CustomEventArgs;
using MaterialRequestCancelDocumentEvents;
using Reports.Controllers;
using Model.ViewModels;
using Jobs.Helpers;


namespace Jobs.Controllers
{
    [Authorize]
    public class MaterialRequestCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IRequisitionCancelLineService _RequisitionCancelLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public MaterialRequestCancelLineController(IRequisitionCancelLineService RequisitionCancelLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _RequisitionCancelLineService = RequisitionCancelLineService;
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
            var p = _RequisitionCancelLineService.GetRequisitionCancelLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            RequisitionCancelFilterViewModel vm = new RequisitionCancelFilterViewModel();
            vm.RequisitionCancelHeaderId = id;
            vm.PersonId = sid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(RequisitionCancelFilterViewModel vm)
        {
            List<RequisitionCancelLineViewModel> temp = _RequisitionCancelLineService.GetRequisitionLineForOrders(vm).ToList();
            RequisitionCancelListModel svm = new RequisitionCancelListModel();
            svm.RequisitionCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(RequisitionCancelListModel vm)
        {
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            var Header = new RequisitionCancelHeaderService(_unitOfWork).Find(vm.RequisitionCancelViewModels.FirstOrDefault().RequisitionCancelHeaderId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = MaterialRequestCancelDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.RequisitionCancelViewModels.FirstOrDefault().RequisitionCancelHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.RequisitionCancelViewModels)
                {

                    decimal balqty = (from p in db.ViewRequisitionBalance
                                      where p.RequisitionLineId == item.RequisitionLineId
                                      select p.BalanceQty).FirstOrDefault();

                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        RequisitionCancelLine line = new RequisitionCancelLine();

                        line.RequisitionCancelHeaderId = item.RequisitionCancelHeaderId;
                        line.RequisitionLineId = item.RequisitionLineId;
                        line.Qty = item.Qty;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.Remark;

                        LineStatus.Add(line.RequisitionLineId, line.Qty);

                        //_RequisitionCancelLineService.Create(line);                      

                        line.ObjectState = Model.ObjectState.Added;
                        db.RequisitionCancelLine.Add(line);

                    }
                }

                new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyCancelMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    MaterialRequestCancelDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.RequisitionCancelViewModels.FirstOrDefault().RequisitionCancelHeaderId), ref db);
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
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Results", vm);
                }

                try
                {
                    MaterialRequestCancelDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.RequisitionCancelViewModels.FirstOrDefault().RequisitionCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.RequisitionCancelHeaderId,
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
        public ActionResult CreateLine(int id, int sid)
        {
            return _Create(id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, int sid)
        {
            return _Create(id, sid);
        }

        public ActionResult _Create(int Id, int sid) //Id ==>Sale Order Header Id
        {
            RequisitionCancelHeader header = new RequisitionCancelHeaderService(_unitOfWork).Find(Id);
            RequisitionCancelLineViewModel svm = new RequisitionCancelLineViewModel();

            //Getting Settings
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.RequisitionSettings = Mapper.Map<RequisitionSetting, RequisitionSettingsViewModel>(settings);

            svm.RequisitionCancelHeaderId = Id;
            svm.PersonId = sid;
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }
            ViewBag.LineMode = "Create";

            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(RequisitionCancelLineViewModel svm)
        {
            bool BeforeSave = true;
            try
            {
                if (svm.RequisitionLineId <= 0)
                    BeforeSave = MaterialRequestCancelDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.RequisitionCancelHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = MaterialRequestCancelDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.RequisitionCancelHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (svm.RequisitionCancelLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.RequisitionCancelLineId <= 0)
            {
                RequisitionCancelHeader temp = new RequisitionCancelHeaderService(_unitOfWork).Find(svm.RequisitionCancelHeaderId);

                RequisitionCancelLine s = new RequisitionCancelLine();
                decimal balqty = (from p in db.ViewRequisitionBalance
                                  where p.RequisitionLineId == svm.RequisitionLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }
                if (ModelState.IsValid && BeforeSave && !EventException)
                {

                    s.Remark = svm.Remark;
                    s.RequisitionCancelHeaderId = svm.RequisitionCancelHeaderId;
                    s.RequisitionLineId = svm.RequisitionLineId;
                    s.Qty = svm.Qty;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;

                    db.RequisitionCancelLine.Add(s);

                    //_RequisitionCancelLineService.Create(s);

                    new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnCancel(s.RequisitionLineId, s.RequisitionCancelLineId, temp.DocDate, s.Qty, ref db, true);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.RequisitionCancelHeader.Add(temp);
                    }

                    try
                    {
                        MaterialRequestCancelDocEvents.onLineSaveEvent(this, new StockEventArgs(s.RequisitionCancelHeaderId, s.RequisitionLineId, EventModeConstants.Add), ref db);
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
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        MaterialRequestCancelDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.RequisitionCancelHeaderId, s.RequisitionLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.RequisitionCancelHeaderId,
                        DocLineId = s.RequisitionCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.RequisitionCancelHeaderId, sid = svm.PersonId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                RequisitionCancelHeader temp = new RequisitionCancelHeaderService(_unitOfWork).Find(svm.RequisitionCancelHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                RequisitionCancelLine s = _RequisitionCancelLineService.Find(svm.RequisitionCancelLineId);

                RequisitionCancelLine ExRec = new RequisitionCancelLine();
                ExRec = Mapper.Map<RequisitionCancelLine>(s);


                decimal balqty = (from p in db.ViewRequisitionBalance
                                  where p.RequisitionLineId == svm.RequisitionLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }


                if (ModelState.IsValid && BeforeSave)
                {
                    if (svm.Qty > 0)
                    {

                        s.Remark = svm.Remark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;

                        new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnCancel(s.RequisitionLineId, s.RequisitionCancelLineId, temp.DocDate, s.Qty, ref db, true);

                    }

                    //_RequisitionCancelLineService.Update(s);
                    s.ObjectState = Model.ObjectState.Modified;
                    db.RequisitionCancelLine.Add(s);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = s,
                    });


                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.RequisitionCancelHeader.Add(temp);
                        //new RequisitionCancelHeaderService(_unitOfWork).Update(temp);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        MaterialRequestCancelDocEvents.onLineSaveEvent(this, new StockEventArgs(s.RequisitionCancelHeaderId, s.RequisitionCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }

                    try
                    {
                        if (EventException)
                        { throw new Exception(); }
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        MaterialRequestCancelDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.RequisitionCancelHeaderId, s.RequisitionCancelLineId, EventModeConstants.Edit), ref db);
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
                        DocId = temp.RequisitionCancelHeaderId,
                        DocLineId = s.RequisitionCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    //End Of Saving Activity Log

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


        [HttpGet]
        private ActionResult _Modify(int id)
        {
            RequisitionCancelLineViewModel temp = _RequisitionCancelLineService.GetRequisitionCancelLine(id);

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

            RequisitionCancelHeader header = new RequisitionCancelHeaderService(_unitOfWork).Find(temp.RequisitionCancelHeaderId);
            //Getting Settings
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            temp.RequisitionSettings = Mapper.Map<RequisitionSetting, RequisitionSettingsViewModel>(settings);

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

        [HttpGet]
        private ActionResult _Delete(int id)
        {
            RequisitionCancelLineViewModel temp = _RequisitionCancelLineService.GetRequisitionCancelLine(id);

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

            RequisitionCancelHeader header = new RequisitionCancelHeaderService(_unitOfWork).Find(temp.RequisitionCancelHeaderId);
            //Getting Settings
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            temp.RequisitionSettings = Mapper.Map<RequisitionSetting, RequisitionSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            RequisitionCancelLineViewModel temp = _RequisitionCancelLineService.GetRequisitionCancelLine(id);
            RequisitionCancelHeader header = new RequisitionCancelHeaderService(_unitOfWork).Find(temp.RequisitionCancelHeaderId);

            //Getting Settings
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);


            temp.RequisitionSettings = Mapper.Map<RequisitionSetting, RequisitionSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(RequisitionCancelLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = MaterialRequestCancelDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.RequisitionCancelHeaderId, vm.RequisitionCancelLineId), ref db);
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


                RequisitionCancelLine RequisitionLine = (from p in db.RequisitionCancelLine
                                                         where p.RequisitionCancelLineId == vm.RequisitionCancelLineId
                                                         select p).FirstOrDefault();

                //RequisitionCancelLine RequisitionLine = _RequisitionCancelLineService.Find(vm.RequisitionCancelLineId);
                RequisitionCancelHeader header = new RequisitionCancelHeaderService(_unitOfWork).Find(RequisitionLine.RequisitionCancelHeaderId);

                RequisitionCancelLine ExRec = new RequisitionCancelLine();
                ExRec = Mapper.Map<RequisitionCancelLine>(RequisitionLine);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                });

                new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnCancel(RequisitionLine.RequisitionLineId, RequisitionLine.RequisitionCancelLineId, header.DocDate, 0, ref db, true);

                RequisitionLine.ObjectState = Model.ObjectState.Deleted;
                db.RequisitionCancelLine.Remove(RequisitionLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.RequisitionCancelHeader.Add(header);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);


                try
                {
                    MaterialRequestCancelDocEvents.onLineDeleteEvent(this, new StockEventArgs(RequisitionLine.RequisitionCancelHeaderId, RequisitionLine.RequisitionCancelLineId), ref db);
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
                        throw new Exception();
                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    return PartialView("_Create", vm);
                }

                try
                {
                    MaterialRequestCancelDocEvents.afterLineDeleteEvent(this, new StockEventArgs(RequisitionLine.RequisitionCancelHeaderId, RequisitionLine.RequisitionCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.RequisitionCancelHeaderId,
                    DocLineId = RequisitionLine.RequisitionCancelLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
        }


        public JsonResult GetCustomProductsForOrder(int id, string term, int Limit)
        {
            return Json(_RequisitionCancelLineService.GetPendingProductsForOrder(id, term, Limit).ToList());
        }

        //public JsonResult GetPendingProductsForFilters(int Id, string term, int Limit)
        //{
        //    return Json(_RequisitionCancelLineService.GetPendingProductsForFilters(Id, term, Limit).ToList());
        //}

        public JsonResult GetPendingProductsForFilters(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var Query = _RequisitionCancelLineService.GetPendingProductsForFilters(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingCostCentersForFilters(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {

            var Query = _RequisitionCancelLineService.GetPendingCostCentersForFilters(filter, searchTerm);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        public JsonResult GetLineDetail(int LineId)
        {
            return Json(_RequisitionCancelLineService.GetLineDetail(LineId));
        }

        public JsonResult GetPendingRequisition(int id, string term, int Limit)//DocTypeId
        {
            return Json(_RequisitionCancelLineService.GetPendingRequisitionsForFilters(id, term, Limit), JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {

            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            TempData["CSEXC"] = null;

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }

}
