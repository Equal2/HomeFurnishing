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
using CustomEventArgs;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class SaleDispatchReturnLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDispatchReturnLineService _SaleDispatchReturnLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public SaleDispatchReturnLineController(ISaleDispatchReturnLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleDispatchReturnLineService = SaleOrder;
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
            var p = _SaleDispatchReturnLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForReceipt(int id, int sid)
        {
            SaleDispatchReturnLineFilterViewModel vm = new SaleDispatchReturnLineFilterViewModel();
            vm.SaleDispatchReturnHeaderId = id;
            vm.BuyerId = sid;
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForOrders(int id, int sid)
        {
            SaleDispatchReturnLineFilterViewModel vm = new SaleDispatchReturnLineFilterViewModel();
            vm.SaleDispatchReturnHeaderId = id;
            vm.BuyerId = sid;
            return PartialView("_OrderFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(SaleDispatchReturnLineFilterViewModel vm)
        {
            List<SaleDispatchReturnLineViewModel> temp = _SaleDispatchReturnLineService.GetSaleDispatchsForFilters(vm).ToList();
            SaleDispatchReturnMasterDetailModel svm = new SaleDispatchReturnMasterDetailModel();
            svm.SaleDispatchReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(SaleDispatchReturnLineFilterViewModel vm)
        {
            List<SaleDispatchReturnLineViewModel> temp = _SaleDispatchReturnLineService.GetSaleOrderForFilters(vm).ToList();
            SaleDispatchReturnMasterDetailModel svm = new SaleDispatchReturnMasterDetailModel();
            svm.SaleDispatchReturnLineViewModel = temp;
            return PartialView("_OrderResults", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(SaleDispatchReturnMasterDetailModel vm)
        {
            int Cnt = 0;
            int Serial = _SaleDispatchReturnLineService.GetMaxSr(vm.SaleDispatchReturnLineViewModel.FirstOrDefault().SaleDispatchReturnHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            bool BeforeSave = true;

            //try
            //{
            //    BeforeSave = SaleDispatchReturnDocEvents.beforeLineSaveBulkEvent(this, new SaleEventArgs(vm.SaleDispatchReturnLineViewModel.FirstOrDefault().SaleDispatchReturnHeaderId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            //if (!BeforeSave)
            //    ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;
                SaleDispatchReturnHeader Header = new SaleDispatchReturnHeaderService(_unitOfWork).Find(vm.SaleDispatchReturnLineViewModel.FirstOrDefault().SaleDispatchReturnHeaderId);

                foreach (var item in vm.SaleDispatchReturnLineViewModel)
                {
                    decimal balqty = (from p in db.ViewSaleDispatchBalance
                                      where p.SaleDispatchLineId == item.SaleDispatchLineId
                                      select p.BalanceQty).FirstOrDefault();


                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        SaleDispatchReturnLine Line = new SaleDispatchReturnLine();
                        Line.SaleDispatchReturnHeaderId = item.SaleDispatchReturnHeaderId;
                        Line.SaleDispatchLineId = item.SaleDispatchLineId;                       
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


                        LineStatus.Add(Line.SaleDispatchLineId, Line.Qty);


                        SaleDispatchLine SaleDispatchLine = new SaleDispatchLineService(_unitOfWork).Find(Line.SaleDispatchLineId);
                        //var receipt = new SaleDispatchLineService(_unitOfWork).Find(item.SaleDispatchLineId );




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
                        StockViewModel.DocHeaderId = Header.SaleDispatchReturnHeaderId;
                        StockViewModel.DocLineId = Line.SaleDispatchLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.BuyerId;
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
                        //StockViewModel.ProductUidId = SaleDispatchLine.ProductUidId;
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

                        Line.ObjectState = Model.ObjectState.Added;
                        db.SaleDispatchReturnLine.Add(Line);
                        //_SaleDispatchReturnLineService.Create(Line);

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
                db.SaleDispatchReturnHeader.Add(Header);

                //new SaleDispatchReturnHeaderService(_unitOfWork).Update(Header);

                //new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyReturnMultiple(LineStatus, Header.DocDate, ref db);

                //try
                //{
                //    SaleDispatchReturnDocEvents.onLineSaveBulkEvent(this, new SaleEventArgs(vm.SaleDispatchReturnLineViewModel.FirstOrDefault().SaleDispatchReturnHeaderId), ref db);
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
                //    SaleDispatchReturnDocEvents.afterLineSaveBulkEvent(this, new SaleEventArgs(vm.SaleDispatchReturnLineViewModel.FirstOrDefault().SaleDispatchReturnHeaderId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.SaleDispatchReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(SaleDispatchReturnLineViewModel vm)
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
            SaleDispatchReturnHeader H = new SaleDispatchReturnHeaderService(_unitOfWork).Find(Id);
            SaleDispatchReturnLineViewModel s = new SaleDispatchReturnLineViewModel();

            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);

            s.SaleDispatchReturnHeaderId = H.SaleDispatchReturnHeaderId;
            s.SaleDispatchReturnHeaderDocNo = H.DocNo;
            s.GodownId = H.GodownId;
            s.BuyerId = sid;
            ViewBag.LineMode = "Create";
            //PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(SaleDispatchReturnLineViewModel svm)
        {

            if (svm.SaleDispatchReturnLineId <= 0)
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

            //    if (svm.SaleDispatchReturnLineId <= 0)
            //        BeforeSave = SaleDispatchReturnDocEvents.beforeLineSaveEvent(this, new SaleEventArgs(svm.SaleDispatchReturnHeaderId, EventModeConstants.Add), ref db);
            //    else
            //        BeforeSave = SaleDispatchReturnDocEvents.beforeLineSaveEvent(this, new SaleEventArgs(svm.SaleDispatchReturnHeaderId, EventModeConstants.Edit), ref db);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            //if (!BeforeSave)
            //    ModelState.AddModelError("", "Validation failed before save.");

            if (svm.SaleDispatchReturnLineId <= 0)
            {
                SaleDispatchReturnHeader Header = new SaleDispatchReturnHeaderService(_unitOfWork).Find(svm.SaleDispatchReturnHeaderId);

                decimal balqty = (from p in db.SaleDispatchLine
                                  join t in db.PackingLine on p.PackingLineId equals t.PackingLineId
                                  where p.SaleDispatchLineId == svm.SaleDispatchLineId
                                  select t.DealQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }

                if (svm.SaleDispatchLineId <= 0)
                {
                    ModelState.AddModelError("SaleDispatchLineId", "Sale Invoice field is required");
                }
                if (svm.DealQty <= 0)
                {
                    ModelState.AddModelError("DealQty", "DealQty field is required");
                }

                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;

                    SaleDispatchReturnLine Line = Mapper.Map<SaleDispatchReturnLineViewModel, SaleDispatchReturnLine>(svm);


                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                    StockViewModel.DocHeaderId = Header.SaleDispatchReturnHeaderId;
                    StockViewModel.DocLineId = Line.SaleDispatchReturnLineId;
                    StockViewModel.DocTypeId = Header.DocTypeId;
                    StockViewModel.StockHeaderDocDate = Header.DocDate;
                    StockViewModel.StockDocDate = Header.DocDate;
                    StockViewModel.DocNo = Header.DocNo;
                    StockViewModel.DivisionId = Header.DivisionId;
                    StockViewModel.SiteId = Header.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = Header.BuyerId;
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

                    Line.Sr = _SaleDispatchReturnLineService.GetMaxSr(svm.SaleDispatchReturnHeaderId);
                    Line.CreatedDate = DateTime.Now;
                    Line.ModifiedDate = DateTime.Now;
                    Line.CreatedBy = User.Identity.Name;
                    Line.ModifiedBy = User.Identity.Name;
                    Line.ObjectState = Model.ObjectState.Added;

                    var SaleDispatchLine = new SaleDispatchLineService(_unitOfWork).Find(Line.SaleDispatchLineId);

                    Line.ObjectState = Model.ObjectState.Added;
                    db.SaleDispatchReturnLine.Add(Line);
                    //_SaleDispatchReturnLineService.Create(Line);

                    //new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnReturn(Line.SaleDispatchLineId, Line.SaleDispatchReturnLineId, Header.DocDate, Line.Qty, ref db, true);


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
                    db.SaleDispatchReturnHeader.Add(Header);
                    //new SaleDispatchReturnHeaderService(_unitOfWork).Update(Header);                   

                    //try
                    //{
                    //    SaleDispatchReturnDocEvents.onLineSaveEvent(this, new SaleEventArgs(Line.SaleDispatchReturnHeaderId, Line.SaleDispatchReturnLineId, EventModeConstants.Add), ref db);
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
                    //    SaleDispatchReturnDocEvents.afterLineSaveEvent(this, new SaleEventArgs(Line.SaleDispatchReturnHeaderId, Line.SaleDispatchReturnLineId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXCL"] += message;
                    //}

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.SaleDispatchReturnHeaderId,
                        DocLineId = Line.SaleDispatchReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Header.DocNo,
                        DocDate = Header.DocDate,
                        DocStatus = Header.Status,
                    }));

                    return RedirectToAction("_Create", new { id = Line.SaleDispatchReturnHeaderId, sid = svm.BuyerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                SaleDispatchReturnHeader Header = new SaleDispatchReturnHeaderService(_unitOfWork).Find(svm.SaleDispatchReturnHeaderId);
                int status = Header.Status;
                StringBuilder logstring = new StringBuilder();

                SaleDispatchReturnLine Line = _SaleDispatchReturnLineService.Find(svm.SaleDispatchReturnLineId);

                SaleDispatchReturnLine ExRec = new SaleDispatchReturnLine();
                ExRec = Mapper.Map<SaleDispatchReturnLine>(Line);


                decimal balqty = (from p in db.SaleDispatchLine
                                  join t in db.PackingLine on p.PackingLineId equals t.PackingLineId
                                  where p.SaleDispatchLineId == svm.SaleDispatchLineId
                                  select t.DealQty).FirstOrDefault();
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
                    db.SaleDispatchReturnLine.Add(Line);


                    //_SaleDispatchReturnLineService.Update(Line);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = Line,
                    });

                    //new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnReturn(Line.SaleDispatchLineId, Line.SaleDispatchReturnLineId, Header.DocDate, Line.Qty, ref db, true);


                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = Header.StockHeaderId ?? 0;
                    StockViewModel.StockId = Line.StockId ?? 0;
                    StockViewModel.DocHeaderId = Header.SaleDispatchReturnHeaderId;
                    StockViewModel.DocLineId = Line.SaleDispatchLineId;
                    StockViewModel.DocTypeId = Header.DocTypeId;
                    StockViewModel.StockHeaderDocDate = Header.DocDate;
                    StockViewModel.StockDocDate = Header.DocDate;
                    StockViewModel.DocNo = Header.DocNo;
                    StockViewModel.DivisionId = Header.DivisionId;
                    StockViewModel.SiteId = Header.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = Header.BuyerId;
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
                        //new SaleDispatchReturnHeaderService(_unitOfWork).Update(Header);
                    }
                    Header.ObjectState = Model.ObjectState.Modified;
                    db.SaleDispatchReturnHeader.Add(Header);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    //try
                    //{
                    //    SaleDispatchReturnDocEvents.onLineSaveEvent(this, new SaleEventArgs(Line.SaleDispatchReturnHeaderId, Line.SaleDispatchReturnLineId, EventModeConstants.Edit), ref db);
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
                    //    SaleDispatchReturnDocEvents.afterLineSaveEvent(this, new SaleEventArgs(Line.SaleDispatchReturnHeaderId, Line.SaleDispatchReturnLineId, EventModeConstants.Edit), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Header.DocTypeId,
                        DocId = Header.SaleDispatchReturnHeaderId,
                        DocLineId = Line.SaleDispatchReturnLineId,
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
            SaleDispatchReturnLineViewModel temp = _SaleDispatchReturnLineService.GetSaleDispatchReturnLine(id);

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

            SaleDispatchReturnHeader H = new SaleDispatchReturnHeaderService(_unitOfWork).Find(temp.SaleDispatchReturnHeaderId);
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
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
            SaleDispatchReturnLineViewModel temp = _SaleDispatchReturnLineService.GetSaleDispatchReturnLine(id);


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

            SaleDispatchReturnHeader H = new SaleDispatchReturnHeaderService(_unitOfWork).Find(temp.SaleDispatchReturnHeaderId);
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            SaleDispatchReturnLineViewModel temp = _SaleDispatchReturnLineService.GetSaleDispatchReturnLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PrepareViewBag(temp);
            SaleDispatchReturnHeader H = new SaleDispatchReturnHeaderService(_unitOfWork).Find(temp.SaleDispatchReturnHeaderId);
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
           
            return PartialView("_Create", temp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(SaleDispatchReturnLineViewModel vm)
        {
            bool BeforeSave = true;
            //try
            //{
            //    BeforeSave = SaleDispatchReturnDocEvents.beforeLineDeleteEvent(this, new SaleEventArgs(vm.SaleDispatchReturnHeaderId, vm.SaleDispatchReturnLineId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            //if (!BeforeSave)
            //    TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {

                int? StockId = 0;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                SaleDispatchReturnLine SaleDispatchReturnLine = db.SaleDispatchReturnLine.Find(vm.SaleDispatchReturnLineId);
                SaleDispatchReturnHeader header = new SaleDispatchReturnHeaderService(_unitOfWork).Find(SaleDispatchReturnLine.SaleDispatchReturnHeaderId);

                //try
                //{
                //    SaleDispatchReturnDocEvents.onLineDeleteEvent(this, new SaleEventArgs(SaleDispatchReturnLine.SaleDispatchReturnHeaderId, SaleDispatchReturnLine.SaleDispatchReturnLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXCL"] += message;
                //    EventException = true;
                //}

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<SaleDispatchReturnLine>(SaleDispatchReturnLine),
                });

                StockId = SaleDispatchReturnLine.StockId;

                //new SaleOrderLineStatusService(_unitOfWork).UpdateSaleQtyOnReturn(SaleDispatchReturnLine.SaleDispatchLineId, SaleDispatchReturnLine.SaleDispatchReturnLineId, header.DocDate, 0, ref db, true);

                //_SaleDispatchReturnLineService.Delete(SaleDispatchReturnLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    //new SaleDispatchReturnHeaderService(_unitOfWork).Update(header);
                }

                header.ObjectState = Model.ObjectState.Modified;
                db.SaleDispatchReturnHeader.Add(header);


                var SaleDispatchLine = db.SaleDispatchLine.Find(SaleDispatchReturnLine.SaleDispatchLineId);

                SaleDispatchReturnLine.ObjectState = Model.ObjectState.Deleted;
                db.SaleDispatchReturnLine.Remove(SaleDispatchReturnLine);


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

                //try
                //{
                //    SaleDispatchReturnDocEvents.afterLineDeleteEvent(this, new SaleEventArgs(SaleDispatchReturnLine.SaleDispatchReturnHeaderId, SaleDispatchReturnLine.SaleDispatchReturnLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.SaleDispatchReturnHeaderId,
                    DocLineId = SaleDispatchReturnLine.SaleDispatchReturnLineId,
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

        public JsonResult GetPendingReceipts(int ProductId, int SaleDispatchReturnHeaderId)
        {
            return Json(new SaleDispatchHeaderService(_unitOfWork).GetPendingReceipts(ProductId, SaleDispatchReturnHeaderId).ToList());
        }

        public JsonResult GetReceiptDetail(int ReceiptLineId)
        {
            return Json(new SaleDispatchLineService(_unitOfWork).GetSaleDispatchDetailBalance(ReceiptLineId));
        }

        public JsonResult GetSaleReceipts(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_SaleDispatchReturnLineService.GetPendingSaleReceiptHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSaleOrders(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_SaleDispatchReturnLineService.GetPendingSaleOrderHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_SaleDispatchReturnLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetProductUidTransactionDetail(int ProductUidId)
        //{
        //    return Json(_SaleDispatchReturnLineService.GetProductUidTransactionDetail(ProductUidId), JsonRequestBehavior.AllowGet);
        //}

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
