using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using System.Web.UI.WebControls;
using Model.ViewModels;
using AutoMapper;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using PurchaseGoodsReturnDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseGoodsReturnLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseGoodsReturnLineService _PurchaseGoodsReturnLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseGoodsReturnLineController(IPurchaseGoodsReturnLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseGoodsReturnLineService = SaleOrder;
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
            var p = _PurchaseGoodsReturnLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForReceipt(int id, int sid)
        {
            PurchaseGoodsReturnLineFilterViewModel vm = new PurchaseGoodsReturnLineFilterViewModel();
            vm.PurchaseGoodsReturnHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForOrders(int id, int sid)
        {
            PurchaseGoodsReturnLineFilterViewModel vm = new PurchaseGoodsReturnLineFilterViewModel();
            vm.PurchaseGoodsReturnHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_OrderFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseGoodsReturnLineFilterViewModel vm)
        {
            List<PurchaseGoodsReturnLineViewModel> temp = _PurchaseGoodsReturnLineService.GetPurchaseGoodsReceiptsForFilters(vm).ToList();
            PurchaseGoodsReturnMasterDetailModel svm = new PurchaseGoodsReturnMasterDetailModel();
            svm.PurchaseGoodsReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(PurchaseGoodsReturnLineFilterViewModel vm)
        {
            List<PurchaseGoodsReturnLineViewModel> temp = _PurchaseGoodsReturnLineService.GetPurchaseOrderForFilters(vm).ToList();
            PurchaseGoodsReturnMasterDetailModel svm = new PurchaseGoodsReturnMasterDetailModel();
            svm.PurchaseGoodsReturnLineViewModel = temp;
            return PartialView("_OrderResults", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseGoodsReturnMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _PurchaseGoodsReturnLineService.GetMaxSr(vm.PurchaseGoodsReturnLineViewModel.FirstOrDefault().PurchaseGoodsReturnHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseGoodsReturnDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReturnLineViewModel.FirstOrDefault().PurchaseGoodsReturnHeaderId), ref db);
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
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;
                PurchaseGoodsReturnHeader Header = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(vm.PurchaseGoodsReturnLineViewModel.FirstOrDefault().PurchaseGoodsReturnHeaderId);

                foreach (var item in vm.PurchaseGoodsReturnLineViewModel)
                {
                    decimal balqty = (from p in db.ViewPurchaseGoodsReceiptBalance
                                      where p.PurchaseGoodsReceiptLineId == item.PurchaseGoodsReceiptLineId
                                      select p.BalanceQty).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        PurchaseGoodsReturnLine Line = new PurchaseGoodsReturnLine();
                        Line.PurchaseGoodsReturnHeaderId = item.PurchaseGoodsReturnHeaderId;
                        Line.PurchaseGoodsReceiptLineId = item.PurchaseGoodsReceiptLineId;                       
                        Line.Qty = item.Qty;
                        Line.Sr = Serial++;
                        Line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        Line.DealUnitId = item.DealUnitId;
                        Line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        Line.Remark = item.Remark;
                        Line.CreatedDate = DateTime.Now;
                        Line.ModifiedDate = DateTime.Now;
                        Line.CreatedBy = User.Identity.Name;
                        Line.ModifiedBy = User.Identity.Name;


                        LineStatus.Add(Line.PurchaseGoodsReceiptLineId, Line.Qty);


                        PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(Line.PurchaseGoodsReceiptLineId);
                        //var receipt = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(item.PurchaseGoodsReceiptLineId );




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
                        StockViewModel.DocHeaderId = Header.PurchaseGoodsReturnHeaderId;
                        StockViewModel.DocLineId = Line.PurchaseGoodsReceiptLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.SupplierId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.HeaderProcessId = ProcessId;
                        StockViewModel.GodownId = Header.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = Line.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = item.Specification;
                        StockViewModel.Dimension1Id = item.Dimension1Id;
                        StockViewModel.Dimension2Id = item.Dimension2Id;
                        StockViewModel.ProductUidId = PurchaseGoodsReceiptLine.ProductUidId;
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


                        if (PurchaseGoodsReceiptLine.ProductUidId.HasValue && PurchaseGoodsReceiptLine.ProductUidId > 0)
                        {
                            ProductUid Uid = new ProductUidService(_unitOfWork).Find(PurchaseGoodsReceiptLine.ProductUidId.Value);



                            Line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                            Line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                            Line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                            Line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                            Line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                            Line.ProductUidStatus = Uid.Status;
                            Line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                            Line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                            Uid.LastTransactionDocId = Header.PurchaseGoodsReturnHeaderId;
                            Uid.LastTransactionDocDate = Header.DocDate;
                            Uid.LastTransactionDocNo = Header.DocNo;
                            Uid.LastTransactionDocTypeId = Header.DocTypeId;
                            Uid.LastTransactionPersonId = Header.SupplierId;
                            Uid.Status = ProductUidStatusConstants.Issue;
                            //Uid.CurrenctProcessId = temp.ProcessId;
                            var Site = new SiteService(_unitOfWork).FindByPerson(Header.SupplierId);
                            if (Site != null)
                                Uid.CurrenctGodownId = Site.DefaultGodownId;
                            else
                                Uid.CurrenctGodownId = null;

                            Uid.ModifiedBy = User.Identity.Name;
                            Uid.ModifiedDate = DateTime.Now;
                            Uid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(Uid);
                            //new ProductUidService(_unitOfWork).Update(Uid);

                        }





                        Line.ObjectState = Model.ObjectState.Added;
                        db.PurchaseGoodsReturnLine.Add(Line);
                        //_PurchaseGoodsReturnLineService.Create(Line);

                        Cnt = Cnt + 1;

                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;                    
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReturnHeader.Add(Header);

                //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(Header);

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyReturnMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    PurchaseGoodsReturnDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReturnLineViewModel.FirstOrDefault().PurchaseGoodsReturnHeaderId), ref db);
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
                    PurchaseGoodsReturnDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReturnLineViewModel.FirstOrDefault().PurchaseGoodsReturnHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseGoodsReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(PurchaseGoodsReturnLineViewModel vm)
        {
            //ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
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
            PurchaseGoodsReturnHeader H = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Id);
            PurchaseGoodsReturnLineViewModel s = new PurchaseGoodsReturnLineViewModel();

            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            s.PurchaseGoodsReturnHeaderId = H.PurchaseGoodsReturnHeaderId;
            s.PurchaseGoodsReturnHeaderDocNo = H.DocNo;
            s.GodownId = H.GodownId;
            s.SupplierId = sid;
            ViewBag.LineMode = "Create";
            //PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseGoodsReturnLineViewModel svm)
        {

            if (svm.PurchaseGoodsReturnLineId <= 0)
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

                if (svm.PurchaseGoodsReturnLineId <= 0)
                    BeforeSave = PurchaseGoodsReturnDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseGoodsReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseGoodsReturnDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseGoodsReturnHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.PurchaseGoodsReturnLineId <= 0)
            {
                PurchaseGoodsReturnHeader Header = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(svm.PurchaseGoodsReturnHeaderId);

                decimal balqty = (from p in db.PurchaseGoodsReceiptLine
                                  where p.PurchaseGoodsReceiptLineId == svm.PurchaseGoodsReceiptLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }

                if (svm.PurchaseGoodsReceiptLineId <= 0)
                {
                    ModelState.AddModelError("PurchaseGoodsLineId", "Purchase Invoice field is required");
                }
                if (svm.DealQty <= 0)
                {
                    ModelState.AddModelError("DealQty", "DealQty field is required");
                }

                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;

                    PurchaseGoodsReturnLine Line = Mapper.Map<PurchaseGoodsReturnLineViewModel, PurchaseGoodsReturnLine>(svm);


                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                    StockViewModel.DocHeaderId = Header.PurchaseGoodsReturnHeaderId;
                    StockViewModel.DocLineId = Line.PurchaseGoodsReturnLineId;
                    StockViewModel.DocTypeId = Header.DocTypeId;
                    StockViewModel.StockHeaderDocDate = Header.DocDate;
                    StockViewModel.StockDocDate = Header.DocDate;
                    StockViewModel.DocNo = Header.DocNo;
                    StockViewModel.DivisionId = Header.DivisionId;
                    StockViewModel.SiteId = Header.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = Header.SupplierId;
                    StockViewModel.ProductId = svm.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = Header.GodownId;
                    StockViewModel.GodownId = Header.GodownId;
                    StockViewModel.ProcessId = ProcessId;
                    StockViewModel.LotNo = null;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = Line.Qty;
                    StockViewModel.Qty_Rec = 0;
                    StockViewModel.Rate = null;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = svm.Dimension1Id;
                    StockViewModel.Dimension2Id = svm.Dimension2Id;
                    StockViewModel.ProductUidId = svm.ProductUidId;
                    StockViewModel.Remark = Line.Remark;
                    StockViewModel.Status = Header.Status;
                    StockViewModel.CreatedBy = Header.CreatedBy;
                    StockViewModel.CreatedDate = DateTime.Now;
                    StockViewModel.ModifiedBy = Header.ModifiedBy;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    Line.StockId = StockViewModel.StockId;

                    Line.Sr = _PurchaseGoodsReturnLineService.GetMaxSr(svm.PurchaseGoodsReturnHeaderId);
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedDate = DateTime.Now;
                    Line.CreatedBy = User.Identity.Name;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ObjectState = Model.ObjectState.Added;





                    var PurchaseGoodsReceiptLine = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(Line.PurchaseGoodsReceiptLineId);

                    if (PurchaseGoodsReceiptLine.ProductUidId.HasValue && PurchaseGoodsReceiptLine.ProductUidId > 0)
                    {
                        ProductUid Uid = new ProductUidService(_unitOfWork).Find(PurchaseGoodsReceiptLine.ProductUidId.Value);



                        Line.ProductUidLastTransactionDocId = Uid.LastTransactionDocId;
                        Line.ProductUidLastTransactionDocDate = Uid.LastTransactionDocDate;
                        Line.ProductUidLastTransactionDocNo = Uid.LastTransactionDocNo;
                        Line.ProductUidLastTransactionDocTypeId = Uid.LastTransactionDocTypeId;
                        Line.ProductUidLastTransactionPersonId = Uid.LastTransactionPersonId;
                        Line.ProductUidStatus = Uid.Status;
                        Line.ProductUidCurrentProcessId = Uid.CurrenctProcessId;
                        Line.ProductUidCurrentGodownId = Uid.CurrenctGodownId;


                        Uid.LastTransactionDocId = Header.PurchaseGoodsReturnHeaderId;
                        Uid.LastTransactionDocDate = Header.DocDate;
                        Uid.LastTransactionDocNo = Header.DocNo;
                        Uid.LastTransactionDocTypeId = Header.DocTypeId;
                        Uid.LastTransactionPersonId = Header.SupplierId;
                        Uid.Status = ProductUidStatusConstants.Issue;
                        //Uid.CurrenctProcessId = temp.ProcessId;
                        var Site = new SiteService(_unitOfWork).FindByPerson(Header.SupplierId);
                        if (Site != null)
                            Uid.CurrenctGodownId = Site.DefaultGodownId;
                        else
                            Uid.CurrenctGodownId = null;

                        Uid.ModifiedBy = User.Identity.Name;
                        Uid.ModifiedDate = DateTime.Now;
                        Uid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(Uid);
                        //new ProductUidService(_unitOfWork).Update(Uid);

                    }



                    Line.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReturnLine.Add(Line);
                    //_PurchaseGoodsReturnLineService.Create(Line);

                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnReturn(Line.PurchaseGoodsReceiptLineId, Line.PurchaseGoodsReturnLineId, Header.DocDate, Line.Qty, ref db, true);


                    if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                    {
                        Header.Status = (int)StatusConstants.Modified;
                        Header.ModifiedBy = User.Identity.Name;
                        Header.ModifiedDate = DateTime.Now;
                    }

                    if (StockViewModel != null)
                    {
                        if (Header.StockHeaderId == null)
                        {
                            Header.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                    }

                    Header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(Header);
                    //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(Header);                   

                    try
                    {
                        PurchaseGoodsReturnDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(Line.PurchaseGoodsReturnHeaderId, Line.PurchaseGoodsReturnLineId, EventModeConstants.Add), ref db);
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
                        PurchaseGoodsReturnDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(Line.PurchaseGoodsReturnHeaderId, Line.PurchaseGoodsReturnLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.PurchaseGoodsReturnHeaderId,
                        DocLineId = Line.PurchaseGoodsReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Header.DocNo,
                        DocDate = Header.DocDate,
                        DocStatus = Header.Status,
                    }));

                    return RedirectToAction("_Create", new { id = Line.PurchaseGoodsReturnHeaderId, sid = svm.SupplierId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseGoodsReturnHeader Header = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(svm.PurchaseGoodsReturnHeaderId);
                int status = Header.Status;
                StringBuilder logstring = new StringBuilder();

                PurchaseGoodsReturnLine Line = _PurchaseGoodsReturnLineService.Find(svm.PurchaseGoodsReturnLineId);

                PurchaseGoodsReturnLine ExRec = new PurchaseGoodsReturnLine();
                ExRec = Mapper.Map<PurchaseGoodsReturnLine>(Line);


                decimal balqty = (from p in db.PurchaseGoodsReceiptLine
                                  where p.PurchaseGoodsReceiptLineId == svm.PurchaseGoodsReceiptLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty + Line.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }

                if (svm.DealQty <= 0)
                {
                    ModelState.AddModelError("DealQty", "DealQty field is required");
                }


                if (ModelState.IsValid)
                {
                    int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;

                    Line.Remark = svm.Remark;
                    Line.Qty = svm.Qty;
                    Line.DealQty = svm.DealQty;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ModifiedDate = DateTime.Now;
                    Line.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnLine.Add(Line);


                    //_PurchaseGoodsReturnLineService.Update(Line);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = Line,
                    });

                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnReturn(Line.PurchaseGoodsReceiptLineId, Line.PurchaseGoodsReturnLineId, Header.DocDate, Line.Qty, ref db, true);


                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                    StockViewModel.StockId = Line.StockId ?? 0;
                    StockViewModel.DocHeaderId = Header.PurchaseGoodsReturnHeaderId;
                    StockViewModel.DocLineId = Line.PurchaseGoodsReceiptLineId;
                    StockViewModel.DocTypeId = Header.DocTypeId;
                    StockViewModel.StockHeaderDocDate = Header.DocDate;
                    StockViewModel.StockDocDate = Header.DocDate;
                    StockViewModel.DocNo = Header.DocNo;
                    StockViewModel.DivisionId = Header.DivisionId;
                    StockViewModel.SiteId = Header.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = Header.SupplierId;
                    StockViewModel.ProductId = svm.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = Header.GodownId;
                    StockViewModel.GodownId = Header.GodownId;
                    StockViewModel.ProcessId = ProcessId;
                    StockViewModel.LotNo = null;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = svm.Qty;
                    StockViewModel.Qty_Rec = 0;
                    StockViewModel.Rate = null;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = svm.Dimension1Id;
                    StockViewModel.Dimension2Id = svm.Dimension2Id;
                    StockViewModel.ProductUidId = svm.ProductUidId;
                    StockViewModel.Remark = Line.Remark;
                    StockViewModel.Status = Header.Status;
                    StockViewModel.CreatedBy = Header.CreatedBy;
                    StockViewModel.CreatedDate = Header.CreatedDate;
                    StockViewModel.ModifiedBy = User.Identity.Name;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }



                    if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                    {
                        Header.Status = (int)StatusConstants.Modified;
                        Header.ModifiedDate = DateTime.Now;
                        Header.ModifiedBy = User.Identity.Name;
                        //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(Header);
                    }
                    Header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(Header);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseGoodsReturnDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(Line.PurchaseGoodsReturnHeaderId, Line.PurchaseGoodsReturnLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseGoodsReturnDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(Line.PurchaseGoodsReturnHeaderId, Line.PurchaseGoodsReturnLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.PurchaseGoodsReturnHeaderId,
                        DocLineId = Line.PurchaseGoodsReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = Header.DocNo,
                        xEModifications = Modifications,
                        DocDate = Header.DocDate,
                        DocStatus = Header.Status,
                    }));
                    //End of Saving the Activity Log


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
            PurchaseGoodsReturnLineViewModel temp = _PurchaseGoodsReturnLineService.GetPurchaseGoodsReturnLine(id);

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

            PrepareViewBag(temp);

            PurchaseGoodsReturnHeader H = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReturnHeaderId);
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
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
            PurchaseGoodsReturnLineViewModel temp = _PurchaseGoodsReturnLineService.GetPurchaseGoodsReturnLine(id);


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

            PrepareViewBag(temp);

            PurchaseGoodsReturnHeader H = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReturnHeaderId);
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            PurchaseGoodsReturnLineViewModel temp = _PurchaseGoodsReturnLineService.GetPurchaseGoodsReturnLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PrepareViewBag(temp);
            PurchaseGoodsReturnHeader H = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReturnHeaderId);
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
           
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseGoodsReturnLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseGoodsReturnDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReturnHeaderId, vm.PurchaseGoodsReturnLineId), ref db);
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


                PurchaseGoodsReturnLine PurchaseGoodsReturnLine = db.PurchaseGoodsReturnLine.Find(vm.PurchaseGoodsReturnLineId);
                PurchaseGoodsReturnHeader header = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(PurchaseGoodsReturnLine.PurchaseGoodsReturnHeaderId);

                try
                {
                    PurchaseGoodsReturnDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseGoodsReturnLine.PurchaseGoodsReturnHeaderId, PurchaseGoodsReturnLine.PurchaseGoodsReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseGoodsReturnLine>(PurchaseGoodsReturnLine),
                });

                StockId = PurchaseGoodsReturnLine.StockId;

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnReturn(PurchaseGoodsReturnLine.PurchaseGoodsReceiptLineId, PurchaseGoodsReturnLine.PurchaseGoodsReturnLineId, header.DocDate, 0, ref db, true);

                //_PurchaseGoodsReturnLineService.Delete(PurchaseGoodsReturnLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(header);
                }

                header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReturnHeader.Add(header);


                var PurchaseGoodsReceiptLine = db.PurchaseGoodsReceiptLine.Find(PurchaseGoodsReturnLine.PurchaseGoodsReceiptLineId);

                if (PurchaseGoodsReceiptLine.ProductUidId.HasValue)
                {
                    ProductUid ProductUid = new ProductUidService(_unitOfWork).Find(PurchaseGoodsReceiptLine.ProductUidId.Value);

                    if (header.DocNo != ProductUid.LastTransactionDocNo || header.DocTypeId != ProductUid.LastTransactionDocTypeId)
                    {
                        ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                        PrepareViewBag(vm);
                        ViewBag.LineMode = "Delete";
                        return PartialView("_Create", vm);
                    }

                    ProductUid.LastTransactionDocDate = PurchaseGoodsReturnLine.ProductUidLastTransactionDocDate;
                    ProductUid.LastTransactionDocId = PurchaseGoodsReturnLine.ProductUidLastTransactionDocId;
                    ProductUid.LastTransactionDocNo = PurchaseGoodsReturnLine.ProductUidLastTransactionDocNo;
                    ProductUid.LastTransactionDocTypeId = PurchaseGoodsReturnLine.ProductUidLastTransactionDocTypeId;
                    ProductUid.LastTransactionPersonId = PurchaseGoodsReturnLine.ProductUidLastTransactionPersonId;
                    ProductUid.CurrenctGodownId = PurchaseGoodsReturnLine.ProductUidCurrentGodownId;
                    ProductUid.CurrenctProcessId = PurchaseGoodsReturnLine.ProductUidCurrentProcessId;
                    ProductUid.Status = PurchaseGoodsReturnLine.ProductUidStatus;
                    ProductUid.ObjectState = Model.ObjectState.Modified;
                    db.ProductUid.Add(ProductUid);


                    //new ProductUidService(_unitOfWork).Update(ProductUid);
                }

                PurchaseGoodsReturnLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReturnLine.Remove(PurchaseGoodsReturnLine);


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
                    return PartialView("_Create", vm);
                }

                try
                {
                    PurchaseGoodsReturnDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseGoodsReturnLine.PurchaseGoodsReturnHeaderId, PurchaseGoodsReturnLine.PurchaseGoodsReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseGoodsReturnHeaderId,
                    DocLineId = PurchaseGoodsReturnLine.PurchaseGoodsReturnLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
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

        public JsonResult GetPendingReceipts(int ProductId, int PurchaseGoodsReturnHeaderId)
        {
            return Json(new PurchaseGoodsReceiptHeaderService(_unitOfWork).GetPendingReceipts(ProductId, PurchaseGoodsReturnHeaderId).ToList());
        }

        public JsonResult GetReceiptDetail(int ReceiptLineId)
        {
            return Json(new PurchaseGoodsReceiptLineService(_unitOfWork).GetPurchaseGoodsReceiptDetailBalance(ReceiptLineId));
        }

        public JsonResult GetPurchaseReceipts(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseGoodsReturnLineService.GetPendingPurchaseReceiptHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPurchaseOrders(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseGoodsReturnLineService.GetPendingPurchaseOrderHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_PurchaseGoodsReturnLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductUidTransactionDetail(int ProductUidId)
        {
            return Json(_PurchaseGoodsReturnLineService.GetProductUidTransactionDetail(ProductUidId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string deliveryunitid)
        {
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, deliveryunitid);

            Model.Models.Unit Unit = new UnitService(_unitOfWork).Find(deliveryunitid);

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


    }
}
