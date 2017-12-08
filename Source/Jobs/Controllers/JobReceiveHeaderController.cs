using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using Jobs.Helpers;
using System.Configuration;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using JobReceiveDocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobReceiveHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobReceiveHeaderService _JobReceiveHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobReceiveHeaderController(IJobReceiveHeaderService JobReceiveHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveHeaderService = JobReceiveHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!JobReceiveEvents.Initialized)
            {
                JobReceiveEvents Obj = new JobReceiveEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public ActionResult DocumentTypeIndex(int id)//DocumentCategoryId
        {
            var p = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var  SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, DivisionId, SiteId);
            if(settings !=null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.SqlProcGatePass = settings.SqlProcGatePass;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }

        }

        // GET: /JobReceiveHeaderMaster/

        public ActionResult Index(int id, string IndexType)//DocumentTypeId
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            var JobReceiveHeader = _JobReceiveHeaderService.GetJobReceiveHeaderList(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobReceiveHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<JobReceiveIndexViewModel> p = _JobReceiveHeaderService.GetJobReceiveHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }
        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<JobReceiveIndexViewModel> p = _JobReceiveHeaderService.GetJobReceiveHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }
        // GET: /JobReceiveHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            JobReceiveHeaderViewModel vm = new JobReceiveHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = id }).Warning("Please create job receive settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                vm.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            vm.ProcessId = settings.ProcessId;
            vm.DocDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            PrepareViewBag(id);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobReceiveHeaderViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (vm.JobReceiveHeaderId <= 0)
                    BeforeSave = JobReceiveDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobReceiveHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReceiveDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobReceiveHeaderId, EventModeConstants.Edit), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before save";

            #endregion

            if (vm.JobWorkerDocDate > vm.DocDate)
                ModelState.AddModelError("JobWorkerDocDate", "Party Doc Date can not be greater then Receive Date.");

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.JobReceiveHeaderId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(vm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXC"] += ExceptionMsg;

            #endregion

            if (ModelState.IsValid && BeforeSave && !EventException && (TimePlanValidation || Continue))
            {

                #region CreateRecord
                if (vm.JobReceiveHeaderId <= 0)
                {

                    JobReceiveHeader header = new JobReceiveHeader();
                    header = Mapper.Map<JobReceiveHeaderViewModel, JobReceiveHeader>(vm);
                    header.CreatedBy = User.Identity.Name;
                    header.CreatedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;
                    header.Status = (int)StatusConstants.Drafted;

                    header.ObjectState = Model.ObjectState.Added;
                    db.JobReceiveHeader.Add(header);
                    //_JobReceiveHeaderService.Create(header);

                    try
                    {
                        JobReceiveDocEvents.onHeaderSaveEvent(this, new JobEventArgs(header.JobReceiveHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
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
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                    try
                    {
                        JobReceiveDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(header.JobReceiveHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.JobReceiveHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocDate = header.DocDate,
                        DocNo = header.DocNo,
                        DocStatus = header.Status,
                    }));


                    return RedirectToAction("Modify", new { id = header.JobReceiveHeaderId }).Success("Data saved successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    bool GodownChanged = false;
                    bool DocDateChanged = false;
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveHeader temp = _JobReceiveHeaderService.Find(vm.JobReceiveHeaderId);

                    GodownChanged = (temp.GodownId == vm.GodownId) ? false : true;
                    DocDateChanged = (temp.DocDate == vm.DocDate) ? false : true;

                    JobReceiveHeader ExRec = new JobReceiveHeader();
                    ExRec = Mapper.Map<JobReceiveHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocDate = vm.DocDate;
                    temp.DocNo = vm.DocNo;
                    temp.JobWorkerDocNo = vm.JobWorkerDocNo;
                    temp.JobWorkerDocDate = vm.JobWorkerDocDate;
                    temp.ProcessId = vm.ProcessId;
                    temp.JobWorkerId = vm.JobWorkerId;
                    temp.JobReceiveById = vm.JobReceiveById;
                    temp.GodownId = vm.GodownId;
                    temp.Remark = vm.Remark;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_JobReceiveHeaderService.Update(temp);
                    db.JobReceiveHeader.Add(temp);

                    if (temp.StockHeaderId != null)
                    {
                        StockHeader S = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId.Value);

                        S.DocDate = temp.DocDate;
                        S.DocNo = temp.DocNo;
                        S.PersonId = temp.JobWorkerId;
                        S.ProcessId = temp.ProcessId;
                        S.GodownId = temp.GodownId;
                        S.Remark = temp.Remark;
                        S.Status = temp.Status;
                        S.ModifiedBy = temp.ModifiedBy;
                        S.ModifiedDate = temp.ModifiedDate;
                        S.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(S);
                        //new StockHeaderService(_unitOfWork).UpdateStockHeader(S);
                    }

                    if (GodownChanged || DocDateChanged)
                        new StockService(_unitOfWork).UpdateStockGodownId(temp.StockHeaderId, temp.GodownId, temp.DocDate, db);



                    if (temp.JobWorkerId != ExRec.JobWorkerId || temp.DocNo != ExRec.DocNo || temp.DocDate != ExRec.DocDate)
                    {
                        _JobReceiveHeaderService.UpdateProdUidJobWorkers(ref db, temp);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReceiveDocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobReceiveHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
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
                        TempData["CSEXC"] += message;
                        PrepareViewBag(temp.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", vm);
                    }

                    try
                    {
                        JobReceiveDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(temp.JobReceiveHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        xEModifications = Modifications,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = temp.DocTypeId }).Success("Data saved successfully");

                }
                #endregion

            }


            var ModelStateErrorList = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
            string Messsages = "";
            if (ModelStateErrorList.Count > 0)
            {
                foreach (var ModelStateError in ModelStateErrorList)
                {
                    foreach (var Error in ModelStateError)
                    {
                        if (!Messsages.Contains(Error.ErrorMessage))
                            Messsages = Error.ErrorMessage + System.Environment.NewLine;
                    }
                }
                if (Messsages != "")
                    ModelState.AddModelError("", Messsages);
            }

            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, int? DocLineId)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", DocLineId = DocLineId });
        }



        // GET: /ProductMaster/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            JobReceiveHeaderViewModel pt = _JobReceiveHeaderService.GetJobReceiveHeader(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, pt.DocTypeId, pt.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            if (pt == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pt), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation)
                TempData["CSEXC"] += ExceptionMsg;
            #endregion

            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }
            //Job Receive Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = pt.DocTypeId }).Warning("Please create job receive settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);

            ViewBag.Mode = "Edit";
            if ((System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }



        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType, int? DocLineId)
        {
            if (DocLineId.HasValue)
                ViewBag.DocLineId = DocLineId;

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            JobReceiveHeaderViewModel pt = _JobReceiveHeaderService.GetJobReceiveHeader(id);

            //Job Receive Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveSettings", new { id = pt.DocTypeId }).Warning("Please create job receive settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }
            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }



        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobReceiveHeader JobReceiveHeader = _JobReceiveHeaderService.Find(id);

            if (JobReceiveHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, JobReceiveHeader.DocTypeId, JobReceiveHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobReceiveHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation && !Continue)
            {
                return PartialView("AjaxError");
            }
            #endregion

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            bool BeforeSave = true;

            int MainSiteId = (from S in db.Site where S.SiteCode == "MAIN" select S).FirstOrDefault().SiteId;

            try
            {
                BeforeSave = JobReceiveDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int? StockHeaderId = 0;
                //int ProcessId = new ProcessService(_unitOfWork).Find(ProcessConstants.Weaving).ProcessId;

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = (from p in db.JobReceiveHeader
                            where p.JobReceiveHeaderId == vm.id
                            select p).FirstOrDefault();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveHeader>(temp),
                });


                StockHeaderId = temp.StockHeaderId;

                //var line = new JobReceiveLineService(_unitOfWork).GetLineListForIndex(vm.id);
                var line = (from p in db.JobReceiveLine
                            where p.JobReceiveHeaderId == vm.id
                            select p).ToList();

                var JRLineIds = line.Select(m => m.JobReceiveLineId).ToArray();

                var JobReceiveLineStatusRecords = (from p in db.JobReceiveLineStatus
                                                   where JRLineIds.Contains(p.JobReceiveLineId ?? 0)
                                                   select p).ToList();

                var ProductUids = line.Select(m => m.ProductUidId).ToArray();

                var BarCodeRecords = (from p in db.ProductUid
                                      where ProductUids.Contains(p.ProductUIDId)
                                      select p).ToList();

                var GeneratedBarCodeRecords = (from L in db.JobReceiveLine
                                               join P in db.ProductUid on L.ProductUidId equals P.ProductUIDId
                                      where L.JobReceiveHeaderId == vm.id && L.ProductUidHeaderId != null && L.ProductUidId != null
                                      select P).ToList();


                List<int> StockIdList = new List<int>();
                List<int> StockProcessIdList = new List<int>();

                try
                {
                    JobReceiveDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                new JobOrderLineStatusService(_unitOfWork).DeleteJobQtyOnReceiveMultiple(temp.JobReceiveHeaderId, ref db);

                foreach (var item in JobReceiveLineStatusRecords)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveLineStatus.Remove(item);
                }

                if (StockHeaderId != null)
                {
                    var Stocks = new StockService(_unitOfWork).GetStockForStockHeaderId((int)StockHeaderId);
                    foreach (var item in Stocks)
                    {
                        if (item.StockId != null)
                        {
                            StockIdList.Add((int)item.StockId);
                        }
                    }

                    var StockProcesses = new StockProcessService(_unitOfWork).GetStockProcessForStockHeaderId((int)StockHeaderId);
                    foreach (var item in StockProcesses)
                    {
                        if (item.StockProcessId != null)
                        {
                            StockProcessIdList.Add((int)item.StockProcessId);
                        }
                    }
                }

                

                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReceiveLine>(item),
                    });

                    //if (item.StockId != null)
                    //{
                    //    StockIdList.Add((int)item.StockId);
                    //}


                    //if (item.StockProcessId != null)
                    //{
                    //    StockProcessIdList.Add((int)item.StockProcessId);
                    //}


                    var Productuid = item.ProductUidId;



                    if (Productuid != null && Productuid != 0)
                    {
                        //Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)Productuid, "Job Receive-" + item.JobReceiveHeaderId.ToString());

                        ProductUid ProductUid = BarCodeRecords.Where(m => m.ProductUIDId == Productuid).FirstOrDefault();

                        if (!(item.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && item.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || temp.SiteId == MainSiteId)
                        {

                            if ((temp.DocNo != ProductUid.LastTransactionDocNo || temp.DocTypeId != ProductUid.LastTransactionDocTypeId))
                            {
                                ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                                return PartialView("_Reason", vm);
                            }

                            if (item.ProductUidHeaderId == null || item.ProductUidHeaderId == 0)
                            {
                                ProductUid.LastTransactionDocDate = item.ProductUidLastTransactionDocDate;
                                ProductUid.LastTransactionDocId = item.ProductUidLastTransactionDocId;
                                ProductUid.LastTransactionDocNo = item.ProductUidLastTransactionDocNo;
                                ProductUid.LastTransactionDocTypeId = item.ProductUidLastTransactionDocTypeId;
                                ProductUid.LastTransactionPersonId = item.ProductUidLastTransactionPersonId;
                                ProductUid.CurrenctGodownId = item.ProductUidCurrentGodownId;
                                ProductUid.CurrenctProcessId = item.ProductUidCurrentProcessId;
                                ProductUid.Status = item.ProductUidStatus;
                                if (!string.IsNullOrEmpty(ProductUid.ProcessesDone))
                                    ProductUid.ProcessesDone = ProductUid.ProcessesDone.Replace("|" + temp.ProcessId.ToString() + "|", "");
                                ProductUid.ModifiedBy = User.Identity.Name;
                                ProductUid.ModifiedDate = DateTime.Now;

                                ProductUid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(ProductUid);
                            }

                            new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(item.JobReceiveHeaderId, temp.DocTypeId, temp.SiteId, temp.DivisionId, ref db);

                        }
                        else
                        {
                            var MainJobRec = (from p in db.JobReceiveLine
                                              join t in db.JobReceiveHeader on p.JobReceiveHeaderId equals t.JobReceiveHeaderId
                                              join d in db.DocumentType on t.DocTypeId equals d.DocumentTypeId
                                              where p.ProductUidId == Productuid && t.SiteId != temp.SiteId && d.DocumentTypeName == TransactionDoctypeConstants.WeavingBazar
                                              && p.LockReason != null
                                              select p).ToList().LastOrDefault();

                            if (MainJobRec != null)
                            {
                                MainJobRec.LockReason = null;
                                MainJobRec.ObjectState = Model.ObjectState.Modified;
                                db.JobReceiveLine.Add(MainJobRec);
                            }
                        }
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveLine.Remove(item);
                }

                var Boms = (from p in db.JobReceiveBom
                            where p.JobReceiveHeaderId == temp.JobReceiveHeaderId
                            select p).ToList();

                //var StockProcessIds = Boms.Select(m => m.StockProcessId).ToArray();

                //var StockProcessRecords = (from p in db.StockProcess
                //                           where StockProcessIds.Contains(p.StockProcessId)
                //                           select p).ToList();

                foreach (var item2 in Boms)
                {
                    //if (item2.StockProcessId != null)
                    //{
                    //    var StockProcessRec = StockProcessRecords.Where(m => m.StockProcessId == item2.StockProcessId).FirstOrDefault();
                    //    StockProcessRec.ObjectState = Model.ObjectState.Deleted;
                    //    db.StockProcess.Remove(StockProcessRec);
                    //}
                    item2.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveBom.Remove(item2);
                }

                new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);
                new StockProcessService(_unitOfWork).DeleteStockProcessDBMultiple(StockProcessIdList, ref db, true);

                temp.ObjectState = Model.ObjectState.Deleted;
                db.JobReceiveHeader.Remove(temp);

                if (StockHeaderId != null)
                {
                    var STockHeader = (from p in db.StockHeader
                                       where p.StockHeaderId == StockHeaderId
                                       select p).FirstOrDefault();

                    STockHeader.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeader.Remove(STockHeader);
                }

                foreach(var item in GeneratedBarCodeRecords)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.ProductUid.Remove(item);
                }


                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (EventException)
                    { throw new Exception(); }

                    db.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    JobReceiveDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = temp.DocNo,
                    DocDate = temp.DocDate,
                    xEModifications = Modifications,
                    DocStatus = temp.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            JobReceiveHeader s = db.JobReceiveHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;

            
            try
            {
                TimePlanValidation = Submitvalidation(id, out ExceptionMsg);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Submit, User.Identity.Name, out ExceptionMsg, out Continue);
                TempData["CSEXC"] += ExceptionMsg;
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                TimePlanValidation = false;
            }

            if (!TimePlanValidation && !Continue)
            {
                return RedirectToAction("Index", new { id = s.DocTypeId, IndexType = IndexType });
            }
            #endregion

            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "submit" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Submit")]
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                JobReceiveHeader pd = new JobReceiveHeaderService(_unitOfWork).Find(Id);
                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveHeader.Add(pd);
                    //_JobReceiveHeaderService.Update(pd);

                    try
                    {
                        JobReceiveDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
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
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }

                    try
                    {
                        JobReceiveDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.JobReceiveHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocDate = pd.DocDate,
                        DocNo = pd.DocNo,
                        DocStatus = pd.Status,
                    }));

                    string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobReceiveHeaders", "JobReceiveHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {
                            var PendingtoSubmitCount = PendingToSubmitCount(pd.DocTypeId);
                            if (PendingtoSubmitCount > 0)
                                //return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId });
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveHeader" + "/" + "Index_PendingToSubmit" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                            else
                                //return RedirectToAction("Index", new { id = pd.DocTypeId });
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;

                        }
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveHeader" + "/" + "Submit" + "/" + nextId + "?TransactionType=submitContinue&IndexType=" + IndexType;
                    }
                    else
                    {
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    }

                    #region "For Calling Customise Menu"
                    int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

                    var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pd.DocTypeId, DivisionId, SiteId);

                    if (settings != null)
                    {
                        if (settings.OnSubmitMenuId != null)
                        {
                            //ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + pd.DocTypeId;
                            return Action_Menu(Id, (int)settings.OnSubmitMenuId, ReturnUrl);
                        }
                        else
                            return Redirect(ReturnUrl);
                    }
                    #endregion
                }
                else
                {
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
                }
            }

            return View();
        }



        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReceiveDocEvents.beforeHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before review.";

            if (ModelState.IsValid && BeforeSave)
            {
                JobReceiveHeader pd = new JobReceiveHeaderService(_unitOfWork).Find(Id);

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.JobReceiveHeader.Add(pd);


                //_JobReceiveHeaderService.Update(pd);

                try
                {
                    JobReceiveDocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                db.SaveChanges();
                //_unitOfWork.Save();

                try
                {
                    JobReceiveDocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobReceiveHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocDate = pd.DocDate,
                    DocNo = pd.DocNo,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    JobReceiveHeader HEader = _JobReceiveHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobReceiveHeaders", "JobReceiveHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = PendingToReviewCount(HEader.DocTypeId);
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId });
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType });

                    }

                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType });
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success(pd.DocNo + " Reviewed Successfully.");
            }

            return View();
        }







        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobReceiveHeaders", "JobReceiveHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobReceiveHeaders", "JobReceiveHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        [HttpGet]
        private ActionResult PrintOut(int id, string SqlProcForPrint)
        {
            String query = SqlProcForPrint;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);
        }

        [HttpGet]
        public ActionResult Print(int id)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
            {
                var SEttings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, SEttings.SqlProcDocumentPrint);
            }
            else
                return HttpNotFound();

        }

        [HttpGet]
        public ActionResult PrintAfter_Submit(int id)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
            {
                var SEttings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint_AfterSubmit))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, SEttings.SqlProcDocumentPrint_AfterSubmit);
            }
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult PrintAfter_Approve(int id)
        {
            JobReceiveHeader header = _JobReceiveHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint_AfterApprove))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, SEttings.SqlProcDocumentPrint_AfterApprove);
            }
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            JobReceiveSettings SEttings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            //if (!Dt.ReportMenuId.HasValue)
            //    throw new Exception("Report Menu not configured in document types");
            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Job Receive Report&DocTypeId=" + id.ToString());


            Model.Models.Menu menu = new MenuService(_unitOfWork).Find(Dt.ReportMenuId ?? 0);

            if (menu != null)
            {
                ReportHeader header = new ReportHeaderService(_unitOfWork).GetReportHeaderByName(menu.MenuName);

                ReportLine Line = new ReportLineService(_unitOfWork).GetReportLineByName("DocumentType", header.ReportHeaderId);
                if (Line != null)
                    DefaultValue.Add(Line.ReportLineId, id.ToString());
                ReportLine Site = new ReportLineService(_unitOfWork).GetReportLineByName("Site", header.ReportHeaderId);
                if (Site != null)
                    DefaultValue.Add(Site.ReportLineId, ((int)System.Web.HttpContext.Current.Session["SiteId"]).ToString());
                ReportLine Division = new ReportLineService(_unitOfWork).GetReportLineByName("Division", header.ReportHeaderId);
                if (Division != null)
                    DefaultValue.Add(Division.ReportLineId, ((int)System.Web.HttpContext.Current.Session["DivisionId"]).ToString());
                //ReportLine Process = new ReportLineService(_unitOfWork).GetReportLineByName("Process", header.ReportHeaderId);
                //if (Process != null)
                //    DefaultValue.Add(Process.ReportLineId, ((int)SEttings.ProcessId).ToString());
            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }


        public int PendingToSubmitCount(int id)
        {
            return (_JobReceiveHeaderService.GetJobReceiveHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_JobReceiveHeaderService.GetJobReceiveHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetSummary(int id)
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Rug/WeavingReceive/GetSummary/" + id);
        }

        public ActionResult GetBarCodesForIAP(int id)
        {
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Rug/WeavingReceive/GetBarCodesForIAP/" + id);
        }


        public ActionResult Wizard(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobReceiveHeaderViewModel vm = new JobReceiveHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.WizardMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.WizardMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {
                        if (menuviewmodel.AreaName != null && menuviewmodel.AreaName != "")
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.AreaName + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                        }
                        else
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
                        }
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobReceiveHeaderViewModel vm = new JobReceiveHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.ImportMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.ImportMenuId);

                    if (menuviewmodel == null)
                    {
                        return View("~/Views/Shared/UnderImplementation.cshtml");
                    }
                    else if (!string.IsNullOrEmpty(menuviewmodel.URL))
                    {
                        if (menuviewmodel.AreaName != null && menuviewmodel.AreaName != "")
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.AreaName + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                        }
                        else
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                        }
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = id });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public ActionResult Action_Menu(int Id, int MenuId, string ReturnUrl)
        {
            MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu(MenuId);

            if (menuviewmodel != null)
            {
                if (!string.IsNullOrEmpty(menuviewmodel.URL))
                {
                    return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id + "?ReturnUrl=" + ReturnUrl);
                }
                else
                {
                    return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id, ReturnUrl = ReturnUrl });
                }
            }
            return null;
        }



        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);

                if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DocTypeId, Settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "GeneratePrints") == false)
                {
                    return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
                }

                string ReportSql = "";

                if (!string.IsNullOrEmpty(Settings.DocumentPrint))
                    ReportSql = db.ReportHeader.Where((m) => m.ReportName == Settings.DocumentPrint).FirstOrDefault().ReportSQL;

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.JobReceiveHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobReceiveHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (!string.IsNullOrEmpty(ReportSql))
                        {
                            Pdf = drp.rsDirectDocumentPrint(ReportSql, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }
                        else
                        {

                            if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified || pd.Status == (int)StatusConstants.Import)
                            {
                                //LogAct(item.ToString());
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                                PdfStream.Add(Pdf);
                            }
                            else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                            {
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

                                PdfStream.Add(Pdf);
                            }
                            else if (pd.Status == (int)StatusConstants.Approved)
                            {
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
                                PdfStream.Add(Pdf);
                            }

                        }
                    }

                    PdfMerger pm = new PdfMerger();

                    byte[] Merge = pm.MergeFiles(PdfStream);

                    if (Merge != null)
                        return File(Merge, "application/pdf");

                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _JobReceiveHeaderService.GetCustomPerson(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

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

        public JsonResult IsDuplicatePartyDocNo(int PersonId, string PartyDocNo)
        {
            var temp = (from H in db.JobReceiveHeader
                        where H.JobWorkerId == PersonId && H.JobWorkerDocNo == PartyDocNo
                        select H).FirstOrDefault();
            if (temp == null)
            {
                return Json(false);
            }
            else{
                return Json(true);
            }
        }


        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {   
            Msg = "";
            int Stockline = (new JobReceiveLineService(_unitOfWork).GetLineListForIndex(id)).Count();
            if (Stockline == 0)
            {
                Msg = "Add Line Record. <br />";
            }
            else
            {
                Msg = "";
            }
            return (string.IsNullOrEmpty(Msg));
        }

        #endregion submitValidation
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
