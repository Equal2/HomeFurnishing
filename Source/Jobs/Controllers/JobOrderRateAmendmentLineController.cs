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
using JobOrderAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderRateAmendmentLineController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        private bool EventException = false;
        IJobOrderRateAmendmentLineService _JobOrderRateAmendmentLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public JobOrderRateAmendmentLineController(IJobOrderRateAmendmentLineService JobOrderRateAmendmentLineService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderRateAmendmentLineService = JobOrderRateAmendmentLineService;
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
            var p = _JobOrderRateAmendmentLineService.GetJobOrderRateAmendmentLineForHeader(id);
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ForOrder(int id, int? sid)
        {
            JobOrderAmendmentFilterViewModel vm = new JobOrderAmendmentFilterViewModel();
            vm.JobOrderAmendmentHeaderId = id;
            vm.JobWorkerId = sid;
            JobOrderAmendmentHeader Header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            return PartialView("_Filters", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(JobOrderAmendmentFilterViewModel vm)
        {
            List<JobOrderRateAmendmentLineViewModel> temp = _JobOrderRateAmendmentLineService.GetJobOrderLineForMultiSelect(vm).ToList();
            JobOrderAmendmentMasterDetailModel svm = new JobOrderAmendmentMasterDetailModel();
            svm.JobOrderRateAmendmentLineViewModel = temp;
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(JobOrderAmendmentMasterDetailModel vm)
        {
            int Serial = _JobOrderRateAmendmentLineService.GetMaxSr(vm.JobOrderRateAmendmentLineViewModel.FirstOrDefault().JobOrderAmendmentHeaderId);
            var Header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(vm.JobOrderRateAmendmentLineViewModel.FirstOrDefault().JobOrderAmendmentHeaderId);
            Dictionary<int, decimal> LineStatus = new Dictionary<int, decimal>();

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderAmendmentDocEvents.beforeLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderRateAmendmentLineViewModel.FirstOrDefault().JobOrderAmendmentHeaderId), ref db);
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
                foreach (var item in vm.JobOrderRateAmendmentLineViewModel.Where(m => (m.AmendedRate - m.JobOrderRate) != 0 && m.AAmended == false))
                {

                    JobOrderRateAmendmentLine line = new JobOrderRateAmendmentLine();

                    line.JobOrderAmendmentHeaderId = item.JobOrderAmendmentHeaderId;
                    line.JobOrderLineId = item.JobOrderLineId;
                    line.Qty = item.Qty;
                    line.AmendedRate = item.AmendedRate;
                    line.Rate = item.AmendedRate - item.JobOrderRate;
                    line.Amount = DecimalRoundOff.amountToFixed((item.DealQty * line.Rate), Settings.AmountRoundOff);
                    line.JobOrderRate = item.JobOrderRate;
                    line.JobWorkerId = item.JobWorkerId;
                    line.Sr = Serial++;
                    line.CreatedDate = DateTime.Now;
                    line.ModifiedDate = DateTime.Now;
                    line.CreatedBy = User.Identity.Name;
                    line.ModifiedBy = User.Identity.Name;
                    line.Remark = item.Remark;
                    LineStatus.Add(line.JobOrderLineId, line.Rate);

                    line.ObjectState = Model.ObjectState.Added;
                    db.JobOrderRateAmendmentLine.Add(line);

                    //_JobOrderRateAmendmentLineService.Create(line);                    
                }


                if(Header.Status !=(int)StatusConstants.Drafted && Header.Status!=(int)StatusConstants.Import)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }

                Header.ObjectState = Model.ObjectState.Modified;
                db.JobOrderAmendmentHeader.Add(Header);

                new JobOrderLineStatusService(_unitOfWork).UpdateJobRateOnAmendmentMultiple(LineStatus, Header.DocDate, ref db);

                try
                {
                    JobOrderAmendmentDocEvents.onLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderRateAmendmentLineViewModel.FirstOrDefault().JobOrderAmendmentHeaderId), ref db);
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
                    JobOrderAmendmentDocEvents.afterLineSaveBulkEvent(this, new JobEventArgs(vm.JobOrderRateAmendmentLineViewModel.FirstOrDefault().JobOrderAmendmentHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.JobOrderAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.MultipleCreate,                    
                    DocNo = Header.DocNo,
                    DocDate = Header.DocDate,
                    DocStatus=Header.Status,
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
            JobOrderAmendmentHeader header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(Id);
            JobOrderRateAmendmentLineViewModel svm = new JobOrderRateAmendmentLineViewModel();

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
            svm.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            svm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);
            ViewBag.LineMode = "Create";
            svm.JobOrderAmendmentHeaderId = Id;
            svm.JobWorkerId = sid.HasValue ? sid.Value : 0;
            return PartialView("_Create", svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(JobOrderRateAmendmentLineViewModel svm)
        {
            bool BeforeSave = true;
            if (svm.JobOrderRateAmendmentLineId <= 0)
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

            #region BeforeSave
            try
            {
                if (svm.JobOrderRateAmendmentLineId <= 0)
                    BeforeSave = JobOrderAmendmentDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderAmendmentHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobOrderAmendmentDocEvents.beforeLineSaveEvent(this, new JobEventArgs(svm.JobOrderAmendmentHeaderId, EventModeConstants.Edit), ref db);
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

            JobOrderAmendmentHeader temp2 = new JobOrderAmendmentHeaderService(_unitOfWork).Find(svm.JobOrderAmendmentHeaderId);

            JobOrderSettings Settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(temp2.DocTypeId, temp2.DivisionId, temp2.SiteId);

            if (svm.JobOrderRateAmendmentLineId <= 0)
            {
                JobOrderRateAmendmentLine s = new JobOrderRateAmendmentLine();

                if (ModelState.IsValid && BeforeSave && !EventException)
                {

                    if (svm.Rate != 0)
                    {
                        s.Remark = svm.Remark;
                        s.JobOrderAmendmentHeaderId = svm.JobOrderAmendmentHeaderId;
                        s.JobOrderLineId = svm.JobOrderLineId;
                        s.Qty = svm.Qty;
                        s.AmendedRate = svm.AmendedRate;
                        s.Amount = svm.Amount;
                        s.JobOrderRate = svm.JobOrderRate;
                        s.JobWorkerId = svm.JobWorkerId;
                        s.Rate = svm.Rate;
                        s.Remark = svm.Remark;
                        s.Sr = _JobOrderRateAmendmentLineService.GetMaxSr(s.JobOrderAmendmentHeaderId);
                        s.CreatedDate = DateTime.Now;
                        s.ModifiedDate = DateTime.Now;
                        s.CreatedBy = User.Identity.Name;
                        s.ModifiedBy = User.Identity.Name;
                        //_JobOrderRateAmendmentLineService.Create(s);
                        s.ObjectState = Model.ObjectState.Added;
                        db.JobOrderRateAmendmentLine.Add(s);

                        if (temp2.Status != (int)StatusConstants.Drafted)
                        {
                            temp2.Status = (int)StatusConstants.Modified;
                            temp2.ModifiedBy = User.Identity.Name;
                            temp2.ModifiedDate = DateTime.Now;
                        }

                        //new JobOrderAmendmentHeaderService(_unitOfWork).Update(temp2);
                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.JobOrderAmendmentHeader.Add(temp2);

                        new JobOrderLineStatusService(_unitOfWork).UpdateJobRateOnAmendment(svm.JobOrderLineId, s.JobOrderRateAmendmentLineId, temp2.DocDate, s.Rate, ref db);

                        try
                        {
                            JobOrderAmendmentDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobOrderAmendmentHeaderId, s.JobOrderRateAmendmentLineId, EventModeConstants.Add), ref db);
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
                            JobOrderAmendmentDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobOrderAmendmentHeaderId, s.JobOrderRateAmendmentLineId, EventModeConstants.Add), ref db);
                        }
                        catch (Exception ex)
                        {
                            string message = _exception.HandleException(ex);
                            TempData["CSEXCL"] += message;
                        }

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = temp2.DocTypeId,
                            DocId = temp2.JobOrderAmendmentHeaderId,
                            DocLineId = s.JobOrderRateAmendmentLineId,
                            ActivityType = (int)ActivityTypeContants.Added,                           
                            DocNo = temp2.DocNo,
                            DocDate = temp2.DocDate,
                            DocStatus=temp2.Status,
                        }));

                    }

                    if (svm.JobOrderSettings.isVisibleJobWorkerLine)
                        return RedirectToAction("_Create", new { id = s.JobOrderAmendmentHeaderId });
                    else
                        return RedirectToAction("_Create", new { id = s.JobOrderAmendmentHeaderId, sid = svm.JobWorkerId });
                }
                return PartialView("_Create", svm);


            }
            else
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                JobOrderAmendmentHeader temp = new JobOrderAmendmentHeaderService(_unitOfWork).Find(svm.JobOrderAmendmentHeaderId);
                int status = temp.Status;
                StringBuilder logstring = new StringBuilder();

                JobOrderRateAmendmentLine s = _JobOrderRateAmendmentLineService.Find(svm.JobOrderRateAmendmentLineId);


                JobOrderRateAmendmentLine ExRecLine = new JobOrderRateAmendmentLine();
                ExRecLine = Mapper.Map<JobOrderRateAmendmentLine>(s);


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
                    db.JobOrderRateAmendmentLine.Add(s);
                    //_JobOrderRateAmendmentLineService.Update(s);

                    new JobOrderLineStatusService(_unitOfWork).UpdateJobRateOnAmendment(s.JobOrderLineId, s.JobOrderRateAmendmentLineId, temp.DocDate, s.Rate, ref db);

                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    //new JobOrderAmendmentHeaderService(_unitOfWork).Update(temp);

                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderAmendmentHeader.Add(temp);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
                        Obj = s,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobOrderAmendmentDocEvents.onLineSaveEvent(this, new JobEventArgs(s.JobOrderAmendmentHeaderId, s.JobOrderRateAmendmentLineId, EventModeConstants.Edit), ref db);
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
                        JobOrderAmendmentDocEvents.afterLineSaveEvent(this, new JobEventArgs(s.JobOrderAmendmentHeaderId, s.JobOrderRateAmendmentLineId, EventModeConstants.Edit), ref db);
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
                        DocId = temp.JobOrderAmendmentHeaderId,
                        DocLineId = s.JobOrderRateAmendmentLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,                       
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        xEModifications = Modifications,
                        DocStatus=temp.Status,
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
            JobOrderRateAmendmentLineViewModel temp = _JobOrderRateAmendmentLineService.GetJobOrderRateAmendmentLine(id);

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

            JobOrderAmendmentHeader header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(temp.JobOrderAmendmentHeaderId);
            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);

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
            JobOrderRateAmendmentLineViewModel temp = _JobOrderRateAmendmentLineService.GetJobOrderRateAmendmentLine(id);

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

            JobOrderAmendmentHeader header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(temp.JobOrderAmendmentHeaderId);

            //Getting Settings
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);

            temp.JobOrderSettings = Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(header.DocTypeId);

            return PartialView("_Create", temp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JobOrderRateAmendmentLine line = db.JobOrderRateAmendmentLine.Find(id);
            line.ObjectState = ObjectState.Deleted;
            int HeaderId = line.JobOrderAmendmentHeaderId;
            _JobOrderRateAmendmentLineService.Delete(id);

            _unitOfWork.Save();

            return RedirectToAction("Index", new { Id = HeaderId }).Success("Data deleted successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(JobOrderRateAmendmentLineViewModel vm)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobOrderAmendmentDocEvents.beforeLineDeleteEvent(this, new JobEventArgs(vm.JobOrderAmendmentHeaderId, vm.JobOrderRateAmendmentLineId), ref db);
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



                JobOrderRateAmendmentLine JobOrderLine = db.JobOrderRateAmendmentLine.Find(vm.JobOrderRateAmendmentLineId);
                JobOrderAmendmentHeader header = new JobOrderAmendmentHeaderService(_unitOfWork).Find(JobOrderLine.JobOrderAmendmentHeaderId);

                new JobOrderLineStatusService(_unitOfWork).UpdateJobRateOnAmendment(JobOrderLine.JobOrderLineId, JobOrderLine.JobOrderRateAmendmentLineId, header.DocDate, 0, ref db);

                //_JobOrderRateAmendmentLineService.Delete(vm.JobOrderRateAmendmentLineId);

                JobOrderLine.ObjectState = Model.ObjectState.Deleted;

                db.JobOrderRateAmendmentLine.Remove(JobOrderLine);

                if (header.Status != (int)StatusConstants.Drafted)
                {
                    header.Status = (int)StatusConstants.Modified;
                    //new JobOrderAmendmentHeaderService(_unitOfWork).Update(header);
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.ObjectState = Model.ObjectState.Modified;
                    db.JobOrderAmendmentHeader.Add(header);
                }

                try
                {
                    JobOrderAmendmentDocEvents.onLineDeleteEvent(this, new JobEventArgs(JobOrderLine.JobOrderAmendmentHeaderId, JobOrderLine.JobOrderRateAmendmentLineId), ref db);
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
                    JobOrderAmendmentDocEvents.afterLineDeleteEvent(this, new JobEventArgs(JobOrderLine.JobOrderAmendmentHeaderId, JobOrderLine.JobOrderRateAmendmentLineId), ref db);
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
            return Json(_JobOrderRateAmendmentLineService.GetPendingJobOrdersForRateAmndmt(HeaderId, term, Limit).ToList());
        }

        public JsonResult GetLineDetail(int LineId)
        {
            return Json(new JobOrderLineService(_unitOfWork).GetLineDetail(LineId));
        }

        public JsonResult ValidateJobOrder(int LineId, int HeaderId)
        {
            return Json(new JobOrderLineService(_unitOfWork).ValidateJobOrder(LineId, HeaderId));
        }

        public JsonResult GetLineDetailFromUId(string UID)
        {
            return Json(new JobOrderLineService(_unitOfWork).GetLineDetailFromUId(UID));
        }

        public JsonResult GetPendingJobOrders(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _JobOrderRateAmendmentLineService.GetPendingJobOrdersForRateAmendment(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobOrderRateAmendmentLineService.GetPendingJobOrdersForRateAmendment(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetPendingJobOrderProducts(string searchTerm, int pageSize, int pageNum, int filter)//Order Header ID
        {
            var temp = _JobOrderRateAmendmentLineService.GetPendingProductsForRateAmndmt(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _JobOrderRateAmendmentLineService.GetPendingProductsForRateAmndmt(filter, searchTerm).Count();

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
