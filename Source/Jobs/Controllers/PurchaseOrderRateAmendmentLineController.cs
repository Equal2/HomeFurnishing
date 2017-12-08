using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using AutoMapper;
using Model.ViewModels;
using Jobs.Helpers;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using PurchaseOrderAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderRateAmendmentLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;
        IPurchaseOrderRateAmendmentLineService _PurchaseOrderRateAmendmentLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public PurchaseOrderRateAmendmentLineController(IPurchaseOrderRateAmendmentLineService PurchaseOrderRateAmendmentLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseOrderRateAmendmentLineService = PurchaseOrderRateAmendmentLineService;
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
            var p = _PurchaseOrderRateAmendmentLineService.GetPurchaseOrderRateAmendmentLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int? sid)
        {
            PurchaseOrderAmendmentFilterViewModel vm = new PurchaseOrderAmendmentFilterViewModel();
            vm.PurchaseOrderAmendmentHeaderId = id;
            vm.SupplierId = sid;
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(PurchaseOrderAmendmentFilterViewModel vm)
        {
            List<PurchaseOrderRateAmendmentLineViewModel> temp = _PurchaseOrderRateAmendmentLineService.GetPurchaseOrderLineForMultiSelect(vm).ToList();
            PurchaseOrderAmendmentMasterDetailModel svm = new PurchaseOrderAmendmentMasterDetailModel();
            svm.PurchaseOrderRateAmendmentLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(PurchaseOrderAmendmentMasterDetailModel vm)
        {
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();
            int pk = 0;
            int Serial = _PurchaseOrderRateAmendmentLineService.GetMaxSr(vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId);
            bool HeaderChargeEdit = false;
            var Header = new PurchaseOrderAmendmentHeaderService(db).Find(vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId);

            PurchaseOrderSetting Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            int? MaxLineId = new PurchaseOrderRateAmendmentLineChargeService(_unitOfWork).GetMaxProductCharge(Header.PurchaseOrderAmendmentHeaderId, "Web.PurchaseOrderRateAmendmentLines", "PurchaseOrderAmendmentHeaderId", "PurchaseOrderRateAmendmentLineId");

            int PersonCount = 0;
            if (!Settings.CalculationId.HasValue)
            {
                throw new Exception("Calculation not configured in purchase order settings");
            }
            int CalculationId = Settings.CalculationId ?? 0;
            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseOrderAmendmentDocEvents.beforeLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");
            #endregion

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.PurchaseOrderRateAmendmentLineViewModel.Where(m => (m.AmendedRate - m.PurchaseOrderRate) != 0 && m.AAmended == false))
                {

                    PurchaseOrderRateAmendmentLine line = new PurchaseOrderRateAmendmentLine();

                    line.PurchaseOrderAmendmentHeaderId = item.PurchaseOrderAmendmentHeaderId;
                    line.PurchaseOrderLineId = item.PurchaseOrderLineId;
                    line.Qty = item.Qty;
                    line.AmendedRate = item.AmendedRate;
                    line.Rate = item.AmendedRate - item.PurchaseOrderRate;
                    line.Amount = item.DealQty * line.Rate;
                    line.PurchaseOrderRate = item.PurchaseOrderRate;
                    line.Sr = Serial++;
                    line.CreatedDate = DateTime.Now;
                    line.ModifiedDate = DateTime.Now;
                    line.CreatedBy = User.Identity.Name;
                    line.ModifiedBy = User.Identity.Name;
                    line.Remark = item.Remark;
                    LineStatus.Add(line.PurchaseOrderLineId, line.Rate);

                    line.ObjectState = Model.ObjectState.Added;
                    db.PurchaseOrderRateAmendmentLine.Add(line);

                    LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.PurchaseOrderRateAmendmentLineId, HeaderTableId = item.PurchaseOrderAmendmentHeaderId, PersonID = Header.SupplierId, DealQty = item.DealQty });

                    pk++;
                }

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseRateOnAmendmentMultiple(LineStatus, Header.DocDate, ref db);


                new ChargesCalculationService(_unitOfWork).CalculateCharges(LineList, vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId, CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.PurchaseOrderAmendmentHeaderCharges", "Web.PurchaseOrderRateAmendmentLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    PurchaseOrderRateAmendmentLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, PurchaseOrderRateAmendmentLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    //new PurchaseOrderLineChargeService(_unitOfWork).Create(PoLineCharge);
                    db.PurchaseOrderRateAmendmentLineCharge.Add(PoLineCharge);

                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        PurchaseOrderAmendmentHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, PurchaseOrderAmendmentHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId;
                        POHeaderCharge.PersonID = Header.SupplierId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        //new PurchaseOrderHeaderChargeService(_unitOfWork).Create(POHeaderCharge);
                        db.PurchaseOrderAmendmentHeaderCharges.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new PurchaseOrderAmendmentHeaderChargeService(_unitOfWork).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        //new PurchaseOrderHeaderChargeService(_unitOfWork).Update(footercharge);
                        db.PurchaseOrderAmendmentHeaderCharges.Add(footercharge);
                    }

                }


                try
                {
                    PurchaseOrderAmendmentDocEvents.onLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId), ref db);
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
                    PurchaseOrderAmendmentDocEvents.afterLineSaveBulkEvent(this, new PurchaseEventArgs(vm.PurchaseOrderRateAmendmentLineViewModel.FirstOrDefault().PurchaseOrderAmendmentHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.PurchaseOrderAmendmentHeaderId,
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
        public ActionResult CreateLine(int Id, int? sid)
        {
            return _Create(Id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int Id, int? sid)
        {
            return _Create(Id, sid);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Approve(int Id, int? sid)
        {
            return _Create(Id, sid);
        }

        public ActionResult _Create(int Id, int? sid) //Id ==>Sale Order Header Id
        {
            PurchaseOrderAmendmentHeader header = new PurchaseOrderAmendmentHeaderService(db).Find(Id);
            PurchaseOrderRateAmendmentLineViewModel svm = new PurchaseOrderRateAmendmentLineViewModel();

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            svm.DocTypeId = header.DocTypeId;
            svm.DivisionId = header.DivisionId;
            svm.SiteId = header.SiteId;
            ViewBag.LineMode = "Create";
            svm.PurchaseOrderAmendmentHeaderId = Id;
            return PartialView("_Create", svm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult _CreatePost(PurchaseOrderRateAmendmentLineViewModel svm)
        {
            bool BeforeSave = true;
            if (svm.PurchaseOrderRateAmendmentLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            #region BeforeSave
            try
            {
                if (svm.PurchaseOrderRateAmendmentLineId <= 0)
                    BeforeSave = PurchaseOrderAmendmentDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseOrderAmendmentDocEvents.beforeLineSaveEvent(this, new PurchaseEventArgs(svm.PurchaseOrderAmendmentHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");
            #endregion

            if (svm.PurchaseOrderRateAmendmentLineId <= 0)
            {
                PurchaseOrderRateAmendmentLine s = new PurchaseOrderRateAmendmentLine();

                if (ModelState.IsValid && BeforeSave && !EventException)
                {

                    if (svm.Rate != 0)
                    {
                        s.Remark = svm.Remark;
                        s.PurchaseOrderAmendmentHeaderId = svm.PurchaseOrderAmendmentHeaderId;
                        s.PurchaseOrderLineId = svm.PurchaseOrderLineId;
                        s.Qty = svm.Qty;
                        s.AmendedRate = svm.AmendedRate;
                        s.Amount = svm.Amount;
                        s.PurchaseOrderRate = svm.PurchaseOrderRate;
                        s.Rate = svm.Rate;
                        s.Remark = svm.Remark;
                        s.Sr = _PurchaseOrderRateAmendmentLineService.GetMaxSr(s.PurchaseOrderAmendmentHeaderId);
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now;
                        s.CreatedBy = User.Identity.Name;
                        s.ModifiedBy = User.Identity.Name;
                        //_PurchaseOrderRateAmendmentLineService.Create(s);
                        s.ObjectState = Model.ObjectState.Added;
                        db.PurchaseOrderRateAmendmentLine.Add(s);

                        PurchaseOrderAmendmentHeader temp2 = new PurchaseOrderAmendmentHeaderService(db).Find(s.PurchaseOrderAmendmentHeaderId);
                        if (temp2.Status != (int)StatusConstants.Drafted)
                        {
                            temp2.Status = (int)StatusConstants.Modified;
                            temp2.ModifiedBy = User.Identity.Name;
                            temp2.ModifiedDate = DateTime.Now;
                        }

                        //new PurchaseOrderAmendmentHeaderService(_unitOfWork).Update(temp2);
                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderAmendmentHeader.Add(temp2);

                        new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseRateOnAmendment(svm.PurchaseOrderLineId, s.PurchaseOrderRateAmendmentLineId, temp2.DocDate, s.Rate, ref db);


                        if (svm.linecharges != null)
                            foreach (var item in svm.linecharges)
                            {
                                item.LineTableId = s.PurchaseOrderRateAmendmentLineId;
                                item.PersonID = temp2.SupplierId;
                                item.HeaderTableId = s.PurchaseOrderAmendmentHeaderId;
                                item.ObjectState = Model.ObjectState.Added;
                                db.PurchaseOrderRateAmendmentLineCharge.Add(item);
                            }

                        if (svm.footercharges != null)
                            foreach (var item in svm.footercharges)
                            {

                                if (item.Id > 0)
                                {

                                    var footercharge = new PurchaseOrderAmendmentHeaderChargeService(_unitOfWork).Find(item.Id);
                                    footercharge.Rate = item.Rate;
                                    footercharge.Amount = item.Amount;
                                    footercharge.ObjectState = Model.ObjectState.Modified;
                                    db.PurchaseOrderAmendmentHeaderCharges.Add(footercharge);
                                }

                                else
                                {
                                    item.HeaderTableId = s.PurchaseOrderAmendmentHeaderId;
                                    item.PersonID = temp2.SupplierId;
                                    item.ObjectState = Model.ObjectState.Added;
                                    db.PurchaseOrderAmendmentHeaderCharges.Add(item);
                                }
                            }


                        try
                        {
                            PurchaseOrderAmendmentDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderAmendmentHeaderId, s.PurchaseOrderRateAmendmentLineId, EventModeConstants.Add), ref db);
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
                            PurchaseOrderAmendmentDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderAmendmentHeaderId, s.PurchaseOrderRateAmendmentLineId, EventModeConstants.Add), ref db);
                        }
                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXCL"] += message;
                        }

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = temp2.DocTypeId,
                            DocId = temp2.PurchaseOrderAmendmentHeaderId,
                            DocLineId = s.PurchaseOrderRateAmendmentLineId,
                            ActivityType = (int)ActivityTypeContants.Added,
                            DocNo = temp2.DocNo,
                            DocDate = temp2.DocDate,
                            DocStatus = temp2.Status,
                        }));

                    }

                    return RedirectToAction("_Create", new { id = s.PurchaseOrderAmendmentHeaderId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                PurchaseOrderAmendmentHeader temp = new PurchaseOrderAmendmentHeaderService(db).Find(svm.PurchaseOrderAmendmentHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                PurchaseOrderRateAmendmentLine s = _PurchaseOrderRateAmendmentLineService.Find(svm.PurchaseOrderRateAmendmentLineId);


                PurchaseOrderRateAmendmentLine ExRecLine = new PurchaseOrderRateAmendmentLine();
                ExRecLine = Mapper.Map<PurchaseOrderRateAmendmentLine>(s);


                if (ModelState.IsValid && BeforeSave && !EventException)
                {
                    if (svm.Rate != 0)
                    {

                        s.Remark = svm.Remark;
                        s.AmendedRate = svm.AmendedRate;
                        s.Rate = svm.Rate;
                        s.Amount = svm.Amount;
                        s.ModifiedBy = User.Identity.Name;
                        s.ModifiedDate = DateTime.Now;
                    }

                    s.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderRateAmendmentLine.Add(s);
                    //_PurchaseOrderRateAmendmentLineService.Update(s);

                    new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseRateOnAmendment(s.PurchaseOrderLineId, s.PurchaseOrderRateAmendmentLineId, temp.DocDate, s.Rate, ref db);

                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    //new PurchaseOrderAmendmentHeaderService(_unitOfWork).Update(temp);

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderAmendmentHeader.Add(temp);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
                        Obj = s,
                    });


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = db.PurchaseOrderRateAmendmentLineCharge.Find(item.Id);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseOrderRateAmendmentLineCharge.Add(productcharge);
                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = db.PurchaseOrderAmendmentHeaderCharges.Find(item.Id);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseOrderAmendmentHeaderCharges.Add(footercharge);
                        }


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseOrderAmendmentDocEvents.onLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderAmendmentHeaderId, s.PurchaseOrderRateAmendmentLineId, EventModeConstants.Edit), ref db);
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
                        PurchaseOrderAmendmentDocEvents.afterLineSaveEvent(this, new PurchaseEventArgs(s.PurchaseOrderAmendmentHeaderId, s.PurchaseOrderRateAmendmentLineId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    //SAving the Activity Log::

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = s.PurchaseOrderAmendmentHeaderId,
                        DocLineId = s.PurchaseOrderRateAmendmentLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    //End Of Saving Activity Log

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

        [HttpGet]
        public ActionResult _ModifyLineAfterApprove(int id)
        {
            return _Modify(id);
        }

        [HttpGet]
        private ActionResult _Modify(int id)
        {
            PurchaseOrderRateAmendmentLineViewModel temp = _PurchaseOrderRateAmendmentLineService.GetPurchaseOrderRateAmendmentLine(id);

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

            PurchaseOrderAmendmentHeader header = new PurchaseOrderAmendmentHeaderService(db).Find(temp.PurchaseOrderAmendmentHeaderId);
            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            temp.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);

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
        public ActionResult _DeleteLine_AfterApprove(int id)
        {
            return _Delete(id);
        }


        [HttpGet]
        private ActionResult _Delete(int id)
        {
            PurchaseOrderRateAmendmentLineViewModel temp = _PurchaseOrderRateAmendmentLineService.GetPurchaseOrderRateAmendmentLine(id);

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

            PurchaseOrderAmendmentHeader header = new PurchaseOrderAmendmentHeaderService(db).Find(temp.PurchaseOrderAmendmentHeaderId);

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            temp.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeletePost(PurchaseOrderRateAmendmentLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseOrderAmendmentDocEvents.beforeLineDeleteEvent(this, new PurchaseEventArgs(vm.PurchaseOrderAmendmentHeaderId, vm.PurchaseOrderRateAmendmentLineId), ref db);
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



                PurchaseOrderRateAmendmentLine PurchaseOrderLine = db.PurchaseOrderRateAmendmentLine.Find(vm.PurchaseOrderRateAmendmentLineId);
                PurchaseOrderAmendmentHeader header = new PurchaseOrderAmendmentHeaderService(db).Find(PurchaseOrderLine.PurchaseOrderAmendmentHeaderId);

                new PurchaseOrderLineStatusService(_unitOfWork).UpdatePurchaseRateOnAmendment(PurchaseOrderLine.PurchaseOrderLineId, PurchaseOrderLine.PurchaseOrderRateAmendmentLineId, header.DocDate, 0, ref db);

                var chargeslist = (from p in db.PurchaseOrderRateAmendmentLineCharge
                                   where p.LineTableId == PurchaseOrderLine.PurchaseOrderRateAmendmentLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseOrderRateAmendmentLineCharge.Remove(item);
                    }

                PurchaseOrderLine.ObjectState = Model.ObjectState.Deleted;

                db.PurchaseOrderRateAmendmentLine.Remove(PurchaseOrderLine);

                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderAmendmentHeader.Add(header);
                }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = db.PurchaseOrderAmendmentHeaderCharges.Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.PurchaseOrderAmendmentHeaderCharges.Add(footer);
                    }

                try
                {
                    PurchaseOrderAmendmentDocEvents.onLineDeleteEvent(this, new PurchaseEventArgs(PurchaseOrderLine.PurchaseOrderAmendmentHeaderId, PurchaseOrderLine.PurchaseOrderRateAmendmentLineId), ref db);
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
                    return PartialView("_Create", vm);
                }

                try
                {
                    PurchaseOrderAmendmentDocEvents.afterLineDeleteEvent(this, new PurchaseEventArgs(PurchaseOrderLine.PurchaseOrderAmendmentHeaderId, PurchaseOrderLine.PurchaseOrderRateAmendmentLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }
            }
            return Json(new { success = true });
        }


        public JsonResult GetPendingOrders(int HeaderId, string term, int Limit)
        {
            return Json(_PurchaseOrderRateAmendmentLineService.GetPendingPurchaseOrdersForRateAmndmt(HeaderId, term, Limit).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new PurchaseOrderLineService(_unitOfWork).GetLineDetail(LineId));
        }

        public JsonResult ValidatePurchaseOrder(int LineId, int HeaderId)
        {
            return Json(_PurchaseOrderRateAmendmentLineService.ValidatePurchaseOrder(LineId, HeaderId));
        }

        public JsonResult GetPendingPurchaseOrders(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _PurchaseOrderRateAmendmentLineService.GetPendingPurchaseOrdersForRateAmendment(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _PurchaseOrderRateAmendmentLineService.GetPendingPurchaseOrdersForRateAmendment(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingPurchaseOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _PurchaseOrderRateAmendmentLineService.GetPendingProductsForRateAmndmt(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _PurchaseOrderRateAmendmentLineService.GetPendingProductsForRateAmndmt(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

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

    }

}
