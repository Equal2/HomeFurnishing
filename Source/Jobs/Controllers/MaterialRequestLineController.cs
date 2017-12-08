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
using MaterialRequestDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class MaterialRequestLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        IRequisitionLineService _RequisitionLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public MaterialRequestLineController(IRequisitionLineService Stock, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _RequisitionLineService = Stock;
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
            var p = _RequisitionLineService.GetRequisitionLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        private void PrepareViewBag(RequisitionLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            RequisitionHeaderViewModel H = new RequisitionHeaderService(_unitOfWork).GetRequisitionHeader(vm.RequisitionHeaderId);
            ViewBag.DocNo = H.DocTypeName + "-" + H.DocNo;
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

        public ActionResult _Create(int Id) //Id ==>Sale Order Header Id
        {
            RequisitionHeader H = new RequisitionHeaderService(_unitOfWork).Find(Id);
            RequisitionLineViewModel s = new RequisitionLineViewModel();

            //Getting Settings
            var settings = new MaterialRequestSettingsService(_unitOfWork).GetMaterialRequestSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.MaterialRequestSettings = Mapper.Map<MaterialRequestSettings, MaterialRequestSettingsViewModel>(settings);

            s.RequisitionHeaderId = H.RequisitionHeaderId;
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
        public ActionResult _CreatePost(RequisitionLineViewModel svm)
        {
            RequisitionHeader temp = new RequisitionHeaderService(_unitOfWork).Find(svm.RequisitionHeaderId);

            RequisitionLine s = Mapper.Map<RequisitionLineViewModel, RequisitionLine>(svm);
            if (svm.MaterialRequestSettings != null)
            {
                if (svm.MaterialRequestSettings.isMandatoryProcessLine == true && (svm.ProcessId <= 0 || svm.ProcessId == null))
                {
                    ModelState.AddModelError("ProcessId", "The Process field is required");
                }
            }

            bool BeforeSave = true;
            try
            {

                if (svm.RequisitionLineId <= 0)
                    BeforeSave = MaterialRequestDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.RequisitionHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = MaterialRequestDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.RequisitionHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "The Qty field is required");
            }

            if (svm.RequisitionLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.RequisitionLineId <= 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    //_RequisitionLineService.Create(s);
                    db.RequisitionLine.Add(s);

                    new RequisitionLineStatusService(_unitOfWork).CreateLineStatus(s.RequisitionLineId, ref db);

                    //RequisitionHeader header = new RequisitionHeaderService(_unitOfWork).Find(s.RequisitionHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;

                        temp.ObjectState = Model.ObjectState.Modified;
                        db.RequisitionHeader.Add(temp);
                        //new RequisitionHeaderService(_unitOfWork).Update(temp);
                    }

                    try
                    {
                        MaterialRequestDocEvents.onLineSaveEvent(this, new StockEventArgs(s.RequisitionHeaderId, s.RequisitionLineId, EventModeConstants.Add), ref db);
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
                        MaterialRequestDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.RequisitionHeaderId, s.RequisitionLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.RequisitionHeaderId,
                        DocLineId = s.RequisitionLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.RequisitionHeaderId });

                }


                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    RequisitionLine templine = _RequisitionLineService.Find(s.RequisitionLineId);

                    int Status = temp.Status;

                    RequisitionLine ExRec = new RequisitionLine();
                    ExRec = Mapper.Map<RequisitionLine>(templine);

                    templine.ProductId = s.ProductId;
                    templine.Specification = s.Specification;
                    templine.Dimension1Id = s.Dimension1Id;
                    templine.Dimension2Id = s.Dimension2Id;
                    templine.Dimension3Id = s.Dimension3Id;
                    templine.Dimension4Id = s.Dimension4Id;
                    templine.ProcessId = s.ProcessId;
                    templine.DueDate = s.DueDate;
                    templine.Remark = s.Remark;
                    templine.Qty = s.Qty;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    //_RequisitionLineService.Update(templine);
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.RequisitionLine.Add(templine);


                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;

                        temp.ObjectState = Model.ObjectState.Modified;
                        db.RequisitionHeader.Add(temp);
                        //new RequisitionHeaderService(_unitOfWork).Update(temp);
                    }
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        MaterialRequestDocEvents.onLineSaveEvent(this, new StockEventArgs(s.RequisitionHeaderId, templine.RequisitionLineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(svm);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        MaterialRequestDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.RequisitionHeaderId, templine.RequisitionLineId, EventModeConstants.Edit), ref db);
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
                        DocId = templine.RequisitionHeaderId,
                        DocLineId = templine.RequisitionLineId,
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
            RequisitionLineViewModel temp = _RequisitionLineService.GetRequisitionLine(id);

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

            RequisitionHeader H = new RequisitionHeaderService(_unitOfWork).Find(temp.RequisitionHeaderId);

            //Getting Settings
            var settings = new MaterialRequestSettingsService(_unitOfWork).GetMaterialRequestSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.MaterialRequestSettings = Mapper.Map<MaterialRequestSettings, MaterialRequestSettingsViewModel>(settings);
            
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

        private ActionResult _Delete(int id)
        {
            RequisitionLineViewModel temp = _RequisitionLineService.GetRequisitionLine(id);

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

            RequisitionHeader H = new RequisitionHeaderService(_unitOfWork).Find(temp.RequisitionHeaderId);

            //Getting Settings
            var settings = new MaterialRequestSettingsService(_unitOfWork).GetMaterialRequestSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.MaterialRequestSettings = Mapper.Map<MaterialRequestSettings, MaterialRequestSettingsViewModel>(settings);

            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        private ActionResult _Detail(int id)
        {
            RequisitionLineViewModel temp = _RequisitionLineService.GetRequisitionLine(id);

            RequisitionHeader H = new RequisitionHeaderService(_unitOfWork).Find(temp.RequisitionHeaderId);

            //Getting Settings
            var settings = new MaterialRequestSettingsService(_unitOfWork).GetMaterialRequestSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.MaterialRequestSettings = Mapper.Map<MaterialRequestSettings, MaterialRequestSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(RequisitionLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = MaterialRequestDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.RequisitionHeaderId, vm.RequisitionLineId), ref db);
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

                //RequisitionLine RequisitionLine = _RequisitionLineService.Find(vm.RequisitionLineId);

                RequisitionLine RequisitionLine = (from p in db.RequisitionLine
                                                   where p.RequisitionLineId == vm.RequisitionLineId
                                                   select p).FirstOrDefault();

                RequisitionLineStatus LineStat = (from p in db.RequisitionLineStatus
                                                  where p.RequisitionLineId == vm.RequisitionLineId
                                                  select p).FirstOrDefault();

                RequisitionHeader header = new RequisitionHeaderService(_unitOfWork).Find(RequisitionLine.RequisitionHeaderId);

                RequisitionLine ExRec = new RequisitionLine();
                ExRec = Mapper.Map<RequisitionLine>(RequisitionLine);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                });

                //new RequisitionLineStatusService(_unitOfWork).Delete(RequisitionLine.RequisitionLineId);
                if (LineStat != null)
                {
                    LineStat.ObjectState = Model.ObjectState.Deleted;
                    db.RequisitionLineStatus.Remove(LineStat);
                }
                RequisitionLine.ObjectState = Model.ObjectState.Deleted;
                db.RequisitionLine.Remove(RequisitionLine);

                //_RequisitionLineService.Delete(RequisitionLine);

                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;

                    db.RequisitionHeader.Add(header);
                    //new RequisitionHeaderService(_unitOfWork).Update(header);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    MaterialRequestDocEvents.onLineDeleteEvent(this, new StockEventArgs(RequisitionLine.RequisitionHeaderId, RequisitionLine.RequisitionLineId), ref db);
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
                    MaterialRequestDocEvents.afterLineDeleteEvent(this, new StockEventArgs(RequisitionLine.RequisitionHeaderId, RequisitionLine.RequisitionLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.RequisitionHeaderId,
                    DocLineId = RequisitionLine.RequisitionLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));           

            }

            return Json(new { success = true });

        }

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId });
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
