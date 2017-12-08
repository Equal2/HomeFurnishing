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
using PurchaseIndentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseIndentLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;
        IPurchaseIndentLineService _PurchaseIndentLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseIndentLineController(IPurchaseIndentLineService PurchaseIndent, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseIndentLineService = PurchaseIndent;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _ForMaterialPlan(int id)
        {
            PurchaseIndentLineFilterViewModel vm = new PurchaseIndentLineFilterViewModel();
            vm.PurchaseIndentHeaderId = id;
            PurchaseIndentHeader Header = new PurchaseIndentHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseIndentLineFilterViewModel vm)
        {
            List<PurchaseIndentLineViewModel> temp = _PurchaseIndentLineService.GetPurchaseIndentForFilters(vm).ToList();

            PurchaseIndentMasterDetailModel svm = new PurchaseIndentMasterDetailModel();
            svm.PurchaseIndentLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseIndentMasterDetailModel vm)
        {
            int Serial = _PurchaseIndentLineService.GetMaxSr(vm.PurchaseIndentLineViewModel.FirstOrDefault().PurchaseIndentHeaderId);
            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseIndentDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseIndentLineViewModel.FirstOrDefault().PurchaseIndentHeaderId), ref db);
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
                PurchaseIndentHeader header = new PurchaseIndentHeaderService(_unitOfWork).Find(vm.PurchaseIndentLineViewModel.FirstOrDefault().PurchaseIndentHeaderId);
                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentHeader.Add(header);
                }

                foreach (var item in vm.PurchaseIndentLineViewModel)
                {
                    if (item.Qty > 0)
                    {
                        PurchaseIndentLine line = new PurchaseIndentLine();

                        line.PurchaseIndentHeaderId = item.PurchaseIndentHeaderId;
                        line.MaterialPlanLineId = item.MaterialPlanLineId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Specification = item.Specification;
                        line.Remark = item.Remark;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;

                        line.ObjectState = Model.ObjectState.Added;
                        //_PurchaseIndentLineService.Create(line);
                        db.PurchaseIndentLine.Add(line);

                    }
                }

                try
                {
                    PurchaseIndentDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseIndentLineViewModel.FirstOrDefault().PurchaseIndentHeaderId), ref db);
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
                    PurchaseIndentDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseIndentLineViewModel.FirstOrDefault().PurchaseIndentHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseIndentHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = header.DocNo,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _PurchaseIndentLineService.GetPurchaseIndentLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Create(int Id) //Id ==>Sale Order Header Id
        {
            PurchaseIndentHeader H = new PurchaseIndentHeaderService(_unitOfWork).Find(Id);
            PurchaseIndentLineViewModel s = new PurchaseIndentLineViewModel();
            s.PurchaseIndentHeaderId = H.PurchaseIndentHeaderId;
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            return View(s);
        }

        [HttpGet]
        public ActionResult CreateLine(int id)
        {
            return _Create(id, null);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id)
        {
            return _Create(id, null);
        }

        public ActionResult _Create(int Id, DateTime? date) //Id ==>Sale Order Header Id
        {
            PurchaseIndentHeader H = new PurchaseIndentHeaderService(_unitOfWork).Find(Id);
            PurchaseIndentLineViewModel s = new PurchaseIndentLineViewModel();

            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            s.PurchaseIndentHeaderId = H.PurchaseIndentHeaderId;
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            if (date != null) s.DueDate = date;
            ViewBag.LineMode = "Create";
            return PartialView("_Create", s);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseIndentLineViewModel svm)
        {
            PurchaseIndentLine s = Mapper.Map<PurchaseIndentLineViewModel, PurchaseIndentLine>(svm);
            PurchaseIndentHeader temp = new PurchaseIndentHeaderService(_unitOfWork).Find(s.PurchaseIndentHeaderId);
            ViewBag.Status = temp.Status;

            if (svm.PurchaseIndentLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (temp.DocDate > s.DueDate && s.DueDate != null)
            {
                ModelState.AddModelError("DueDate", "Duedate should be greater than Docdate");
            }
            if (s.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "Qty field is required");
            }

            bool BeforeSave = true;

            try
            {

                if (svm.PurchaseIndentLineId <= 0)
                    BeforeSave = PurchaseIndentDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseIndentHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseIndentDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseIndentHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.PurchaseIndentLineId <= 0)
                {
                    s.Sr = _PurchaseIndentLineService.GetMaxSr(s.PurchaseIndentHeaderId);
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    //_PurchaseIndentLineService.Create(s);
                    db.PurchaseIndentLine.Add(s);

                    PurchaseIndentHeader header = new PurchaseIndentHeaderService(_unitOfWork).Find(s.PurchaseIndentHeaderId);
                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        //new PurchaseIndentHeaderService(_unitOfWork).Update(header);
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        header.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseIndentHeader.Add(header);
                    }

                    try
                    {
                        PurchaseIndentDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentHeaderId, s.PurchaseIndentLineId, EventModeConstants.Add), ref db);
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
                        PurchaseIndentDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentHeaderId, s.PurchaseIndentLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseIndentHeaderId,
                        DocLineId = s.PurchaseIndentLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.PurchaseIndentHeaderId, date = ((svm.DueDate == null) ? null : svm.DueDate) });
                    //return _Create(svm.PurchaseIndentHeaderId, ((svm.DueDate == null) ? null : svm.DueDate));

                }


                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseIndentLine templine = _PurchaseIndentLineService.Find(s.PurchaseIndentLineId);

                    int Status = temp.Status;
                    PurchaseIndentLine ExRec = new PurchaseIndentLine();
                    ExRec = Mapper.Map<PurchaseIndentLine>(templine);


                    templine.DueDate = s.DueDate;
                    templine.ProductId = s.ProductId;
                    templine.Qty = s.Qty;
                    templine.Remark = s.Remark;
                    templine.Dimension1Id = s.Dimension1Id;
                    templine.Dimension2Id = s.Dimension2Id;
                    templine.Specification = s.Specification;
                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    //_PurchaseIndentLineService.Update(templine);
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentLine.Add(templine);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });



                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        //new PurchaseIndentHeaderService(_unitOfWork).Update(temp);
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseIndentHeader.Add(temp);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseIndentDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentHeaderId, templine.PurchaseIndentLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseIndentDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentHeaderId, templine.PurchaseIndentLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = templine.PurchaseIndentHeaderId,
                        DocLineId = templine.PurchaseIndentLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return Json(new { success = true });
                }

            }

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
            PurchaseIndentLineViewModel temp = _PurchaseIndentLineService.GetPurchaseIndentLine(id);

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

            PurchaseIndentHeader H = new PurchaseIndentHeaderService(_unitOfWork).Find(temp.PurchaseIndentHeaderId);
            //ViewBag.DocNo = H.DocNo;

            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

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

            PurchaseIndentLineViewModel PurchaseIndentLine = _PurchaseIndentLineService.GetPurchaseIndentLine(id);
            if (PurchaseIndentLine == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocumentLine(new DocumentUniqueId { LockReason = PurchaseIndentLine.LockReason }, User.Identity.Name, out ExceptionMsg, out Continue);

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

            PurchaseIndentHeader H = new PurchaseIndentHeaderService(_unitOfWork).Find(PurchaseIndentLine.PurchaseIndentHeaderId);

            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            PurchaseIndentLine.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);

            return PartialView("_Create", PurchaseIndentLine);
        }

        public ActionResult _Detail(int id)
        {

            PurchaseIndentLineViewModel PurchaseIndentLine = _PurchaseIndentLineService.GetPurchaseIndentLine(id);

            if (PurchaseIndentLine == null)
            {
                return HttpNotFound();
            }

            PurchaseIndentHeader H = new PurchaseIndentHeaderService(_unitOfWork).Find(PurchaseIndentLine.PurchaseIndentHeaderId);

            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            PurchaseIndentLine.PurchIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);

            return PartialView("_Create", PurchaseIndentLine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseIndentLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseIndentDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseIndentHeaderId, vm.PurchaseIndentLineId), ref db);
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

                PurchaseIndentLine PurchaseIndentLine = db.PurchaseIndentLine.Find(vm.PurchaseIndentLineId);

                try
                {
                    PurchaseIndentDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseIndentLine.PurchaseIndentHeaderId, PurchaseIndentLine.PurchaseIndentLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseIndentLine>(PurchaseIndentLine),
                });

                //_PurchaseIndentLineService.Delete(PurchaseIndentLine);
                PurchaseIndentHeader header = new PurchaseIndentHeaderService(_unitOfWork).Find(PurchaseIndentLine.PurchaseIndentHeaderId);
                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    //new PurchaseIndentHeaderService(_unitOfWork).Update(header);
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentHeader.Add(header);
                }

                PurchaseIndentLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseIndentLine.Remove(PurchaseIndentLine);

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
                    PurchaseIndentDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseIndentLine.PurchaseIndentHeaderId, PurchaseIndentLine.PurchaseIndentLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseIndentHeaderId,
                    DocLineId = PurchaseIndentLine.PurchaseIndentLineId,
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
            List<Product> ProductJson = new List<Product>();

            ProductJson.Add(new Product()
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId,
                ProductSpecification = product.ProductSpecification,
            });

            return Json(ProductJson);
        }

        public JsonResult GetMaterialPlans(int id, string term)//Indent Header ID
        {

            return Json(_PurchaseIndentLineService.GetPendingMaterialPlanHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseIndentLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
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
