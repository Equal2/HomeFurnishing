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
//using PurchaseQuotationReceiveDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseQuotationLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseQuotationLineService _PurchaseQuotationLineService;
        IPurchaseQuotationHeaderService _PurchaseQuotationHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseQuotationLineController(IPurchaseQuotationLineService SaleOrder, IPurchaseQuotationHeaderService QuotationHeader, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseQuotationLineService = SaleOrder;
            _PurchaseQuotationHeaderService = QuotationHeader;
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
            var p = _PurchaseQuotationLineService.GetPurchaseQuotationLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForIndent(int id, int sid)
        {
            PurchaseQuotationLineFilterViewModel vm = new PurchaseQuotationLineFilterViewModel();

            PurchaseQuotationHeader Header = _PurchaseQuotationHeaderService.Find(id);

            PurchaseQuotationSetting Settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            vm.PurchaseQuotationSettings = Mapper.Map<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>(Settings);

            vm.PurchaseQuotationHeaderId = id;
            vm.SupplierId = sid;
            PrepareViewBag(null);
            return PartialView("_IndentFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostIndents(PurchaseQuotationLineFilterViewModel vm)
        {

            if (vm.PurchaseQuotationSettings.isMandatoryRate && (vm.Rate == null || vm.Rate == 0))
            {
                ModelState.AddModelError("", "Rate is mandatory");
                PrepareViewBag(null);
                return PartialView("_IndentFilters", vm);
            }


            List<PurchaseQuotationLineViewModel> temp = _PurchaseQuotationLineService.GetPurchaseIndentForFilters(vm).ToList();
            PurchaseQuotationMasterDetailModel svm = new PurchaseQuotationMasterDetailModel();
            svm.PurchaseQuotationLineViewModel = temp;

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

            PurchaseQuotationHeader Header = _PurchaseQuotationHeaderService.Find(vm.PurchaseQuotationHeaderId);

            PurchaseQuotationSetting Settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            svm.PurchaseQuotationSettings = Mapper.Map<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>(Settings);


            return PartialView("_IndentResults", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseQuotationMasterDetailModel vm)
        {
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int ReceiptLinepk = 0;
            int StockPk = 0;
            int Cnt = 0;
            int Serial = _PurchaseQuotationLineService.GetMaxSr(vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId);
            bool HeaderChargeEdit = false;

            PurchaseQuotationHeader Header = _PurchaseQuotationHeaderService.Find(vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId);

            PurchaseQuotationSetting Settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new PurchaseQuotationLineChargeService(_unitOfWork).GetMaxProductCharge(Header.PurchaseQuotationHeaderId, "Web.PurchaseQuotationLines", "PurchaseQuotationHeaderId", "PurchaseQuotationLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            bool BeforeSave = true;

            //try
            //{
            //    BeforeSave = PurchaseQuotationReceiveDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;

                var IndentLineIds = vm.PurchaseQuotationLineViewModel.Select(m => m.PurchaseIndentLineId).ToArray();

                var IndentBalRecords = (from p in db.ViewPurchaseIndentBalance
                                        where IndentLineIds.Contains(p.PurchaseIndentLineId)
                                        select new
                                        {
                                            LineId = p.PurchaseIndentLineId,
                                            BalQty = p.BalanceQty,
                                        }).ToList();

                foreach (var item in vm.PurchaseQuotationLineViewModel)
                {

                    decimal balqty = IndentBalRecords.Where(m => m.LineId == item.PurchaseIndentLineId).FirstOrDefault().BalQty;

                    if (item.Qty > 0 && (Settings.isMandatoryRate == true ? item.Rate > 0 : 1 == 1) && item.Qty <= balqty)
                    {

                        PurchaseGoodsReceiptLine ReceiptLine = new PurchaseGoodsReceiptLine();
                        var PurchaseIndentLine = new PurchaseIndentLineService(_unitOfWork).Find(item.PurchaseIndentLineId.Value);


                        PurchaseQuotationLine line = new PurchaseQuotationLine();
                        line.PurchaseQuotationHeaderId = item.PurchaseQuotationHeaderId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Rate = item.Rate;
                        line.PurchaseIndentLineId = item.PurchaseIndentLineId;
                        line.DiscountPer = item.DiscountPer;
                        line.Qty = item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        line.DealQty = item.Qty * item.UnitConversionMultiplier;
                        line.Amount = (line.DealQty * item.Rate);
                        line.CreatedDate = DateTime.Now;
                        line.ProductId = PurchaseIndentLine.ProductId;
                        line.Dimension1Id = PurchaseIndentLine.Dimension1Id;
                        line.Dimension2Id = PurchaseIndentLine.Dimension2Id;
                        line.Specification = PurchaseIndentLine.Specification;
                        line.Sr = Serial++;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.PurchaseQuotationLineId = pk;
                        line.ObjectState = Model.ObjectState.Added;
                        //_PurchaseQuotationLineService.Create(line);
                        db.PurchaseQuotationLine.Add(line);


                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseQuotationLineId, HeaderTableId = item.PurchaseQuotationHeaderId, PersonID = Header.SupplierId });

                        pk++;
                        StockPk++;
                    }
                }

                if (Header.Status != (int)StatusConstants.Drafted)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseQuotationHeader.Add(Header);

                //new PurchaseGoodsReceiptHeaderService(_unitOfWork).Update(ReceiptHeader);

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseQuotationHeaderCharges", "Web.PurchaseQuotationLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {
                    PurchaseQuotationLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseQuotationLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.PurchaseQuotationLineCharge.Add(PoLineCharge);
                    //new PurchaseQuotationLineChargeService(_unitOfWork).Create(PoLineCharge);
                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        PurchaseQuotationHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseQuotationHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId;
                        POHeaderCharge.PersonID = Header.SupplierId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.PurchaseQuotationHeaderCharge.Add(POHeaderCharge);
                        //new PurchaseQuotationHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new PurchaseQuotationHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseQuotationHeaderCharge.Add(footercharge);
                        //new PurchaseQuotationHeaderChargeService(_unitOfWork).Update(footercharge);
                    }

                }

                //try
                //{
                //    PurchaseQuotationReceiveDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId), ref db);
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
                    return PartialView("_IndentResults", vm);
                }

                //try
                //{
                //    PurchaseQuotationReceiveDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseQuotationLineViewModel.FirstOrDefault().PurchaseQuotationHeaderId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseQuotationHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_IndentResults", vm);

        }



        private void PrepareViewBag(PurchaseQuotationLineViewModel vm)
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
            PurchaseQuotationHeader H = _PurchaseQuotationHeaderService.Find(Id);
            PurchaseQuotationLineViewModel s = new PurchaseQuotationLineViewModel();

            //Getting Settings
            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchQuotationSettings = Mapper.Map<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>(settings);

            s.PurchaseQuotationHeaderId = H.PurchaseQuotationHeaderId;
            s.PurchaseQuotationHeaderDocNo = H.DocNo;
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
        public ActionResult _CreatePost(PurchaseQuotationLineViewModel svm)
        {
            PurchaseQuotationLine s = Mapper.Map<PurchaseQuotationLineViewModel, PurchaseQuotationLine>(svm);
            PurchaseQuotationHeader temp = _PurchaseQuotationHeaderService.Find(s.PurchaseQuotationHeaderId);

            if (svm.PurchaseQuotationLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.PurchQuotationSettings != null)
            {
                if (svm.PurchQuotationSettings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
            }

            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "The Qty field is required");
            }

            bool BeforeSave = true;

            //try
            //{

            //    if (svm.PurchaseQuotationHeaderId <= 0)
            //        BeforeSave = PurchaseQuotationReceiveDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseQuotationHeaderId, EventModeConstants.Add), ref db);
            //    else
            //        BeforeSave = PurchaseQuotationReceiveDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseQuotationHeaderId, EventModeConstants.Edit), ref db);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXCL"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.FullFinishing).ProcessId;
                if (svm.PurchaseQuotationLineId <= 0)
                {

                    s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.Sr = _PurchaseQuotationLineService.GetMaxSr(s.PurchaseQuotationHeaderId);
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseQuotationLine.Add(s);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.PurchaseQuotationLineId;
                            item.PersonID = temp.SupplierId;
                            item.HeaderTableId = s.PurchaseQuotationHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.PurchaseQuotationLineCharge.Add(item);
                            //new PurchaseQuotationLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new PurchaseQuotationHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.PurchaseQuotationHeaderCharge.Add(footercharge);
                                //new PurchaseQuotationHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.PurchaseQuotationHeaderId;
                                item.PersonID = temp.SupplierId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.PurchaseQuotationHeaderCharge.Add(item);
                                //new PurchaseQuotationHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }



                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseQuotationHeader.Add(temp);

                    //try
                    //{
                    //    PurchaseQuotationReceiveDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseQuotationHeaderId, s.PurchaseQuotationLineId, EventModeConstants.Add), ref db);
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
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);
                    }
                    //try
                    //{
                    //    PurchaseQuotationReceiveDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseQuotationHeaderId, s.PurchaseQuotationLineId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXCL"] += message;
                    //}

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseQuotationHeaderId,
                        DocLineId = s.PurchaseQuotationLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = s.PurchaseQuotationHeaderId, sid = svm.SupplierId, IsIndentBased = (svm.PurchaseIndentLineId.HasValue && svm.PurchaseIndentLineId.Value > 0 ? true : false) });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseQuotationHeader header = _PurchaseQuotationHeaderService.Find(svm.PurchaseQuotationHeaderId);
                    int status = header.Status;
                    PurchaseQuotationLine temp1 = _PurchaseQuotationLineService.Find(svm.PurchaseQuotationLineId);

                    PurchaseQuotationLine ExRecI = new PurchaseQuotationLine();
                    ExRecI = Mapper.Map<PurchaseQuotationLine>(temp1);

                    temp1.DiscountPer = svm.DiscountPer;
                    temp1.Amount = svm.Amount;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.DealQty = svm.DealQty;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.Qty = svm.Qty;
                    temp1.Rate = svm.Rate;
                    temp1.Remark = svm.Remark;
                    temp1.Dimension1Id = svm.Dimension1Id;
                    temp1.Dimension2Id = svm.Dimension2Id;
                    temp1.Specification = svm.Specification;
                    temp1.LotNo = svm.LotNo;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseQuotationLine.Add(temp1);
                    //_PurchaseQuotationLineService.Update(temp1);

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

                        //new PurchaseQuotationHeaderService(_unitOfWork).Update(header);
                    }

                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseQuotationHeader.Add(header);

                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = db.PurchaseQuotationLineCharge.Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseQuotationLineCharge.Add(productcharge);
                            //new PurchaseQuotationLineChargeService(_unitOfWork).Update(productcharge);

                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = db.PurchaseQuotationHeaderCharge.Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseQuotationHeaderCharge.Add(footercharge);
                            //new PurchaseQuotationHeaderChargeService(_unitOfWork).Update(footercharge);

                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    //try
                    //{
                    //    PurchaseQuotationReceiveDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(temp1.PurchaseQuotationHeaderId, temp1.PurchaseQuotationLineId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(null);
                        return PartialView("_Create", svm);
                    }

                    //try
                    //{
                    //    PurchaseQuotationReceiveDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(temp1.PurchaseQuotationHeaderId, temp1.PurchaseQuotationLineId, EventModeConstants.Edit), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}


                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp1.PurchaseQuotationHeaderId,
                        DocLineId = temp1.PurchaseQuotationLineId,
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
            PurchaseQuotationLineViewModel temp = _PurchaseQuotationLineService.GetPurchaseQuotationLine(id);
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
                temp.PurchaseIndentDocNo = Indent.PurchaseIndentNo;
                temp.PurchaseIndentLineId = Indent.PurchaseIndentLineId;
                temp.IndentBalanceQty = Indent.BalanceQty;
                IndentBased = true;
            }

            //temp.Qty = (temp.AdjShortQty) - temp.ShortQty + temp.DocQty;

            PurchaseQuotationHeader H = _PurchaseQuotationHeaderService.Find(temp.PurchaseQuotationHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.CalculateDiscountOnRate = H.CalculateDiscountOnRate;
            temp.PurchQuotationSettings = Mapper.Map<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>(settings);

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
            PurchaseQuotationLineViewModel temp = _PurchaseQuotationLineService.GetPurchaseQuotationLine(id);
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
                ViewBag.LineMode = "Delete";

            if (temp.PurchaseIndentLineId.HasValue)
            {

                var Indent = (from p in db.ViewPurchaseIndentBalance
                              where p.PurchaseIndentLineId == temp.PurchaseIndentLineId
                              select new { p.PurchaseIndentLineId, p.PurchaseIndentNo, p.BalanceQty }).FirstOrDefault();
                temp.PurchaseIndentDocNo = Indent.PurchaseIndentNo;
                temp.PurchaseIndentLineId = Indent.PurchaseIndentLineId;
                temp.IndentBalanceQty = Indent.BalanceQty;
                IndentBased = true;
            }

            PurchaseQuotationHeader H = _PurchaseQuotationHeaderService.Find(temp.PurchaseQuotationHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchQuotationSettings = Mapper.Map<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>(settings);
            if (IndentBased == true)
            {
                return PartialView("_CreateForIndent", temp);
            }
            else
            {
                return PartialView("_Create", temp);
            }
        }


        public ActionResult _Detail(int id)
        {
            PurchaseQuotationLineViewModel temp = _PurchaseQuotationLineService.GetPurchaseQuotationLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PurchaseQuotationHeader H = _PurchaseQuotationHeaderService.Find(temp.PurchaseQuotationHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseQuotationSettingService(_unitOfWork).GetPurchaseQuotationSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchQuotationSettings = Mapper.Map<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>(settings);
            return PartialView("_Create", temp);
        }

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(PurchaseQuotationLineViewModel vm)
        {
            bool BeforeSave = true;
            //try
            //{
            //    BeforeSave = PurchaseQuotationReceiveDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseQuotationHeaderId, vm.PurchaseQuotationLineId), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                TempData["CSEXC"] += "Validation failed before delete.";

            if (BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseQuotationLine PurchaseQuotationLine = db.PurchaseQuotationLine.Find(vm.PurchaseQuotationLineId);

                //try
                //{
                //    PurchaseQuotationReceiveDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseQuotationLine.PurchaseQuotationHeaderId, PurchaseQuotationLine.PurchaseQuotationLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXCL"] += message;
                //    EventException = true;
                //}

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseQuotationLine>(PurchaseQuotationLine),
                });

                //_PurchaseQuotationLineService.Delete(PurchaseQuotationLine);
                PurchaseQuotationHeader header = _PurchaseQuotationHeaderService.Find(PurchaseQuotationLine.PurchaseQuotationHeaderId);

                var chargeslist = (from p in db.PurchaseQuotationLineCharge
                                   where p.LineTableId == PurchaseQuotationLine.PurchaseQuotationLineId
                                   select p).ToList();


                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseQuotationLineCharge.Remove(item);
                        //new PurchaseQuotationLineChargeService(_unitOfWork).Delete(item.Id);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = new PurchaseQuotationHeaderChargeService(_unitOfWork).Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseQuotationHeaderCharge.Add(footer);
                        //new PurchaseQuotationHeaderChargeService(_unitOfWork).Update(footer);
                    }

                PurchaseQuotationLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseQuotationLine.Remove(PurchaseQuotationLine);

                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                }
                header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseQuotationHeader.Add(header);

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

                //try
                //{
                //    PurchaseQuotationReceiveDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseQuotationLine.PurchaseQuotationHeaderId, PurchaseQuotationLine.PurchaseQuotationLineId), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseQuotationHeaderId,
                    DocLineId = PurchaseQuotationLine.PurchaseQuotationLineId,
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

        public JsonResult GetPendingIndents(int ProductId, int PurchaseQuotationHeaderId)
        {
            return Json(_PurchaseQuotationLineService.GetPendingIndentsForQuotation(ProductId, PurchaseQuotationHeaderId).ToList());
        }

        public JsonResult GetIndentDetail(int IndentId)
        {
            return Json(_PurchaseQuotationLineService.GetPurchaseIndentDetailBalance(IndentId));
        }

        public JsonResult getunitconversiondetailjson(int productid, string unitid, string DealUnitId, int PurchaseQuotationHeaderId)
        {

            PurchaseQuotationHeader Invoice = _PurchaseQuotationHeaderService.Find(PurchaseQuotationHeaderId);

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

        public JsonResult GetPurchaseIndents(int id, string term)//Invoice Header ID
        {
            return Json(_PurchaseQuotationLineService.GetPendingPurchaseIndentHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term, int Limit)//Indent Header ID
        {
            return Json(_PurchaseQuotationLineService.GetProductHelpListforAC(id, term, Limit), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductHelpList(string searchTerm, int pageSize, int pageNum, int filter)//Indent Header ID
        {
            var Query = _PurchaseQuotationLineService.GetProductHelpList(filter, searchTerm);

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
    }
}
