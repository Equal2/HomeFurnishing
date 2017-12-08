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
using PurchaseInvoiceDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseInvoiceLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseInvoiceLineService _PurchaseInvoiceLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PurchaseInvoiceLineController(IPurchaseInvoiceLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
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
            var p = _PurchaseInvoiceLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForReceipt(int id, int sid)
        {
            PurchaseInvoiceLineFilterViewModel vm = new PurchaseInvoiceLineFilterViewModel();
            vm.PurchaseInvoiceHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_Filters", vm);
        }

        public ActionResult _ForOrder(int id, int sid)
        {
            PurchaseInvoiceLineFilterViewModel vm = new PurchaseInvoiceLineFilterViewModel();
            vm.PurchaseInvoiceHeaderId = id;
            PurchaseInvoiceHeader Header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.SupplierId = sid;
            return PartialView("_OrderFilters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseInvoiceLineFilterViewModel vm)
        {
            List<PurchaseInvoiceLineViewModel> temp = _PurchaseInvoiceLineService.GetPurchaseReceiptForFilters(vm).ToList();
            PurchaseInvoiceMasterDetailModel svm = new PurchaseInvoiceMasterDetailModel();
            svm.PurchaseInvoiceLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPostOrders(PurchaseInvoiceLineFilterViewModel vm)
        {
            List<PurchaseInvoiceLineViewModel> temp = _PurchaseInvoiceLineService.GetPurchaseOrderForFilters(vm).ToList();
            PurchaseInvoiceMasterDetailModel svm = new PurchaseInvoiceMasterDetailModel();
            svm.PurchaseInvoiceLineViewModel = temp;
            return PartialView("_Results", svm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseInvoiceMasterDetailModel vm)
        {
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int Serial = _PurchaseInvoiceLineService.GetMaxSr(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId);
            bool HeaderChargeEdit = false;

            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();
            List<LineReferenceIds> RefIds = new List<LineReferenceIds>();

            PurchaseInvoiceHeader Header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId);

            PurchaseInvoiceSetting Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new PurchaseInvoiceLineChargeService(_unitOfWork).GetMaxProductCharge(Header.PurchaseInvoiceHeaderId, "Web.PurchaseInvoiceLines", "PurchaseInvoiceHeaderId", "PurchaseInvoiceLineId");

            int PersonCount = 0;
            int CalculationId = Settings.CalculationId;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            var RecptLineIds = vm.PurchaseInvoiceLineViewModel.Select(m => m.PurchaseGoodsReceiptLineId).ToArray();

            var RecptRecords = (from p in db.ViewPurchaseGoodsReceiptBalance
                                where RecptLineIds.Contains(p.PurchaseGoodsReceiptLineId)
                                select new
                                {
                                    LineId = p.PurchaseGoodsReceiptLineId,
                                    BalQty = p.BalanceQty,
                                }).ToList();

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseInvoiceDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId), ref db);
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
                foreach (var item in vm.PurchaseInvoiceLineViewModel)
                {
                    decimal balqty = RecptRecords.Where(m => m.LineId == item.PurchaseGoodsReceiptLineId).FirstOrDefault().BalQty;
                    if (item.DealQty > 0 && (Settings.isMandatoryRate == true ? item.Rate > 0 : 1 == 1) && item.Qty <= balqty)
                    {
                        PurchaseInvoiceLine line = new PurchaseInvoiceLine();
                        line.PurchaseInvoiceHeaderId = item.PurchaseInvoiceHeaderId;
                        line.PurchaseGoodsReceiptLineId = item.PurchaseGoodsReceiptLineId;
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Rate = item.Rate;
                        line.DealUnitId = item.DealUnitId;
                        line.DocQty = item.DocQty;
                        line.DealQty = item.DealQty;
                        line.DiscountPer = item.DiscountPer;
                        //line.Amount = (item.DealQty * item.RateAfterDiscount);
                        if (Settings.CalculateDiscountOnRate)
                        {
                            var temprate = item.Rate - (item.Rate * item.DiscountPer / 100);
                            line.Amount = line.DealQty * temprate ?? 0;
                        }
                        else
                        {
                            var DiscountAmt = (item.Rate * line.DealQty) * item.DiscountPer / 100;
                            line.Amount = (item.Rate * line.DealQty) - (DiscountAmt ?? 0);
                        }
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.Sr = Serial++;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.PurchaseInvoiceLineId = pk;
                        line.ObjectState = Model.ObjectState.Added;
                        db.PurchaseInvoiceLine.Add(line);
                        //_PurchaseInvoiceLineService.Create(line);

                        LineStatus.Add(line.PurchaseGoodsReceiptLineId, line.DocQty);


                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseInvoiceLineId, HeaderTableId = item.PurchaseInvoiceHeaderId, PersonID = Header.SupplierId, DealQty = line.DealQty });
                        RefIds.Add(new LineReferenceIds { LineId = line.PurchaseInvoiceLineId, RefLineId = line.PurchaseGoodsReceiptLineId });
                        pk++;
                    }
                }

                int[] RecLineIds = null;
                RecLineIds = RefIds.Select(m => m.RefLineId).ToArray();

                var temp = (from p in db.PurchaseGoodsReceiptLine
                            where RecLineIds.Contains(p.PurchaseGoodsReceiptLineId)
                            join t in db.PurchaseOrderLine on p.PurchaseOrderLineId equals t.PurchaseOrderLineId
                            join LineCharge in db.PurchaseOrderLineCharge on p.PurchaseOrderLineId equals LineCharge.LineTableId
                            join HeaderCharge in db.PurchaseOrderHeaderCharges on t.PurchaseOrderHeaderId equals HeaderCharge.HeaderTableId
                            group new { p, LineCharge, HeaderCharge } by new { p.PurchaseGoodsReceiptLineId } into g
                            select new
                            {
                                LineId = g.Key.PurchaseGoodsReceiptLineId,
                                HeaderCharges = g.Select(m => m.HeaderCharge).ToList(),
                                Linecharges = g.Select(m => m.LineCharge).ToList(),
                            }).ToList();

                var temp2 = (from p in LineList
                             join t in RefIds on p.LineTableId equals t.LineId
                             join t2 in temp on t.RefLineId equals t2.LineId into table
                             from LineLis in table.DefaultIfEmpty()
                             orderby p.LineTableId
                             select new LineDetailListViewModel
                             {
                                 Amount = p.Amount,
                                 DealQty = p.DealQty,
                                 HeaderTableId = p.HeaderTableId,
                                 LineTableId = p.LineTableId,
                                 PersonID = p.PersonID,
                                 Rate = p.Rate,
                                 RLineCharges = (LineLis == null ? null : Mapper.Map<List<LineChargeViewModel>>(LineLis.Linecharges)),
                             }).ToList();



                new ChargesCalculationService(_unitOfWork).CalculateCharges(temp2, vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseInvoiceHeaderCharges", "Web.PurchaseInvoiceLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

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
                        //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                        db.PurchaseInvoiceHeaderCharge.Add(POHeaderCharge);
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

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyInvoiceMultiple(LineStatus, Header.DocDate, ref db);

                if (Header.Status != (int)StatusConstants.Drafted)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                    Header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(Header);
                }


                try
                {
                    PurchaseInvoiceDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId), ref db);
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
                    PurchaseInvoiceDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceLineViewModel.FirstOrDefault().PurchaseInvoiceHeaderId), ref db);
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
            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(Id);
            PurchaseInvoiceLineViewModel s = new PurchaseInvoiceLineViewModel();

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.PurchaseInvoiceHeaderId = H.PurchaseInvoiceHeaderId;
            s.PurchaseInvoiceHeaderDocNo = H.DocNo;
            s.SupplierId = sid;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;
            ViewBag.LineMode = "Create";
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseInvoiceLineViewModel svm)
        {
            PurchaseInvoiceLine s = Mapper.Map<PurchaseInvoiceLineViewModel, PurchaseInvoiceLine>(svm);
            PurchaseInvoiceHeader temp = new PurchaseInvoiceHeaderService(_unitOfWork).Find(s.PurchaseInvoiceHeaderId);

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

            if (svm.PurchaseGoodsReceiptLineId == 0)
            {
                ModelState.AddModelError("PurchaseGoodsReceiptLineId", "Goods Receipt field is required");
            }

            if (svm.DealQty <= 0)
            {
                ModelState.AddModelError("DealQty", "DealQty field is required");
            }

            bool BeforeSave = true;

            try
            {

                if (svm.PurchaseInvoiceLineId <= 0)
                    BeforeSave = PurchaseInvoiceDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseInvoiceHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseInvoiceDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseInvoiceHeaderId, EventModeConstants.Edit), ref db);

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
                if (svm.PurchaseInvoiceLineId <= 0)
                {
                    s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.Sr = _PurchaseInvoiceLineService.GetMaxSr(s.PurchaseInvoiceHeaderId);
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseInvoiceLine.Add(s);
                    //_PurchaseInvoiceLineService.Create(s);

                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnInvoice(s.PurchaseGoodsReceiptLineId, s.PurchaseInvoiceLineId, temp.DocDate, s.DocQty, ref db, true);



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
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        //new PurchaseInvoiceHeaderService(_unitOfWork).Update(temp);
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(temp);

                    try
                    {
                        PurchaseInvoiceDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseInvoiceHeaderId, s.PurchaseInvoiceLineId, EventModeConstants.Add), ref db);
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
                        PurchaseInvoiceDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseInvoiceHeaderId, s.PurchaseInvoiceLineId, EventModeConstants.Add), ref db);
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


                    return RedirectToAction("_Create", new { id = s.PurchaseInvoiceHeaderId, sid = svm.SupplierId });
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseInvoiceHeader header = new PurchaseInvoiceHeaderService(_unitOfWork).Find(svm.PurchaseInvoiceHeaderId);
                    StringBuilder logstring = new StringBuilder();
                    int status = header.Status;
                    PurchaseInvoiceLine temp1 = db.PurchaseInvoiceLine.Find(svm.PurchaseInvoiceLineId);

                    PurchaseInvoiceLine ExRec = new PurchaseInvoiceLine();
                    ExRec = Mapper.Map<PurchaseInvoiceLine>(temp1);


                    temp1.DiscountPer = svm.DiscountPer;
                    temp1.Amount = svm.Amount;
                    //temp1.PurchaseGoodsReceiptLineId = svm.PurchaseGoodsReceiptLineId;
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
                        ExObj = ExRec,
                        Obj = temp1,
                    });

                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnInvoice(temp1.PurchaseGoodsReceiptLineId, temp1.PurchaseInvoiceLineId, temp.DocDate, temp1.DocQty, ref db, true);

                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        //new PurchaseInvoiceHeaderService(_unitOfWork).Update(header);
                    }

                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceHeader.Add(header);

                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = db.PurchaseInvoiceLineCharge.Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
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
                        PurchaseInvoiceDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(temp1.PurchaseInvoiceHeaderId, temp1.PurchaseInvoiceLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseInvoiceDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(temp1.PurchaseInvoiceHeaderId, temp1.PurchaseInvoiceLineId, EventModeConstants.Edit), ref db);
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

            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
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
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_Create", temp);
        }

        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeletePost(PurchaseInvoiceLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseInvoiceDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceHeaderId, vm.PurchaseInvoiceLineId), ref db);
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
                    PurchaseInvoiceDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseInvoiceLine.PurchaseInvoiceHeaderId, PurchaseInvoiceLine.PurchaseInvoiceLineId), ref db);
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

                PurchaseInvoiceHeader header = db.PurchaseInvoiceHeader.Find(PurchaseInvoiceLine.PurchaseInvoiceHeaderId);

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseQtyOnInvoice(PurchaseInvoiceLine.PurchaseGoodsReceiptLineId, PurchaseInvoiceLine.PurchaseInvoiceLineId, header.DocDate, 0, ref db, true);

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

                PurchaseInvoiceLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseInvoiceLine.Remove(PurchaseInvoiceLine);


                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    //new PurchaseInvoiceHeaderService(_unitOfWork).Update(header);
                }
                header.ObjectState = Model.ObjectState.Modified;
                db.PurchaseInvoiceHeader.Add(header);



                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = db.PurchaseInvoiceHeaderCharge.Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseInvoiceHeaderCharge.Add(footer);
                        //new PurchaseInvoiceHeaderChargeService(_unitOfWork).Update(footer);
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
                    PurchaseInvoiceDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseInvoiceLine.PurchaseInvoiceHeaderId, PurchaseInvoiceLine.PurchaseInvoiceLineId), ref db);
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

        public ActionResult _Detail(int id)
        {
            PurchaseInvoiceLineViewModel temp = _PurchaseInvoiceLineService.GetPurchaseInvoiceLine(id);

            PurchaseInvoiceHeader H = new PurchaseInvoiceHeaderService(_unitOfWork).Find(temp.PurchaseInvoiceHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.PurchInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
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

        public JsonResult GetReceiptDetail(int ReceiptId)
        {
            return Json(new PurchaseGoodsReceiptLineService(_unitOfWork).GetPurchaseGoodsReceiptDetailBalanceForInvoice(ReceiptId));
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

        public JsonResult GetPurchaseOrders(int id, string term)//Invoice Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseInvoiceLineService.GetPendingPurchaseOrderHelpList(id, term), JsonRequestBehavior.AllowGet);
        }




        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {

            return Json(_PurchaseInvoiceLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

    }
}
