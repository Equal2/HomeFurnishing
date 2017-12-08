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
using PurchaseInvoiceReturnDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseInvoiceReturnLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        IPurchaseInvoiceReturnLineService _PurchaseInvoiceReturnLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseInvoiceReturnLineController(IPurchaseInvoiceReturnLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseInvoiceReturnLineService = SaleOrder;
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
            var p = _PurchaseInvoiceReturnLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForInvoice(int id, int sid)
        {
            PurchaseInvoiceReturnLineFilterViewModel vm = new PurchaseInvoiceReturnLineFilterViewModel();
            vm.PurchaseInvoiceReturnHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForReceipt(int id, int sid)
        {
            PurchaseInvoiceReturnLineFilterViewModel vm = new PurchaseInvoiceReturnLineFilterViewModel();
            vm.PurchaseInvoiceReturnHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_ReceiptFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseInvoiceReturnLineFilterViewModel vm)
        {
            List<PurchaseInvoiceReturnLineViewModel> temp = _PurchaseInvoiceReturnLineService.GetPurchaseInvoiceForFilters(vm).ToList();
            PurchaseInvoiceReturnMasterDetailModel svm = new PurchaseInvoiceReturnMasterDetailModel();
            svm.PurchaseInvoiceReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostReceipts(PurchaseInvoiceReturnLineFilterViewModel vm)
        {
            List<PurchaseInvoiceReturnLineViewModel> temp = _PurchaseInvoiceReturnLineService.GetPurchaseReceiptForFilters(vm).ToList();
            PurchaseInvoiceReturnMasterDetailModel svm = new PurchaseInvoiceReturnMasterDetailModel();
            svm.PurchaseInvoiceReturnLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseInvoiceReturnMasterDetailModel vm)
        {
            int Cnt = 0;

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int Gpk = 0;
            int Serial = _PurchaseInvoiceReturnLineService.GetMaxSr(vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId);
            bool HeaderChargeEdit = false;

            PurchaseInvoiceReturnHeader Header = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId);

            PurchaseInvoiceSetting Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new PurchaseInvoiceReturnLineChargeService(_unitOfWork).GetMaxProductCharge(Header.PurchaseInvoiceReturnHeaderId, "Web.PurchaseInvoiceReturnLines", "PurchaseInvoiceReturnHeaderId", "PurchaseInvoiceReturnLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            PurchaseGoodsReturnHeader GoodsRetHeader = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Header.PurchaseGoodsReturnHeaderId ?? 0);

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseInvoiceReturnDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId), ref db);
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


                var InvLineIds = vm.PurchaseInvoiceReturnLineViewModel.Select(m => m.PurchaseInvoiceLineId).ToArray();
                var InvBalRecords = (from p in db.ViewPurchaseInvoiceBalance
                                     where InvLineIds.Contains(p.PurchaseInvoiceLineId)
                                     select new
                                     {
                                         LineId = p.PurchaseInvoiceLineId,
                                         BalQty = p.BalanceQty,
                                     }).ToList();

                foreach (var item in vm.PurchaseInvoiceReturnLineViewModel)
                {
                    decimal balqty = InvBalRecords.Where(m => m.LineId == item.PurchaseInvoiceLineId).FirstOrDefault().BalQty;


                    if (item.Qty > 0 && item.Qty <= balqty)
                    {
                        PurchaseInvoiceReturnLine line = new PurchaseInvoiceReturnLine();
                        //var receipt = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(item.PurchaseGoodsReceiptLineId );


                        line.PurchaseInvoiceReturnHeaderId = item.PurchaseInvoiceReturnHeaderId;
                        line.PurchaseInvoiceLineId = item.PurchaseInvoiceLineId;
                        line.Qty = item.Qty;
                        line.Sr = Serial++;
                        line.DiscountPer = item.DiscountPer;
                        line.Rate = item.Rate;
                        line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Amount = item.RateAfterDiscount * line.DealQty;

                        line.Remark = item.Remark;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.PurchaseInvoiceReturnLineId = pk;


                        PurchaseGoodsReturnLine GLine = Mapper.Map<PurchaseInvoiceReturnLine, PurchaseGoodsReturnLine>(line);
                        GLine.PurchaseGoodsReceiptLineId = new PurchaseInvoiceLineService(_unitOfWork).Find(line.PurchaseInvoiceLineId).PurchaseGoodsReceiptLineId;
                        GLine.PurchaseGoodsReturnHeaderId = GoodsRetHeader.PurchaseGoodsReturnHeaderId;
                        GLine.PurchaseGoodsReturnLineId = Gpk;
                        GLine.Qty = line.Qty;
                        //GLine.ObjectState = Model.ObjectState.Added;


                        PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(GLine.PurchaseGoodsReceiptLineId);

                        StockViewModel StockViewModel = new StockViewModel();


                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = GoodsRetHeader.StockHeaderId ?? 0;
                        }
                        else
                        {
                            if (GoodsRetHeader.StockHeaderId != null && GoodsRetHeader.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)GoodsRetHeader.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }

                        }

                        StockViewModel.StockId = -Cnt;

                        StockViewModel.DocHeaderId = GoodsRetHeader.PurchaseGoodsReturnHeaderId;
                        StockViewModel.DocLineId = PurchaseGoodsReceiptLine.PurchaseGoodsReceiptLineId;
                        StockViewModel.DocTypeId = GoodsRetHeader.DocTypeId;
                        StockViewModel.StockHeaderDocDate = GoodsRetHeader.DocDate;
                        StockViewModel.StockDocDate = GoodsRetHeader.DocDate;
                        StockViewModel.DocNo = GoodsRetHeader.DocNo;
                        StockViewModel.DivisionId = GoodsRetHeader.DivisionId;
                        StockViewModel.SiteId = GoodsRetHeader.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = GoodsRetHeader.SupplierId;
                        StockViewModel.ProductId = PurchaseGoodsReceiptLine.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = GoodsRetHeader.GodownId;
                        StockViewModel.HeaderProcessId = ProcessId;
                        StockViewModel.GodownId = GoodsRetHeader.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = GLine.Qty;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = null;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = PurchaseGoodsReceiptLine.Specification;
                        StockViewModel.Dimension1Id = PurchaseGoodsReceiptLine.Dimension1Id;
                        StockViewModel.Dimension2Id = PurchaseGoodsReceiptLine.Dimension2Id;
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
                            GoodsRetHeader.StockHeaderId = StockViewModel.StockHeaderId;
                        }


                        GLine.StockId = StockViewModel.StockId;


                        GLine.ObjectState = Model.ObjectState.Added;
                        db.PurchaseGoodsReturnLine.Add(GLine);
                        //new PurchaseGoodsReturnLineService(_unitOfWork).Create(GLine);

                        line.PurchaseGoodsReturnLineId = GLine.PurchaseGoodsReturnLineId;
                        line.ObjectState = Model.ObjectState.Added;
                        //_PurchaseInvoiceReturnLineService.Create(line);
                        db.PurchaseInvoiceReturnLine.Add(line);

                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseInvoiceReturnLineId, HeaderTableId = item.PurchaseInvoiceReturnHeaderId, PersonID = Header.SupplierId, DealQty = line.DealQty });
                        Gpk++;
                        pk++;

                        Cnt = Cnt + 1;
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {

                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    


                    //GoodsRetHeader.Status = (int)StatusConstants.Modified;
                    GoodsRetHeader.ModifiedBy = User.Identity.Name;
                    GoodsRetHeader.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseInvoiceReturnHeader.Add(Header);

                //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(GoodsRetHeader);
                GoodsRetHeader.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReturnHeader.Add(GoodsRetHeader);

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseInvoiceReturnHeaderCharges", "Web.PurchaseInvoiceReturnLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    PurchaseInvoiceReturnLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseInvoiceReturnLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.PurchaseInvoiceReturnLineCharge.Add(PoLineCharge);
                    //new PurchaseInvoiceReturnLineChargeService(_unitOfWork).Create(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        PurchaseInvoiceReturnHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseInvoiceReturnHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId;
                        POHeaderCharge.PersonID = Header.SupplierId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.PurchaseInvoiceReturnHeaderCharge.Add(POHeaderCharge);
                        //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseInvoiceReturnHeaderCharge.Add(footercharge);
                        //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);
                    }

                }

                try
                {
                    PurchaseInvoiceReturnDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId), ref db);
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

                    //_unitOfWork.Save();
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
                    PurchaseInvoiceReturnDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceReturnLineViewModel.FirstOrDefault().PurchaseInvoiceReturnHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(PurchaseInvoiceReturnLineViewModel vm)
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
            PurchaseInvoiceReturnHeader H = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(Id);
            PurchaseInvoiceReturnLineViewModel s = new PurchaseInvoiceReturnLineViewModel();

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            s.PurchaseInvoiceReturnHeaderId = H.PurchaseInvoiceReturnHeaderId;
            s.PurchaseInvoiceReturnHeaderDocNo = H.DocNo;
            s.SupplierId = sid;
            s.DocTypeId = H.DocTypeId;
            s.DivisionId = H.DivisionId;
            s.SiteId = H.SiteId;
            ViewBag.LineMode = "Create";
            //PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseInvoiceReturnLineViewModel svm)
        {

            PurchaseInvoiceReturnHeader temp = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(svm.PurchaseInvoiceReturnHeaderId);

            if (svm.PurchaseInvoiceReturnLineId <= 0)
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

                if (svm.PurchaseInvoiceReturnLineId <= 0)
                    BeforeSave = PurchaseInvoiceReturnDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseInvoiceReturnDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.PurchaseInvoiceReturnLineId <= 0)
            {

                PurchaseGoodsReturnHeader PurchaseGoodsReturnHeader = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find((int)temp.PurchaseGoodsReturnHeaderId);

                decimal balqty = (from p in db.PurchaseInvoiceLine
                                  where p.PurchaseInvoiceLineId == svm.PurchaseInvoiceLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }
                if (svm.Qty == 0)
                {
                    ModelState.AddModelError("Qty", "Please Check Qty");
                }

                if (svm.PurchaseInvoiceLineId <= 0)
                {
                    ModelState.AddModelError("PurchaseInvoiceLineId", "Purchase Invoice field is required");
                }

                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;

                    PurchaseInvoiceReturnLine s = Mapper.Map<PurchaseInvoiceReturnLineViewModel, PurchaseInvoiceReturnLine>(svm);
                    s.Sr = _PurchaseInvoiceReturnLineService.GetMaxSr(s.PurchaseInvoiceReturnHeaderId);
                    s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;

                    PurchaseGoodsReturnLine Gline = Mapper.Map<PurchaseInvoiceReturnLine, PurchaseGoodsReturnLine>(s);
                    Gline.PurchaseGoodsReceiptLineId = new PurchaseInvoiceLineService(_unitOfWork).Find(s.PurchaseInvoiceLineId).PurchaseGoodsReceiptLineId;
                    Gline.PurchaseGoodsReturnHeaderId = temp.PurchaseGoodsReturnHeaderId ?? 0;
                    Gline.Qty = svm.Qty;





                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = PurchaseGoodsReturnHeader.StockHeaderId ?? 0;
                    StockViewModel.DocHeaderId = PurchaseGoodsReturnHeader.PurchaseGoodsReturnHeaderId;
                    StockViewModel.DocLineId = Gline.PurchaseGoodsReturnLineId;
                    StockViewModel.DocTypeId = PurchaseGoodsReturnHeader.DocTypeId;
                    StockViewModel.StockHeaderDocDate = PurchaseGoodsReturnHeader.DocDate;
                    StockViewModel.StockDocDate = PurchaseGoodsReturnHeader.DocDate;
                    StockViewModel.DocNo = PurchaseGoodsReturnHeader.DocNo;
                    StockViewModel.DivisionId = PurchaseGoodsReturnHeader.DivisionId;
                    StockViewModel.SiteId = PurchaseGoodsReturnHeader.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = PurchaseGoodsReturnHeader.SupplierId;
                    StockViewModel.ProductId = svm.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = PurchaseGoodsReturnHeader.GodownId;
                    StockViewModel.GodownId = PurchaseGoodsReturnHeader.GodownId;
                    StockViewModel.ProcessId = ProcessId;
                    StockViewModel.LotNo = null;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = Gline.Qty;
                    StockViewModel.Qty_Rec = 0;
                    StockViewModel.Rate = null;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = svm.Dimension1Id;
                    StockViewModel.Dimension2Id = svm.Dimension2Id;
                    StockViewModel.ProductUidId = svm.ProductUidId;
                    StockViewModel.Remark = svm.Remark;
                    StockViewModel.Status = PurchaseGoodsReturnHeader.Status;
                    StockViewModel.CreatedBy = User.Identity.Name;
                    StockViewModel.CreatedDate = DateTime.Now;
                    StockViewModel.ModifiedBy = User.Identity.Name;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    Gline.StockId = StockViewModel.StockId;

                    Gline.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReturnLine.Add(Gline);
                    //new PurchaseGoodsReturnLineService(_unitOfWork).Create(Gline);

                    s.PurchaseGoodsReturnLineId = Gline.PurchaseGoodsReturnLineId;
                    s.ObjectState = Model.ObjectState.Added;
                    //_PurchaseInvoiceReturnLineService.Create(s);
                    db.PurchaseInvoiceReturnLine.Add(s);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.PurchaseInvoiceReturnLineId;
                            item.PersonID = temp.SupplierId;
                            item.HeaderTableId = s.PurchaseInvoiceReturnHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.PurchaseInvoiceReturnLineCharge.Add(item);
                            //new PurchaseInvoiceReturnLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.PurchaseInvoiceReturnHeaderCharge.Add(footercharge);
                                //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.PurchaseInvoiceReturnHeaderId;
                                item.PersonID = temp.SupplierId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.PurchaseInvoiceReturnHeaderCharge.Add(item);
                                //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }


                    PurchaseInvoiceReturnHeader temp2 = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(s.PurchaseInvoiceReturnHeaderId);
                    if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                    {
                        temp2.Status = (int)StatusConstants.Modified;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ModifiedDate = DateTime.Now;

                        //PurchaseGoodsReturnHeader.Status = temp2.Status;
                        PurchaseGoodsReturnHeader.ModifiedBy = User.Identity.Name;
                        PurchaseGoodsReturnHeader.ModifiedDate = DateTime.Now;
                    }

                    if (StockViewModel != null)
                    {
                        if (PurchaseGoodsReturnHeader.StockHeaderId == null)
                        {
                            PurchaseGoodsReturnHeader.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                        //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(PurchaseGoodsReturnHeader);
                    }
                    //new PurchaseInvoiceReturnHeaderService(_unitOfWork).Update(temp2);

                    temp2.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceReturnHeader.Add(temp2);

                    PurchaseGoodsReturnHeader.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(PurchaseGoodsReturnHeader);

                    try
                    {
                        PurchaseInvoiceReturnDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseInvoiceReturnHeaderId, s.PurchaseInvoiceReturnLineId, EventModeConstants.Add), ref db);
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
                        PurchaseInvoiceReturnDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseInvoiceReturnHeaderId, s.PurchaseInvoiceReturnLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseInvoiceReturnHeaderId,
                        DocLineId = s.PurchaseInvoiceReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.PurchaseInvoiceReturnHeaderId, sid = svm.SupplierId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;

                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                PurchaseInvoiceReturnLine line = _PurchaseInvoiceReturnLineService.Find(svm.PurchaseInvoiceReturnLineId);
                PurchaseGoodsReturnHeader PurchaseGoodsReturnHeader = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find((int)temp.PurchaseGoodsReturnHeaderId);

                PurchaseInvoiceReturnLine ExRec = new PurchaseInvoiceReturnLine();
                ExRec = Mapper.Map<PurchaseInvoiceReturnLine>(line);


                decimal balqty = (from p in db.PurchaseInvoiceLine
                                  where p.PurchaseInvoiceLineId == svm.PurchaseInvoiceLineId
                                  select p.DealQty).FirstOrDefault();
                if (balqty + line.Qty < svm.Qty)
                {
                    ModelState.AddModelError("Qty", "Qty Exceeding Invoice Qty");
                }


                if (ModelState.IsValid)
                {
                    if (svm.Qty > 0)
                    {
                        line.DiscountPer = svm.DiscountPer;
                        line.Remark = svm.Remark;
                        line.Qty = svm.Qty;
                        line.DealQty = svm.DealQty;
                        line.Amount = svm.Amount;
                        line.ModifiedBy = User.Identity.Name;
                        line.ModifiedDate = DateTime.Now;
                    }

                    line.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceReturnLine.Add(line);
                    //_PurchaseInvoiceReturnLineService.Update(line);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = line,
                    });

                    PurchaseGoodsReturnLine GLine = new PurchaseGoodsReturnLineService(_unitOfWork).Find(line.PurchaseGoodsReturnLineId ?? 0);

                    PurchaseGoodsReturnLine ExRecR = new PurchaseGoodsReturnLine();
                    ExRecR = Mapper.Map<PurchaseGoodsReturnLine>(GLine);

                    GLine.Remark = line.Remark;
                    GLine.Qty = line.Qty;
                    GLine.DealQty = line.DealQty;

                    GLine.ObjectState = Model.ObjectState.Modified;
                    //new PurchaseGoodsReturnLineService(_unitOfWork).Update(GLine);
                    db.PurchaseGoodsReturnLine.Add(GLine);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecR,
                        Obj = GLine,
                    });


                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = PurchaseGoodsReturnHeader.StockHeaderId ?? 0;
                    StockViewModel.StockId = GLine.StockId ?? 0;
                    StockViewModel.DocHeaderId = PurchaseGoodsReturnHeader.PurchaseGoodsReturnHeaderId;
                    StockViewModel.DocLineId = GLine.PurchaseGoodsReceiptLineId;
                    StockViewModel.DocTypeId = PurchaseGoodsReturnHeader.DocTypeId;
                    StockViewModel.StockHeaderDocDate = PurchaseGoodsReturnHeader.DocDate;
                    StockViewModel.StockDocDate = PurchaseGoodsReturnHeader.DocDate;
                    StockViewModel.DocNo = PurchaseGoodsReturnHeader.DocNo;
                    StockViewModel.DivisionId = PurchaseGoodsReturnHeader.DivisionId;
                    StockViewModel.SiteId = PurchaseGoodsReturnHeader.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = ProcessId;
                    StockViewModel.PersonId = PurchaseGoodsReturnHeader.SupplierId;
                    StockViewModel.ProductId = svm.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = PurchaseGoodsReturnHeader.GodownId;
                    StockViewModel.GodownId = PurchaseGoodsReturnHeader.GodownId;
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
                    StockViewModel.Remark = svm.Remark;
                    StockViewModel.Status = PurchaseGoodsReturnHeader.Status;
                    StockViewModel.CreatedBy = PurchaseGoodsReturnHeader.CreatedBy;
                    StockViewModel.CreatedDate = PurchaseGoodsReturnHeader.CreatedDate;
                    StockViewModel.ModifiedBy = User.Identity.Name;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        //PurchaseGoodsReturnHeader.Status = temp.Status;
                        PurchaseGoodsReturnHeader.ModifiedBy = User.Identity.Name;
                        PurchaseGoodsReturnHeader.ModifiedDate = DateTime.Now;
                        //new PurchaseInvoiceReturnHeaderService(_unitOfWork).Update(temp);
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceReturnHeader.Add(temp);

                    PurchaseGoodsReturnHeader.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(PurchaseGoodsReturnHeader);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new PurchaseInvoiceReturnLineChargeService(_unitOfWork).Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseInvoiceReturnLineCharge.Add(productcharge);
                            //new PurchaseInvoiceReturnLineChargeService(_unitOfWork).Update(productcharge);

                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;

                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseInvoiceReturnHeaderCharge.Add(footercharge);
                            //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Update(footercharge);
                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseInvoiceReturnDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(line.PurchaseInvoiceReturnHeaderId, line.PurchaseInvoiceReturnLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseInvoiceReturnDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(line.PurchaseInvoiceReturnHeaderId, line.PurchaseInvoiceReturnLineId, EventModeConstants.Edit), ref db);
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
                        DocId = temp.PurchaseInvoiceReturnHeaderId,
                        DocLineId = line.PurchaseInvoiceReturnLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
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
            PurchaseInvoiceReturnLineViewModel temp = _PurchaseInvoiceReturnLineService.GetPurchaseInvoiceReturnLine(id);

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

            PurchaseInvoiceReturnHeader H = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceReturnHeaderId);
            PrepareViewBag(temp);
            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
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
            PurchaseInvoiceReturnLineViewModel temp = _PurchaseInvoiceReturnLineService.GetPurchaseInvoiceReturnLine(id);

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

            PurchaseInvoiceReturnHeader H = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            
            return PartialView("_Create", temp);
        }

        public ActionResult _Detail(int id)
        {
            PurchaseInvoiceReturnLineViewModel temp = _PurchaseInvoiceReturnLineService.GetPurchaseInvoiceReturnLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PrepareViewBag(temp);

            PurchaseInvoiceReturnHeader H = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceReturnHeaderId);

            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            
            return PartialView("_Create", temp);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseInvoiceReturnLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseInvoiceReturnDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceReturnHeaderId, vm.PurchaseInvoiceReturnLineId), ref db);
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

                int? StockId = 0;

                PurchaseInvoiceReturnLine PurchaseInvoiceReturnLine = db.PurchaseInvoiceReturnLine.Find(vm.PurchaseInvoiceReturnLineId);

                try
                {
                    PurchaseInvoiceReturnDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseInvoiceReturnLine.PurchaseInvoiceReturnHeaderId, PurchaseInvoiceReturnLine.PurchaseInvoiceReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseInvoiceReturnLine>(PurchaseInvoiceReturnLine),
                });

                PurchaseGoodsReturnLine Gline = db.PurchaseGoodsReturnLine.Find(PurchaseInvoiceReturnLine.PurchaseGoodsReturnLineId ?? 0);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseGoodsReturnLine>(Gline),
                });

                StockId = Gline.StockId;

                var chargeslist = (from p in db.PurchaseInvoiceReturnLineCharge
                                   where p.LineTableId == vm.PurchaseInvoiceReturnLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseInvoiceReturnLineCharge.Remove(item);
                        //new PurchaseInvoiceReturnLineChargeService(_unitOfWork).Delete(item.Id);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseInvoiceReturnHeaderCharge.Add(footer);
                        //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Update(footer);
                    }

                PurchaseInvoiceReturnLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseInvoiceReturnLine.Remove(PurchaseInvoiceReturnLine);

                //_PurchaseInvoiceReturnLineService.Delete(PurchaseInvoiceReturnLine.PurchaseInvoiceReturnLineId);
                PurchaseInvoiceReturnHeader header = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(PurchaseInvoiceReturnLine.PurchaseInvoiceReturnHeaderId);
                PurchaseGoodsReturnHeader Gheader = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Gline.PurchaseGoodsReturnHeaderId);
                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;

                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceReturnHeader.Add(header);
                    //new PurchaseInvoiceReturnHeaderService(_unitOfWork).Update(header);L

                    //Gheader.Status = (int)StatusConstants.Modified;
                    Gheader.ModifiedBy = User.Identity.Name;
                    Gheader.ModifiedDate = DateTime.Now;
                    db.PurchaseGoodsReturnHeader.Add(Gheader);
                    //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(Gheader);
                }

                //new PurchaseGoodsReturnLineService(_unitOfWork).Delete(Gline);
                Gline.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReturnLine.Remove(Gline);

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
                    PurchaseInvoiceReturnDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseInvoiceReturnLine.PurchaseInvoiceReturnHeaderId, PurchaseInvoiceReturnLine.PurchaseInvoiceReturnLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseInvoiceReturnHeaderId,
                    DocLineId = PurchaseInvoiceReturnLine.PurchaseInvoiceReturnLineId,
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

        public JsonResult GetPendingInvoices(int ProductId, int PurchaseInvoiceReturnHeaderId, string term, int Limit)
        {
            return Json(new PurchaseInvoiceHeaderService(_unitOfWork).GetPendingInvoices(ProductId, PurchaseInvoiceReturnHeaderId, term, Limit).ToList());
        }

        public JsonResult GetInvoiceDetail(int InvoiceLineId)
        {
            return Json(new PurchaseInvoiceLineService(_unitOfWork).GetPurchaseInvoiceLineBalance(InvoiceLineId));
        }

        public JsonResult GetPurchaseInvoices(int id, string term)//Receipt Header ID
        {
            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseInvoiceReturnLineService.GetPendingPurchaseInvoiceHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPurchaseGoodsReceipts(int id, string term)//Receipt Header ID
        {
            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseInvoiceReturnLineService.GetPendingPurchaseReceiptHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_PurchaseInvoiceReturnLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }


    }
}
