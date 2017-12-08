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
using Jobs.Helpers;
using AutoMapper;
using System.Xml.Linq;
using System.Configuration;
using JobInvoiceAmendmentDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Reports;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobInvoiceAmendmentHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobInvoiceAmendmentHeaderService _JobInvoiceAmendmentHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobInvoiceAmendmentHeaderController(IJobInvoiceAmendmentHeaderService JobInvoiceAmendmentHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceAmendmentHeaderService = JobInvoiceAmendmentHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!JobInvoiceAmendmentEvents.Initialized)
            {
                JobInvoiceAmendmentEvents Obj = new JobInvoiceAmendmentEvents();
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
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            ViewBag.Name = DocType.DocumentTypeName;
            ViewBag.id = id;
            ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).GetDocumentTypeList(DocType.DocumentTypeName).ToList();
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList(DocType.DocumentTypeName).ToList();
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            if (settings !=null)
            {

                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;                
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }
        }

        // GET: /JobInvoiceAmendmentHeaderMaster/

        public ActionResult Index(int id, string IndexType)
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            ViewBag.Name = DocType.DocumentTypeName;
            PrepareViewBag(id);
            ViewBag.id = id;
            IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> JobInvoiceAmendmentHeader = _JobInvoiceAmendmentHeaderService.GetJobInvoiceAmendmentHeaderList(id, User.Identity.Name);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobInvoiceAmendmentHeader);
        }


        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> p = _JobInvoiceAmendmentHeaderService.GetJobInvoiceAmendmentHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<JobInvoiceAmendmentHeaderIndexViewModel> p = _JobInvoiceAmendmentHeaderService.GetJobInvoiceAmendmentHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }



        // GET: /JobInvoiceAmendmentHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobInvoiceAmendmentHeaderViewModel vm = new JobInvoiceAmendmentHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;
            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobInvoiceAmendment", "JobInvoiceSettings", new { id = id }).Warning("Please create job amendment settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.ProcessId = settings.ProcessId;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobInvoiceAmendmentHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobInvoiceAmendmentHeaderViewModel vm)
        {
            bool BeforeSave = true;

            JobInvoiceAmendmentHeader pt = Mapper.Map<JobInvoiceAmendmentHeaderViewModel, JobInvoiceAmendmentHeader>(vm);

            if (vm.JobInvoiceSettings.isVisibleHeaderJobWorker && !vm.JobWorkerId.HasValue)
            {
                ModelState.AddModelError("JobWorkerId", "The JobWorker field is required");
            }

            #region BeforeSave
            try
            {

                if (vm.JobInvoiceAmendmentHeaderId <= 0)
                    BeforeSave = JobInvoiceAmendmentDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobInvoiceAmendmentHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceAmendmentDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobInvoiceAmendmentHeaderId, EventModeConstants.Edit), ref db);

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

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.JobInvoiceAmendmentHeaderId <= 0)
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
                if (vm.JobInvoiceAmendmentHeaderId <= 0)
                {
                    pt.Status = (int)StatusConstants.Drafted;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceAmendmentHeader.Add(pt);
                    //_JobInvoiceAmendmentHeaderService.Create(pt);

                    try
                    {
                        JobInvoiceAmendmentDocEvents.onHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceAmendmentHeaderId, EventModeConstants.Add), ref db);
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
                        //_unitOfWork.Save();
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
                        JobInvoiceAmendmentDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceAmendmentHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobInvoiceAmendmentHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Modify", new { id = pt.JobInvoiceAmendmentHeaderId }).Success("Data saved Successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceAmendmentHeader temp = _JobInvoiceAmendmentHeaderService.Find(pt.JobInvoiceAmendmentHeaderId);


                    JobInvoiceAmendmentHeader ExRec = new JobInvoiceAmendmentHeader();
                    ExRec = Mapper.Map<JobInvoiceAmendmentHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocDate = pt.DocDate;
                    temp.DocNo = pt.DocNo;
                    temp.DocTypeId = pt.DocTypeId;
                    temp.ProcessId = pt.ProcessId;
                    temp.OrderById = pt.OrderById;
                    temp.Remark = pt.Remark;
                    temp.JobWorkerId = pt.JobWorkerId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceAmendmentHeader.Add(temp);
                    //_JobInvoiceAmendmentHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });



                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceAmendmentDocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobInvoiceAmendmentHeaderId, EventModeConstants.Edit), ref db);
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
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", pt);
                    }

                    try
                    {
                        JobInvoiceAmendmentDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceAmendmentHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceAmendmentHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = pt.DocNo,
                        xEModifications = Modifications,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Index", new { id = temp.DocTypeId }).Success("Data saved successfully");
                }
                #endregion
            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }




        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail" });
        }



        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;

            JobInvoiceAmendmentHeader pt = _JobInvoiceAmendmentHeaderService.Find(id);
            JobInvoiceAmendmentHeaderViewModel temp = AutoMapper.Mapper.Map<JobInvoiceAmendmentHeader, JobInvoiceAmendmentHeaderViewModel>(pt);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, pt.DocTypeId, pt.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
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

            //Job Order Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobInvoiceSettings", new { id = pt.DocTypeId }).Warning("Please create job order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }
            ViewBag.Mode = "Edit";
            if ((System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobInvoiceAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", temp);
        }



        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType)
        {
            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            JobInvoiceAmendmentHeader pt = _JobInvoiceAmendmentHeaderService.Find(id);
            JobInvoiceAmendmentHeaderViewModel temp = AutoMapper.Mapper.Map<JobInvoiceAmendmentHeader, JobInvoiceAmendmentHeaderViewModel>(pt);

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobInvoiceAmendment", "JobInvoiceSettings", new { id = id }).Warning("Please create job amendment settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            temp.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            temp.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobInvoiceAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", temp);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            JobInvoiceAmendmentHeader s = db.JobInvoiceAmendmentHeader.Find(id);
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
                BeforeSave = JobInvoiceAmendmentDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            JobInvoiceAmendmentHeader pd = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    //_JobInvoiceAmendmentHeaderService.Update(pd);

                    //_unitOfWork.Save();
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceAmendmentHeader.Add(pd);

                    try
                    {
                        JobInvoiceAmendmentDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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
                        //_unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }

                    try
                    {
                        JobInvoiceAmendmentDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.JobInvoiceAmendmentHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobInvoiceAmendmentHeaders", "JobInvoiceAmendmentHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = PendingToSubmitCount(pd.DocTypeId);
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Receive " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Receive " + pd.DocNo + " submitted successfully.");

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Receive " + pd.DocNo + " submitted successfully.");

                    }
                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Receive " + pd.DocNo + " submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
        }



        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {

            JobInvoiceAmendmentHeader pd = new JobInvoiceAmendmentHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.JobInvoiceAmendmentHeader.Add(pd);

                db.SaveChanges();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobInvoiceAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    JobInvoiceAmendmentHeader HEader = _JobInvoiceAmendmentHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobInvoiceAmendmentHeaders", "JobInvoiceAmendmentHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = PendingToReviewCount(HEader.DocTypeId);
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Record Reviewed Successfully.");
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
        }


        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobInvoiceAmendmentHeader JobInvoiceAmendmentHeader = db.JobInvoiceAmendmentHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, JobInvoiceAmendmentHeader.DocTypeId, JobInvoiceAmendmentHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            if (!string.IsNullOrEmpty(JobInvoiceAmendmentHeader.LockReason))
            {
                TempData["CSEXC"] += "Record Locked : " + JobInvoiceAmendmentHeader.LockReason;
                return PartialView("AjaxError");
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobInvoiceAmendmentHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            if (JobInvoiceAmendmentHeader == null)
            {
                return HttpNotFound();
            }
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
            try
            {
                BeforeSave = JobInvoiceAmendmentDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
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
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = db.JobInvoiceAmendmentHeader.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobInvoiceAmendmentHeader>(temp),
                });

                try
                {
                    JobInvoiceAmendmentDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                var lines = (from p in db.JobInvoiceRateAmendmentLine
                             where p.JobInvoiceAmendmentHeaderId == vm.id
                             select p).ToList();

                //Commented No InvoiceLineStatus
                //new JobInvoiceLineStatusService(_unitOfWork).DeleteJobRateOnAmendmentMultiple(vm.id, ref db);

                var LineIds = lines.Select(m => m.JobInvoiceRateAmendmentLineId).ToArray();

                var LineCharges = (from p in db.JobInvoiceRateAmendmentLineCharge
                                   where LineIds.Contains(p.LineTableId)
                                   select p).ToList();

                var HeaderCharges = (from p in db.JobInvoiceAmendmentHeaderCharge
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();



                //new LedgerHeaderService(_unitOfWork).Delete(LedgerHeaders.LedgerHeaderId);




                foreach (var item in LineCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceRateAmendmentLineCharge.Remove(item);
                }


                foreach (var item in lines)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobInvoiceRateAmendmentLine>(item),
                    });

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceRateAmendmentLine.Remove(item);
                }

                foreach (var item in HeaderCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceAmendmentHeaderCharge.Remove(item);
                }

                temp.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceAmendmentHeader.Remove(temp);

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
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    JobInvoiceAmendmentDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.JobInvoiceAmendmentHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = temp.DocNo,
                    xEModifications = Modifications,
                    DocDate = temp.DocDate,
                    DocStatus = temp.Status,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobInvoiceAmendmentHeaders", "JobInvoiceAmendmentHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobInvoiceAmendmentHeaders", "JobInvoiceAmendmentHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }
        [HttpGet]
        public ActionResult History()
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
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);

            if (header.Status == (int)StatusConstants.Drafted)
            {
                var SEttings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
            {
                var SEttings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            JobInvoiceAmendmentHeader header = _JobInvoiceAmendmentHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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

            JobInvoiceSettings SEttings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            if (!Dt.ReportMenuId.HasValue)
                throw new Exception("Report Menu not configured in document types");

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
                ReportLine Process = new ReportLineService(_unitOfWork).GetReportLineByName("Process", header.ReportHeaderId);
                if (Process != null)
                    DefaultValue.Add(Process.ReportLineId, ((int)SEttings.ProcessId).ToString());
            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }



        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        public int PendingToSubmitCount(int id)
        {
            return (_JobInvoiceAmendmentHeaderService.GetJobInvoiceAmendmentHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_JobInvoiceAmendmentHeaderService.GetJobInvoiceAmendmentHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }


        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = db.JobInvoiceAmendmentHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobInvoiceAmendmentHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (!string.IsNullOrEmpty(ReportSql))
                        {
                            //string rsReportName=context.Database.SqlQuery<string>(ReportSql).FirstOrDefault();
                            Pdf = drp.rsDirectDocumentPrint(ReportSql, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }
                        else
                        {

                            if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified)
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


        public ActionResult Wizard(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobInvoiceAmendmentHeaderViewModel vm = new JobInvoiceAmendmentHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);

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
                        //return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + id + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = id });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";
            int JobInvoiceRateAmendmentLine = (new JobInvoiceRateAmendmentLineService(_unitOfWork).GetJobInvoiceRateAmendmentLineForHeader(id)).Count();
            if (JobInvoiceRateAmendmentLine == 0)
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
