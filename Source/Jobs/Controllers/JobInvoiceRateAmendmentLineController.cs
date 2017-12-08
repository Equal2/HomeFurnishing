using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using AutoMapper;
using Model.ViewModels;
using Jobs.Helpers;
using Model;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using JobInvoiceAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobInvoiceRateAmendmentLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        IJobInvoiceRateAmendmentLineService _JobInvoiceRateAmendmentLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        public JobInvoiceRateAmendmentLineController(IJobInvoiceRateAmendmentLineService JobInvoiceRateAmendmentLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceRateAmendmentLineService = JobInvoiceRateAmendmentLineService;
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
            var p = _JobInvoiceRateAmendmentLineService.GetJobInvoiceRateAmendmentLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int? sid)
        {
            JobInvoiceAmendmentFilterViewModel vm = new JobInvoiceAmendmentFilterViewModel();
            vm.JobInvoiceAmendmentHeaderId = id;
            vm.JobWorkerId = sid;
            JobInvoiceAmendmentHeader Header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobInvoiceAmendmentFilterViewModel vm)
        {
            List<JobInvoiceRateAmendmentLineViewModel> temp = _JobInvoiceRateAmendmentLineService.GetJobInvoiceLineForMultiSelect(vm).ToList();
            JobInvoiceAmendmentMasterDetailModel svm = new JobInvoiceAmendmentMasterDetailModel();
            svm.JobInvoiceRateAmendmentLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobInvoiceAmendmentMasterDetailModel vm)
        {
            int Serial = _JobInvoiceRateAmendmentLineService.GetMaxSr(vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId);
            var Header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId);
            JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();

            List<LineReferenceIds> RefIds = new List<LineReferenceIds>();
            bool HeaderChargeEdit = false;

            int? MaxLineId = new JobInvoiceRateAmendmentLineChargeService(db).GetMaxProductCharge(Header.JobInvoiceAmendmentHeaderId, "Web.JobInvoiceRateAmendmentLines", "JobInvoiceAmendmentHeaderId", "JobInvoiceRateAmendmentLineId");

            int pk = 0;
            int PersonCount = 0;
            //int? CalculationId = Settings.CalculationId;
            List<LineDetailListViewModel> LineList = new List<LineDetailListViewModel>();

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceAmendmentDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");


            int? CalculationId = 0;

            int JobInvoiceLineId = vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceLineId;
            var SalesTaxGroupPerson = (from L in db.JobInvoiceLine where L.JobInvoiceLineId == JobInvoiceLineId select new { SalesTaxGroupPersonId = (int?)L.JobInvoiceHeader.SalesTaxGroupPerson.ChargeGroupPersonId ?? 0 }).FirstOrDefault();
            if (SalesTaxGroupPerson != null)
            {
                CalculationId = new ChargeGroupPersonCalculationService(_unitOfWork).GetChargeGroupPersonCalculation(Header.DocTypeId, SalesTaxGroupPerson.SalesTaxGroupPersonId, Header.SiteId, Header.DivisionId) ?? 0;
            }
            if (CalculationId == 0)
                CalculationId = Settings.CalculationId ?? 0;

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.JobInvoiceRateAmendmentLineViewModel.Where(m => (m.AmendedRate - m.JobInvoiceRate) != 0 && m.AAmended == false))
                {
                    

                    JobInvoiceRateAmendmentLine line = new JobInvoiceRateAmendmentLine();

                    line.JobInvoiceAmendmentHeaderId = item.JobInvoiceAmendmentHeaderId;
                    line.JobInvoiceLineId = item.JobInvoiceLineId;
                    line.Qty = item.Qty;
                    line.AmendedRate = item.AmendedRate;
                    line.Rate = item.AmendedRate - item.JobInvoiceRate;
                    line.Amount = DecimalRoundOff.amountToFixed(item.DealQty * line.Rate, Settings.AmountRoundOff);
                    line.JobInvoiceRate = item.JobInvoiceRate;
                    line.JobWorkerId = item.JobWorkerId;
                    line.Sr = Serial++;
                    line.JobInvoiceRateAmendmentLineId = pk;
                    line.CreatedDate = DateTime.Now;
                    line.ModifiedDate = DateTime.Now;
                    line.CreatedBy = User.Identity.Name;
                    line.ModifiedBy = User.Identity.Name;
                    line.Remark = item.Remark;
                    LineStatus.Add(line.JobInvoiceLineId, line.Rate);
                    RefIds.Add(new LineReferenceIds { LineId = line.JobInvoiceRateAmendmentLineId, RefLineId = line.JobInvoiceLineId });

                    line.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceRateAmendmentLine.Add(line);

                    LineList.Add(new LineDetailListViewModel { Amount = line.Amount, Rate = line.Rate, LineTableId = line.JobInvoiceRateAmendmentLineId, HeaderTableId = item.JobInvoiceAmendmentHeaderId, PersonID = Header.JobWorkerId, DealQty = 0 });
                    //_JobInvoiceRateAmendmentLineService.Create(line);
                    pk++;
                }

                //Commented No InvoiceLineStatus
                //new JobInvoiceLineStatusService(_unitOfWork).UpdateJobRateOnAmendmentMultiple(LineStatus, Header.DocDate, ref db);


                if (Header.Status != (int)StatusConstants.Drafted && Header.Status != (int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobInvoiceAmendmentHeader.Add(Header);




                int[] RecLineIds = null;
                RecLineIds = RefIds.Select(m => m.RefLineId).ToArray();

                var Charges = (from p in db.JobInvoiceLine
                               where RecLineIds.Contains(p.JobInvoiceLineId)
                               join LineCharge in db.JobInvoiceLineCharge on p.JobInvoiceLineId equals LineCharge.LineTableId
                               join HeaderCharge in db.JobInvoiceHeaderCharges on p.JobInvoiceHeaderId equals HeaderCharge.HeaderTableId
                               group new { p, LineCharge, HeaderCharge } by new { p.JobInvoiceLineId } into g
                               select new
                               {
                                   LineId = g.Key.JobInvoiceLineId,
                                   HeaderCharges = g.Select(m => m.HeaderCharge).ToList(),
                                   Linecharges = g.Select(m => m.LineCharge).ToList(),
                               }).ToList();



                var LineListWithReferences = (from p in LineList
                                              join t in RefIds on p.LineTableId equals t.LineId
                                              join t2 in Charges on t.RefLineId equals t2.LineId into table
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
                                                  CostCenterId = p.CostCenterId,
                                                  RLineCharges = (LineLis == null ? null : Mapper.Map<List<LineChargeViewModel>>(LineLis.Linecharges)),
                                              }).ToList();


                if (CalculationId != null)
                    new ChargesCalculationService(_unitOfWork).CalculateCharges(LineListWithReferences, vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId, (int)CalculationId, MaxLineId, out LineCharges, out HeaderChargeEdit, out HeaderCharges, "Web.JobInvoiceAmendmentHeaderCharges", "Web.JobInvoiceRateAmendmentLineCharges", out PersonCount, Header.DocTypeId, Header.SiteId, Header.DivisionId);

                //Saving Charges
                foreach (var item in LineCharges)
                {

                    JobInvoiceRateAmendmentLineCharge PoLineCharge = Mapper.Map<LineChargeViewModel, JobInvoiceRateAmendmentLineCharge>(item);
                    PoLineCharge.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceRateAmendmentLineCharge.Add(PoLineCharge);
                }


                //Saving Header charges
                for (int i = 0; i < HeaderCharges.Count(); i++)
                {

                    if (!HeaderChargeEdit)
                    {
                        JobInvoiceAmendmentHeaderCharge POHeaderCharge = Mapper.Map<HeaderChargeViewModel, JobInvoiceAmendmentHeaderCharge>(HeaderCharges[i]);
                        POHeaderCharge.HeaderTableId = vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId;
                        POHeaderCharge.PersonID = Header.JobWorkerId;
                        POHeaderCharge.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceAmendmentHeaderCharge.Add(POHeaderCharge);
                    }
                    else
                    {
                        var footercharge = new JobInvoiceAmendmentHeaderChargeService(db).Find(HeaderCharges[i].Id);
                        footercharge.Rate = HeaderCharges[i].Rate;
                        footercharge.Amount = HeaderCharges[i].Amount;
                        footercharge.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceAmendmentHeaderCharge.Add(footercharge);
                    }

                }


                try
                {
                    JobInvoiceAmendmentDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId), ref db);
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
                    JobInvoiceAmendmentDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobInvoiceRateAmendmentLineViewModel.FirstOrDefault().JobInvoiceAmendmentHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobInvoiceAmendmentHeaderId,
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
            JobInvoiceAmendmentHeader header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(Id);
            JobInvoiceRateAmendmentLineViewModel svm = new JobInvoiceRateAmendmentLineViewModel();

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            ViewBag.LineMode = "Create";
            svm.JobInvoiceAmendmentHeaderId = Id;
            svm.JobWorkerId = sid.HasValue ? sid.Value : 0;
            svm.DocTypeId = header.DocTypeId;
            svm.SiteId = header.SiteId;
            svm.DivisionId = header.DivisionId;
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobInvoiceRateAmendmentLineViewModel svm)
        {
            bool BeforeSave = true;
            if (svm.JobInvoiceRateAmendmentLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (svm.JobWorkerId == 0)
            {
                ModelState.AddModelError("JobWorkerId", "The JobWorker field is required");
            }

            try
            {

                if (svm.JobInvoiceRateAmendmentLineId <= 0)
                    BeforeSave = JobInvoiceAmendmentDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceAmendmentHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceAmendmentDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobInvoiceAmendmentHeaderId, EventModeConstants.Edit), ref db);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXCL"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save.");

            if (svm.JobInvoiceRateAmendmentLineId <= 0)
            {
                JobInvoiceRateAmendmentLine s = new JobInvoiceRateAmendmentLine();

                if (ModelState.IsValid && BeforeSave && !EventException)
                {

                    if (svm.Rate != 0)
                    {
                        s.Remark = svm.Remark;
                        s.JobInvoiceAmendmentHeaderId = svm.JobInvoiceAmendmentHeaderId;
                        s.JobInvoiceLineId = svm.JobInvoiceLineId;
                        s.Qty = svm.Qty;
                        s.AmendedRate = svm.AmendedRate;
                        s.Amount = svm.Amount;
                        s.JobInvoiceRate = svm.JobInvoiceRate;
                        s.JobWorkerId = svm.JobWorkerId;
                        s.Rate = svm.Rate;
                        s.Remark = svm.Remark;
                        s.Sr = _JobInvoiceRateAmendmentLineService.GetMaxSr(s.JobInvoiceAmendmentHeaderId);
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now;
                        s.CreatedBy = User.Identity.Name;
                        s.ModifiedBy = User.Identity.Name;
                        s.ObjectState = Model.ObjectState.Added;
                        db.JobInvoiceRateAmendmentLine.Add(s);



                        if (svm.linecharges != null)
                            foreach (var item in svm.linecharges)
                            {
                                item.LineTableId = s.JobInvoiceRateAmendmentLineId;
                                item.PersonID = s.JobWorkerId;
                                item.HeaderTableId = s.JobInvoiceAmendmentHeaderId;                                
                                item.ObjectState = Model.ObjectState.Added;
                                db.JobInvoiceRateAmendmentLineCharge.Add(item);
                            }

                        if (svm.footercharges != null)
                        {
                            int PersonCount = (from p in db.JobInvoiceRateAmendmentLine
                                               where p.JobInvoiceAmendmentHeaderId== s.JobInvoiceAmendmentHeaderId
                                               group p by p.JobWorkerId into g
                                               select g).Count();

                            foreach (var item in svm.footercharges)
                            {

                                if (item.Id > 0)
                                {


                                    var footercharge = new JobInvoiceAmendmentHeaderChargeService(db).Find(item.Id);
                                    if (PersonCount > 1 || footercharge.PersonID != s.JobWorkerId)
                                        footercharge.PersonID = null;

                                    footercharge.Rate = item.Rate;
                                    footercharge.Amount = item.Amount;
                                    footercharge.ObjectState = Model.ObjectState.Modified;
                                    db.JobInvoiceAmendmentHeaderCharge.Add(footercharge);
                                }

                                else
                                {
                                    item.HeaderTableId = s.JobInvoiceAmendmentHeaderId;
                                    item.PersonID = s.JobWorkerId;
                                    item.ObjectState = Model.ObjectState.Added;
                                    db.JobInvoiceAmendmentHeaderCharge.Add(item);
                                }
                            }

                        }


                        JobInvoiceAmendmentHeader temp2 = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(s.JobInvoiceAmendmentHeaderId);
                        if (temp2.Status != (int)StatusConstants.Drafted && temp2.Status != (int)StatusConstants.Import)
                        {
                            temp2.Status = (int)StatusConstants.Modified;
                            temp2.ModifiedBy = User.Identity.Name;
                            temp2.ModifiedDate = DateTime.Now;

                        }

                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceAmendmentHeader.Add(temp2);

                        //Commented No InvoiceLineStatus
                        //new JobInvoiceLineStatusService(_unitOfWork).UpdateJobRateOnAmendment(svm.JobInvoiceLineId, s.JobInvoiceRateAmendmentLineId, temp2.DocDate, s.Rate, ref db);

                        try
                        {
                            JobInvoiceAmendmentDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobInvoiceAmendmentHeaderId, s.JobInvoiceRateAmendmentLineId, EventModeConstants.Add), ref db);
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
                            return PartialView("_Create", svm);
                        }
                        try
                        {
                            JobInvoiceAmendmentDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobInvoiceAmendmentHeaderId, s.JobInvoiceRateAmendmentLineId, EventModeConstants.Add), ref db);
                        }
                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXCL"] += message;
                        }

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = temp2.DocTypeId,
                            DocId = temp2.JobInvoiceAmendmentHeaderId,
                            DocLineId = s.JobInvoiceRateAmendmentLineId,
                            ActivityType = (int)ActivityTypeContants.Added,
                            DocNo = temp2.DocNo,
                            DocDate = temp2.DocDate,
                            DocStatus = temp2.Status,
                        }));

                    }

                    if (!svm.JobInvoiceSettings.isVisibleHeaderJobWorker)
                        return RedirectToAction("_Create", new { id = svm.JobInvoiceAmendmentHeaderId });
                    else
                        return RedirectToAction("_Create", new { id = svm.JobInvoiceAmendmentHeaderId, sid = svm.JobWorkerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                JobInvoiceAmendmentHeader temp = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(svm.JobInvoiceAmendmentHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                JobInvoiceRateAmendmentLine s = _JobInvoiceRateAmendmentLineService.Find(svm.JobInvoiceRateAmendmentLineId);


                JobInvoiceRateAmendmentLine ExRecLine = new JobInvoiceRateAmendmentLine();
                ExRecLine = Mapper.Map<JobInvoiceRateAmendmentLine>(s);


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
                    db.JobInvoiceRateAmendmentLine.Add(s);                    

                    //Commented No InvoiceLineStatus
                    //new JobInvoiceLineStatusService(_unitOfWork).UpdateJobRateOnAmendment(s.JobInvoiceLineId, s.JobInvoiceRateAmendmentLineId, temp.DocDate, s.Rate, ref db);

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceAmendmentHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
                        Obj = s,
                    });


                    if (svm.linecharges != null)
                        foreach (var item in svm.linecharges)
                        {
                            var productcharge = new JobInvoiceRateAmendmentLineChargeService(db).Find(item.Id);

                            JobInvoiceRateAmendmentLineCharge ExRecC = new JobInvoiceRateAmendmentLineCharge();
                            ExRecC = Mapper.Map<JobInvoiceRateAmendmentLineCharge>(productcharge);

                            productcharge.Rate = item.Rate;
                            productcharge.Amount = item.Amount;
                            productcharge.DealQty = item.DealQty;
                            productcharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceRateAmendmentLineCharge.Add(productcharge);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecC,
                                Obj = productcharge,
                            });
                        }


                    if (svm.footercharges != null)
                        foreach (var item in svm.footercharges)
                        {
                            var footercharge = new JobInvoiceAmendmentHeaderChargeService(db).Find(item.Id);

                            JobInvoiceAmendmentHeaderCharge ExRecC = new JobInvoiceAmendmentHeaderCharge();
                            ExRecC = Mapper.Map<JobInvoiceAmendmentHeaderCharge>(footercharge);

                            footercharge.Rate = item.Rate;
                            footercharge.Amount = item.Amount;
                            footercharge.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceAmendmentHeaderCharge.Add(footercharge);

                            LogList.Add(new LogTypeViewModel
                            {
                                ExObj = ExRecC,
                                Obj = footercharge,
                            });
                        }



                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceAmendmentDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobInvoiceAmendmentHeaderId, s.JobInvoiceRateAmendmentLineId, EventModeConstants.Edit), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    try
                    {
                        JobInvoiceAmendmentDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobInvoiceAmendmentHeaderId, s.JobInvoiceRateAmendmentLineId, EventModeConstants.Edit), ref db);
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
                        DocId = temp.JobInvoiceAmendmentHeaderId,
                        DocLineId = s.JobInvoiceRateAmendmentLineId,
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
            JobInvoiceRateAmendmentLineViewModel temp = _JobInvoiceRateAmendmentLineService.GetJobInvoiceRateAmendmentLine(id);

            if (temp == null)
            {
                return HttpNotFound();
            }

            JobInvoiceAmendmentHeader header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(temp.JobInvoiceAmendmentHeaderId);

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
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

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
            JobInvoiceRateAmendmentLineViewModel temp = _JobInvoiceRateAmendmentLineService.GetJobInvoiceRateAmendmentLine(id);

            if (temp == null) { return HttpNotFound(); }

            JobInvoiceAmendmentHeader header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(temp.JobInvoiceAmendmentHeaderId);

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

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

            return PartialView("_Create", temp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JobInvoiceRateAmendmentLine line = db.JobInvoiceRateAmendmentLine.Find(id);
            line.ObjectState = ObjectState.Deleted;
            int HeaderId = line.JobInvoiceAmendmentHeaderId;
            _JobInvoiceRateAmendmentLineService.Delete(id);

            _unitOfWork.Save();

            return RedirectToAction("Index", new { Id = HeaderId }).Success("Data deleted successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobInvoiceRateAmendmentLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceAmendmentDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobInvoiceAmendmentHeaderId, vm.JobInvoiceRateAmendmentLineId), ref db);
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

                JobInvoiceRateAmendmentLine JobInvoiceLine = db.JobInvoiceRateAmendmentLine.Find(vm.JobInvoiceRateAmendmentLineId);
                JobInvoiceAmendmentHeader header = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(JobInvoiceLine.JobInvoiceAmendmentHeaderId);

                //Commented No InvoiceLineStatus
                //new JobInvoiceLineStatusService(_unitOfWork).UpdateJobRateOnAmendment(JobInvoiceLine.JobInvoiceLineId, JobInvoiceLine.JobInvoiceRateAmendmentLineId, header.DocDate, 0, ref db);

                JobInvoiceLine.ObjectState = Model.ObjectState.Deleted;

                db.JobInvoiceRateAmendmentLine.Remove(JobInvoiceLine);

                if (header.Status != (int)StatusConstants.Drafted && header.Status != (int)StatusConstants.Import)
                {
                    header.Status = (int)StatusConstants.Modified;                    
                    header.ObjectState = Model.ObjectState.Modified;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    db.JobInvoiceAmendmentHeader.Add(header);
                }

                var chargeslist = (from p in db.JobInvoiceRateAmendmentLineCharge
                                   where p.LineTableId == vm.JobInvoiceRateAmendmentLineId
                                   select p).ToList();

                if (chargeslist != null)
                    foreach (var item in chargeslist)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobInvoiceRateAmendmentLineCharge.Remove(item);
                    }

                if (vm.footercharges != null)
                    foreach (var item in vm.footercharges)
                    {
                        var footer = new JobInvoiceAmendmentHeaderChargeService(db).Find(item.Id);
                        footer.Rate = item.Rate;
                        footer.Amount = item.Amount;
                        footer.ObjectState = Model.ObjectState.Modified;
                        db.JobInvoiceAmendmentHeaderCharge.Add(footer);
                    }

                try
                {
                    JobInvoiceAmendmentDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobInvoiceLine.JobInvoiceAmendmentHeaderId, JobInvoiceLine.JobInvoiceRateAmendmentLineId), ref db);
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
                    JobInvoiceAmendmentDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobInvoiceLine.JobInvoiceAmendmentHeaderId, JobInvoiceLine.JobInvoiceRateAmendmentLineId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }
            }
            return Json(new { success = true });
        }


        public JsonResult GetPendingInvoices(int HeaderId, string term, int Limit)
        {
            return Json(_JobInvoiceRateAmendmentLineService.GetPendingJobInvoicesForRateAmndmt(HeaderId, term, Limit).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new JobInvoiceLineService(_unitOfWork).GetLineDetail(LineId));
        }

        public JsonResult ValidateJobInvoice(int LineId, int HeaderId)
        {
            return Json(new JobInvoiceLineService(_unitOfWork).ValidateJobInvoice(LineId, HeaderId));
        }

        public JsonResult GetLineDetailFromUId(string UID, int HeaderId)
        {

            var LineDetail = _JobInvoiceRateAmendmentLineService.GetLineDetailFromUId(UID, HeaderId);

            return Json(new { success = LineDetail != null ? true : false, LineDetail = LineDetail });

        }

        public JsonResult GetPendingJobInvoices(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _JobInvoiceRateAmendmentLineService.GetPendingJobInvoicesForRateAmendment(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobInvoiceRateAmendmentLineService.GetPendingJobInvoicesForRateAmendment(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingJobInvoiceProducts(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _JobInvoiceRateAmendmentLineService.GetPendingProductsForRateAmndmt(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobInvoiceRateAmendmentLineService.GetPendingProductsForRateAmndmt(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingJobInvoiceJobWorkers(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _JobInvoiceRateAmendmentLineService.GetPendingJobWorkersForRateAmndmt(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobInvoiceRateAmendmentLineService.GetPendingJobWorkersForRateAmndmt(filter, searchTerm).Count();

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
