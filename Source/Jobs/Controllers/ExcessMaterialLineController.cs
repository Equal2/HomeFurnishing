using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using ExcessMaterialDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class ExcessMaterialLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;

        IExcessMaterialLineService _ExcessMaterialLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ExcessMaterialLineController(IExcessMaterialLineService ExcessMaterial, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ExcessMaterialLineService = ExcessMaterial;
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
            var p = _ExcessMaterialLineService.GetExcessMaterialLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        private void PrepareViewBag(ExcessMaterialLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ExcessMaterialHeaderViewModel H = new ExcessMaterialHeaderService(_unitOfWork, db).GetExcessMaterialHeader(vm.ExcessMaterialHeaderId);
            ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
        }

        [HttpGet]
        public ActionResult CreateLine(int id)
        {
            return _Create(id);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool? IsProdBased)
        {
            return _Create(id);
        }

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            ExcessMaterialHeader H = new ExcessMaterialHeaderService(_unitOfWork, db).Find(Id);
            ExcessMaterialLineViewModel s = new ExcessMaterialLineViewModel();

            //Getting Settings
            var settings = new ExcessMaterialSettingsService(_unitOfWork, db).GetExcessMaterialSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.ExcessMaterialSettings = Mapper.Map<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>(settings);
            s.ExcessMaterialHeaderId = H.ExcessMaterialHeaderId;
            ViewBag.Status = H.Status;
            PrepareViewBag(s);
            if (!string.IsNullOrEmpty((string)TempData["CSEXCL"]))
            {
                ViewBag.CSEXCL = TempData["CSEXCL"];
                TempData["CSEXCL"] = null;
            }
            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(ExcessMaterialLineViewModel svm)
        {
            ExcessMaterialHeader temp = new ExcessMaterialHeaderService(_unitOfWork, db).Find(svm.ExcessMaterialHeaderId);
            ExcessMaterialLine s = Mapper.Map<ExcessMaterialLineViewModel, ExcessMaterialLine>(svm);            

            bool BeforeSave = true;
            try
            {

                if (svm.ExcessMaterialLineId <= 0)
                    BeforeSave = ExcessMaterialDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.ExcessMaterialHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = ExcessMaterialDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.ExcessMaterialHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.ProductId <= 0)
                ModelState.AddModelError("ProductId", "The Product field is required");

            if (svm.ExcessMaterialLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }


            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.ExcessMaterialLineId <= 0)
                {

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ProductUidId = svm.ProductUidId;
                    s.Sr = _ExcessMaterialLineService.GetMaxSr(s.ExcessMaterialHeaderId);
                    s.ObjectState = Model.ObjectState.Added;
                    db.ExcessMaterialLine.Add(s);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        db.ExcessMaterialHeader.Add(temp);
                    }

                    try
                    {
                        ExcessMaterialDocEvents.onLineSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, s.ExcessMaterialLineId, EventModeConstants.Add), ref db);
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
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.ExcessMaterialHeaderId,
                        DocLineId = s.ExcessMaterialLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    try
                    {
                        ExcessMaterialDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, s.ExcessMaterialLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    return RedirectToAction("_Create", new { id = svm.ExcessMaterialHeaderId });

                }


                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    int status = temp.Status;
                    ExcessMaterialLine templine = _ExcessMaterialLineService.Find(s.ExcessMaterialLineId);

                    ExcessMaterialLine ExRec = new ExcessMaterialLine();
                    ExRec = Mapper.Map<ExcessMaterialLine>(templine);

                    templine.ProductId = s.ProductId;
                    templine.ProductUidId = s.ProductUidId;
                    templine.Dimension1Id = s.Dimension1Id;
                    templine.Dimension2Id = s.Dimension2Id;
                    templine.Dimension3Id = s.Dimension3Id;
                    templine.Dimension4Id = s.Dimension4Id;
                    templine.LotNo = s.LotNo;
                    templine.ProcessId = s.ProcessId;
                    templine.ProductUidId = s.ProductUidId;
                    templine.Remark = s.Remark;
                    templine.Qty = s.Qty;
                    templine.Remark = s.Remark;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.ExcessMaterialLine.Add(templine);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ObjectState = Model.ObjectState.Modified;
                    }
                    db.ExcessMaterialHeader.Add(temp);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        ExcessMaterialDocEvents.onLineSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, templine.ExcessMaterialLineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        ExcessMaterialDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, templine.ExcessMaterialLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = templine.ExcessMaterialHeaderId,
                        DocLineId = templine.ExcessMaterialLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));                   

                    //End of Saving the Activity Log

                    return Json(new { success = true });
                }

            }
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


        private ActionResult _Modify(int id)
        {
            ExcessMaterialLineViewModel temp = _ExcessMaterialLineService.GetExcessMaterialLine(id);

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

            ExcessMaterialHeader H = new ExcessMaterialHeaderService(_unitOfWork,db).Find(temp.ExcessMaterialHeaderId);

            //Getting Settings
            var settings = new ExcessMaterialSettingsService(_unitOfWork, db).GetExcessMaterialSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ExcessMaterialSettings = Mapper.Map<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>(settings);

           
            PrepareViewBag(temp);
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
            ExcessMaterialLineViewModel temp = _ExcessMaterialLineService.GetExcessMaterialLine(id);

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

            ExcessMaterialHeader H = new ExcessMaterialHeaderService(_unitOfWork, db).Find(temp.ExcessMaterialHeaderId);

            //Getting Settings
            var settings = new ExcessMaterialSettingsService(_unitOfWork, db).GetExcessMaterialSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.ExcessMaterialSettings = Mapper.Map<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>(settings);


            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(ExcessMaterialLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = ExcessMaterialDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.ExcessMaterialHeaderId, vm.ExcessMaterialLineId), ref db);
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
                int? ProdUid = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                //ExcessMaterialLine ExcessMaterialLine = _ExcessMaterialLineService.Find(vm.ExcessMaterialLineId);
                ExcessMaterialLine ExcessMaterialLine = (from p in db.ExcessMaterialLine
                                       where p.ExcessMaterialLineId == vm.ExcessMaterialLineId
                                       select p).FirstOrDefault();
                ExcessMaterialHeader header = new ExcessMaterialHeaderService(_unitOfWork,db).Find(ExcessMaterialLine.ExcessMaterialHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<ExcessMaterialLine>(ExcessMaterialLine),
                });

                ProdUid = ExcessMaterialLine.ProductUidId;
                
                ExcessMaterialLine.ObjectState = Model.ObjectState.Deleted;
                db.ExcessMaterialLine.Remove(ExcessMaterialLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.ExcessMaterialHeader.Add(header);
                }


                //if (ProdUid != null && ProdUid > 0)
                //{
                //    ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)ProdUid, "ExcessMaterial Head-" + vm.ExcessMaterialHeaderId.ToString());

                //    ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)ProdUid);

                //    ProductUid.LastTransactionDocDate = ProductUidDetail.LastTransactionDocDate;
                //    ProductUid.LastTransactionDocId = ProductUidDetail.LastTransactionDocId;
                //    ProductUid.LastTransactionDocNo = ProductUidDetail.LastTransactionDocNo;
                //    ProductUid.LastTransactionDocTypeId = ProductUidDetail.LastTransactionDocTypeId;
                //    ProductUid.LastTransactionPersonId = ProductUidDetail.LastTransactionPersonId;
                //    ProductUid.CurrenctGodownId = ProductUidDetail.CurrenctGodownId;
                //    ProductUid.CurrenctProcessId = ProductUidDetail.CurrenctProcessId;
                //    ProductUid.ObjectState = Model.ObjectState.Modified;
                //    db.ProductUid.Add(ProductUid);
                //}


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    ExcessMaterialDocEvents.onLineDeleteEvent(this, new StockEventArgs(ExcessMaterialLine.ExcessMaterialHeaderId, ExcessMaterialLine.ExcessMaterialLineId), ref db);
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
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    ExcessMaterialDocEvents.afterLineDeleteEvent(this, new StockEventArgs(ExcessMaterialLine.ExcessMaterialHeaderId, ExcessMaterialLine.ExcessMaterialLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.ExcessMaterialHeaderId,
                    DocLineId = ExcessMaterialLine.ExcessMaterialLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));               

            }

            return Json(new { success = true });

        }

        public JsonResult GetBarCodeDetails(string ProductUId)
        {
            return Json(_ExcessMaterialLineService.GetBarCodeDetail(ProductUId), JsonRequestBehavior.AllowGet);
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
