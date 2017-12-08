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
using Presentation;
using AutoMapper;
using Model.ViewModels;
using Model;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDeliveryOrderCancelLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDeliveryOrderCancelLineService _SaleDeliveryOrderCancelLineService;
        IUnitOfWork _unitOfWork;
        IActivityLogService _ActivityLogService;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;


        public SaleDeliveryOrderCancelLineController(ISaleDeliveryOrderCancelLineService SaleDeliveryOrderCancelLineService, IUnitOfWork unitOfWork, IActivityLogService aclog, IExceptionHandlingService exec)
        {
            _SaleDeliveryOrderCancelLineService = SaleDeliveryOrderCancelLineService;
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
            var p = _SaleDeliveryOrderCancelLineService.GetSaleDeliveryOrderCancelLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            SaleDeliveryOrderCancelFilterViewModel vm = new SaleDeliveryOrderCancelFilterViewModel();
            vm.SaleDeliveryOrderCancelHeaderId = id;
            vm.BuyerId = sid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleDeliveryOrderCancelFilterViewModel vm)
        {
            List<SaleDeliveryOrderCancelLineViewModel> temp = _SaleDeliveryOrderCancelLineService.GetSaleDeliveryOrderLineForMultiSelect(vm).ToList();
            SaleDeliveryOrderCancelMasterDetailModel svm = new SaleDeliveryOrderCancelMasterDetailModel();
            svm.SaleDeliveryOrderCancelViewModels = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleDeliveryOrderCancelMasterDetailModel vm)
        {
            int Serial = _SaleDeliveryOrderCancelLineService.GetMaxSr(vm.SaleDeliveryOrderCancelViewModels.FirstOrDefault().SaleDeliveryOrderCancelHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            var Header = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(vm.SaleDeliveryOrderCancelViewModels.FirstOrDefault().SaleDeliveryOrderCancelHeaderId);

            bool BeforeSave = true;

            //try
            //{
            //    BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeLineSaveBulkEvent(this, new SaleDeliveryEventArgs(vm.SaleDeliveryOrderCancelViewModels.FirstOrDefault().SaleDeliveryOrderCancelHeaderId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.SaleDeliveryOrderCancelViewModels)
                {
                    decimal balqty = (from p in db.ViewSaleDeliveryOrderBalance
                                      where p.SaleDeliveryOrderLineId == item.SaleDeliveryOrderLineId
                                      select p.BalanceQty).FirstOrDefault();

                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        SaleDeliveryOrderCancelLine line = new SaleDeliveryOrderCancelLine();

                        line.SaleDeliveryOrderCancelHeaderId = item.SaleDeliveryOrderCancelHeaderId;
                        line.SaleDeliveryOrderLineId = item.SaleDeliveryOrderLineId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.Remark = item.Remark;

                        LineStatus.Add(line.SaleDeliveryOrderLineId, line.Qty);

                        line.ObjectState = Model.ObjectState.Added;
                        db.SaleDeliveryOrderCancelLine.Add(line);

                        //_SaleDeliveryOrderCancelLineService.Create(line);

                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    Header.Status = (int)StatusConstants.Modified;

                    Header.ObjectState = Model.ObjectState.Modified;

                    db.SaleDeliveryOrderCancelHeader.Add(Header);
                }

                //new SaleDeliveryOrderLineStatusService(_unitOfWork).UpdateSaleDeliveryQtyCancelMultiple(LineStatus, Header.DocDate, ref db);

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.onLineSaveBulkEvent(this, new SaleDeliveryEventArgs(vm.SaleDeliveryOrderCancelViewModels.FirstOrDefault().SaleDeliveryOrderCancelHeaderId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXCL"] += message;
                //    EventException = true;
                //}

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

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.afterLineSaveBulkEvent(this, new SaleDeliveryEventArgs(vm.SaleDeliveryOrderCancelViewModels.FirstOrDefault().SaleDeliveryOrderCancelHeaderId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.SaleDeliveryOrderCancelHeaderId,
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
            SaleDeliveryOrderCancelHeader H = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(Id);
            SaleDeliveryOrderCancelLineViewModel svm = new SaleDeliveryOrderCancelLineViewModel();

            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            svm.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);

            svm.SaleDeliveryOrderCancelHeaderId = Id;
            svm.BuyerId = sid;
            ViewBag.LineMode = "Create";
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleDeliveryOrderCancelLineViewModel svm)
        {

            if (svm.SaleDeliveryOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            bool BeforeSave = true;

            //try
            //{

            //    if (svm.SaleDeliveryOrderCancelLineId <= 0)
            //        BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeLineSaveEvent(this, new SaleDeliveryEventArgs(svm.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Add), ref db);
            //    else
            //        BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeLineSaveEvent(this, new SaleDeliveryEventArgs(svm.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Edit), ref db);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (svm.SaleDeliveryOrderCancelLineId <= 0)
            {

                SaleDeliveryOrderCancelLine s = new SaleDeliveryOrderCancelLine();
                decimal balqty = (from p in db.ViewSaleDeliveryOrderBalance
                                  where p.SaleDeliveryOrderLineId == svm.SaleDeliveryOrderLineId
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
                    s.SaleDeliveryOrderCancelHeaderId = svm.SaleDeliveryOrderCancelHeaderId;
                    s.SaleDeliveryOrderLineId = svm.SaleDeliveryOrderLineId;
                    s.Qty = svm.Qty;
                    s.Sr = _SaleDeliveryOrderCancelLineService.GetMaxSr(s.SaleDeliveryOrderCancelHeaderId);
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.SaleDeliveryOrderCancelLine.Add(s);
                    //_SaleDeliveryOrderCancelLineService.Create(s);

                    SaleDeliveryOrderCancelHeader temp2 = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(s.SaleDeliveryOrderCancelHeaderId);


                    //new SaleDeliveryOrderLineStatusService(_unitOfWork).UpdateSaleDeliveryQtyOnCancel(s.SaleDeliveryOrderLineId, s.SaleDeliveryOrderCancelLineId, temp2.DocDate, s.Qty, ref db, true);


                    if (temp2.Status != (int)StatusConstants.Drafted)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;
                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.SaleDeliveryOrderCancelHeader.Add(temp2);
                    }

                    //new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Update(temp2);

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.onLineSaveEvent(this, new SaleDeliveryEventArgs(s.SaleDeliveryOrderCancelHeaderId, s.SaleDeliveryOrderCancelLineId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXCL"] += message;
                    //    EventException = true;
                    //}

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

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.afterLineSaveEvent(this, new SaleDeliveryEventArgs(s.SaleDeliveryOrderCancelHeaderId, s.SaleDeliveryOrderCancelLineId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXCL"] += message;
                    //}

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp2.DocTypeId,
                        DocId = temp2.SaleDeliveryOrderCancelHeaderId,
                        DocLineId = s.SaleDeliveryOrderCancelLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp2.DocNo,
                        DocDate = temp2.DocDate,
                        DocStatus = temp2.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.SaleDeliveryOrderCancelHeaderId, sid = svm.BuyerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                SaleDeliveryOrderCancelHeader temp = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(svm.SaleDeliveryOrderCancelHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                SaleDeliveryOrderCancelLine s = db.SaleDeliveryOrderCancelLine.Find(svm.SaleDeliveryOrderCancelLineId);


                SaleDeliveryOrderCancelLine ExRec = new SaleDeliveryOrderCancelLine();
                ExRec = Mapper.Map<SaleDeliveryOrderCancelLine>(s);


                decimal balqty = (from p in db.ViewSaleDeliveryOrderBalance
                                  where p.SaleDeliveryOrderLineId == svm.SaleDeliveryOrderLineId
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

                        //new SaleDeliveryOrderLineStatusService(_unitOfWork).UpdateSaleDeliveryQtyOnCancel(s.SaleDeliveryOrderLineId, s.SaleDeliveryOrderCancelLineId, temp.DocDate, s.Qty, ref db, true);

                    }

                    //_SaleDeliveryOrderCancelLineService.Update(s);
                    s.ObjectState = Model.ObjectState.Modified;
                    db.SaleDeliveryOrderCancelLine.Add(s);

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
                        db.SaleDeliveryOrderCancelHeader.Add(temp);
                        //new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Update(temp);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.onLineSaveEvent(this, new SaleDeliveryEventArgs(s.SaleDeliveryOrderCancelHeaderId, s.SaleDeliveryOrderCancelLineId, EventModeConstants.Edit), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXCL"] += message;
                    //    EventException = true;
                    //}


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

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.afterLineSaveEvent(this, new SaleDeliveryEventArgs(s.SaleDeliveryOrderCancelHeaderId, s.SaleDeliveryOrderCancelLineId, EventModeConstants.Edit), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = s.SaleDeliveryOrderCancelHeaderId,
                        DocLineId = s.SaleDeliveryOrderCancelLineId,
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
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }




        private ActionResult _Modify(int id)
        {
            SaleDeliveryOrderCancelLineViewModel temp = _SaleDeliveryOrderCancelLineService.GetSaleDeliveryOrderCancelLine(id);

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

            SaleDeliveryOrderCancelHeader H = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(temp.SaleDeliveryOrderCancelHeaderId);

            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);
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
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }

        private ActionResult _Delete(int id)
        {
            var line = _SaleDeliveryOrderCancelLineService.GetSaleDeliveryOrderCancelLine(id);

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

            SaleDeliveryOrderCancelHeader H = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(line.SaleDeliveryOrderCancelHeaderId);

            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            line.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);
            return PartialView("_Create", line);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SaleDeliveryOrderCancelLine line = _SaleDeliveryOrderCancelLineService.Find(id);
            line.ObjectState = ObjectState.Deleted;
            int HeaderId = line.SaleDeliveryOrderCancelHeaderId;
            _SaleDeliveryOrderCancelLineService.Delete(id);

            _unitOfWork.Save();

            return RedirectToAction("Index", new { Id = HeaderId }).Success("Data deleted successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleDeliveryOrderCancelLineViewModel vm)
        {
            bool BeforeSave = true;
            //try
            //{
            //    BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeLineDeleteEvent(this, new SaleDeliveryEventArgs(vm.SaleDeliveryOrderCancelHeaderId, vm.SaleDeliveryOrderCancelLineId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";
            if (BeforeSave && !EventException)
            {

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleDeliveryOrderCancelLine SaleDeliveryOrderCancelLine = db.SaleDeliveryOrderCancelLine.Find(vm.SaleDeliveryOrderCancelLineId);

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.onLineDeleteEvent(this, new SaleDeliveryEventArgs(SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelHeaderId, SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXCL"] += message;
                //    EventException = true;
                //}

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<SaleDeliveryOrderCancelLine>(SaleDeliveryOrderCancelLine),
                });

                SaleDeliveryOrderCancelHeader header = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelHeaderId);
                //new SaleDeliveryOrderLineStatusService(_unitOfWork).UpdateSaleDeliveryQtyOnCancel(SaleDeliveryOrderCancelLine.SaleDeliveryOrderLineId, SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelLineId, header.DocDate, 0, ref db, true);

                //_SaleDeliveryOrderCancelLineService.Delete(vm.SaleDeliveryOrderCancelLineId);

                SaleDeliveryOrderCancelLine.ObjectState = Model.ObjectState.Deleted;
                db.SaleDeliveryOrderCancelLine.Remove(SaleDeliveryOrderCancelLine);


                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.SaleDeliveryOrderCancelHeader.Add(header);
                    //new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Update(header);
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

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.afterLineDeleteEvent(this, new SaleDeliveryEventArgs(SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelHeaderId, SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.SaleDeliveryOrderCancelHeaderId,
                    DocLineId = SaleDeliveryOrderCancelLine.SaleDeliveryOrderCancelLineId,
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
            SaleDeliveryOrderCancelLineViewModel temp = _SaleDeliveryOrderCancelLineService.GetSaleDeliveryOrderCancelLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            SaleDeliveryOrderCancelHeader H = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(temp.SaleDeliveryOrderCancelHeaderId);

            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }


        public JsonResult GetPendingOrders(int ProductId, int SaleDeliveryOrderCancelHeaderId)
        {
            return Json(new SaleDeliveryOrderHeaderService(_unitOfWork).GetPendingSaleDeliveryOrdersForOrderCancel(ProductId, SaleDeliveryOrderCancelHeaderId).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new SaleDeliveryOrderLineService(_unitOfWork).GetLineDetail(LineId));
        }
        class TempSaleOrderQty
        {
            public decimal BalanceQty { get; set; }
        }
        //public JsonResult GetSaleDeliveryOrderLineJson(int SaleDeliveryOrderLineId)
        //{
        //    decimal s = _SaleDeliveryOrderCancelLineService.GetBalanceQuantity(SaleDeliveryOrderLineId);
        //    TempSaleOrderQty temp = new TempSaleOrderQty();
        //    temp.BalanceQty = s;
        //    return Json(temp);
        //}

        public JsonResult GetSaleDeliveryOrderDocNoOnLoad(int SaleDeliveryOrderLineId)
        {

            var temp = new SaleOrderLineService(_unitOfWork).GetSaleOrderLineVM(SaleDeliveryOrderLineId);

            return Json(temp);
        }

        public JsonResult GetSaleDeliveryOrders(string searchTerm, int pageSize, int pageNum, int filter)//Receipt Header ID
        {

            var Query = _SaleDeliveryOrderCancelLineService.GetPendingSaleDeliveryOrderHelpList(filter, searchTerm);

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

        public JsonResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//Indent Header ID
        {
            var Query = _SaleDeliveryOrderCancelLineService.GetProductHelpList(filter, searchTerm);

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
