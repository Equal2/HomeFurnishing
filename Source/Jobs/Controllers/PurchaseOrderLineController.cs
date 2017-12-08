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
using PurchaseOrderDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{

    [Authorize]
    public class PurchaseOrderLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseOrderLineService _PurchaseOrderLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public PurchaseOrderLineController(IPurchaseOrderLineService SaleOrder, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseOrderLineService = SaleOrder;
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
            var p = _PurchaseOrderLineService.GetLineListForIndex(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }
        public ActionResult _ForIndent(int id, int sid)
        {
            PurchaseOrderLineFilterViewModel vm = new PurchaseOrderLineFilterViewModel();
            vm.PurchaseOrderHeaderId = id;
            vm.SupplierId = sid;
            PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            PrepareViewBag(null);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseOrderLineFilterViewModel vm)
        {
            List<PurchaseOrderLineViewModel> temp = _PurchaseOrderLineService.GetPurchaseIndentForFilters(vm).ToList();

            bool UnitConvetsionException = (from p in temp
                                            where p.UnitConversionException == true
                                            select p).Any();

            if (UnitConvetsionException)
            {
                ModelState.AddModelError("", "Unit Conversion are missing for few Products");
            }

            PurchaseOrderMasterDetailModel svm = new PurchaseOrderMasterDetailModel();
            svm.PurchaseOrderLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseOrderMasterDetailModel vm)
        {
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int Serial = _PurchaseOrderLineService.GetMaxSr(vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId);
            bool HeaderChargeEdit = false;
            db.Configuration.AutoDetectChangesEnabled = false;

            PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId);

            PurchaseOrderSetting Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new PurchaseOrderLineChargeService(_unitOfWork).GetMaxProductCharge(Header.PurchaseOrderHeaderId, "Web.PurchaseOrderLines", "PurchaseOrderHeaderId", "PurchaseOrderLineId");

            int PersonCount = 0;
            if (!Settings.CalculationId.HasValue)
            {
                throw new Exception("Calculation not configured in purchase order settings");
            }

            int CalculationId = Settings.CalculationId ?? 0;

            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            bool BeforeSave = true;

            try
            {
                BeforeSave = PurchaseOrderDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId), ref db);
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
                //decimal Qty = vm.PurchaseOrderLineViewModel.Where(m => m.Rate > 0).Sum(m => m.Qty);

                decimal Qty = vm.PurchaseOrderLineViewModel.Sum(m => m.Qty);

                List<string> uids = _PurchaseOrderLineService.GetProcGenProductUids(Header.DocTypeId, Qty, Header.DivisionId, Header.SiteId);

                foreach (var item in vm.PurchaseOrderLineViewModel)
                {
                    if (item.Qty > 0)
                    {

                        PurchaseOrderLine line = new PurchaseOrderLine();
                        var indent = new PurchaseIndentLineService(_unitOfWork).Find(item.PurchaseIndentLineId ?? 0);
                        line.PurchaseOrderHeaderId = item.PurchaseOrderHeaderId;
                        line.PurchaseIndentLineId = item.PurchaseIndentLineId;
                        line.ProductId = indent.ProductId;
                        line.Dimension1Id = indent.Dimension1Id;
                        line.Dimension2Id = indent.Dimension2Id;
                        line.Specification = indent.Specification;
                        line.Qty = item.Qty;
                        line.Rate = item.Rate;
                        line.DealQty = item.UnitConversionMultiplier * item.Qty;
                        line.DealUnitId = item.DealUnitId;
                        line.DiscountPer = item.DiscountPer;
                        line.Amount = (line.DealQty * line.Rate);
                        line.UnitConversionMultiplier = item.UnitConversionMultiplier;
                        line.Sr = Serial++;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.PurchaseOrderLineId = pk;


                        // List<string> uids = _PurchaseOrderLineService.GetProcGenProductUids(Header.DocTypeId, line.Qty, Header.DivisionId, Header.SiteId);

                        //if (uids.Count > 0)
                        //{
                        //    ProductUidHeader ProdUidHeader = new ProductUidHeader();

                        //    ProdUidHeader.ProductUidHeaderId = -Cnt;
                        //    ProdUidHeader.ProductId = line.ProductId;
                        //    ProdUidHeader.Dimension1Id = line.Dimension1Id;
                        //    ProdUidHeader.Dimension2Id = line.Dimension2Id;
                        //    ProdUidHeader.GenDocId = Header.PurchaseOrderHeaderId;
                        //    ProdUidHeader.GenDocNo = Header.DocNo;
                        //    ProdUidHeader.GenDocTypeId = Header.DocTypeId;
                        //    ProdUidHeader.GenDocDate = Header.DocDate;
                        //    ProdUidHeader.GenPersonId = Header.SupplierId;
                        //    ProdUidHeader.CreatedBy = User.Identity.Name;
                        //    ProdUidHeader.CreatedDate = DateTime.Now;
                        //    ProdUidHeader.ModifiedBy = User.Identity.Name;
                        //    ProdUidHeader.ModifiedDate = DateTime.Now;
                        //    ProdUidHeader.ObjectState = Model.ObjectState.Added;
                        //    //new ProductUidHeaderService(_unitOfWork).Create(ProdUidHeader);
                        //    db.ProductUidHeader.Add(ProdUidHeader);

                        //    Cnt++;

                        //    line.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                        //    line.ObjectState = Model.ObjectState.Added;
                        //    //_PurchaseOrderLineService.Create(line);
                        //    db.PurchaseOrderLine.Add(line);


                        //    int count = 0;
                        //    //foreach (string UidItem in uids)
                        //    for (int A = CountUid; A < item.Qty; A++)
                        //    {
                        //        ProductUid ProdUid = new ProductUid();

                        //        ProdUid.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                        //        //ProdUid.ProductUidName = UidItem;
                        //        ProdUid.ProductUidName = uids[CountUid];
                        //        ProdUid.ProductId = line.ProductId;
                        //        ProdUid.IsActive = true;
                        //        ProdUid.CreatedBy = User.Identity.Name;
                        //        ProdUid.CreatedDate = DateTime.Now;
                        //        ProdUid.ModifiedBy = User.Identity.Name;
                        //        ProdUid.ModifiedDate = DateTime.Now;
                        //        //ProdUid.GenLineId = line.PurchaseOrderLineId;
                        //        ProdUid.GenDocId = Header.PurchaseOrderHeaderId;
                        //        ProdUid.GenDocNo = Header.DocNo;
                        //        ProdUid.GenDocTypeId = Header.DocTypeId;
                        //        ProdUid.GenDocDate = Header.DocDate;
                        //        ProdUid.GenPersonId = Header.SupplierId;
                        //        ProdUid.Dimension1Id = line.Dimension1Id;
                        //        ProdUid.Dimension2Id = line.Dimension2Id;
                        //        ProdUid.CurrenctProcessId = null;
                        //        ProdUid.Status = ProductUidStatusConstants.Issue;
                        //        ProdUid.LastTransactionDocId = Header.PurchaseOrderHeaderId;
                        //        ProdUid.LastTransactionDocNo = Header.DocNo;
                        //        ProdUid.LastTransactionDocTypeId = Header.DocTypeId;
                        //        ProdUid.LastTransactionDocDate = Header.DocDate;
                        //        ProdUid.LastTransactionPersonId = Header.SupplierId;
                        //        ProdUid.LastTransactionLineId = line.PurchaseOrderLineId;
                        //        ProdUid.ProductUIDId = count;
                        //        ProdUid.ObjectState = Model.ObjectState.Added;
                        //        //new ProductUidService(_unitOfWork).Create(ProdUid);
                        //        db.ProductUid.Add(ProdUid);

                        //        count++;
                        //        CountUid++;
                        //    }
                        //}
                        //else
                        //{
                        line.ObjectState = Model.ObjectState.Added;
                        //_PurchaseOrderLineService.Create(line);
                        db.PurchaseOrderLine.Add(line);
                        //}

                        PurchaseOrderLineStatus St = new PurchaseOrderLineStatus();
                        St.PurchaseOrderLineId = line.PurchaseOrderLineId;
                        St.ObjectState = Model.ObjectState.Added;
                        db.PurchaseOrderLineStatus.Add(St);


                        LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseOrderLineId, HeaderTableId = item.PurchaseOrderHeaderId, PersonID = Header.SupplierId, DealQty = line.DealQty });

                        pk++;
                    }

                }

                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseOrderHeaderCharges", "Web.PurchaseOrderLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    PurchaseOrderLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseOrderLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    //new PurchaseOrderLineChargeService(_unitOfWork).Create(PoLineCharge);
                    db.PurchaseOrderLineCharge.Add(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        PurchaseOrderHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseOrderHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId;
                        POHeaderCharge.PersonID = Header.SupplierId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        //new PurchaseOrderHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                        db.PurchaseOrderHeaderCharges.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new PurchaseOrderHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        //new PurchaseOrderHeaderChargeService(_unitOfWork).Update(footercharge);
                        db.PurchaseOrderHeaderCharges.Add(footercharge);
                    }

                }

                try
                {
                    PurchaseOrderDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId), ref db);
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
                    PurchaseOrderDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderLineViewModel.FirstOrDefault().PurchaseOrderHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus = Header.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Results", vm);

        }



        private void PrepareViewBag(PurchaseOrderLineViewModel vm)
        {

            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        [HttpGet]
        public ActionResult CreateLine(int id, int sid)
        {
            return _Create(id, null, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, int sid)
        {
            return _Create(id, null, sid);
        }

        public ActionResult _Create(int Id, DateTime? date, int sid) //Id ==>Sale Order Header Id
        {
            PurchaseOrderHeader H = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
            PurchaseOrderLineViewModel s = new PurchaseOrderLineViewModel();

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);

            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);

            s.PurchaseOrderHeaderId = H.PurchaseOrderHeaderId;
            s.PurchaseOrderHeaderDocNo = H.DocNo;
            s.SupplierId = sid;
            s.DocTypeId = H.DocTypeId;
            s.SiteId = H.SiteId;
            s.DivisionId = H.DivisionId;
            if (date != null) s.DueDate = date;
            ViewBag.DocNo = H.DocNo;
            ViewBag.Status = H.Status;
            ViewBag.LineMode = "Create";
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        public ActionResult _CreatePost(PurchaseOrderLineViewModel svm)
        {

            PurchaseOrderLine s = Mapper.Map<PurchaseOrderLineViewModel, PurchaseOrderLine>(svm);
            PurchaseOrderHeader temp = new PurchaseOrderHeaderService(_unitOfWork).Find(s.PurchaseOrderHeaderId);

            //PurchaseOrderLineCharge charge= new CalculationService(_unitOfWork).GetLineFromFormCollection(collection);


            if (svm.PurchaseOrderLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.PurchOrderSettings != null)
            {
                if (svm.PurchOrderSettings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
            }

            if (svm.DueDate != null && svm.DueDate < temp.DocDate)
            {
                ModelState.AddModelError("DueDate", "DueDate greater than DocDate");
            }
            if (svm.Qty <= 0)
            {
                ModelState.AddModelError("Qty", "Qty field is required");
            }
            if (svm.DealQty <= 0)
            {
                ModelState.AddModelError("DealQty", "DealQty field is required");
            }


            bool BeforeSave = true;

            try
            {

                if (svm.PurchaseOrderLineId <= 0)
                    BeforeSave = PurchaseOrderDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseOrderHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseOrderDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseOrderHeaderId, EventModeConstants.Edit), ref db);

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
                if (svm.PurchaseOrderLineId <= 0)
                {
                    //List<string> uids = _PurchaseOrderLineService.GetProcGenProductUids(temp.DocTypeId, s.Qty, temp.DivisionId, temp.SiteId);

                    //if (uids.Count > 0)
                    //{
                    //ProductUidHeader ProdUidHeader = new ProductUidHeader();

                    //ProdUidHeader.ProductId = s.ProductId;
                    //ProdUidHeader.Dimension1Id = s.Dimension1Id;
                    //ProdUidHeader.Dimension2Id = s.Dimension2Id;
                    //ProdUidHeader.GenDocId = s.PurchaseOrderHeaderId;
                    //ProdUidHeader.GenDocNo = temp.DocNo;
                    //ProdUidHeader.GenDocTypeId = temp.DocTypeId;
                    //ProdUidHeader.GenDocDate = temp.DocDate;
                    //ProdUidHeader.GenPersonId = temp.SupplierId;
                    //ProdUidHeader.CreatedBy = User.Identity.Name;
                    //ProdUidHeader.CreatedDate = DateTime.Now;
                    //ProdUidHeader.ModifiedBy = User.Identity.Name;
                    //ProdUidHeader.ModifiedDate = DateTime.Now;
                    //ProdUidHeader.ObjectState = Model.ObjectState.Added;
                    ////new ProductUidHeaderService(_unitOfWork).Create(ProdUidHeader);
                    //db.ProductUidHeader.Add(ProdUidHeader);
                    //s.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;


                    //s.DiscountPer = svm.DiscountPer;
                    //s.CreatedDate = DateTime.Now;
                    //s.ModifiedDate = DateTime.Now;
                    //s.Sr = _PurchaseOrderLineService.GetMaxSr(s.PurchaseOrderHeaderId);
                    //s.CreatedBy = User.Identity.Name;
                    //s.ModifiedBy = User.Identity.Name;
                    //s.ObjectState = Model.ObjectState.Added;
                    ////_PurchaseOrderLineService.Create(s);
                    //db.PurchaseOrderLine.Add(s);




                    //int count = 0;
                    //foreach (string item in uids)
                    //{
                    //    ProductUid ProdUid = new ProductUid();

                    //    ProdUid.ProductUidHeaderId = s.ProductUidHeaderId;
                    //    ProdUid.ProductUidName = item;
                    //    ProdUid.ProductId = s.ProductId;
                    //    ProdUid.IsActive = true;
                    //    ProdUid.CreatedBy = User.Identity.Name;
                    //    ProdUid.CreatedDate = DateTime.Now;
                    //    ProdUid.ModifiedBy = User.Identity.Name;
                    //    ProdUid.ModifiedDate = DateTime.Now;
                    //    ProdUid.GenLineId = s.PurchaseOrderLineId;
                    //    ProdUid.GenDocId = s.PurchaseOrderHeaderId;
                    //    ProdUid.GenDocNo = temp.DocNo;
                    //    ProdUid.GenDocTypeId = temp.DocTypeId;
                    //    ProdUid.GenDocDate = temp.DocDate;
                    //    ProdUid.GenPersonId = temp.SupplierId;
                    //    ProdUid.Dimension1Id = s.Dimension1Id;
                    //    ProdUid.Dimension2Id = s.Dimension2Id;
                    //    ProdUid.CurrenctProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase).ProcessId;
                    //    ProdUid.Status = ProductUidStatusConstants.Issue;
                    //    ProdUid.LastTransactionDocId = s.PurchaseOrderHeaderId;
                    //    ProdUid.LastTransactionDocNo = temp.DocNo;
                    //    ProdUid.LastTransactionDocTypeId = temp.DocTypeId;
                    //    ProdUid.LastTransactionDocDate = temp.DocDate;
                    //    ProdUid.LastTransactionPersonId = temp.SupplierId;
                    //    ProdUid.LastTransactionLineId = s.PurchaseOrderLineId;
                    //    ProdUid.ProductUIDId = count;
                    //    ProdUid.ObjectState = Model.ObjectState.Added;
                    //    db.ProductUid.Add(ProdUid);
                    //    //new ProductUidService(_unitOfWork).Create(ProdUid);

                    //    count++;
                    //}

                    //}
                    //else
                    //{
                    //    s.DiscountPer = svm.DiscountPer;
                    //    s.CreatedDate = DateTime.Now;
                    //    s.ModifiedDate = DateTime.Now;
                    //    s.CreatedBy = User.Identity.Name;
                    //    s.ModifiedBy = User.Identity.Name;
                    //    s.ObjectState = Model.ObjectState.Added;
                    //    //_PurchaseOrderLineService.Create(s);
                    //    db.PurchaseOrderLine.Add(s);
                    //}

                    s.DiscountPer = svm.DiscountPer;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.Sr = _PurchaseOrderLineService.GetMaxSr(s.PurchaseOrderHeaderId);
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    //_PurchaseOrderLineService.Create(s);
                    db.PurchaseOrderLine.Add(s);

                    new PurchaseOrderLineStatusService(_unitOfWork).CreateLineStatus(s.PurchaseOrderLineId, ref db);


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            item.LineTableId = s.PurchaseOrderLineId;
                            item.PersonID = temp.SupplierId;
                            item.HeaderTableId = s.PurchaseOrderHeaderId;
                            item.ObjectState = Model.ObjectState.Added;
                            db.PurchaseOrderLineCharge.Add(item);
                            //new PurchaseOrderLineChargeService(_unitOfWork).Create(item);
                        }

                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {

                            if (item.Id > 0)
                            {

                                var footercharge = new PurchaseOrderHeaderChargeService(_unitOfWork).Find(item.Id);
                                footercharge.Rate = item.Rate;
                                footercharge.Amount = item.Amount;
                                footercharge.ObjectState = Model.ObjectState.Modified;
                                db.PurchaseOrderHeaderCharges.Add(footercharge);
                                //new PurchaseOrderHeaderChargeService(_unitOfWork).Update(footercharge);

                            }

                            else
                            {
                                item.HeaderTableId = s.PurchaseOrderHeaderId;
                                item.PersonID = temp.SupplierId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.PurchaseOrderHeaderCharges.Add(item);
                                //new PurchaseOrderHeaderChargeService(_unitOfWork).Create(item);
                            }
                        }

                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderHeader.Add(temp);
                        //new PurchaseOrderHeaderService(_unitOfWork).Update(temp);
                    }

                    try
                    {
                        PurchaseOrderDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderHeaderId, s.PurchaseOrderLineId, EventModeConstants.Add), ref db);
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
                        PurchaseOrderDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderHeaderId, s.PurchaseOrderLineId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXCL"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseOrderHeaderId,
                        DocLineId = s.PurchaseOrderLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));


                    return RedirectToAction("_Create", new { id = s.PurchaseOrderHeaderId, date = ((s.DueDate == null) ? null : s.DueDate), sid = svm.SupplierId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseOrderHeader header = new PurchaseOrderHeaderService(_unitOfWork).Find(svm.PurchaseOrderHeaderId);
                    int status = header.Status;
                    StringBuilder logstring = new StringBuilder();

                    PurchaseOrderLine temp1 = _PurchaseOrderLineService.Find(svm.PurchaseOrderLineId);

                    PurchaseOrderLine ExRec = new PurchaseOrderLine();
                    ExRec = Mapper.Map<PurchaseOrderLine>(temp1);

                    if (temp1.ProductUidHeaderId != null)
                    {
                        ProductUidHeader ProdUidHeader = new ProductUidHeaderService(_unitOfWork).Find((int)temp1.ProductUidHeaderId);

                        ProdUidHeader.ProductId = s.ProductId;
                        ProdUidHeader.Dimension1Id = s.Dimension1Id;
                        ProdUidHeader.Dimension2Id = s.Dimension2Id;
                        ProdUidHeader.GenDocId = s.PurchaseOrderHeaderId;
                        ProdUidHeader.GenDocNo = temp.DocNo;
                        ProdUidHeader.GenDocTypeId = temp.DocTypeId;
                        ProdUidHeader.GenDocDate = temp.DocDate;
                        ProdUidHeader.GenPersonId = temp.SupplierId;
                        ProdUidHeader.CreatedBy = User.Identity.Name;
                        ProdUidHeader.CreatedDate = DateTime.Now;
                        ProdUidHeader.ModifiedBy = User.Identity.Name;
                        ProdUidHeader.ModifiedDate = DateTime.Now;
                        ProdUidHeader.ObjectState = Model.ObjectState.Modified;
                        db.ProductUidHeader.Add(ProdUidHeader);
                        //new ProductUidHeaderService(_unitOfWork).Update(ProdUidHeader);

                        if (svm.Qty > temp1.Qty)
                        {
                            List<string> uids = _PurchaseOrderLineService.GetProcGenProductUids(temp.DocTypeId, svm.Qty - temp1.Qty, temp.DivisionId, temp.SiteId);
                            int count = 0;
                            foreach (string item in uids)
                            {
                                ProductUid ProdUid = new ProductUid();

                                ProdUid.ProductUidName = item;
                                ProdUid.ProductId = s.ProductId;
                                ProdUid.IsActive = true;
                                ProdUid.CreatedBy = User.Identity.Name;
                                ProdUid.CreatedDate = DateTime.Now;
                                ProdUid.ModifiedBy = User.Identity.Name;
                                ProdUid.ModifiedDate = DateTime.Now;
                                ProdUid.GenLineId = s.PurchaseOrderLineId;
                                ProdUid.GenDocId = s.PurchaseOrderHeaderId;
                                ProdUid.GenDocNo = temp.DocNo;
                                ProdUid.GenDocTypeId = temp.DocTypeId;
                                ProdUid.GenDocDate = temp.DocDate;
                                ProdUid.GenPersonId = temp.SupplierId;
                                ProdUid.Dimension1Id = s.Dimension1Id;
                                ProdUid.Dimension2Id = s.Dimension2Id;
                                ProdUid.CurrenctProcessId = null;
                                ProdUid.Status = ProductUidStatusConstants.Issue;
                                ProdUid.LastTransactionDocId = s.PurchaseOrderHeaderId;
                                ProdUid.LastTransactionDocNo = temp.DocNo;
                                ProdUid.LastTransactionDocTypeId = temp.DocTypeId;
                                ProdUid.LastTransactionDocDate = temp.DocDate;
                                ProdUid.LastTransactionPersonId = temp.SupplierId;
                                ProdUid.LastTransactionLineId = s.PurchaseOrderLineId;
                                ProdUid.ProductUIDId = count;
                                ProdUid.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                                ProdUid.ObjectState = Model.ObjectState.Added;
                                //new ProductUidService(_unitOfWork).Create(ProdUid);
                                db.ProductUid.Add(ProdUid);

                                count++;
                            }
                        }
                        else if (svm.Qty < temp1.Qty)
                        {
                            var ProductUidToDelete = (from L in db.ProductUid
                                                      where L.ProductUidHeaderId == ProdUidHeader.ProductUidHeaderId
                                                      select L).Take((int)(temp1.Qty - svm.Qty));

                            foreach (var item in ProductUidToDelete)
                            {
                                item.ObjectState = Model.ObjectState.Deleted;
                                db.ProductUid.Remove(item);
                            }
                        }
                    }

                    temp1.DiscountPer = svm.DiscountPer;
                    temp1.DueDate = svm.DueDate;
                    temp1.ProductId = svm.ProductId;
                    temp1.Specification = svm.Specification;
                    temp1.PurchaseIndentLineId = svm.PurchaseIndentLineId;
                    temp1.LotNo = svm.LotNo;
                    temp1.Dimension1Id = svm.Dimension1Id;
                    temp1.Dimension2Id = svm.Dimension2Id;
                    temp1.Qty = svm.Qty;
                    temp1.DealQty = svm.DealQty;
                    temp1.DealUnitId = svm.DealUnitId;
                    temp1.Rate = svm.Rate ?? 0;
                    temp1.Amount = svm.Amount ?? 0;
                    temp1.Remark = svm.Remark;
                    temp1.UnitConversionMultiplier = svm.UnitConversionMultiplier;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    temp1.ObjectState = Model.ObjectState.Modified;
                    //_PurchaseOrderLineService.Update(temp1);
                    db.PurchaseOrderLine.Add(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
                    });



                    if (header.Status != (int)StatusConstants.Drafted)
                    {
                        header.Status = (int)StatusConstants.Modified;
                        //new PurchaseOrderHeaderService(_unitOfWork).Update(header);
                        header.ModifiedBy = User.Identity.Name;
                        header.ModifiedDate = DateTime.Now;
                        header.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderHeader.Add(header);
                    }


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = db.PurchaseOrderLineCharge.Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            //new PurchaseOrderLineChargeService(_unitOfWork).Update(productcharge);
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseOrderLineCharge.Add(productcharge);
                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = db.PurchaseOrderHeaderCharges.Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseOrderHeaderCharges.Add(footercharge);
                            //new PurchaseOrderHeaderChargeService(_unitOfWork).Update(footercharge);
                        }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseOrderDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderHeaderId, s.PurchaseOrderLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseOrderDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderHeaderId, s.PurchaseOrderLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = header.PurchaseOrderHeaderId,
                        DocLineId = temp1.PurchaseOrderLineId,
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

        private ActionResult _Modify(int id)
        {
            PurchaseOrderLineViewModel temp = _PurchaseOrderLineService.GetPurchaseOrderLine(id);

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

            PurchaseOrderHeader H = new PurchaseOrderHeaderService(_unitOfWork).Find(temp.PurchaseOrderHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);


            temp.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
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
            PurchaseOrderLineViewModel temp = _PurchaseOrderLineService.GetPurchaseOrderLine(id);

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

            PurchaseOrderHeader H = new PurchaseOrderHeaderService(_unitOfWork).Find(temp.PurchaseOrderHeaderId);
            PrepareViewBag(temp);
            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            temp.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            return PartialView("_Create", temp);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseOrderLineViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseOrderDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseOrderHeaderId, vm.PurchaseOrderLineId), ref db);
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

                string Exception = "";
                PurchaseOrderLine PurchaseOrderLine = db.PurchaseOrderLine.Find(vm.PurchaseOrderLineId);

                try
                {
                    PurchaseOrderDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseOrderLine.PurchaseOrderHeaderId, PurchaseOrderLine.PurchaseOrderLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXCL"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseOrderLine>(PurchaseOrderLine),
                });


                var PurchaseOrderLineStatus = db.PurchaseOrderLineStatus.Find(PurchaseOrderLine.PurchaseOrderLineId);

                var chargeslist = (from p in db.PurchaseOrderLineCharge
                                   where p.LineTableId == PurchaseOrderLine.PurchaseOrderLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseOrderLineCharge.Remove(item);
                        //new PurchaseOrderLineChargeService(_unitOfWork).Delete(item.Id);
                    }

                PurchaseOrderLineStatus.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseOrderLineStatus.Remove(PurchaseOrderLineStatus);

                var ProductUidHeaderId = PurchaseOrderLine.ProductUidHeaderId;

                PurchaseOrderLine.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseOrderLine.Remove(PurchaseOrderLine);

                //new PurchaseOrderLineStatusService(_unitOfWork).Delete(PurchaseOrderLine.PurchaseOrderLineId);

                PurchaseOrderHeader header = new PurchaseOrderHeaderService(_unitOfWork).Find(PurchaseOrderLine.PurchaseOrderHeaderId);
                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderHeader.Add(header);
                    //new PurchaseOrderHeaderService(_unitOfWork).Update(header);
                }



                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = db.PurchaseOrderHeaderCharges.Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderHeaderCharges.Add(footer);
                        //new PurchaseOrderHeaderChargeService(_unitOfWork).Update(footer);
                    }

                var ProductUid = (from p in db.ProductUid
                                  where p.ProductUidHeaderId == ProductUidHeaderId
                                  select p).ToList();

                new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(vm.PurchaseOrderLineId, header.DocTypeId, header.SiteId, header.DivisionId, ref db);

                foreach (var item in ProductUid)
                {
                    if (item.LastTransactionDocNo == header.DocNo && item.LastTransactionDocTypeId == header.DocTypeId)
                    {
                        //new ProductUidService(_unitOfWork).Delete(item);
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.ProductUid.Remove(item);
                    }
                    else
                    {
                        Exception = "Record Cannot be deleted as it is in use by other documents";
                        break;
                    }
                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (!string.IsNullOrEmpty(Exception) || EventException)
                        throw new Exception(Exception);

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
                    PurchaseOrderDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseOrderLine.PurchaseOrderHeaderId, PurchaseOrderLine.PurchaseOrderLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = header.DocTypeId,
                    DocId = header.PurchaseOrderHeaderId,
                    DocLineId = PurchaseOrderLine.PurchaseOrderLineId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocNo = header.DocNo,
                    xEModifications = Modifications,
                    DocDate = header.DocDate,
                    DocStatus = header.Status,
                }));

            }

            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult _Detail(int id)
        {
            PurchaseOrderLineViewModel temp = _PurchaseOrderLineService.GetPurchaseOrderLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            PurchaseOrderHeader H = new PurchaseOrderHeaderService(_unitOfWork).Find(temp.PurchaseOrderHeaderId);
            PrepareViewBag(temp);

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(H.DocTypeId, H.DivisionId, H.SiteId);

            temp.PurchOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);

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

        public JsonResult getunitconversiondetailjson(int productid, string UnitId, string DealUnitId)
        {
            UnitConversion uc = new UnitConversionService(_unitOfWork).GetUnitConversion(productid, UnitId, DealUnitId);
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

        public JsonResult GetPendingIndentsCount(int ProductId)
        {
            return Json(_PurchaseOrderLineService.GetPendingPurchaseIndentCount(ProductId));
        }

        public JsonResult GetProductDetailJson(int ProductId, int HeaderId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);
            List<Product> ProductJson = new List<Product>();

            string DealUnitId;

            var temp = _PurchaseOrderLineService.GetLineListForIndex(HeaderId).OrderByDescending(m => m.PurchaseOrderLineId).FirstOrDefault();

            if (temp != null)
            {
                DealUnitId = temp.DealUnitId;
            }
            else
                DealUnitId = product.UnitId;

            //ProductJson.Add(new Product()
            //{
            //    ProductId = product.ProductId,
            //    StandardCost = product.StandardCost,
            //    UnitId = product.UnitId
            //});

            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, DealUnitId = DealUnitId });
        }

        public JsonResult GetPendingIndents(int ProductId, int PurchaseOrderHeaderId)
        {
            return Json(new PurchaseIndentHeaderService(_unitOfWork).GetPendingPurchaseIndents(ProductId, PurchaseOrderHeaderId).ToList());
        }

        public JsonResult GetIndentDetail(int IndentId)
        {
            return Json(_PurchaseOrderLineService.GetPurchaseIndentLineForOrder(IndentId));
        }


        public JsonResult GetPurchaseIndents(int id, string term)//Order Header ID
        {

            //string DocTypes = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId).filterContraDocTypes;

            return Json(_PurchaseOrderLineService.GetPendingPurchaseIndentHelpList(id, term), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomProducts(int id, string term)//Indent Header ID
        {
            return Json(_PurchaseOrderLineService.GetProductHelpList(id, term), JsonRequestBehavior.AllowGet);
        }
    }
}
