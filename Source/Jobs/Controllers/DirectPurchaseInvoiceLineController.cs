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
using PurchaseInvoiceReceiveDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class DirectPurchaseInvoiceLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseInvoiceLineService _PurchaseInvoiceLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public DirectPurchaseInvoiceLineController(IPurchaseInvoiceLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseInvoiceLineService = SaleOrder;
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
            var p = _PurchaseInvoiceLineService.GetLineListForIndexDirectPurchaseInvoice(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _ForIndent(int id, int sid)
        {
            PurchaseInvoiceLineFilterViewModel vm = new PurchaseInvoiceLineFilterViewModel();

            PurchaseInvoiceHeader Header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(id);

            PurchaseInvoiceSetting Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(Settings);

            vm.PurchaseInvoiceHeaderId = id;
            vm.SupplierId = sid;
            PrepareViewBag(null);
            return PartialView("_IndentFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostIndents(PurchaseInvoiceLineFilterViewModel vm)
        {

            if (vm.PurchaseInvoiceSettings.isMandatoryRate && (vm.Rate == null || vm.Rate == 0))
            {
                ModelState.AddModelError("", "Rate is mandatory");
                PrepareViewBag(null);
                return PartialView("_IndentFilters", vm);
            }


            List<PurchaseInvoiceLineViewModel> temp = _PurchaseInvoiceLineService.GetPurchaseIndentForFilters(vm).ToList();
            PurchaseInvoiceMasterDetailModel svm = new PurchaseInvoiceMasterDetailModel();
            svm.PurchaseInvoiceLineViewModel = temp;

            bool UnitConvetsionException = (from p in temp
                                            where p.UnitConversionException == true
                                            select p).Any();


            if (UnitConvetsionException)
            {
                ViewBag.UnitConversionException = UnitConvetsionException;

                string Products = string.Join(",", from p in temp
                                                   where p.UnitConversionException == true
                                                   select p.ProductName);


                ModelState.AddModelError("", "Unit Conversion are missing for few Products -" + Products);
            }

            PurchaseInvoiceHeader Header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(vm.PurchaseInvoiceHeaderId);

            PurchaseInvoiceSetting Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            svm.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(Settings);


            return PartialView("_IndentResults", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseInvoiceMasterDetailModel vm)
        {
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int ReceiptLinepk = 0;
            int StockPk = 0;
            int Cnt = 0;
            int Serial = _PurchaseInvoiceLineService.GetMaxSr(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId);
            bool HeaderChargeEdit = false;

            PurchaseInvoiceHeader Header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId);

            PurchaseInvoiceSetting Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new PurchaseInvoiceLineChargeService(_unitOfWork).GetMaxProductCharge(Header.PurchaseInvoiceHeaderId, "Web.PurchaseInvoiceLines", "PurchaseInvoiceHeaderId", "PurchaseInvoiceLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId), ref db);
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
                PurchaseGoodsReceiptHeader ReceiptHeader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(Header.PurchaseGoodsReceiptHeaderId.Value);


                var IndentLineIds = vm.PurchaseInvoiceLineViewModel.Select(m => m.PurchaseIndentLineId).ToArray();

                var IndentBalRecords = (from p in db.ViewPurchaseIndentBalance
                                        where IndentLineIds.Contains(p.PurchaseIndentLineId)
                                        select new
                                        {
                                            LineId = p.PurchaseIndentLineId,
                                            BalQty = p.BalanceQty,
                                        }).ToList();

                foreach (var item in vm.PurchaseInvoiceLineViewModel)
                {

                    decimal balqty = IndentBalRecords.Where(m => m.LineId == item.PurchaseIndentLineId).FirstOrDefault().BalQty;

                    if (item.Qty > 0 && (Settings.isMandatoryRate == true ? item.Rate > 0 : 1 == 1) && item.Qty <= balqty)
                    {

                        PurchaseGoodsReceiptLine ReceiptLine = new PurchaseGoodsReceiptLine();
                        var PurchaseIndentLine = new PurchaseIndentLineService(_unitOfWork).Find(item.PurchaseIndentLineId.Value);


                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = ReceiptHeader.StockHeaderId ?? 0;
                        }
                        else
                        {
                            if (ReceiptHeader.StockHeaderId != null && ReceiptHeader.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)ReceiptHeader.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = ReceiptHeader.PurchaseGoodsReceiptHeaderId;
                        StockViewModel.DocLineId = ReceiptLine.PurchaseGoodsReceiptLineId;
                        StockViewModel.DocTypeId = ReceiptHeader.DocTypeId;
                        StockViewModel.StockHeaderDocDate = ReceiptHeader.DocDate;
                        StockViewModel.StockDocDate = ReceiptHeader.DocDate;
                        StockViewModel.DocNo = ReceiptHeader.DocNo;
                        StockViewModel.DivisionId = ReceiptHeader.DivisionId;
                        StockViewModel.SiteId = ReceiptHeader.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = ReceiptHeader.SupplierId;
                        StockViewModel.ProductId = PurchaseIndentLine.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = ReceiptHeader.GodownId;
                        StockViewModel.HeaderProcessId = ProcessId;
                        StockViewModel.GodownId = ReceiptHeader.GodownId;
                        StockViewModel.Remark = ReceiptHeader.Remark;
                        StockViewModel.Status = ReceiptHeader.Status;
                        StockViewModel.ProcessId = ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = null;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = PurchaseIndentLine.Qty;
                        StockViewModel.Rate = item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = PurchaseIndentLine.Specification;
                        StockViewModel.Dimension1Id = PurchaseIndentLine.Dimension1Id;
                        StockViewModel.Dimension2Id = PurchaseIndentLine.Dimension2Id;
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
                            return PartialView("_IndentResults", vm);
                        }


                        if (Cnt == 0)
                        {
                            ReceiptHeader.StockHeaderId = StockViewModel.StockHeaderId;
                        }


                        ReceiptLine.StockId = StockViewModel.StockId;
                        ReceiptLine.PurchaseGoodsReceiptHeaderId = ReceiptHeader.PurchaseGoodsReceiptHeaderId;
                        ReceiptLine.PurchaseIndentLineId = item.PurchaseIndentLineId;
                        ReceiptLine.ProductId = PurchaseIndentLine.ProductId;
                        ReceiptLine.Dimension1Id = PurchaseIndentLine.Dimension1Id;
                        ReceiptLine.Dimension2Id = PurchaseIndentLine.Dimension2Id;
                        ReceiptLine.Specification = PurchaseIndentLine.Specification;
                        ReceiptLine.Qty = item.Qty;
                        ReceiptLine.DealQty = item.Qty * item.UnitConversionMultiplier;
                        ReceiptLine.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        ReceiptLine.LotNo = item.LotNo;
                        ReceiptLine.DealUnitId = item.DealUnitId;
                        ReceiptLine.DocQty = item.Qty;
                        ReceiptLine.Sr = Serial;
                        ReceiptLine.CreatedDate = DateTime.Now;
                        ReceiptLine.ModifiedDate = DateTime.Now;
                        ReceiptLine.CreatedBy = User.Identity.Name;
                        ReceiptLine.ModifiedBy = User.Identity.Name;
                        ReceiptLine.PurchaseGoodsReceiptLineId = ReceiptLinepk;
                        ReceiptLine.ObjectState = Model.ObjectState.Added;
                        db.PurchaseGoodsReceiptLine.Add(ReceiptLine);
                        //new PurchaseGoodsReceiptLineService(_unitOfWork).Create(ReceiptLine);

                        Cnt = Cnt + 1;










                        PurchaseInvoiceLine line = new PurchaseInvoiceLine();
                        line.PurchaseInvoiceHeaderId = item.PurchaseInvoiceHeaderId;
                        line.PurchaseGoodsReceiptLineId = ReceiptLine.PurchaseGoodsReceiptLineId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Rate = item.Rate;
                        line.DiscountPer = item.DiscountPer;
                        line.DealUnitId = item.DealUnitId;
                        line.DealQty = item.Qty * item.UnitConversionMultiplier;
                        line.Amount = (line.DealQty * item.Rate);
                        line.CreatedDate = DateTime.Now;
                        line.Sr = Serial++;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.PurchaseInvoiceLineId = pk;
                        line.ObjectState = Model.ObjectState.Added;
                        //_PurchaseInvoiceLineService.Create(line);
                        db.PurchaseInvoiceLine.Add(line);


                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseInvoiceLineId, HeaderTableId = item.PurchaseInvoiceHeaderId, PersonID = Header.SupplierId });

                        pk++;
                        ReceiptLinepk++;
                        StockPk++;
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;

                    ReceiptHeader.Status = Header.Status;
                    ReceiptHeader.ModifiedBy = User.Identity.Name;
                    ReceiptHeader.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseInvoiceHeader.Add(Header);

                ReceiptHeader.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReceiptHeader.Add(ReceiptHeader);

                //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseInvoiceHeaderCharges", "Web.PurchaseInvoiceLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    PurchaseInvoiceLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseInvoiceLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.PurchaseInvoiceLineCharge.Add(PoLineCharge);
                    //new PurchaseInvoiceLineChargeService(_unitOfWork).Create(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        PurchaseInvoiceHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseInvoiceHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId;
                        POHeaderCharge.PersonID = Header.SupplierId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.PurchaseInvoiceHeaderCharge.Add(POHeaderCharge);
                        //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new PurchaseInvoiceHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseInvoiceHeaderCharge.Add(footercharge);
                        //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);
                    }

                }

                try
                {
                    PurchaseInvoiceReceiveDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId), ref db);
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
                    PurchaseInvoiceReceiveDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(PurchaseInvoiceLineViewModel vm)
        {
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id, int sid, bool IsIndentBased)
        {
            return _Create(id, sid, IsIndentBased);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, int sid, bool IsIndentBased)
        {
            return _Create(id, sid, IsIndentBased);
        }

        public ActionResult _Create(int Id, int sid, bool IsIndentBased) //Id ==>Sale Order Header Id
        {
            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);
            PurchaseInvoiceLineViewModel s = new PurchaseInvoiceLineViewModel();

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            s.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId;
            s.PurchaseInvoiceHeaderDocNo = H.DocNo;
            s.SupplierId = sid;
            s.DocTypeId = H.DocTypeId;
            s.DivisionId = H.DivisionId;
            s.SiteId = H.SiteId;
            s.CalculateDiscountOnRate = H.CalculateDiscountOnRate;
            ViewBag.LineMode = "Create";
            PrepareViewBag(null);

            if (IsIndentBased == true)
            {
                return PartialView("_CreateForIndent", s);

            }
            else
            {
                return PartialView("_Create", s);
            }

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseInvoiceLineViewModel svm)
        {
            PurchaseInvoiceLine s = Mapper.Map<PurchaseInvoiceLineViewModel, PurchaseInvoiceLine>(svm);
            PurchaseInvoiceHeader temp = new PurchaseInvoiceHeaderService(_unitOfWork).Find(s.PurchaseInvoiceHeaderId);



            PurchaseGoodsReceiptLine ReceiptLine = Mapper.Map<PurchaseInvoiceLineViewModel, PurchaseGoodsReceiptLine>(svm);

            PurchaseGoodsReceiptHeader ReceiptHeader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReceiptHeaderId.Value);


            if (svm.PurchaseInvoiceLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.PurchInvoiceSettings != null)
            {
                if (svm.PurchInvoiceSettings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
            }

            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "The Qty field is required");
            }

            bool BeforeSave = true;

            try
            {

                if (svm.PurchaseInvoiceHeaderId <= 0)
                    BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseInvoiceHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseInvoiceHeaderId, EventModeConstants.Edit), ref db);

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
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;
                if (svm.PurchaseInvoiceLineId <= 0)
                {

                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = ReceiptHeader.StockHeaderId ?? 0;
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
                    StockViewModel.ProductId = ReceiptLine.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = ReceiptHeader.GodownId;
                    StockViewModel.GodownId = ReceiptHeader.GodownId;
                    StockViewModel.ProcessId = ProcessId;
                    StockViewModel.LotNo = svm.LotNo;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = 0;
                    StockViewModel.Qty_Rec = svm.Qty;
                    StockViewModel.Rate = s.Rate;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = svm.Dimension1Id;
                    StockViewModel.Dimension2Id = svm.Dimension2Id;
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


                    ReceiptLine.PurchaseGoodsReceiptHeaderId = ReceiptHeader.PurchaseGoodsReceiptHeaderId;
                    ReceiptLine.StockId = StockViewModel.StockId;
                    ReceiptLine.ProductId = svm.ProductId;
                    ReceiptLine.Dimension1Id = svm.Dimension1Id;
                    ReceiptLine.Dimension2Id = svm.Dimension2Id;
                    ReceiptLine.Qty = svm.Qty;
                    ReceiptLine.DealUnitId = svm.DealUnitId;
                    ReceiptLine.Sr = _PurchaseInvoiceLineService.GetMaxSr(s.PurchaseInvoiceHeaderId);
                    ReceiptLine.DealQty = svm.DealQty;
                    ReceiptLine.Specification = svm.Specification;
                    ReceiptLine.LotNo = svm.LotNo;
                    ReceiptLine.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    ReceiptLine.DocQty = svm.Qty;
                    ReceiptLine.PurchaseIndentLineId = svm.PurchaseIndentLineId;
                    ReceiptLine.CreatedBy = User.Identity.Name;
                    ReceiptLine.CreatedDate = DateTime.Now;
                    ReceiptLine.ModifiedBy = User.Identity.Name;
                    ReceiptLine.ModifiedDate = DateTime.Now;
                    ReceiptLine.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReceiptLine.Add(ReceiptLine);
                    //new PurchaseGoodsReceiptLineService(_unitOfWork).Create(ReceiptLine);


                    s.PurchaseGoodsReceiptLineId = ReceiptLine.PurchaseGoodsReceiptLineId;
                    s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.Sr = ReceiptLine.Sr;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseInvoiceLine.Add(s);
                    //_PurchaseInvoiceLineService.Create(s);

                    if (ReceiptHeader.StockHeaderId == null)
                    {
                        ReceiptHeader.StockHeaderId = StockViewModel.StockHeaderId;
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);
                    }


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.PurchaseInvoiceLineId;
                            item.PersonID = temp.SupplierId;
                            item.HeaderTableId = s.PurchaseInvoiceHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.PurchaseInvoiceLineCharge.Add(item);
                            //new PurchaseInvoiceLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new PurchaseInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.PurchaseInvoiceHeaderCharge.Add(footercharge);
                                //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.PurchaseInvoiceHeaderId;
                                item.PersonID = temp.SupplierId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.PurchaseInvoiceHeaderCharge.Add(item);
                                //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }



                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                        //new PurchaseInvoiceHeaderService(_unitOfWork).Update(temp);
                        ReceiptHeader.Status = (int)StatusConstants.Modified;
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);
                        ReceiptHeader.ModifiedDate = DateTime.Now;
                        ReceiptHeader.ModifiedBy = User.Identity.Name;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(temp);


                    ReceiptHeader.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptHeader.Add(ReceiptHeader);

                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseInvoiceHeaderId, s.PurchaseInvoiceLineId, EventModeConstants.Add), ref db);
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
                        PurchaseInvoiceReceiveDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseInvoiceHeaderId, s.PurchaseInvoiceLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseInvoiceHeaderId,
                        DocLineId = s.PurchaseInvoiceLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.PurchaseInvoiceHeaderId, sid = svm.SupplierId, IsIndentBased = (svm.PurchaseIndentLineId.HasValue && svm.PurchaseIndentLineId.Value > 0 ? true : false) });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseGoodsReceiptHeader Recheader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(ReceiptHeader.PurchaseGoodsReceiptHeaderId);
                    PurchaseGoodsReceiptLine Rectemp1 = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(svm.PurchaseGoodsReceiptLineId);

                    PurchaseGoodsReceiptLine ExRec = new PurchaseGoodsReceiptLine();
                    ExRec = Mapper.Map<PurchaseGoodsReceiptLine>(Rectemp1);

                    StockViewModel StockViewModel = new StockViewModel();
                    StockViewModel.StockHeaderId = ReceiptHeader.StockHeaderId ?? 0;
                    StockViewModel.StockId = Rectemp1.StockId ?? 0;
                    StockViewModel.DocHeaderId = ReceiptHeader.PurchaseGoodsReceiptHeaderId;
                    StockViewModel.DocLineId = svm.PurchaseGoodsReceiptLineId;
                    StockViewModel.DocTypeId = ReceiptHeader.DocTypeId;
                    StockViewModel.StockHeaderDocDate = ReceiptHeader.DocDate;
                    StockViewModel.StockDocDate = ReceiptHeader.DocDate;
                    StockViewModel.DocNo = ReceiptHeader.DocNo;
                    StockViewModel.DivisionId = ReceiptHeader.DivisionId;
                    StockViewModel.SiteId = ReceiptHeader.SiteId;
                    StockViewModel.CurrencyId = null;
                    StockViewModel.HeaderProcessId = null;
                    StockViewModel.PersonId = ReceiptHeader.SupplierId;
                    StockViewModel.ProductId = svm.ProductId;
                    StockViewModel.HeaderFromGodownId = null;
                    StockViewModel.HeaderGodownId = null;
                    StockViewModel.GodownId = ReceiptHeader.GodownId;
                    StockViewModel.ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;
                    StockViewModel.LotNo = svm.LotNo;
                    StockViewModel.CostCenterId = null;
                    StockViewModel.Qty_Iss = 0;
                    StockViewModel.Qty_Rec = svm.Qty;
                    StockViewModel.Rate = svm.Rate;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = svm.Specification;
                    StockViewModel.Dimension1Id = svm.Dimension1Id;
                    StockViewModel.Dimension2Id = svm.Dimension2Id;
                    StockViewModel.Remark = svm.Remark;
                    StockViewModel.Status = ReceiptHeader.Status;
                    StockViewModel.CreatedBy = Rectemp1.CreatedBy;
                    StockViewModel.CreatedDate = Rectemp1.CreatedDate;
                    StockViewModel.ModifiedBy = User.Identity.Name;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }



                    //Rectemp1.ProductUidId = svm.ProductUidId;
                    Rectemp1.ProductId = svm.ProductId;
                    Rectemp1.Specification = svm.Specification;
                    //Rectemp1.PurchaseOrderLineId = svm.PurchaseOrderLineId;
                    Rectemp1.Dimension1Id = svm.Dimension1Id;
                    Rectemp1.Dimension2Id = svm.Dimension2Id;
                    Rectemp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    //Rectemp1.PurchaseIndentLineId = svm.PurchaseIndentLineId;
                    Rectemp1.LotNo = svm.LotNo;
                    //Rectemp1.DebitNoteAmount = svm.DebitNoteAmount;
                    //Rectemp1.DebitNoteReason = svm.DebitNoteReason;
                    Rectemp1.Qty = svm.Qty;
                    Rectemp1.DocQty = svm.Qty;
                    Rectemp1.DealQty = svm.DealQty;
                    Rectemp1.DealUnitId = svm.DealUnitId;
                    Rectemp1.Remark = svm.Remark;
                    Rectemp1.ModifiedDate = DateTime.Now;
                    Rectemp1.ModifiedBy = User.Identity.Name;
                    Rectemp1.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptLine.Add(Rectemp1);
                    //new PurchaseGoodsReceiptLineService(_unitOfWork).Update(Rectemp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = Rectemp1,
                    });



                    PurchaseInvoiceHeader header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(svm.PurchaseInvoiceHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    PurchaseInvoiceLine temp1 = _PurchaseInvoiceLineService.Find(svm.PurchaseInvoiceLineId);

                    PurchaseInvoiceLine ExRecI = new PurchaseInvoiceLine();
                    ExRecI = Mapper.Map<PurchaseInvoiceLine>(temp1);

                    temp1.DiscountPer = svm.DiscountPer;
                    temp1.Amount = svm.Amount;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.DealQty = svm.DealQty;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.Rate = svm.Rate;
                    temp1.Remark = svm.Remark;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceLine.Add(temp1);
                    //_PurchaseInvoiceLineService.Update(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecI,
                        Obj = temp1,
                    });

                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;

                        //new PurchaseInvoiceHeaderService(_unitOfWork).Update(header);

                        Recheader.Status = (int)StatusConstants.Modified;
                        Recheader.ModifiedBy = User.Identity.Name;
                        Recheader.ModifiedDate = DateTime.Now;
                        //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(Recheader);
                    }

                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(header);

                    Recheader.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptHeader.Add(Recheader);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = db.PurchaseInvoiceLineCharge.Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseInvoiceLineCharge.Add(productcharge);
                            //new PurchaseInvoiceLineChargeService(_unitOfWork).Update(productcharge);

                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = db.PurchaseInvoiceHeaderCharge.Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseInvoiceHeaderCharge.Add(footercharge);
                            //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Update(footercharge);

                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseInvoiceReceiveDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(temp1.PurchaseInvoiceHeaderId, temp1.PurchaseInvoiceLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseInvoiceReceiveDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(temp1.PurchaseInvoiceHeaderId, temp1.PurchaseInvoiceLineId, EventModeConstants.Edit), ref db);
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
                        DocId = temp1.PurchaseInvoiceHeaderId,
                        DocLineId = temp1.PurchaseInvoiceLineId,
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
            PurchaseInvoiceLineViewModel temp = _PurchaseInvoiceLineService.GetPurchaseInvoiceLine(id);
            bool IndentBased = false;
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

            if (temp.PurchaseIndentLineId.HasValue)
            {

                var Indent = (from p in db.ViewPurchaseIndentBalance
                              where p.PurchaseIndentLineId == temp.PurchaseIndentLineId
                              select new { p.PurchaseIndentLineId, p.PurchaseIndentNo, p.BalanceQty }).FirstOrDefault();
                temp.PurchaseIndentHeaderDocNo = Indent.PurchaseIndentNo;
                temp.PurchaseIndentLineId = Indent.PurchaseIndentLineId;
                temp.IndentBalQty = Indent.BalanceQty;
                IndentBased = true;
            }

            temp.Qty = (temp.AdjShortQty) - temp.ShortQty + temp.DocQty;

            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.CalculateDiscountOnRate = H.CalculateDiscountOnRate;
            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            if (IndentBased == true)
            {
                return PartialView("_CreateForIndent", temp);
            }
            else
            {
                return PartialView("_Create", temp);
            }
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
            PurchaseInvoiceLineViewModel temp = _PurchaseInvoiceLineService.GetPurchaseInvoiceLine(id);

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

            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            return PartialView("_Create", temp);
        }


        public ActionResult _Detail(int id)
        {
            PurchaseInvoiceLineViewModel temp = _PurchaseInvoiceLineService.GetPurchaseInvoiceLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            return PartialView("_Create", temp);
        }

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(PurchaseInvoiceLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseInvoiceReceiveDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceHeaderId, vm.PurchaseInvoiceLineId), ref db);
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

                PurchaseInvoiceLine PurchaseInvoiceLine = db.PurchaseInvoiceLine.Find(vm.PurchaseInvoiceLineId);

                try
                {
                    PurchaseInvoiceReceiveDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseInvoiceLine.PurchaseInvoiceHeaderId, PurchaseInvoiceLine.PurchaseInvoiceLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseInvoiceLine>(PurchaseInvoiceLine),
                });

                //_PurchaseInvoiceLineService.Delete(PurchaseInvoiceLine);
                PurchaseInvoiceHeader header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(PurchaseInvoiceLine.PurchaseInvoiceHeaderId);

                var chargeslist = (from p in db.PurchaseInvoiceLineCharge
                                   where p.LineTableId == PurchaseInvoiceLine.PurchaseInvoiceLineId
                                   select p).ToList();


                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseInvoiceLineCharge.Remove(item);
                        //new PurchaseInvoiceLineChargeService(_unitOfWork).Delete(item.Id);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = new PurchaseInvoiceHeaderChargeService(_unitOfWork).Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseInvoiceHeaderCharge.Add(footer);
                        //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Update(footer);
                    }

                PurchaseInvoiceLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseInvoiceLine.Remove(PurchaseInvoiceLine);

                int? StockId = 0;

                PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine = db.PurchaseGoodsReceiptLine.Find(vm.PurchaseGoodsReceiptLineId);

                StockId = PurchaseGoodsReceiptLine.StockId;

                //new PurchaseGoodsReceiptLineService(_unitOfWork).Delete(PurchaseGoodsReceiptLine);

                PurchaseGoodsReceiptLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReceiptLine.Remove(PurchaseGoodsReceiptLine);

                PurchaseGoodsReceiptHeader Receiptheader = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(PurchaseGoodsReceiptLine.PurchaseGoodsReceiptHeaderId);
                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;

                    //new PurchaseInvoiceHeaderService(_unitOfWork).Update(header);

                    Receiptheader.Status = (int)StatusConstants.Modified;
                    Receiptheader.ModifiedDate = DateTime.Now;
                    Receiptheader.ModifiedBy = User.Identity.Name;

                    //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(Receiptheader);
                }
                header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseInvoiceHeader.Add(header);

                Receiptheader.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReceiptHeader.Add(Receiptheader);

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
                    return PartialView("_Create", vm);
                }

                try
                {
                    PurchaseInvoiceReceiveDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseInvoiceLine.PurchaseInvoiceHeaderId, PurchaseInvoiceLine.PurchaseInvoiceLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseInvoiceHeaderId,
                    DocLineId = PurchaseInvoiceLine.PurchaseInvoiceLineId,
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

        public JsonResult GetProductDetailJson(int ProductId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            ProductViewModel ProductJson = new ProductViewModel();

            ProductJson.ProductId = product.ProductId;
            ProductJson.StandardCost = product.StandardCost;
            ProductJson.UnitId = product.UnitId;
            ProductJson.ProductSpecification = product.ProductSpecification;
            ProductJson.UnitName = product.UnitName;


            return Json(ProductJson);
        }

        public JsonResult GetPendingReceipts(int ProductId, int PurchaseInvoiceHeaderId)
        {
            return Json(new PurchaseGoodsReceiptHeaderService(_unitOfWork).GetPendingReceiptsForInvoice(ProductId, PurchaseInvoiceHeaderId).ToList());
        }

        public JsonResult GetPendingIndents(int ProductId, int PurchaseInvoiceHeaderId)
        {
            return Json(new PurchaseGoodsReceiptHeaderService(_unitOfWork).GetPendingIndentsForInvoice(ProductId, PurchaseInvoiceHeaderId).ToList());
        }

        public JsonResult GetReceiptDetail(int ReceiptId)
        {
            return Json(new PurchaseGoodsReceiptLineService(_unitOfWork).GetPurchaseGoodsReceiptDetailBalance(ReceiptId));
        }

        public JsonResult GetIndentDetail(int IndentId)
        {
            return Json(new PurchaseGoodsReceiptLineService(_unitOfWork).GetPurchaseIndentDetailBalance(IndentId));
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string DealUnitId, int PurchaseInvoiceHeaderId)
        {

            PurchaseInvoiceHeader Invoice = new PurchaseInvoiceHeaderService(_unitOfWork).Find(PurchaseInvoiceHeaderId);

            if (Invoice.UnitConversionForId.HasValue && Invoice.UnitConversionForId > 0)
            {
                UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversionForUCF(productid, unitid, DealUnitId, Invoice.UnitConversionForId ?? 0);
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


                return Json(unitconversionjson);
            }
            else
            {
                UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, unitid, DealUnitId);
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


                return Json(unitconversionjson);
            }

        }

        public JsonResult GetPurchaseReceipts(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseInvoiceLineService.GetPendingPurchaseReceiptHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPurchaseIndents(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseInvoiceLineService.GetPendingPurchaseIndentHelpList(id, term), JsonRequestBehavior.AllowGet);
        }




        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_PurchaseInvoiceLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }


    }
}
