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
using Jobs.Helpers;
using Model.ViewModel;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using StockIssueDocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class StockLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;
        IStockLineService _StockLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public StockLineController(IStockLineService Stock, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockLineService = Stock;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _ForRequisition(int id, int sid)
        {
            RequisitionFiltersForIssue vm = new RequisitionFiltersForIssue();
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.StockHeaderId = id;
            vm.PersonId = sid;
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(RequisitionFiltersForIssue vm, string All)
        {
            List<StockLineViewModel> temp = _StockLineService.GetRequisitionsForFilters(vm, (string.IsNullOrEmpty(All) ? false : true)).ToList();

            StockMasterDetailModel svm = new StockMasterDetailModel();
            svm.StockLineViewModel = temp;
            //Getting Settings           
            var Header = new StockHeaderService(_unitOfWork).Find(vm.StockHeaderId);
            svm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId));
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(StockMasterDetailModel vm)
        {
            int Cnt = 0;
            int pk = 0;
            int Serial = _StockLineService.GetMaxSr(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);

            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);


            if (Settings.isMandatoryLineCostCenter == true && vm.StockLineViewModel.Where(m => m.CostCenterId == null).Any())
            {
                ModelState.AddModelError("", "CostCenter is mandatory");
            }

            decimal Qty = vm.StockLineViewModel.Where(m => m.Rate > 0).Sum(m => m.Qty);

            bool BeforeSave = true;
            try
            {
                BeforeSave = StockIssueDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
                foreach (var item in vm.StockLineViewModel)
                {
                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                    if (item.Qty > 0)
                    {
                        StockLine line = new StockLine();

                        line.StockHeaderId = item.StockHeaderId;
                        line.RequisitionLineId = item.RequisitionLineId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.CostCenterId = item.CostCenterId;
                        line.Qty = item.Qty;
                        line.DocNature = StockNatureConstants.Issue;
                        line.Rate = item.Rate ?? 0;
                        line.Amount = (line.Qty * line.Rate);
                        line.ReferenceDocId = item.ReferenceDocId;
                        line.ReferenceDocTypeId = item.ReferenceDocTypeId;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.StockLineId = pk;
                        line.Sr = Serial++;
                        line.ObjectState = Model.ObjectState.Added;
                        db.StockLine.Add(line);
                        pk++;
                        Cnt = Cnt + 1;
                        if (line.RequisitionLineId.HasValue)
                            LineStatus.Add(line.RequisitionLineId.Value, line.Qty);
                    }

                }
                //new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyIssueMultiple(LineStatus, Header.DocDate, ref db);

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.StockHeader.Add(Header);

                try
                {
                    StockIssueDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
                    StockIssueDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.StockHeaderId,
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
        public JsonResult Index(int id)
        {
            var p = _StockLineService.GetStockLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        private void PrepareViewBag(StockLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            StockHeaderViewModel H = new StockHeaderService(_unitOfWork).GetStockHeader(vm.StockHeaderId);
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
            StockHeader H = new StockHeaderService(_unitOfWork).Find(Id);
            StockLineViewModel s = new StockLineViewModel();

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            s.PersonId = H.PersonId;
            s.StockHeaderId = H.StockHeaderId;
            s.GodownId = H.GodownId;
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
        public ActionResult _CreatePost(StockLineViewModel svm)
        {
            StockHeader temp = new StockHeaderService(_unitOfWork).Find(svm.StockHeaderId);
            StockLine s = Mapper.Map<StockLineViewModel, StockLine>(svm);

            if (svm.StockHeaderSettings != null)
            {
                if (svm.StockHeaderSettings.isMandatoryProcessLine == true && (svm.FromProcessId <= 0 || svm.FromProcessId == null))
                {
                    ModelState.AddModelError("FromProcessId", "The Process field is required");
                }
                if (svm.StockHeaderSettings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
                if (svm.StockHeaderSettings.isMandatoryLineCostCenter == true && !svm.CostCenterId.HasValue)
                {
                    ModelState.AddModelError("CostCenterId", "The Cost Center field is required");
                }

            }

            bool BeforeSave = true;
            try
            {

                if (svm.StockLineId <= 0)
                    BeforeSave = StockIssueDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = StockIssueDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);

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

            if (svm.StockLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (svm.StockLineId <= 0)
                {

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.DocNature = StockNatureConstants.Issue;
                    s.ModifiedBy = User.Identity.Name;
                    s.ProductUidId = svm.ProductUidId;
                    s.Sr = _StockLineService.GetMaxSr(s.StockHeaderId);
                    s.ObjectState = Model.ObjectState.Added;
                    db.StockLine.Add(s);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;

                        db.StockHeader.Add(temp);
                    }                

                    try
                    {
                        StockIssueDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
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
                        StockIssueDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderId,
                        DocLineId = s.StockLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.StockHeaderId });

                }


                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    int status = temp.Status;
                    StockLine templine = _StockLineService.Find(s.StockLineId);

                    StockLine ExRec = new StockLine();
                    ExRec = Mapper.Map<StockLine>(templine);

                    templine.ProductId = s.ProductId;
                    templine.ProductUidId = s.ProductUidId;
                    templine.RequisitionLineId = s.RequisitionLineId;
                    templine.Specification = s.Specification;
                    templine.Dimension1Id = s.Dimension1Id;
                    templine.Dimension2Id = s.Dimension2Id;
                    templine.Dimension3Id = s.Dimension3Id;
                    templine.Dimension4Id = s.Dimension4Id;
                    templine.CostCenterId = s.CostCenterId;
                    templine.DocNature = StockNatureConstants.Issue;
                    templine.Rate = s.Rate;
                    templine.Amount = s.Amount;
                    templine.LotNo = s.LotNo;
                    templine.FromProcessId = s.FromProcessId;
                    templine.Remark = s.Remark;
                    templine.Qty = s.Qty;
                    templine.Remark = s.Remark;
                    templine.ReferenceDocId = s.ReferenceDocId;
                    templine.ReferenceDocTypeId = s.ReferenceDocTypeId;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.StockLine.Add(templine);

                    //if (templine.RequisitionLineId.HasValue)
                    //    new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnIssue(templine.RequisitionLineId.Value, templine.StockLineId, temp.DocDate, templine.Qty, ref db, true);


                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ObjectState = Model.ObjectState.Modified;
                    }
                    db.StockHeader.Add(temp);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        StockIssueDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
                        StockIssueDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
                        DocId = templine.StockHeaderId,
                        DocLineId = templine.StockLineId,
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
            StockLineViewModel temp = _StockLineService.GetStockLineForIssue(id);

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

            StockHeader H = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);


            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            temp.GodownId = H.GodownId;
          
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
            StockLineViewModel temp = _StockLineService.GetStockLineForIssue(id);

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

            StockHeader H = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            temp.GodownId = H.GodownId;

            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        private ActionResult _Detail(int id)
        {
            StockLineViewModel temp = _StockLineService.GetStockLineForIssue(id);

            if (temp == null)
            {
                return HttpNotFound();
            }         

            StockHeader H = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            temp.GodownId = H.GodownId;

            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(StockLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = StockIssueDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.StockHeaderId, vm.StockLineId), ref db);
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

                //StockLine StockLine = _StockLineService.Find(vm.StockLineId);
                StockLine StockLine = (from p in db.StockLine
                                       where p.StockLineId == vm.StockLineId
                                       select p).FirstOrDefault();
                StockHeader header = new StockHeaderService(_unitOfWork).Find(StockLine.StockHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<StockLine>(StockLine),
                });

                ProdUid = StockLine.ProductUidId;

                if (StockLine.RequisitionLineId.HasValue)
                    new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnIssue(StockLine.RequisitionLineId.Value, StockLine.StockLineId, header.DocDate, 0, ref db, true);

                StockLine.ObjectState = Model.ObjectState.Deleted;
                db.StockLine.Remove(StockLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(header);
                }


                //if (ProdUid != null && ProdUid > 0)
                //{
                //    ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)ProdUid, "Stock Head-" + vm.StockHeaderId.ToString());

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
                    StockIssueDocEvents.onLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
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
                    StockIssueDocEvents.afterLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.StockHeaderId,
                    DocLineId = StockLine.StockLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));           

            }

            return Json(new { success = true });

        }

        public JsonResult GetProductDetailJson(int ProductId, int StockId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, UnitName = product.UnitName, Specification = product.ProductSpecification });
        }

        public JsonResult GetPendingRequestOrders(int ProductId, int sid)
        {
            return Json(new RequisitionLineService(_unitOfWork).GetPendingRequisitionLines(ProductId, sid).ToList());
        }

        public JsonResult GetRequOrderDetail(int RequisitionLineId)
        {
            return Json(new RequisitionLineService(_unitOfWork).GetRequsitionLineDetail(RequisitionLineId));
        }


        public JsonResult GetRequisitionDetail(int RequisitionId)
        {
            return Json(_StockLineService.GetRequisitionBalanceForIssue(RequisitionId));
        }

        public JsonResult GetCustomProducts(int id, int PersonId, string term, int Limit)//Indent Header ID
        {
            return Json(_StockLineService.GetProductHelpList(id, PersonId, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRequisitions(int id, int PersonId, string term)//Receipt Header ID
        {
            return Json(_StockLineService.GetPendingRequisitionHelpList(id, PersonId, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProducts(int id, int PersonId, string term, int Limit)//SupplierID
        {
            return Json(_StockLineService.GetProductHelpListForFilters(id, PersonId, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int MaterialIssueId, string ProcName)
        {
            return Json(_StockLineService.GetExcessStock(ProductId, Dim1, Dim2, ProcId, Lot, MaterialIssueId, ProcName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = _StockLineService.GetCostCentersForIssueFilters(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _StockLineService.GetCostCentersForIssueFilters(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public JsonResult ValidateBarCode(string ProductUId,int StockHeader)
        {
            return Json(_StockLineService.ValidateBarCodeOnStock(ProductUId, StockHeader), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetLineCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = _StockLineService.GetCostCentersForLine(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _StockLineService.GetCostCentersForLine(filter, searchTerm).Count();

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
