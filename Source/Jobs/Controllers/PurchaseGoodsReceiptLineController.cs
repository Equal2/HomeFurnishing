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
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using PurchaseGoodsReceiptDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseGoodsReceiptLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseGoodsReceiptLineService _PurchaseGoodsReceiptLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseGoodsReceiptLineController(IPurchaseGoodsReceiptLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseGoodsReceiptLineService = SaleOrder;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            PurchaseGoodsReceiptLineFilterViewModel vm = new PurchaseGoodsReceiptLineFilterViewModel();
            vm.PurchaseGoodsReceiptHeaderId = id;
            PurchaseGoodsReceiptHeader Header = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.SupplierId = sid;
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseGoodsReceiptLineFilterViewModel vm)
        {
            List<PurchaseGoodsReceiptLineViewModel> temp = _PurchaseGoodsReceiptLineService.GetPurchaseOrdersForFilters(vm).ToList();
            PurchaseGoodsReceiptHeader H = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(vm.PurchaseGoodsReceiptHeaderId);
            PurchaseGoodsReceiptMasterDetailModel svm = new PurchaseGoodsReceiptMasterDetailModel();
            svm.PurchaseGoodsReceiptLineViewModel = temp;
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            svm.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseGoodsReceiptMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _PurchaseGoodsReceiptLineService.GetMaxSr(vm.PurchaseGoodsReceiptLineViewModel.FirstOrDefault().PurchaseGoodsReceiptHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseGoodsReceiptDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReceiptLineViewModel.FirstOrDefault().PurchaseGoodsReceiptHeaderId), ref db);
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
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase).ProcessId;
                PurchaseGoodsReceiptHeader Header = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(vm.PurchaseGoodsReceiptLineViewModel.FirstOrDefault().PurchaseGoodsReceiptHeaderId);

                var PurchaseOrderLineIds = vm.PurchaseGoodsReceiptLineViewModel.Select(m => m.PurchaseOrderLineId).ToArray();

                var PurchaseOrderRecords = (from p in db.PurchaseOrderLine
                                            where PurchaseOrderLineIds.Contains(p.PurchaseOrderLineId)
                                            select p).ToList();

                foreach (var item in vm.PurchaseGoodsReceiptLineViewModel)
                {
                    if (item.Qty > 0)
                    {
                        PurchaseGoodsReceiptLine Line = new PurchaseGoodsReceiptLine();
                        var PurchaseOrderLine = PurchaseOrderRecords.Where(m => m.PurchaseOrderLineId == item.PurchaseOrderLineId).FirstOrDefault();


                        Line.PurchaseGoodsReceiptHeaderId = item.PurchaseGoodsReceiptHeaderId;
                        Line.PurchaseOrderLineId = item.PurchaseOrderLineId;
                        Line.ProductId = PurchaseOrderLine.ProductId;
                        Line.Dimension1Id = PurchaseOrderLine.Dimension1Id;
                        Line.Dimension2Id = PurchaseOrderLine.Dimension2Id;
                        Line.Specification = PurchaseOrderLine.Specification;
                        Line.Qty = item.Qty;
                        Line.DealQty = item.DealQty;
                        Line.Sr = Serial++;
                        Line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        Line.LotNo = item.LotNo;
                        Line.DealUnitId = item.DealUnitId;                        
                        Line.DocQty = item.DocQty;
                        Line.CreatedDate = DateTime.Now;
                        Line.ModifiedDate = DateTime.Now;
                        Line.CreatedBy = User.Identity.Name;
                        Line.ModifiedBy = User.Identity.Name;

                        if (Line.PurchaseOrderLineId.HasValue)
                            LineStatus.Add(Line.PurchaseOrderLineId.Value, Line.Qty);


                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
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
                        StockViewModel.DocHeaderId = Header.PurchaseGoodsReceiptHeaderId;
                        StockViewModel.DocLineId = Line.PurchaseGoodsReceiptLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.SupplierId;
                        StockViewModel.ProductId = Line.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.HeaderProcessId = ProcessId;
                        StockViewModel.GodownId = Header.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = Line.Qty;
                        StockViewModel.Rate = PurchaseOrderLine.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = Line.Specification;
                        StockViewModel.Dimension1Id = Line.Dimension1Id;
                        StockViewModel.Dimension2Id = Line.Dimension2Id;
                        StockViewModel.ProductUidId = Line.ProductUidId;
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


                        Line.StockId = StockViewModel.StockId;




                        if (Line.ProductUidId.HasValue)
                        {

                            ProductUid Uid = new ProductUidService(_unitOfWork).Find(Line.ProductUidId ?? 0);

                            Line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            Line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            Line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            Line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            Line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            Line.ProductUidStatus = Uid.Status;
                            Line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            Line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                            Uid.LastTransactionDocId = Header.PurchaseGoodsReceiptHeaderId;
                            Uid.LastTransactionDocDate = Header.DocDate;
                            Uid.LastTransactionDocNo = Header.DocNo;
                            Uid.LastTransactionDocTypeId = Header.DocTypeId;
                            Uid.LastTransactionPersonId = Header.SupplierId;
                            Uid.Status = ProductUidStatusConstants.Receive;
                            Uid.CurrenctProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase).ProcessId;
                            Uid.CurrenctGodownId = Header.GodownId;
                            Uid.ModifiedBy = User.Identity.Name;
                            Uid.ModifiedDate = DateTime.Now;
                            Uid.ObjectState = Model.ObjectState.Modified;
                            //new ProductUidService(_unitOfWork).Update(Uid);

                            db.ProductUid.Add(Uid);

                        }







                        Line.ObjectState = Model.ObjectState.Added;
                        //_PurchaseGoodsReceiptLineService.Create(Line);
                        db.PurchaseGoodsReceiptLine.Add(Line);

                        Cnt = Cnt + 1;
                    }
                }

                //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(Header);

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                Header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReceiptHeader.Add(Header);


                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyReceiveMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    PurchaseGoodsReceiptDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReceiptLineViewModel.FirstOrDefault().PurchaseGoodsReceiptHeaderId), ref db);
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
                    PurchaseGoodsReceiptDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReceiptLineViewModel.FirstOrDefault().PurchaseGoodsReceiptHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseGoodsReceiptHeaderId,
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
            var p = _PurchaseGoodsReceiptLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(PurchaseGoodsReceiptLineViewModel vm)
        {

            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
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

        public ActionResult _Create(int Id, int sid) //Id ==> Header Id
        {
            PurchaseGoodsReceiptHeader H = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(Id);
            PurchaseGoodsReceiptLineViewModel s = new PurchaseGoodsReceiptLineViewModel();

            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.SupplierId = sid;
            s.PurchaseGoodsReceiptHeaderId = Id;
            s.PurchaseGoodsReceiptHeaderDocNo = H.DocNo;
            ViewBag.LineMode = "Create";
            s.GodownId = H.GodownId;
            ViewBag.Status = H.Status;
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseGoodsReceiptLineViewModel svm)
        {
            PurchaseGoodsReceiptLine s = Mapper.Map<PurchaseGoodsReceiptLineViewModel, PurchaseGoodsReceiptLine>(svm);
            PurchaseGoodsReceiptHeader temp = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(s.PurchaseGoodsReceiptHeaderId);

            if (svm.PurchaseOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }
            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "Qty field is required");
            }
            if (svm.DealQty <= 0)
            {
                ModelState.AddModelError("DealQty", "DealQty field is required");
            }
            if (svm.PurchaseOrderLineId <= 0)
            {
                ModelState.AddModelError("PurchaseOrderLineId", "PurchaseOrder field is required");
            }

            bool BeforeSave = true;

            try
            {

                if (svm.PurchaseGoodsReceiptLineId <= 0)
                    BeforeSave = PurchaseGoodsReceiptDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseGoodsReceiptHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseGoodsReceiptDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseGoodsReceiptHeaderId, EventModeConstants.Edit), ref db);

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
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase).ProcessId;
                if (svm.PurchaseGoodsReceiptLineId <= 0)
                {
                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                    StockViewModel.DocHeaderId = temp.PurchaseGoodsReceiptHeaderId;
                    StockViewModel.DocLineId = s.PurchaseGoodsReceiptLineId;
                    StockViewModel.DocTypeId = temp.DocTypeId;
                    StockViewModel.StockHeaderDocDate = temp.DocDate;
                    StockViewModel.StockDocDate = temp.DocDate;
                    StockViewModel.DocNo = temp.DocNo;
                    StockViewModel.DivisionId = temp.DivisionId;
                    StockViewModel.SiteId = temp.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = temp.SupplierId;
                    StockViewModel.ProductId = s.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = temp.GodownId;
                    StockViewModel.GodownId = temp.GodownId;
                    StockViewModel.ProcessId = ProcessId;
                    StockViewModel.LotNo = null;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = 0;
                    StockViewModel.Qty_Rec = s.Qty;
                    StockViewModel.Rate = null;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = s.Specification;
                    StockViewModel.Dimension1Id = s.Dimension1Id;
                    StockViewModel.Dimension2Id = s.Dimension2Id;
                    StockViewModel.ProductUidId = s.ProductUidId;
                    StockViewModel.Remark = s.Remark;
                    StockViewModel.Status = temp.Status;
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
                    s.CreatedDate = DateTime.Now;
                    s.Sr = _PurchaseGoodsReceiptLineService.GetMaxSr(s.PurchaseGoodsReceiptHeaderId);
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;


                    if (s.PurchaseOrderLineId.HasValue)
                        new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnReceive(s.PurchaseOrderLineId.Value, s.PurchaseGoodsReceiptLineId, temp.DocDate, s.Qty, ref db, true);

                    if (temp.StockHeaderId == null)
                    {
                        temp.StockHeaderId = StockViewModel.StockHeaderId;
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(temp);
                    }


                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptHeader.Add(temp);

                    if (svm.ProductUidId != null && svm.ProductUidId > 0)
                    {
                        ProductUid Produid = db.ProductUid.Find(svm.ProductUidId);

                        s.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                        s.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                        s.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                        s.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                        s.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                        s.ProductUidStatus = Produid.Status;
                        s.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                        s.ProductUidCurrentGodownId = Produid.CurrenctGodownId;


                        Produid.LastTransactionDocId = temp.PurchaseGoodsReceiptHeaderId;
                        Produid.LastTransactionDocNo = temp.DocNo;
                        Produid.LastTransactionDocTypeId = temp.DocTypeId;
                        Produid.LastTransactionDocDate = temp.DocDate;
                        Produid.LastTransactionPersonId = temp.SupplierId;
                        Produid.CurrenctGodownId = temp.GodownId;
                        Produid.Status = ProductUidStatusConstants.Receive;
                        Produid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(Produid);
                        //new ProductUidService(_unitOfWork).Update(Produid);
                    }

                    //_PurchaseGoodsReceiptLineService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReceiptLine.Add(s);

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseGoodsReceiptHeaderId, s.PurchaseGoodsReceiptLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                        EventException = true;
                    }

                    //-TO-DO-
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
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseGoodsReceiptHeaderId, s.PurchaseGoodsReceiptLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseGoodsReceiptHeaderId,
                        DocLineId = s.PurchaseGoodsReceiptLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.PurchaseGoodsReceiptHeaderId, sid = svm.SupplierId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseGoodsReceiptHeader header = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(svm.PurchaseGoodsReceiptHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    PurchaseGoodsReceiptLine temp1 = db.PurchaseGoodsReceiptLine.Find(svm.PurchaseGoodsReceiptLineId);

                    PurchaseGoodsReceiptLine ExRec = new PurchaseGoodsReceiptLine();
                    ExRec = Mapper.Map<PurchaseGoodsReceiptLine>(temp1);

                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = temp.StockHeaderId ?? 0;
                    StockViewModel.StockId = temp1.StockId ?? 0;
                    StockViewModel.DocHeaderId = temp.PurchaseGoodsReceiptHeaderId;
                    StockViewModel.DocLineId = s.PurchaseGoodsReceiptLineId;
                    StockViewModel.DocTypeId = temp.DocTypeId;
                    StockViewModel.StockHeaderDocDate = temp.DocDate;
                    StockViewModel.StockDocDate = temp.DocDate;
                    StockViewModel.DocNo = temp.DocNo;
                    StockViewModel.DivisionId = temp.DivisionId;
                    StockViewModel.SiteId = temp.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = null;
                    StockViewModel.PersonId = temp.SupplierId;
                    StockViewModel.ProductId = s.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = null;
                    StockViewModel.GodownId = temp.GodownId;
                    StockViewModel.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase).ProcessId;
                    StockViewModel.LotNo = null;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = 0;
                    StockViewModel.Qty_Rec = s.Qty;
                    StockViewModel.Rate = null;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = null;
                    StockViewModel.Dimension1Id = null;
                    StockViewModel.Dimension2Id = null;
                    StockViewModel.ProductUidId = s.ProductUidId;
                    StockViewModel.Remark = s.Remark;
                    StockViewModel.Status = temp.Status;
                    StockViewModel.CreatedBy = temp1.CreatedBy;
                    StockViewModel.CreatedDate = temp1.CreatedDate;
                    StockViewModel.ModifiedBy = User.Identity.Name;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }



                    temp1.ProductUidId = svm.ProductUidId;
                    temp1.ProductId = svm.ProductId;
                    temp1.Specification = svm.Specification;
                    temp1.PurchaseOrderLineId = svm.PurchaseOrderLineId;
                    temp1.Dimension1Id = svm.Dimension1Id;
                    temp1.Dimension2Id = svm.Dimension2Id;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.LotNo = svm.LotNo;
                    temp1.DebitNoteAmount = svm.DebitNoteAmount;
                    temp1.DebitNoteReason = svm.DebitNoteReason;
                    temp1.Qty = svm.Qty;
                    temp1.DocQty = svm.DocQty;
                    temp1.DealQty = svm.DealQty;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.Remark = svm.Remark;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    //_PurchaseGoodsReceiptLineService.Update(temp1);
                    db.PurchaseGoodsReceiptLine.Add(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    if (temp1.PurchaseOrderLineId.HasValue)
                        new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnReceive(temp1.PurchaseOrderLineId.Value, temp1.PurchaseGoodsReceiptLineId, temp.DocDate, temp1.Qty, ref db, true);


                    if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        header.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseGoodsReceiptHeader.Add(header);
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(header);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseGoodsReceiptHeaderId, s.PurchaseGoodsReceiptLineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseGoodsReceiptHeaderId, s.PurchaseGoodsReceiptLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp1.PurchaseGoodsReceiptHeaderId,
                        DocLineId = temp1.PurchaseGoodsReceiptLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

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


        [HttpGet]
        private ActionResult _Modify(int id)
        {
            PurchaseGoodsReceiptLineViewModel temp = _PurchaseGoodsReceiptLineService.GetPurchaseGoodsReceiptLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PurchaseGoodsReceiptHeader H = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReceiptHeaderId);
            PrepareViewBag(temp);

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

            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
           
            return PartialView("_Create", temp);
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            PurchaseGoodsReceiptLineViewModel temp = _PurchaseGoodsReceiptLineService.GetPurchaseGoodsReceiptLine(id);
            PurchaseGoodsReceiptHeader H = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReceiptHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
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
            PurchaseGoodsReceiptLineViewModel temp = _PurchaseGoodsReceiptLineService.GetPurchaseGoodsReceiptLine(id);
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

            PurchaseGoodsReceiptHeader H = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReceiptHeaderId);
            PrepareViewBag(temp);
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);


            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseGoodsReceiptLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseGoodsReceiptDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReceiptHeaderId, vm.PurchaseGoodsReceiptLineId), ref db);
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
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine = db.PurchaseGoodsReceiptLine.Find(vm.PurchaseGoodsReceiptLineId);
                PurchaseGoodsReceiptHeader header = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(PurchaseGoodsReceiptLine.PurchaseGoodsReceiptHeaderId);
                StockId = PurchaseGoodsReceiptLine.StockId;

                try
                {
                    PurchaseGoodsReceiptDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseGoodsReceiptLine.PurchaseGoodsReceiptHeaderId, PurchaseGoodsReceiptLine.PurchaseGoodsReceiptLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseGoodsReceiptLine>(PurchaseGoodsReceiptLine),
                });

                if (PurchaseGoodsReceiptLine.PurchaseOrderLineId.HasValue)
                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnReceive(PurchaseGoodsReceiptLine.PurchaseOrderLineId.Value, PurchaseGoodsReceiptLine.PurchaseGoodsReceiptLineId, header.DocDate, 0, ref db, true);


                if (vm.ProductUidId != null && vm.ProductUidId != 0)
                {
                    ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)vm.ProductUidId);

                    if (header.DocNo != ProductUid.LastTransactionDocNo || header.DocTypeId != ProductUid.LastTransactionDocTypeId)
                    {
                        ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                        PrepareViewBag(vm);
                        return PartialView("_Create", vm);
                    }

                    ProductUid.LastTransactionDocDate = PurchaseGoodsReceiptLine.ProductUidLastTransactionDocDate;
                    ProductUid.LastTransactionDocId = PurchaseGoodsReceiptLine.ProductUidLastTransactionDocId;
                    ProductUid.LastTransactionDocNo = PurchaseGoodsReceiptLine.ProductUidLastTransactionDocNo;
                    ProductUid.LastTransactionDocTypeId = PurchaseGoodsReceiptLine.ProductUidLastTransactionDocTypeId;
                    ProductUid.LastTransactionPersonId = PurchaseGoodsReceiptLine.ProductUidLastTransactionPersonId;
                    ProductUid.CurrenctGodownId = PurchaseGoodsReceiptLine.ProductUidCurrentGodownId;
                    ProductUid.CurrenctProcessId = PurchaseGoodsReceiptLine.ProductUidCurrentProcessId;
                    ProductUid.Status = PurchaseGoodsReceiptLine.ProductUidStatus;


                    //new ProductUidService(_unitOfWork).Update(ProductUid);
                    ProductUid.ObjectState = Model.ObjectState.Modified;
                    db.ProductUid.Add(ProductUid);

                    new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(vm.PurchaseGoodsReceiptLineId, header.DocTypeId, header.SiteId, header.DivisionId, ref db);
                }


                //_PurchaseGoodsReceiptLineService.Delete(PurchaseGoodsReceiptLine);

                PurchaseGoodsReceiptLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReceiptLine.Remove(PurchaseGoodsReceiptLine);


                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(header);
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptHeader.Add(header);
                }

                if (StockId != null)
                {
                    new StockService(_unitOfWork).DeleteStockDB((int)StockId, ref db, true);
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
                    PrepareViewBag(null);
                    ViewBag.LineMode = "Delete";
                    return PartialView("_Create", vm);
                }

                try
                {
                    PurchaseGoodsReceiptDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseGoodsReceiptLine.PurchaseGoodsReceiptHeaderId, PurchaseGoodsReceiptLine.PurchaseGoodsReceiptLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseGoodsReceiptHeaderId,
                    DocLineId = PurchaseGoodsReceiptLine.PurchaseGoodsReceiptLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
        }

        public JsonResult GetPendingOrders(int ProductId, int PurchaseGoodsReceiptHeaderId)
        {
            return Json(new PurchaseOrderHeaderService(_unitOfWork).GetPendingPurchaseOrders(ProductId, PurchaseGoodsReceiptHeaderId).ToList());
        }

        public JsonResult GetOrderDetail(int OrderId)
        {
            return Json(new PurchaseOrderLineService(_unitOfWork).GetLineDetail(OrderId));
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

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string deliveryunitid)
        {
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, deliveryunitid);

            Unit Unit = new UnitService(_unitOfWork).Find(deliveryunitid);

            List<SelectListItem> unitconversionjson = new List<SelectListItem>();
            if (uc != null)
            {
                unitconversionjson.Add(new SelectListItem
                {
                    Text = uc.Multiplier.ToString(),
                    Value = uc.Multiplier.ToString()
                });
            }
            else
            {
                unitconversionjson.Add(new SelectListItem
                {
                    Text = "0",
                    Value = "0"
                });
            }

            unitconversionjson.Add(new SelectListItem
            {
                Text = Unit.DecimalPlaces.ToString(),
                Value = Unit.DecimalPlaces.ToString()
            });

            return Json(unitconversionjson);
        }

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            ProductJson.Add(new Product()
            {
                ProductId = product.ProductId,
                StandardCost = product.StandardCost,
                UnitId = product.UnitId
            });

            return Json(ProductJson);
        }

        public JsonResult GetPurchaseOrders(int id, string term)//Receipt Header ID
        {
            return Json(_PurchaseGoodsReceiptLineService.GetPendingPurchaseOrderHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProducts(string term)//SupplierID
        {

            return Json(_PurchaseGoodsReceiptLineService.GetProductsHelpList(term), JsonRequestBehavior.AllowGet);
        }


    }


}
