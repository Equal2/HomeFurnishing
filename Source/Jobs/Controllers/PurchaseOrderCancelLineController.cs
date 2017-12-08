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
using AutoMapper;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using PurchaseOrderCancelDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Model.ViewModels;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseOrderCancelLineService _PurchaseOrderCancelLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;


        public PurchaseOrderCancelLineController(IPurchaseOrderCancelLineService PurchaseOrderCancelLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _PurchaseOrderCancelLineService = PurchaseOrderCancelLineService;
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
            var p = _PurchaseOrderCancelLineService.GetPurchaseOrderCancelLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            PurchaseOrderCancelFilterViewModel vm = new PurchaseOrderCancelFilterViewModel();
            vm.PurchaseOrderCancelHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseOrderCancelFilterViewModel vm)
        {
            List<PurchaseOrderCancelLineViewModel> temp = _PurchaseOrderCancelLineService.GetPurchaseOrderLineForMultiSelect(vm).ToList();
            PurchaseOrderCancelMasterDetailModel svm = new PurchaseOrderCancelMasterDetailModel();
            svm.PurchaseOrderCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseOrderCancelMasterDetailModel vm)
        {
            int Serial = _PurchaseOrderCancelLineService.GetMaxSr(vm.PurchaseOrderCancelViewModels.FirstOrDefault().PurchaseOrderCancelHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            var Header = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(vm.PurchaseOrderCancelViewModels.FirstOrDefault().PurchaseOrderCancelHeaderId);

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseOrderCancelDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderCancelViewModels.FirstOrDefault().PurchaseOrderCancelHeaderId), ref db);
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
                foreach (var item in vm.PurchaseOrderCancelViewModels)
                {
                    decimal balqty = (from p in db.ViewPurchaseOrderBalance
                                      where p.PurchaseOrderLineId == item.PurchaseOrderLineId
                                      select p.BalanceQty).FirstOrDefault();

                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        PurchaseOrderCancelLine line = new PurchaseOrderCancelLine();

                        line.PurchaseOrderCancelHeaderId = item.PurchaseOrderCancelHeaderId;
                        line.PurchaseOrderLineId = item.PurchaseOrderLineId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.Remark;

                        LineStatus.Add(line.PurchaseOrderLineId, line.Qty);

                        line.ObjectState = Model.ObjectState.Added;
                        db.PurchaseOrderCancelLine.Add(line);

                        //_PurchaseOrderCancelLineService.Create(line);

                    }
                }
                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyCancelMultiple(LineStatus, Header.DocDate, ref db);


                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {

                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;

                    Header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderCancelHeader.Add(Header);

                }

                try
                {
                    PurchaseOrderCancelDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderCancelViewModels.FirstOrDefault().PurchaseOrderCancelHeaderId), ref db);
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
                    PurchaseOrderCancelDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderCancelViewModels.FirstOrDefault().PurchaseOrderCancelHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseOrderCancelHeaderId,
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

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int id, int sid)
        {
            return _Create(id, sid);
        }

        public ActionResult _Create(int Id, int sid) //Id ==>Sale Order Header Id
        {
            PurchaseOrderCancelHeader H = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(Id);
            PurchaseOrderCancelLineViewModel svm = new PurchaseOrderCancelLineViewModel();

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            svm.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);

            svm.PurchaseOrderCancelHeaderId = Id;
            svm.SupplierId = sid;
            ViewBag.LineMode = "Create";
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseOrderCancelLineViewModel svm)
        {

            if (svm.PurchaseOrderLineId <= 0)
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

                if (svm.PurchaseOrderCancelLineId <= 0)
                    BeforeSave = PurchaseOrderCancelDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseOrderCancelHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseOrderCancelDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseOrderCancelHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (svm.PurchaseOrderCancelLineId <= 0)
            {

                PurchaseOrderCancelLine s = new PurchaseOrderCancelLine();
                decimal balqty = (from p in db.ViewPurchaseOrderBalance
                                  where p.PurchaseOrderLineId == svm.PurchaseOrderLineId
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
                    s.PurchaseOrderCancelHeaderId = svm.PurchaseOrderCancelHeaderId;
                    s.PurchaseOrderLineId = svm.PurchaseOrderLineId;
                    s.Qty = svm.Qty;
                    s.Sr = _PurchaseOrderCancelLineService.GetMaxSr(s.PurchaseOrderCancelHeaderId);
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseOrderCancelLine.Add(s);
                    //_PurchaseOrderCancelLineService.Create(s);

                    PurchaseOrderCancelHeader temp2 = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(s.PurchaseOrderCancelHeaderId);


                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnCancel(s.PurchaseOrderLineId, s.PurchaseOrderCancelLineId, temp2.DocDate, s.Qty, ref db, true);


                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderCancelHeader.Add(temp2);
                    }

                    //new PurchaseOrderCancelHeaderService(_unitOfWork).Update(temp2);

                    try
                    {
                        PurchaseOrderCancelDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderCancelHeaderId, s.PurchaseOrderCancelLineId, EventModeConstants.Add), ref db);
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
                        PurchaseOrderCancelDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderCancelHeaderId, s.PurchaseOrderCancelLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp2.DocTypeId,
                        DocId = s.PurchaseOrderCancelHeaderId,
                        DocLineId = s.PurchaseOrderCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));


                    return RedirectToAction("_Create", new { id = s.PurchaseOrderCancelHeaderId, sid = svm.SupplierId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                PurchaseOrderCancelHeader temp = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(svm.PurchaseOrderCancelHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                PurchaseOrderCancelLine s = db.PurchaseOrderCancelLine.Find(svm.PurchaseOrderCancelLineId);


                PurchaseOrderCancelLine ExRec = new PurchaseOrderCancelLine();
                ExRec = Mapper.Map<PurchaseOrderCancelLine>(s);


                decimal balqty = (from p in db.ViewPurchaseOrderBalance
                                  where p.PurchaseOrderLineId == svm.PurchaseOrderLineId
                                  select p.BalanceQty).FirstOrDefault();
                if (balqty + s.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Balance Qty");
                }


                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    if (svm.Qty > 0)
                    {
                        s.Remark = svm.Remark;
                        s.Qty = svm.Qty;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;

                        new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnCancel(s.PurchaseOrderLineId, s.PurchaseOrderCancelLineId, temp.DocDate, s.Qty, ref db, true);

                    }

                    //_PurchaseOrderCancelLineService.Update(s);
                    s.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderCancelLine.Add(s);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = s,
                    });

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderCancelHeader.Add(temp);
                        //new PurchaseOrderCancelHeaderService(_unitOfWork).Update(temp);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseOrderCancelDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderCancelHeaderId, s.PurchaseOrderCancelLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseOrderCancelDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderCancelHeaderId, s.PurchaseOrderCancelLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = s.PurchaseOrderCancelHeaderId,
                        DocLineId = s.PurchaseOrderCancelLineId,
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




        private ActionResult _Modify(int id)
        {
            PurchaseOrderCancelLineViewModel temp = _PurchaseOrderCancelLineService.GetPurchaseOrderCancelLine(id);

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

            PurchaseOrderCancelHeader H = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(temp.PurchaseOrderCancelHeaderId);

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
           
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
            var line = _PurchaseOrderCancelLineService.GetPurchaseOrderCancelLine(id);

            if (line == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = line.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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

            PurchaseOrderCancelHeader H = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(line.PurchaseOrderCancelHeaderId);

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            line.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);

           
            return PartialView("_Create", line);
        }
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseOrderCancelLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseOrderCancelDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseOrderCancelHeaderId, vm.PurchaseOrderCancelLineId), ref db);
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

                PurchaseOrderCancelLine PurchaseOrderCancelLine = db.PurchaseOrderCancelLine.Find(vm.PurchaseOrderCancelLineId);

                try
                {
                    PurchaseOrderCancelDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseOrderCancelLine.PurchaseOrderCancelHeaderId, PurchaseOrderCancelLine.PurchaseOrderCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseOrderCancelLine>(PurchaseOrderCancelLine),
                });

                PurchaseOrderCancelHeader header = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(PurchaseOrderCancelLine.PurchaseOrderCancelHeaderId);
                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnCancel(PurchaseOrderCancelLine.PurchaseOrderLineId, PurchaseOrderCancelLine.PurchaseOrderCancelLineId, header.DocDate, 0, ref db, true);

                //_PurchaseOrderCancelLineService.Delete(vm.PurchaseOrderCancelLineId);

                PurchaseOrderCancelLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseOrderCancelLine.Remove(PurchaseOrderCancelLine);


                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderCancelHeader.Add(header);
                    //new PurchaseOrderCancelHeaderService(_unitOfWork).Update(header);
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
                    PurchaseOrderCancelDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseOrderCancelLine.PurchaseOrderCancelHeaderId, PurchaseOrderCancelLine.PurchaseOrderCancelLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseOrderCancelHeaderId,
                    DocLineId = PurchaseOrderCancelLine.PurchaseOrderCancelLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));
             
            }
            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            PurchaseOrderCancelLineViewModel temp = _PurchaseOrderCancelLineService.GetPurchaseOrderCancelLine(id);
            PurchaseOrderCancelHeader H = new PurchaseOrderCancelHeaderService(_unitOfWork).Find(temp.PurchaseOrderCancelHeaderId);

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }


        public JsonResult GetPendingOrders(int ProductId, int PurchaseOrderCancelHeaderId)
        {
            return Json(new PurchaseOrderHeaderService(_unitOfWork).GetPendingPurchaseOrdersForOrderCancel(ProductId, PurchaseOrderCancelHeaderId).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new PurchaseOrderLineService(_unitOfWork).GetLineDetail(LineId));
        }
        class TempSaleOrderQty
        {
            public decimal BalanceQty { get; set; }
        }
        //public JsonResult GetPurchaseOrderLineJson(int PurchaseOrderLineId)
        //{
        //    decimal s = _PurchaseOrderCancelLineService.GetBalanceQuantity(PurchaseOrderLineId);
        //    TempSaleOrderQty temp = new TempSaleOrderQty();
        //    temp.BalanceQty = s;
        //    return Json(temp);
        //}

        public JsonResult GetPurchaseOrderDocNoOnLoad(int PurchaseOrderLineId)
        {

            var temp = new SaleOrderLineService(_unitOfWork).GetSaleOrderLineVM(PurchaseOrderLineId);

            return Json(temp);
        }

        public JsonResult GetPurchaseOrders(string searchTerm, int pageSize, int pageNum, int filter)//Receipt Header ID
        {


            var Records = _PurchaseOrderCancelLineService.GetPendingPurchaseOrderHelpList(filter, searchTerm);

            var temp = Records.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Records.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };


            //return Json(_PurchaseOrderCancelLineService.GetPendingPurchaseOrderHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_PurchaseOrderCancelLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
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
