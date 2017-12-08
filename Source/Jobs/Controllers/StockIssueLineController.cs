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
    public class StockIssueLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IStockLineService _StockLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public StockIssueLineController(IStockLineService Stock, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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



        public ActionResult _ForRequisition(int id, int sid)
        {
            RequisitionFiltersForIssue vm = new RequisitionFiltersForIssue();
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.StockHeaderId = id;
            vm.PersonId = sid;
            return PartialView("_Filters", vm);
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

                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = Header.StockHeaderId;
                        }
                        else
                        {
                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = Header.StockHeaderId;
                        StockViewModel.DocLineId = line.StockLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.PersonId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.HeaderProcessId = Header.ProcessId;
                        StockViewModel.GodownId = (int)Header.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = item.ProcessId;
                        StockViewModel.LotNo = item.LotNo;
                        StockViewModel.CostCenterId = (item.CostCenterId == null ? Header.CostCenterId : item.CostCenterId);
                        StockViewModel.Qty_Iss = item.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = item.Specification;
                        StockViewModel.Dimension1Id = item.Dimension1Id;
                        StockViewModel.Dimension2Id = item.Dimension2Id;
                        StockViewModel.Dimension3Id = item.Dimension3Id;
                        StockViewModel.Dimension4Id = item.Dimension4Id;
                        StockViewModel.ProductUidId = item.ProductUidId;
                        StockViewModel.CreatedBy = User.Identity.Name;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        if (Cnt == 0)
                        {
                            Header.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                        line.StockId = StockViewModel.StockId;




                        if (Settings.isPostedInStockProcess ?? false)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessViewModel.StockHeaderId = 0;
                            }
                            StockProcessViewModel.StockProcessId = -Cnt;
                            StockProcessViewModel.DocHeaderId = Header.StockHeaderId;
                            StockProcessViewModel.DocLineId = line.StockLineId;
                            StockProcessViewModel.DocTypeId = Header.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                            StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                            StockProcessViewModel.DocNo = Header.DocNo;
                            StockProcessViewModel.DivisionId = Header.DivisionId;
                            StockProcessViewModel.SiteId = Header.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = Header.PersonId;
                            StockProcessViewModel.ProductId = item.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = Header.GodownId;
                            StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                            StockProcessViewModel.GodownId = (int)Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = item.LotNo;
                            StockProcessViewModel.CostCenterId = (item.CostCenterId == null ? Header.CostCenterId : item.CostCenterId);
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = item.Qty;
                            StockProcessViewModel.Rate = item.Rate;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = item.Specification;
                            StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = item.Dimension4Id;
                            StockProcessViewModel.ProductUidId = item.ProductUidId;
                            StockProcessViewModel.CreatedBy = User.Identity.Name;
                            StockProcessViewModel.CreatedDate = DateTime.Now;
                            StockProcessViewModel.ModifiedBy = User.Identity.Name;
                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                string message = StockProcessPostingError;
                                ModelState.AddModelError("", message);
                                return PartialView("_Results", vm);
                            }

                            line.StockProcessId = StockProcessViewModel.StockProcessId;
                        }

                        if (item.StockInId != null)
                        {
                            StockAdj Adj_IssQty = new StockAdj();
                            Adj_IssQty.StockAdjId = -Cnt;
                            Adj_IssQty.StockInId = (int)item.StockInId;
                            Adj_IssQty.StockOutId = (int)line.StockId;
                            Adj_IssQty.DivisionId = Header.DivisionId;
                            Adj_IssQty.SiteId = Header.SiteId;
                            Adj_IssQty.AdjustedQty = item.Qty;
                            Adj_IssQty.ObjectState = Model.ObjectState.Added;
                            db.StockAdj.Add(Adj_IssQty);
                        }


                        line.StockHeaderId = item.StockHeaderId;
                        line.RequisitionLineId = item.RequisitionLineId;
                        line.StockInId = item.StockInId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.LotNo = item.LotNo;
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
            StockHeader H = db.StockHeader.Find(Id);
            StockLineViewModel s = new StockLineViewModel();            

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.PersonId = H.PersonId;
            if (H.PersonId.HasValue && H.PersonId.Value > 0)
            {
                db.Entry<StockHeader>(H).Reference(m => m.Person).Load();
                s.PersonName = H.Person.Name;
            }
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

            var LastTrRec = (from p in db.StockLine
                             where p.StockHeaderId == Id
                             orderby p.StockLineId descending
                             select new
                             {
                                 ProductName = p.Product.ProductName,
                                 Qty = p.Qty,
                             }).FirstOrDefault();

            if (LastTrRec != null)
                ViewBag.StockLastTransaction = "Last Line -Product : " + LastTrRec.ProductName + ", " + "Qty : " + LastTrRec.Qty;

            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(StockLineViewModel svm)
        {
            StockHeader temp = new StockHeaderService(_unitOfWork).Find(svm.StockHeaderId);
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(temp.DocTypeId, temp.DivisionId, temp.SiteId);
            StockLine s = Mapper.Map<StockLineViewModel, StockLine>(svm);

            if (settings != null)
            {
                if (settings.isVisibleProcessLine == true && settings.isMandatoryProcessLine == true && (svm.FromProcessId <= 0 || svm.FromProcessId == null))
                {
                    ModelState.AddModelError("FromProcessId", "The Process field is required");
                }
                if (settings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
                if (settings.isMandatoryLineCostCenter == true && !svm.CostCenterId.HasValue)
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

            if (svm.Qty <= 0)
                ModelState.AddModelError("Qty", "The Qty field is required");

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
                    StockViewModel StockViewModel = new StockViewModel();
                    StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                    //Posting in Stock
                    StockViewModel.StockHeaderId = temp.StockHeaderId;
                    StockViewModel.DocHeaderId = temp.StockHeaderId;
                    StockViewModel.DocLineId = s.StockLineId;
                    StockViewModel.DocTypeId = temp.DocTypeId;
                    StockViewModel.StockHeaderDocDate = temp.DocDate;
                    StockViewModel.StockDocDate = temp.DocDate;
                    StockViewModel.DocNo = temp.DocNo;
                    StockViewModel.DivisionId = temp.DivisionId;
                    StockViewModel.SiteId = temp.SiteId;
                    StockViewModel.CurrencyId = null;

                    StockViewModel.PersonId = temp.PersonId;
                    StockViewModel.ProductId = s.ProductId;
                    StockViewModel.HeaderFromGodownId = temp.FromGodownId;
                    StockViewModel.HeaderGodownId = temp.GodownId;
                    StockViewModel.GodownId = temp.GodownId ?? 0;

                    StockViewModel.LotNo = s.LotNo;
                    StockViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);


                    StockViewModel.HeaderProcessId = temp.ProcessId;
                    StockViewModel.ProcessId = s.FromProcessId;
                    StockViewModel.Qty_Iss = s.Qty;
                    StockViewModel.Qty_Rec = 0;

                    StockViewModel.Weight_Iss = s.Weight;
                    StockViewModel.Weight_Rec = 0;

                    StockViewModel.Rate = s.Rate;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = s.Specification;
                    StockViewModel.Dimension1Id = s.Dimension1Id;
                    StockViewModel.Dimension2Id = s.Dimension2Id;
                    StockViewModel.Dimension3Id = s.Dimension3Id;
                    StockViewModel.Dimension4Id = s.Dimension4Id;
                    StockViewModel.Remark = s.Remark;
                    StockViewModel.Status = temp.Status;
                    StockViewModel.ProductUidId = svm.ProductUidId;
                    StockViewModel.CreatedBy = temp.CreatedBy;
                    StockViewModel.CreatedDate = DateTime.Now;
                    StockViewModel.ModifiedBy = temp.ModifiedBy;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    s.StockId = StockViewModel.StockId;



                    if (settings.isPostedInStockProcess.HasValue && settings.isPostedInStockProcess == true)
                    {
                        StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

                        StockProcessViewModel.StockHeaderId = StockHeader.StockHeaderId;
                        StockProcessViewModel.StockHeaderId = temp.StockHeaderId;
                        StockProcessViewModel.DocHeaderId = temp.StockHeaderId;
                        StockProcessViewModel.DocLineId = s.StockLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.PersonId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = temp.FromGodownId;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = s.LotNo;
                        StockProcessViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);


                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = s.Qty;

                        StockProcessViewModel.Weight_Iss = 0;
                        StockProcessViewModel.Weight_Rec = s.Weight;

                        StockProcessViewModel.Rate = s.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = s.Specification;
                        StockProcessViewModel.Dimension1Id = s.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = s.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = s.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = s.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.CreatedBy = temp.CreatedBy;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }

                        s.StockProcessId = StockProcessViewModel.StockProcessId;
                    }

                    if (svm.StockInId != null)
                    {
                        StockAdj Adj_IssQty = new StockAdj();
                        Adj_IssQty.StockInId = (int)svm.StockInId;
                        Adj_IssQty.StockOutId = (int)s.StockId;
                        Adj_IssQty.DivisionId = temp.DivisionId;
                        Adj_IssQty.SiteId = temp.SiteId;
                        Adj_IssQty.AdjustedQty = s.Qty ;
                        Adj_IssQty.ObjectState = Model.ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                        //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.DocNature = StockNatureConstants.Issue;
                    s.ModifiedBy = User.Identity.Name;
                    s.ProductUidId = svm.ProductUidId;
                    s.StockInId = svm.StockInId;
                    s.Sr = _StockLineService.GetMaxSr(s.StockHeaderId);


                    if (s.RequisitionLineId.HasValue)
                        new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnIssue(s.RequisitionLineId.Value, s.StockLineId, temp.DocDate, s.Qty, ref db, true);

                    //StockHeader header = new StockHeaderService(_unitOfWork).Find(s.StockHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        db.StockHeader.Add(temp);
                        //new StockHeaderService(_unitOfWork).Update(temp);
                    }



                    if (svm.ProductUidId.HasValue && svm.ProductUidId > 0)
                    {

                        ProductUid Produid = new ProductUidService(_unitOfWork).Find(svm.ProductUidId ?? 0);

                        s.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                        s.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                        s.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                        s.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                        s.ProductUidCurrentGodownId = Produid.CurrenctGodownId;
                        s.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                        s.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                        s.ProductUidStatus = Produid.Status;

                        Produid.LastTransactionDocId = temp.StockHeaderId;
                        Produid.LastTransactionDocNo = temp.DocNo;
                        Produid.LastTransactionDocTypeId = temp.DocTypeId;
                        Produid.LastTransactionDocDate = temp.DocDate;
                        Produid.LastTransactionPersonId = temp.PersonId;
                        Produid.CurrenctGodownId = null;
                        Produid.Status = (!string.IsNullOrEmpty(settings.BarcodeStatusUpdate) ? settings.BarcodeStatusUpdate : ProductUidStatusConstants.Issue);

                        Produid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(Produid);

                    }


                    s.ObjectState = Model.ObjectState.Added;
                    db.StockLine.Add(s);


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



                    if (templine.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = temp.StockHeaderId;
                        StockViewModel.StockId = templine.StockId ?? 0;
                        StockViewModel.DocHeaderId = templine.StockHeaderId;
                        StockViewModel.DocLineId = templine.StockLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;

                        StockViewModel.PersonId = temp.PersonId;
                        StockViewModel.ProductId = s.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = temp.GodownId;
                        StockViewModel.GodownId = temp.GodownId ?? 0;

                        StockViewModel.LotNo = s.LotNo;
                        StockViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);


                        StockViewModel.ProcessId = s.FromProcessId;
                        StockViewModel.HeaderProcessId = temp.ProcessId;
                        StockViewModel.Qty_Iss = s.Qty;
                        StockViewModel.Qty_Rec = 0;

                        StockViewModel.Weight_Iss = s.Weight;
                        StockViewModel.Weight_Rec = 0;

                        StockViewModel.Rate = s.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = s.Specification;
                        StockViewModel.Dimension1Id = s.Dimension1Id;
                        StockViewModel.Dimension2Id = s.Dimension2Id;
                        StockViewModel.Dimension3Id = s.Dimension3Id;
                        StockViewModel.Dimension4Id = s.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.ProductUidId = svm.ProductUidId;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.CreatedBy = templine.CreatedBy;
                        StockViewModel.CreatedDate = templine.CreatedDate;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }




                    if (templine.StockProcessId != null)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                        StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

                        StockProcessViewModel.StockHeaderId = StockHeader.StockHeaderId;
                        StockProcessViewModel.StockProcessId = templine.StockProcessId ?? 0;
                        StockProcessViewModel.DocHeaderId = templine.StockHeaderId;
                        StockProcessViewModel.DocLineId = templine.StockLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.PersonId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = s.LotNo;
                        StockProcessViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);

                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = s.Qty;

                        StockProcessViewModel.Weight_Iss = 0;
                        StockProcessViewModel.Weight_Rec = s.Weight;

                        StockProcessViewModel.Rate = s.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = s.Specification;
                        StockProcessViewModel.Dimension1Id = s.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = s.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = s.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = s.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.CreatedBy = templine.CreatedBy;
                        StockProcessViewModel.CreatedDate = templine.CreatedDate;
                        StockProcessViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }
                    }


                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == templine.StockId
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
                        Adj_IssQty.StockOutId = (int)templine.StockId;
                        Adj_IssQty.DivisionId = temp.DivisionId;
                        Adj_IssQty.SiteId = temp.SiteId;
                        Adj_IssQty.AdjustedQty = svm.Qty;
                        Adj_IssQty.ObjectState = Model.ObjectState.Added;
                        db.StockAdj.Add(Adj_IssQty);
                        //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                    }



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
                    templine.Weight = s.Weight;
                    templine.StockInId = s.StockInId;
                    templine.Remark = s.Remark;
                    templine.ReferenceDocId = s.ReferenceDocId;
                    templine.ReferenceDocTypeId = s.ReferenceDocTypeId;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    //_StockLineService.Update(templine);
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.StockLine.Add(templine);

                    if (templine.RequisitionLineId.HasValue)
                        new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnIssue(templine.RequisitionLineId.Value, templine.StockLineId, temp.DocDate, templine.Qty, ref db, true);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;

                        temp.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(temp);
                    }

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

            temp.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

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

        public ActionResult _Detail(int id)
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

                int? StockId = 0;
                int? StockProcessId = 0;
                int? ProdUid = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                StockLine StockLine = (from p in db.StockLine
                                       where p.StockLineId == vm.StockLineId
                                       select p).FirstOrDefault();

                StockHeader header = new StockHeaderService(_unitOfWork).Find(StockLine.StockHeaderId);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<StockLine>(StockLine),
                });

                StockId = StockLine.StockId;
                StockProcessId = StockLine.StockProcessId;
                ProdUid = StockLine.ProductUidId;

                if (StockLine.RequisitionLineId.HasValue)
                    new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnIssue(StockLine.RequisitionLineId.Value, StockLine.StockLineId, header.DocDate, 0, ref db, true);


                if (ProdUid != null && ProdUid > 0)
                {
                    ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)ProdUid);

                    ProductUid.LastTransactionDocDate = StockLine.ProductUidLastTransactionDocDate;
                    ProductUid.LastTransactionDocId = StockLine.ProductUidLastTransactionDocId;
                    ProductUid.LastTransactionDocNo = StockLine.ProductUidLastTransactionDocNo;
                    ProductUid.LastTransactionDocTypeId = StockLine.ProductUidLastTransactionDocTypeId;
                    ProductUid.LastTransactionPersonId = StockLine.ProductUidLastTransactionPersonId;
                    ProductUid.CurrenctGodownId = StockLine.ProductUidCurrentGodownId;
                    ProductUid.CurrenctProcessId = StockLine.ProductUidCurrentProcessId;
                    ProductUid.Status = StockLine.ProductUidStatus;
                    ProductUid.ObjectState = Model.ObjectState.Modified;
                    db.ProductUid.Add(ProductUid);

                }

                StockLine.ObjectState = Model.ObjectState.Deleted;
                db.StockLine.Remove(StockLine);

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

                if (StockProcessId != null)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB((int)StockProcessId, ref db, true);
                }


                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(header);
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    StockIssueDocEvents.onLineDeleteEvent(this, new StockEventArgs(StockLine.StockHeaderId, StockLine.StockLineId), ref db);
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
                    //_unitOfWork.Save();
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

        public JsonResult GetProductsForAutoComplete(int id, int PersonId, string term, int Limit)//Indent Header ID
        {
            return Json(_StockLineService.GetProductHelpList(id, PersonId, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRequisitions(int id, int PersonId, string term)//Receipt Header ID
        {
            return Json(_StockLineService.GetPendingRequisitionHelpList(id, PersonId, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductsForFilter(int id, int PersonId, string term, int Limit)//SupplierID
        {
            return Json(_StockLineService.GetProductHelpListForFilters(id, PersonId, term, Limit), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetProductsHelpList(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var Query = _StockLineService.GetProductHelpList(filter, searchTerm);

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

        public JsonResult GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int MaterialIssueId, string ProcName)
        {
            return Json(_StockLineService.GetExcessStock(ProductId, Dim1, Dim2, ProcId, Lot, MaterialIssueId, ProcName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var Query = _StockLineService.GetCostCentersForIssueFilters(filter, searchTerm);

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

        public JsonResult GetLineCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var Query = _StockLineService.GetCostCentersForLine(filter, searchTerm);
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

        public ActionResult GetLotNo(string searchTerm, int pageSize, int pageNum, int filter)
        {
            var Query = _StockLineService.GetLotNo(filter, searchTerm);
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
                        join S in db.Stock on p.StockInId equals S.StockId into StockTable from StockTab in StockTable.DefaultIfEmpty()
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

        public ActionResult GetCustomProductGroups(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _StockLineService.GetCustomProductGroups(filter, searchTerm);
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

        public ActionResult GetCustomProductForStockIn(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _StockLineService.GetCustomProductGroups(filter, searchTerm);
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

        public ActionResult GetCustomReferenceDocIds(string searchTerm, int pageSize, int pageNum, int filter)//StockHeaderId
        {
            var Query = _StockLineService.GetCustomReferenceDocIds(filter, searchTerm);
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


        public JsonResult SetSingleReferenceDocId(int StockLineId)
        {
            ComboBoxResult ReferenceDocIdJson = new ComboBoxResult();

            var ReferenceDocs = _StockLineService.SetCustomReferenceDocIds(StockLineId);

            ReferenceDocIdJson.id = ReferenceDocs.id;
            ReferenceDocIdJson.text = ReferenceDocs.text;

            return Json(ReferenceDocIdJson);
        }

        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
                CookieGenerator.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);

            TempData.Remove("CSEXC");

            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
