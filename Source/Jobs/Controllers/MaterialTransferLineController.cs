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
using MaterialTransferDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class MaterialTransferLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IStockLineService _StockLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public MaterialTransferLineController(IStockLineService Stock, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockLineService = Stock;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _ForStockIn(int id)
        {
            StockInFiltersForIssue vm = new StockInFiltersForIssue();
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.StockHeaderId = id;
            return PartialView("_FiltersStockIn", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostStockIn(StockInFiltersForIssue vm)
        {
            List<StockLineViewModel> temp = _StockLineService.GetStockInForFilters(vm).ToList();

            StockMasterDetailModel svm = new StockMasterDetailModel();
            svm.StockLineViewModel = temp;
            //Getting Settings           
            var Header = new StockHeaderService(_unitOfWork).Find(vm.StockHeaderId);
            svm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId));
            return PartialView("_ResultsStockIn", svm);

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
                BeforeSave = MaterialTransferDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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

                        StockViewModel StockViewModel_Issue = new StockViewModel();

                        StockViewModel_Issue.StockId = -Cnt;
                        StockViewModel_Issue.StockHeaderId = Header.StockHeaderId;
                        StockViewModel_Issue.DocHeaderId = Header.StockHeaderId;
                        StockViewModel_Issue.DocLineId = line.StockLineId;
                        StockViewModel_Issue.DocTypeId = Header.DocTypeId;
                        StockViewModel_Issue.StockHeaderDocDate = Header.DocDate;
                        StockViewModel_Issue.StockDocDate = Header.DocDate;
                        StockViewModel_Issue.DocNo = Header.DocNo;
                        StockViewModel_Issue.DivisionId = Header.DivisionId;
                        StockViewModel_Issue.SiteId = Header.SiteId;
                        StockViewModel_Issue.CurrencyId = null;
                        StockViewModel_Issue.PersonId = Header.PersonId;
                        StockViewModel_Issue.ProductId = item.ProductId;
                        StockViewModel_Issue.HeaderFromGodownId = null;
                        StockViewModel_Issue.HeaderGodownId = Header.GodownId;
                        StockViewModel_Issue.HeaderProcessId = Header.ProcessId;
                        StockViewModel_Issue.GodownId = (int)Header.FromGodownId;
                        StockViewModel_Issue.Remark = Header.Remark;
                        StockViewModel_Issue.Status = Header.Status;
                        StockViewModel_Issue.ProcessId = item.ProcessId;
                        StockViewModel_Issue.LotNo = null;
                        StockViewModel_Issue.CostCenterId = (item.CostCenterId == null ? Header.CostCenterId : item.CostCenterId);
                        StockViewModel_Issue.Qty_Iss = item.Qty;
                        StockViewModel_Issue.Qty_Rec = 0;
                        StockViewModel_Issue.Rate = item.Rate;
                        StockViewModel_Issue.ExpiryDate = null;
                        StockViewModel_Issue.Specification = item.Specification;
                        StockViewModel_Issue.Dimension1Id = item.Dimension1Id;
                        StockViewModel_Issue.Dimension2Id = item.Dimension2Id;
                        StockViewModel_Issue.Dimension3Id = item.Dimension3Id;
                        StockViewModel_Issue.Dimension4Id = item.Dimension4Id;
                        StockViewModel_Issue.ProductUidId = item.ProductUidId;
                        StockViewModel_Issue.CreatedBy = User.Identity.Name;
                        StockViewModel_Issue.CreatedDate = DateTime.Now;
                        StockViewModel_Issue.ModifiedBy = User.Identity.Name;
                        StockViewModel_Issue.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel_Issue, ref db);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        line.FromStockId = StockViewModel_Issue.StockId;


                        Cnt = Cnt + 1;
                        StockViewModel StockViewModel_Receive = new StockViewModel();
                        StockViewModel_Receive.StockId = -Cnt;
                        StockViewModel_Receive.StockHeaderId = Header.StockHeaderId;
                        StockViewModel_Receive.DocHeaderId = Header.StockHeaderId;
                        StockViewModel_Receive.DocLineId = line.StockLineId;
                        StockViewModel_Receive.DocTypeId = Header.DocTypeId;
                        StockViewModel_Receive.StockHeaderDocDate = Header.DocDate;
                        StockViewModel_Receive.StockDocDate = Header.DocDate;
                        StockViewModel_Receive.DocNo = Header.DocNo;
                        StockViewModel_Receive.DivisionId = Header.DivisionId;
                        StockViewModel_Receive.SiteId = Header.SiteId;
                        StockViewModel_Receive.CurrencyId = null;
                        StockViewModel_Receive.PersonId = Header.PersonId;
                        StockViewModel_Receive.ProductId = item.ProductId;
                        StockViewModel_Receive.HeaderFromGodownId = null;
                        StockViewModel_Receive.HeaderGodownId = Header.GodownId;
                        StockViewModel_Receive.HeaderProcessId = Header.ProcessId;
                        StockViewModel_Receive.GodownId = (int)Header.GodownId;
                        StockViewModel_Receive.Remark = Header.Remark;
                        StockViewModel_Receive.Status = Header.Status;
                        StockViewModel_Receive.ProcessId = item.ProcessId;
                        StockViewModel_Receive.LotNo = null;
                        StockViewModel_Receive.CostCenterId = (item.CostCenterId == null ? Header.CostCenterId : item.CostCenterId);
                        StockViewModel_Receive.Qty_Iss = 0;
                        StockViewModel_Receive.Qty_Rec = item.Qty;
                        StockViewModel_Receive.Rate = item.Rate;
                        StockViewModel_Receive.ExpiryDate = null;
                        StockViewModel_Receive.Specification = item.Specification;
                        StockViewModel_Receive.Dimension1Id = item.Dimension1Id;
                        StockViewModel_Receive.Dimension2Id = item.Dimension2Id;
                        StockViewModel_Receive.Dimension3Id = item.Dimension3Id;
                        StockViewModel_Receive.Dimension4Id = item.Dimension4Id;
                        StockViewModel_Receive.ProductUidId = item.ProductUidId;
                        StockViewModel_Receive.CreatedBy = User.Identity.Name;
                        StockViewModel_Receive.CreatedDate = DateTime.Now;
                        StockViewModel_Receive.ModifiedBy = User.Identity.Name;
                        StockViewModel_Receive.ModifiedDate = DateTime.Now;

                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel_Receive, ref db);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        line.StockId = StockViewModel_Receive.StockId;


                        

                        if (item.StockInId != null)
                        {
                            StockAdj Adj_IssQty = new StockAdj();
                            Adj_IssQty.StockAdjId = -Cnt;
                            Adj_IssQty.StockInId = (int)item.StockInId;
                            Adj_IssQty.StockOutId = (int)line.FromStockId;
                            Adj_IssQty.DivisionId = Header.DivisionId;
                            Adj_IssQty.SiteId = Header.SiteId;
                            Adj_IssQty.AdjustedQty = item.Qty;
                            Adj_IssQty.ObjectState = Model.ObjectState.Added;
                            db.StockAdj.Add(Adj_IssQty);
                        }


                        line.StockHeaderId = Header.StockHeaderId;
                        line.RequisitionLineId = item.RequisitionLineId;
                        line.StockInId = item.StockInId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.CostCenterId = item.CostCenterId;
                        line.FromProcessId = item.ProcessId;
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
                        //_StockLineService.Create(line);
                        db.StockLine.Add(line);
                        pk++;
                        Cnt = Cnt + 1;
                        if (line.RequisitionLineId.HasValue)
                            LineStatus.Add(line.RequisitionLineId.Value, line.Qty);
                    }

                }
                new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyIssueMultiple(LineStatus, Header.DocDate, ref db);
                //new StockHeaderService(_unitOfWork).Update(Header);

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
                    MaterialTransferDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
                    MaterialTransferDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
            s.StockHeaderId = H.StockHeaderId;
            s.GodownId = H.GodownId;
            s.DocTypeId = H.DocTypeId;
            s.FromGodownId = H.FromGodownId;
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

                if (svm.StockHeaderSettings.isMandatoryProductUID == true && svm.StockHeaderSettings.isVisibleProductUID == true
                    && (svm.ProductUidId == 0 || svm.ProductUidId == null))
                {
                    ModelState.AddModelError("ProductUidId", "Product Uid field is required");
                }

                if (svm.StockHeaderSettings.IsMandatoryStockIn == true)
                {
                    if (svm.StockInId == null)
                    {
                        ModelState.AddModelError("StockInId", "Stock No field is required");
                    }
                }
            }
            bool BeforeSave = true;
            try
            {

                if (svm.StockLineId <= 0)
                    BeforeSave = MaterialTransferDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = MaterialTransferDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

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
                    //Posting in Stock
                    StockViewModel StockViewModel_Issue = new StockViewModel();
                    StockViewModel_Issue.StockHeaderId = temp.StockHeaderId;
                    StockViewModel_Issue.DocHeaderId = temp.StockHeaderId;
                    StockViewModel_Issue.DocLineId = s.StockLineId;
                    StockViewModel_Issue.DocTypeId = temp.DocTypeId;
                    StockViewModel_Issue.StockHeaderDocDate = temp.DocDate;
                    StockViewModel_Issue.StockDocDate = temp.DocDate;
                    StockViewModel_Issue.DocNo = temp.DocNo;
                    StockViewModel_Issue.DivisionId = temp.DivisionId;
                    StockViewModel_Issue.SiteId = temp.SiteId;
                    StockViewModel_Issue.CurrencyId = null;
                    StockViewModel_Issue.HeaderProcessId = null;
                    StockViewModel_Issue.PersonId = temp.PersonId;
                    StockViewModel_Issue.ProductId = s.ProductId;
                    StockViewModel_Issue.HeaderFromGodownId = temp.FromGodownId;
                    StockViewModel_Issue.HeaderGodownId = temp.FromGodownId;
                    StockViewModel_Issue.GodownId = temp.FromGodownId ?? 0;
                    StockViewModel_Issue.ProcessId = s.FromProcessId;
                    StockViewModel_Issue.LotNo = s.LotNo;
                    StockViewModel_Issue.CostCenterId = temp.CostCenterId;
                    StockViewModel_Issue.Qty_Iss = s.Qty;
                    StockViewModel_Issue.Qty_Rec = 0;
                    StockViewModel_Issue.Weight_Iss = s.Weight;
                    StockViewModel_Issue.Weight_Rec = 0;
                    StockViewModel_Issue.Rate = s.Rate;
                    StockViewModel_Issue.ExpiryDate = null;
                    StockViewModel_Issue.Specification = s.Specification;
                    StockViewModel_Issue.Dimension1Id = s.Dimension1Id;
                    StockViewModel_Issue.Dimension2Id = s.Dimension2Id;
                    StockViewModel_Issue.Dimension3Id = s.Dimension3Id;
                    StockViewModel_Issue.Dimension4Id = s.Dimension4Id;
                    StockViewModel_Issue.Remark = s.Remark;
                    StockViewModel_Issue.Status = temp.Status;
                    StockViewModel_Issue.ProductUidId = svm.ProductUidId;
                    StockViewModel_Issue.CreatedBy = temp.CreatedBy;
                    StockViewModel_Issue.CreatedDate = DateTime.Now;
                    StockViewModel_Issue.ModifiedBy = temp.ModifiedBy;
                    StockViewModel_Issue.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel_Issue, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    s.FromStockId = StockViewModel_Issue.StockId;


                    StockViewModel StockViewModel_Receive = new StockViewModel();
                    StockViewModel_Receive.StockHeaderId = temp.StockHeaderId;
                    StockViewModel_Receive.StockId = -1;
                    StockViewModel_Receive.DocHeaderId = temp.StockHeaderId;
                    StockViewModel_Receive.DocLineId = s.StockLineId;
                    StockViewModel_Receive.DocTypeId = temp.DocTypeId;
                    StockViewModel_Receive.StockHeaderDocDate = temp.DocDate;
                    StockViewModel_Receive.StockDocDate = temp.DocDate;
                    StockViewModel_Receive.DocNo = temp.DocNo;
                    StockViewModel_Receive.DivisionId = temp.DivisionId;
                    StockViewModel_Receive.SiteId = temp.SiteId;
                    StockViewModel_Receive.CurrencyId = null;
                    StockViewModel_Receive.HeaderProcessId = null;
                    StockViewModel_Receive.PersonId = temp.PersonId;
                    StockViewModel_Receive.ProductId = s.ProductId;
                    StockViewModel_Receive.HeaderFromGodownId = temp.FromGodownId;
                    StockViewModel_Receive.HeaderGodownId = temp.GodownId;
                    StockViewModel_Receive.GodownId = temp.GodownId ?? 0;
                    StockViewModel_Receive.ProcessId = s.FromProcessId;
                    StockViewModel_Receive.LotNo = s.LotNo;
                    StockViewModel_Receive.CostCenterId = temp.CostCenterId;
                    StockViewModel_Receive.Qty_Iss = 0;
                    StockViewModel_Receive.Qty_Rec = s.Qty;
                    StockViewModel_Receive.Weight_Iss = 0;
                    StockViewModel_Receive.Weight_Rec = s.Weight;
                    StockViewModel_Receive.Rate = s.Rate;
                    StockViewModel_Receive.ExpiryDate = null;
                    StockViewModel_Receive.Specification = s.Specification;
                    StockViewModel_Receive.Dimension1Id = s.Dimension1Id;
                    StockViewModel_Receive.Dimension2Id = s.Dimension2Id;
                    StockViewModel_Receive.Dimension3Id = s.Dimension3Id;
                    StockViewModel_Receive.Dimension4Id = s.Dimension4Id;
                    StockViewModel_Receive.Remark = s.Remark;
                    StockViewModel_Receive.Status = temp.Status;
                    StockViewModel_Receive.ProductUidId = svm.ProductUidId;
                    StockViewModel_Receive.CreatedBy = temp.CreatedBy;
                    StockViewModel_Receive.CreatedDate = DateTime.Now;
                    StockViewModel_Receive.ModifiedBy = temp.ModifiedBy;
                    StockViewModel_Receive.ModifiedDate = DateTime.Now;


                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel_Receive, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    s.StockId = StockViewModel_Receive.StockId;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;


                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)s.FromStockId;
                        Adj_IssQty.DivisionId = temp.DivisionId;
                        Adj_IssQty.SiteId = temp.SiteId;
                        Adj_IssQty.AdjustedQty = s.Qty;
                        Adj_IssQty.ObjectState = Model.ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                        //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }

                    s.StockInId = svm.StockInId;


                    if (s.ProductUidId != null && s.ProductUidId > 0)
                    {
                        //ProductUid Produid = new ProductUidService(_unitOfWork).Find(svm.ProductUidId ?? 0);
                        ProductUid Produid = (from p in db.ProductUid
                                              where p.ProductUIDId == svm.ProductUidId
                                              select p).FirstOrDefault();


                        s.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                        s.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                        s.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                        s.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                        s.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                        s.ProductUidStatus = Produid.Status;
                        s.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                        s.ProductUidCurrentGodownId = Produid.CurrenctGodownId;

                        Produid.LastTransactionDocId = temp.StockHeaderId;
                        Produid.LastTransactionDocNo = temp.DocNo;
                        Produid.LastTransactionDocTypeId = temp.DocTypeId;
                        Produid.LastTransactionDocDate = temp.DocDate;
                        Produid.LastTransactionPersonId = null;
                        Produid.CurrenctGodownId = temp.GodownId;
                        Produid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(Produid);
                        //new ProductUidService(_unitOfWork).Update(Produid);
                        //db.ProductUid.Add(Produid);
                    }

                    s.ObjectState = Model.ObjectState.Added;
                    db.StockLine.Add(s);
                    //_StockLineService.Create(s);


                    //StockHeader header = new StockHeaderService(_unitOfWork).Find(s.StockHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(temp);
                        //new StockHeaderService(_unitOfWork).Update(temp);
                    }

                    try
                    {
                        MaterialTransferDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
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
                        MaterialTransferDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
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
                    templine.Rate = s.Rate;
                    templine.Amount = s.Amount;
                    templine.LotNo = s.LotNo;
                    templine.FromProcessId = s.FromProcessId;
                    templine.Remark = s.Remark;
                    templine.Qty = s.Qty;
                    templine.Weight  = s.Weight;
                    templine.StockInId = s.StockInId;
                    templine.Remark = s.Remark;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.StockLine.Add(templine);
                    //_StockLineService.Update(templine);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });



                    if (templine.FromStockId != null)
                    {
                        StockViewModel StockViewModel_Issue = new StockViewModel();
                        StockViewModel_Issue.StockHeaderId = temp.StockHeaderId;
                        StockViewModel_Issue.StockId = templine.FromStockId ?? 0;
                        StockViewModel_Issue.DocHeaderId = templine.StockHeaderId;
                        StockViewModel_Issue.DocLineId = templine.StockLineId;
                        StockViewModel_Issue.DocTypeId = temp.DocTypeId;
                        StockViewModel_Issue.StockHeaderDocDate = temp.DocDate;
                        StockViewModel_Issue.StockDocDate = temp.DocDate;
                        StockViewModel_Issue.DocNo = temp.DocNo;
                        StockViewModel_Issue.DivisionId = temp.DivisionId;
                        StockViewModel_Issue.SiteId = temp.SiteId;
                        StockViewModel_Issue.CurrencyId = null;
                        StockViewModel_Issue.HeaderProcessId = temp.ProcessId;
                        StockViewModel_Issue.PersonId = temp.PersonId;
                        StockViewModel_Issue.ProductId = s.ProductId;
                        StockViewModel_Issue.HeaderFromGodownId = null;
                        StockViewModel_Issue.HeaderGodownId = temp.FromGodownId;
                        StockViewModel_Issue.GodownId = temp.FromGodownId ?? 0;
                        StockViewModel_Issue.ProcessId = s.FromProcessId;
                        StockViewModel_Issue.LotNo = s.LotNo;
                        StockViewModel_Issue.CostCenterId = temp.CostCenterId;
                        StockViewModel_Issue.Qty_Iss = s.Qty;
                        StockViewModel_Issue.Qty_Rec = 0;
                        StockViewModel_Issue.Weight_Iss = s.Weight;
                        StockViewModel_Issue.Weight_Rec = 0;
                        StockViewModel_Issue.Rate = s.Rate;
                        StockViewModel_Issue.ExpiryDate = null;
                        StockViewModel_Issue.Specification = s.Specification;
                        StockViewModel_Issue.Dimension1Id = s.Dimension1Id;
                        StockViewModel_Issue.Dimension2Id = s.Dimension2Id;
                        StockViewModel_Issue.Dimension3Id = s.Dimension3Id;
                        StockViewModel_Issue.Dimension4Id = s.Dimension4Id;
                        StockViewModel_Issue.Remark = s.Remark;
                        StockViewModel_Issue.ProductUidId = s.ProductUidId;
                        StockViewModel_Issue.Status = temp.Status;
                        StockViewModel_Issue.CreatedBy = templine.CreatedBy;
                        StockViewModel_Issue.CreatedDate = templine.CreatedDate;
                        StockViewModel_Issue.ModifiedBy = User.Identity.Name;
                        StockViewModel_Issue.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel_Issue, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }


                    if (templine.StockId != null)
                    {
                        StockViewModel StockViewModel_Receive = new StockViewModel();
                        StockViewModel_Receive.StockHeaderId = temp.StockHeaderId;
                        StockViewModel_Receive.StockId = templine.StockId ?? 0;
                        StockViewModel_Receive.DocHeaderId = temp.StockHeaderId;
                        StockViewModel_Receive.DocLineId = s.StockLineId;
                        StockViewModel_Receive.DocTypeId = temp.DocTypeId;
                        StockViewModel_Receive.StockHeaderDocDate = temp.DocDate;
                        StockViewModel_Receive.StockDocDate = temp.DocDate;
                        StockViewModel_Receive.DocNo = temp.DocNo;
                        StockViewModel_Receive.DivisionId = temp.DivisionId;
                        StockViewModel_Receive.SiteId = temp.SiteId;
                        StockViewModel_Receive.CurrencyId = null;
                        StockViewModel_Receive.HeaderProcessId = null;
                        StockViewModel_Receive.PersonId = temp.PersonId;
                        StockViewModel_Receive.ProductId = s.ProductId;
                        StockViewModel_Receive.HeaderFromGodownId = temp.FromGodownId;
                        StockViewModel_Receive.HeaderGodownId = temp.GodownId;
                        StockViewModel_Receive.GodownId = temp.GodownId ?? 0;
                        StockViewModel_Receive.ProcessId = s.FromProcessId;
                        StockViewModel_Receive.LotNo = s.LotNo;
                        StockViewModel_Receive.CostCenterId = temp.CostCenterId;
                        StockViewModel_Receive.Qty_Iss = 0;
                        StockViewModel_Receive.Qty_Rec = s.Qty;
                        StockViewModel_Receive.Weight_Iss = 0;
                        StockViewModel_Receive.Weight_Rec = s.Weight;
                        StockViewModel_Receive.Rate = s.Rate;
                        StockViewModel_Receive.ExpiryDate = null;
                        StockViewModel_Receive.Specification = s.Specification;
                        StockViewModel_Receive.Dimension1Id = s.Dimension1Id;
                        StockViewModel_Receive.Dimension2Id = s.Dimension2Id;
                        StockViewModel_Receive.Dimension3Id = s.Dimension3Id;
                        StockViewModel_Receive.Dimension4Id = s.Dimension4Id;
                        StockViewModel_Receive.Remark = s.Remark;
                        StockViewModel_Receive.ProductUidId = s.ProductUidId;
                        StockViewModel_Receive.Status = temp.Status;
                        StockViewModel_Receive.CreatedBy = temp.CreatedBy;
                        StockViewModel_Receive.CreatedDate = DateTime.Now;
                        StockViewModel_Receive.ModifiedBy = temp.ModifiedBy;
                        StockViewModel_Receive.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel_Receive, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }



                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == templine.FromStockId
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        Adj.ObjectState = Model.ObjectState.Deleted;
                        db.StockAdj.Remove(Adj);
                        //new StockAdjService(_unitOfWork).Delete(Adj);
                    }

                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)templine.FromStockId;
                        Adj_IssQty.DivisionId = temp.DivisionId;
                        Adj_IssQty.SiteId = temp.SiteId;
                        Adj_IssQty.AdjustedQty = svm.Qty;
                        Adj_IssQty.ObjectState = Model.ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                        //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(temp);
                        //new StockHeaderService(_unitOfWork).Update(temp);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        MaterialTransferDocEvents.onLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
                        MaterialTransferDocEvents.afterLineSaveEvent(this, new StockEventArgs(s.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
            StockLineViewModel temp = _StockLineService.GetStockLine(id);

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
            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            temp.GodownId = H.GodownId;
            temp.DocTypeId = H.DocTypeId;
            PrepareViewBag(temp);
            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            StockLineViewModel temp = _StockLineService.GetStockLine(id);

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
            StockLineViewModel temp = _StockLineService.GetStockLine(id);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(StockLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = MaterialTransferDocEvents.beforeLineDeleteEvent(this, new StockEventArgs(vm.StockHeaderId, vm.StockLineId), ref db);
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

                int? FromStockId = 0;
                int? StockId = 0;
                int? ProdUid = 0;

                StockLine StockLine = (from p in db.StockLine
                                       where p.StockLineId == vm.StockLineId
                                       select p).FirstOrDefault();

                StockHeader header = new StockHeaderService(_unitOfWork).Find(StockLine.StockHeaderId);

                StockLine ExRec = new StockLine();
                ExRec = Mapper.Map<StockLine>(StockLine);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                });

                FromStockId = StockLine.FromStockId;
                StockId = StockLine.StockId;
                ProdUid = StockLine.ProductUidId;

                if (ProdUid != null && ProdUid != 0)
                {
                    Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)ProdUid, "Stock Transfer-" + vm.StockHeaderId.ToString());

                    ProductUid ProductUid = (from p in db.ProductUid
                                             where p.ProductUIDId == vm.ProductUidId
                                             select p).FirstOrDefault();


                    if (header.StockHeaderId != ProductUid.LastTransactionDocId)
                    {
                        ModelState.AddModelError("", "Bar Code Can't be deleted because this is already issue to another process.");
                        PrepareViewBag(vm);
                        return PartialView("_Create", vm);
                    }


                    ProductUid.LastTransactionDocDate = StockLine.ProductUidLastTransactionDocDate;
                    ProductUid.LastTransactionDocId = StockLine.ProductUidLastTransactionDocId;
                    ProductUid.LastTransactionDocNo = StockLine.ProductUidLastTransactionDocNo;
                    ProductUid.LastTransactionDocTypeId = StockLine.ProductUidLastTransactionDocTypeId;
                    ProductUid.LastTransactionPersonId = StockLine.ProductUidLastTransactionPersonId;
                    ProductUid.CurrenctGodownId = StockLine.ProductUidCurrentGodownId;
                    ProductUid.CurrenctProcessId = StockLine.ProductUidCurrentProcessId;
                    ProductUid.Status = StockLine.ProductUidStatus;




                    //ProductUid.LastTransactionDocDate = ProductUidDetail.LastTransactionDocDate;
                    //ProductUid.LastTransactionDocId = ProductUidDetail.LastTransactionDocId;
                    //ProductUid.LastTransactionDocNo = ProductUidDetail.LastTransactionDocNo;
                    //ProductUid.LastTransactionDocTypeId = ProductUidDetail.LastTransactionDocTypeId;
                    //ProductUid.LastTransactionPersonId = ProductUidDetail.LastTransactionPersonId;
                    //ProductUid.CurrenctGodownId = ProductUidDetail.CurrenctGodownId;
                    //ProductUid.CurrenctProcessId = ProductUidDetail.CurrenctProcessId;
                    ProductUid.ObjectState = Model.ObjectState.Modified;
                    db.ProductUid.Add(ProductUid);

                    //new ProductUidService(_unitOfWork).Update(ProductUid);
                    //db.ProductUid.Add(ProductUid);

                    new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(vm.StockHeaderId, header.DocTypeId, header.SiteId, header.DivisionId, ref db);
                }

                StockLine.ObjectState = Model.ObjectState.Deleted;
                db.StockLine.Remove(StockLine);
                //_StockLineService.Delete(StockLine);


                if (FromStockId != null)
                {
                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == FromStockId
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        //new StockAdjService(_unitOfWork).Delete(Adj);
                        Adj.ObjectState = Model.ObjectState.Deleted;
                        db.StockAdj.Remove(Adj);
                    }


                    new StockService(_unitOfWork).DeleteStockDB((int)FromStockId, ref db, true);
                }

                if (StockId != null)
                {
                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == StockId
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        //new StockAdjService(_unitOfWork).Delete(Adj);
                        Adj.ObjectState = Model.ObjectState.Deleted;
                        db.StockAdj.Remove(Adj);
                    }

                    new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
                }




                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(header);
                    //new StockHeaderService(_unitOfWork).Update(header);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    MaterialTransferDocEvents.onLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
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
                    MaterialTransferDocEvents.afterLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
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

            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId });
        }

        public JsonResult GetPendingProdOrders(int ProductId)
        {
            return Json(new ProdOrderHeaderService(_unitOfWork).GetPendingProdOrders(ProductId).ToList());
        }

        public JsonResult GetProdOrderDetail(int ProdOrderLineId)
        {
            return Json(new ProdOrderLineService(_unitOfWork).GetProdOrderDetailBalance(ProdOrderLineId));
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var temp = _StockLineService.GetCustomProducts(filter, searchTerm).Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = _StockLineService.GetCustomProducts(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int MaterialIssueId, string ProcName)
        {
            return Json(_StockLineService.GetExcessStockForTransfer(ProductId, Dim1, Dim2, ProcId, Lot, MaterialIssueId, ProcName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductPrevProcess(int ProductId, int GodownId, int DocTypeId)
        {
            ProductPrevProcess ProductPrevProcess = new ProductService(_unitOfWork).FGetProductPrevProcess(ProductId, GodownId, DocTypeId);
            List<ProductPrevProcess> ProductPrevProcessJson = new List<ProductPrevProcess>();

            

            if (ProductPrevProcess != null && ProductPrevProcess.ProcessId !=null )
            {
                string ProcessName = new ProcessService(_unitOfWork).Find((int)ProductPrevProcess.ProcessId).ProcessName;
                ProductPrevProcessJson.Add(new ProductPrevProcess()
                {
                    ProcessId = ProductPrevProcess.ProcessId,
                    ProcessName = ProcessName
                });
                return Json(ProductPrevProcessJson);
            }
            else
            {
                return null;
            }

        }

        public JsonResult GetProductCodeDetailJson(string ProductCode)
        {
            Product Product = (from P in db.Product
                               where P.ProductCode == ProductCode
                               select P).FirstOrDefault();

            if (Product != null)
            {
                return Json(Product);
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetProductUidHelpList(string searchTerm, int pageSize, int pageNum, int filter)//SaleInvoiceHeaderId
        {
            List<ComboBoxResult> ProductUidJson = _StockLineService.FGetProductUidHelpList(filter, searchTerm).ToList();

            var count = ProductUidJson.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = ProductUidJson;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult SetSingleProductUid(string Ids)
        {
            ComboBoxResult ProductUidJson = new ComboBoxResult();

            var ProductUid = from L in db.ProductUid
                             where L.ProductUidName == Ids
                             select new
                             {
                                 id = L.ProductUidName,
                                 text = L.ProductUidName
                             };

            ProductUidJson.id = ProductUid.FirstOrDefault().id;
            ProductUidJson.text = ProductUid.FirstOrDefault().text;

            return Json(ProductUidJson);
        }

        public ActionResult GetStockInForProduct(string searchTerm, int pageSize, int pageNum, int filter, int? ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id)//DocTypeId
        {
            var Query = _StockLineService.GetPendingStockInForIssue(filter, ProductId, Dimension1Id, Dimension2Id, Dimension3Id, Dimension4Id, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

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

        public ActionResult GetStockInHeader(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _StockLineService.GetPendingStockInHeaderForIssue(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

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

        public JsonResult GetStockInDetailJson(int StockInId)
        {
            var temp = (from p in db.ViewStockInBalance
                        join S in db.Stock on p.StockInId equals S.StockId into StockTable
                        from StockTab in StockTable.DefaultIfEmpty()
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where p.StockInId == StockInId
                        select new
                        {
                            ProductUidId= StockTab.ProductUidId,
                            ProductUidName = StockTab.ProductUid.ProductUidName,
                            ProductId = p.ProductId,
                            ProductName = ProductTab.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = p.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Id = p.Dimension3Id,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Id = p.Dimension4Id,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            BalanceQty = p.BalanceQty,
                            LotNo = p.LotNo,
                            ProcessId = StockTab.ProcessId,
                            ProcessName = StockTab.Process.ProcessName
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp);
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetStockInBalance(int StockInId)
        {
            var temp = (from L in db.ViewStockInBalance where L.StockInId == StockInId select L).FirstOrDefault();
            if (temp != null)
            {
                return Json(temp.BalanceQty, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
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
