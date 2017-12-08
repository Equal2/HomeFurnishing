using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.SqlClient;
using System.Configuration;
using System.Xml.Linq;
using JobReturnDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobReturnHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobReturnHeaderService _JobReturnHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobReturnHeaderController(IJobReturnHeaderService JobReturnHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReturnHeaderService = JobReturnHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!JobReturnEvents.Initialized)
            {
                JobReturnEvents Obj = new JobReturnEvents();
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

        // GET: /JobReturnHeaderMaster/

        public ActionResult Index(int id, string IndexType)//DocumentTypeID
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }

            var JobReturnHeader = _JobReturnHeaderService.GetJobReturnHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobReturnHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<JobReturnHeaderViewModel> p = _JobReturnHeaderService.GetJobReturnHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }
        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<JobReturnHeaderViewModel> p = _JobReturnHeaderService.GetJobReturnHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }

        void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.ReasonList = new ReasonService(_unitOfWork).FindByDocumentType(id).ToList();
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id,DivisionId,SiteId);
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            if (settings !=null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.SqlProcGatePass = settings.SqlProcGatePass;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }

        }

        // GET: /JobReturnHeaderMaster/Create

        public ActionResult Create(int id)//DocuentTypeId
        {
            PrepareViewBag(id);
            JobReturnHeaderViewModel vm = new JobReturnHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobReturn", "JobReceiveSettings", new { id = id }).Warning("Please create goods return settings");
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

            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.ProcessId = settings.ProcessId;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReturnHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobReturnHeaderViewModel vm)
        {

            JobReturnHeader pt = AutoMapper.Mapper.Map<JobReturnHeaderViewModel, JobReturnHeader>(vm);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (vm.JobReturnHeaderId <= 0)
                    BeforeSave = JobReturnDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReturnDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobReturnHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.JobReturnHeaderId <= 0)
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
                if (vm.JobReturnHeaderId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    //_JobReturnHeaderService.Create(pt);
                    db.JobReturnHeader.Add(pt);

                    try
                    {
                        JobReturnDocEvents.onHeaderSaveEvent(this, new JobEventArgs(pt.JobReturnHeaderId, EventModeConstants.Add), ref db);
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
                        JobReturnDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(pt.JobReturnHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocDate = pt.DocDate,
                        DocNo = pt.DocNo,
                        DocStatus = pt.Status,
                    }));

                    //return Edit(pt.JobReturnHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = pt.JobReturnHeaderId });
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    JobReturnHeader temp = _JobReturnHeaderService.Find(pt.JobReturnHeaderId);

                    JobReturnHeader ExRec = Mapper.Map<JobReturnHeader>(temp);



                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    temp.Remark = pt.Remark;
                    temp.JobWorkerId = pt.JobWorkerId;
                    temp.DocNo = pt.DocNo;
                    temp.OrderById = pt.OrderById;
                    temp.ReasonId = pt.ReasonId;
                    temp.DocDate = pt.DocDate;
                    temp.GodownId = pt.GodownId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_JobReturnHeaderService.Update(temp);
                    db.JobReturnHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReturnDocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobReturnHeaderId, EventModeConstants.Edit), ref db);
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
                        ViewBag.Mode = "Edit";
                        return View("Create", pt);
                    }

                    try
                    {
                        JobReturnDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(temp.JobReturnHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = pt.JobReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        xEModifications = Modifications,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Index", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                #endregion
            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            JobReturnHeaderViewModel pt = _JobReturnHeaderService.GetJobReturnHeader(id);

            if (pt == null)
            {
                return HttpNotFound();
            }

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

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobReturn", "JobReceiveSettings", new { id = pt.DocTypeId }).Warning("Please create Purchase goods return settings");
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
                    DocId = pt.JobReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = "detail" });
        }

        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType)
        {

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            JobReturnHeaderViewModel pt = _JobReturnHeaderService.GetJobReturnHeader(id);
            PrepareViewBag(pt.DocTypeId);

            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobReturn", "JobReceiveSettings", new { id = pt.DocTypeId }).Warning("Please create Purchase goods return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobReceiveSettings = Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            if (pt == null)
            {
                return HttpNotFound();
            }
            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            JobReturnHeader s = db.JobReturnHeader.Find(id);

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
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue, string GenGatePass)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReturnDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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

                JobReturnHeader pd = db.JobReturnHeader.Find(Id);
                int ActivityType;

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    JobReceiveSettings Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                    if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                    {
                        if (String.IsNullOrEmpty(Settings.SqlProcGatePass))
                            throw new Exception("Gate pass Procedure is not Created");

                        SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
                        IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                        if (pd.GatePassHeaderId == null)
                        {
                            SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                            DocDate.SqlDbType = SqlDbType.DateTime;
                            SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
                            //SqlParameter DocType = new SqlParameter("@DocTypeId", pd.DocTypeId);
                            SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId);
                            GatePassHeader GPHeader = new GatePassHeader();
                            GPHeader.CreatedBy = User.Identity.Name;
                            GPHeader.CreatedDate = DateTime.Now;
                            GPHeader.DivisionId = pd.DivisionId;
                            GPHeader.DocDate = DateTime.Now.Date;
                            GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                            GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                            GPHeader.ModifiedBy = User.Identity.Name;
                            GPHeader.ModifiedDate = DateTime.Now;
                            GPHeader.Remark = pd.Remark;
                            GPHeader.PersonId = pd.JobWorkerId;
                            GPHeader.SiteId = pd.SiteId;
                            GPHeader.GodownId = pd.GodownId;

                            GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                            GPHeader.ReferenceDocId = pd.JobReturnHeaderId;
                            GPHeader.ReferenceDocNo = pd.DocNo;

                            GPHeader.ObjectState = Model.ObjectState.Added;
                            db.GatePassHeader.Add(GPHeader);
                            //new GatePassHeaderService(_unitOfWork).Create(GPHeader);


                            foreach (GatePassGeneratedViewModel item in GatePasses)
                            {
                                GatePassLine Gline = new GatePassLine();
                                Gline.CreatedBy = User.Identity.Name;
                                Gline.CreatedDate = DateTime.Now;
                                Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                Gline.ModifiedBy = User.Identity.Name;
                                Gline.ModifiedDate = DateTime.Now;
                                Gline.Product = item.ProductName;
                                Gline.Qty = item.Qty;
                                Gline.Specification = item.Specification;
                                Gline.UnitId = item.UnitId;
                                Gline.ObjectState = Model.ObjectState.Added;
                                db.GatePassLine.Add(Gline);
                                //new GatePassLineService(_unitOfWork).Create(Gline);
                            }

                            pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                        }
                        else
                        {
                            List<GatePassLine> LineList = (from p in db.GatePassLine
                                                           where p.GatePassHeaderId == pd.GatePassHeaderId
                                                           select p).ToList();

                            foreach (var ittem in LineList)
                            {
                                ittem.ObjectState = Model.ObjectState.Deleted;
                                db.GatePassLine.Remove(ittem);
                                //new GatePassLineService(_unitOfWork).Delete(ittem);
                            }

                            GatePassHeader GPHeader = new GatePassHeaderService(_unitOfWork).Find(pd.GatePassHeaderId ?? 0);

                            foreach (GatePassGeneratedViewModel item in GatePasses)
                            {
                                GatePassLine Gline = new GatePassLine();
                                Gline.CreatedBy = User.Identity.Name;
                                Gline.CreatedDate = DateTime.Now;
                                Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                Gline.ModifiedBy = User.Identity.Name;
                                Gline.ModifiedDate = DateTime.Now;
                                Gline.Product = item.ProductName;
                                Gline.Qty = item.Qty;
                                Gline.Specification = item.Specification;
                                Gline.UnitId = item.UnitId;
                                Gline.ObjectState = Model.ObjectState.Added;
                                db.GatePassLine.Add(Gline);

                                //new GatePassLineService(_unitOfWork).Create(Gline);
                            }

                            pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                        }
                    }


                    if (pd.StockHeaderId != null)
                    {
                        StockHeader sh = db.StockHeader.Find(pd.StockHeaderId);
                        sh.GatePassHeaderId = pd.GatePassHeaderId;
                        //new StockHeaderService(_unitOfWork).Update(sh);
                        sh.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(sh);
                    }


                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobReturnHeader.Add(pd);

                    //_JobReturnHeaderService.Update(pd);

                    //_unitOfWork.Save();

                    try
                    {
                        JobReturnDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }

                    try
                    {
                        JobReturnDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.JobReturnHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocDate = pd.DocDate,
                        DocNo = pd.DocNo,
                        DocStatus = pd.Status,
                    }));


                    string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReturnHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobReturnHeaders", "JobReturnHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = PendingToSubmitCount(pd.DocTypeId);
                            if (PendingtoSubmitCount > 0)
                                //return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId });
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReturnHeader" + "/" + "Index_PendingToSubmit" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                            else
                                //return RedirectToAction("Index", new { id = pd.DocTypeId });
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReturnHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                        }

                        //return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue" }).Success("Receive " + pd.DocNo + " submitted successfully.");
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReturnHeader" + "/" + "Submit" + "/" + nextId + "?TransactionType=submitContinue&IndexType=" + IndexType;

                    }

                    else
                    {
                        //return RedirectToAction("Index", new { id = pd.DocTypeId });
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReturnHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
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
                    return RedirectToAction("Index", new { id = pd.DocTypeId }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
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
            JobReturnHeader pd = db.JobReturnHeader.Find(Id);
            if (ModelState.IsValid)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.JobReturnHeader.Add(pd);

                try
                {
                    JobReturnDocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                db.SaveChanges();

                try
                {
                    JobReturnDocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocDate = pd.DocDate,
                    DocNo = pd.DocNo,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    JobReturnHeader HEader = _JobReturnHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobReturnHeaders", "JobReturnHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = PendingToReviewCount(HEader.DocTypeId);
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType });
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType });

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType });
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success(pd.DocNo + " Reviewed Successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in Reviewing.");
        }





        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobReturnHeader JobReturnHeader = db.JobReturnHeader.Find(id);

            if (JobReturnHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, JobReturnHeader.DocTypeId, JobReturnHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobReturnHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobReturnDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";
            #endregion

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                int? StockHeaderId = 0;
                int? GatePassHeaderId = 0;

                var temp = db.JobReturnHeader.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReturnHeader>(temp),
                });

                try
                {
                    JobReturnDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }


                StockHeaderId = temp.StockHeaderId;
                GatePassHeaderId = temp.GatePassHeaderId;

                var line = (from p in db.JobReturnLine
                            where p.JobReturnHeaderId == vm.id
                            select p).ToList();

                int[] ReceiveLineIds = line.Select(m => m.JobReceiveLineId).ToArray();

                var ReceiveRecords = (from p in db.JobReceiveLine
                                      where ReceiveLineIds.Contains(p.JobReceiveLineId)
                                      select p).ToList();

                var BarCodesIDs = ReceiveRecords.Select(m => m.ProductUidId).ToArray();

                var ProductUidRecords = (from p in db.ProductUid
                                         where BarCodesIDs.Contains(p.ProductUIDId)
                                         select p).ToList();

                List<int> StockIdList = new List<int>();

                new JobOrderLineStatusService(_unitOfWork).DeleteJobQtyOnReturnMultiple(temp.JobReturnHeaderId, ref db);
                new JobReceiveLineStatusService(_unitOfWork).DeleteJobReceiveQtyOnReturnMultiple(temp.JobReturnHeaderId, ref db);

                foreach (var item in line)
                {

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReturnLine>(item),
                    });

                    int ReceiveLineId = item.JobReceiveLineId;
                    //new JobReturnLineService(_unitOfWork).Delete(item.JobReturnLineId);
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobReturnLine.Remove(item);

                    var jobreceiveLine = ReceiveRecords.Where(m => m.JobReceiveLineId == ReceiveLineId).FirstOrDefault();

                    if (jobreceiveLine.ProductUidId.HasValue)
                    {
                        //Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues(jobreceiveLine.ProductUidId.Value, "Job Return-" + temp.JobReturnHeaderId.ToString());

                        ProductUid ProductUid = ProductUidRecords.Where(m => m.ProductUIDId == jobreceiveLine.ProductUidId).FirstOrDefault();

                        if (!(item.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && item.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || temp.SiteId == 17)
                        {
                            if ((temp.DocNo != ProductUid.LastTransactionDocNo || temp.DocTypeId != ProductUid.LastTransactionDocTypeId))
                            {
                                ModelState.AddModelError("", "Bar Code Can't be deleted because this is already Proceed to another process.");
                                return PartialView("_Reason", vm);
                            }


                            ProductUid.LastTransactionDocDate = item.ProductUidLastTransactionDocDate;
                            ProductUid.LastTransactionDocId = item.ProductUidLastTransactionDocId;
                            ProductUid.LastTransactionDocNo = item.ProductUidLastTransactionDocNo;
                            ProductUid.LastTransactionDocTypeId = item.ProductUidLastTransactionDocTypeId;
                            ProductUid.LastTransactionPersonId = item.ProductUidLastTransactionPersonId;
                            ProductUid.CurrenctGodownId = item.ProductUidCurrentGodownId;
                            ProductUid.CurrenctProcessId = item.ProductUidCurrentProcessId;
                            ProductUid.Status = item.ProductUidStatus;

                            //ProductUid.LastTransactionDocDate = ProductUidDetail.LastTransactionDocDate;
                            //ProductUid.LastTransactionDocId = ProductUidDetail.LastTransactionDocId;
                            //ProductUid.LastTransactionDocNo = ProductUidDetail.LastTransactionDocNo;
                            //ProductUid.LastTransactionDocTypeId = ProductUidDetail.LastTransactionDocTypeId;
                            //ProductUid.LastTransactionPersonId = ProductUidDetail.LastTransactionPersonId;
                            //ProductUid.CurrenctGodownId = ProductUidDetail.CurrenctGodownId;
                            //ProductUid.CurrenctProcessId = ProductUidDetail.CurrenctProcessId;

                            //new ProductUidService(_unitOfWork).Update(ProductUid);
                            ProductUid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(ProductUid);

                        }
                    }

                }

                //foreach (var item in StockIdList)
                //{
                //    new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                //}
                new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);

                temp.ObjectState = Model.ObjectState.Deleted;
                db.JobReturnHeader.Add(temp);
                //_JobReturnHeaderService.Delete(vm.id);

                if (GatePassHeaderId.HasValue)
                {
                    var GatePassHEader = db.GatePassHeader.Find(GatePassHeaderId.Value);

                    var GatePassLines = (from p in db.GatePassLine
                                         where p.GatePassHeaderId == GatePassHEader.GatePassHeaderId
                                         select p).ToList();


                    foreach (var item in GatePassLines)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.GatePassLine.Remove(item);
                        //new GatePassLineService(_unitOfWork).Delete(item.GatePassLineId);
                    }

                    GatePassHEader.ObjectState = Model.ObjectState.Deleted;
                    db.GatePassHeader.Remove(GatePassHEader);
                    //new GatePassHeaderService(_unitOfWork).Delete(GatePassHEader);
                }

                if (StockHeaderId != null)
                {
                    var StockHeader = db.StockHeader.Find(StockHeaderId);
                    StockHeader.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeader.Remove(StockHeader);
                    //new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
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
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                try
                {
                    JobReturnDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                #region ActivityLog

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = temp.DocNo,
                    DocDate = temp.DocDate,
                    xEModifications = Modifications,
                    DocStatus = temp.Status,
                }));
                #endregion


                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobReturnHeaders", "JobReturnHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobReturnHeaders", "JobReturnHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            //if (!Dt.ReportMenuId.HasValue)
            //throw new Exception("Report Menu not configured in document types");

            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Job Return Report&DocTypeId=" + id.ToString());


            Menu menu = new MenuService(_unitOfWork).Find(Dt.ReportMenuId ?? 0);

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

            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        private ActionResult PrintOut(int id, string SqlProcForPrint)
        {

            string query;
            query = SqlProcForPrint;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);

        }

        [HttpGet]
        public ActionResult Print(int id)
        {
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
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
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
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
            JobReturnHeader header = _JobReturnHeaderService.Find(id);
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
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobReturnHeaderViewModel vm = new JobReturnHeaderViewModel();

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


        public int PendingToSubmitCount(int id)
        {
            return (_JobReturnHeaderService.GetJobReturnHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }
        public int PendingToReviewCount(int id)
        {
            return (_JobReturnHeaderService.GetJobReturnHeaderListPendingToReview(id, User.Identity.Name)).Count();
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




        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;

                var Settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string JobHeaderIds = "";

                try
                {

                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {
                        TimePlanValidation = true;
                        var pd = db.JobReturnHeader.Find(item);


                        if (!pd.GatePassHeaderId.HasValue)
                        {
                            #region DocTypeTimeLineValidation
                            try
                            {

                                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pd), DocumentTimePlanTypeConstants.GatePassCreate, User.Identity.Name, out ExceptionMsg, out Continue);

                            }
                            catch (Exception ex)
                            {
                                string message = _exception.HandleException(ex);
                                TempData["CSEXC"] += message;
                                TimePlanValidation = false;
                            }
                            #endregion

                            if ((TimePlanValidation || Continue))
                            {

                                if (Settings.isPostedInStock.HasValue && Settings.isPostedInStock == true)
                                {

                                    var StockHead = (from p in db.StockHeader
                                                     where p.StockHeaderId == pd.StockHeaderId
                                                     select p).FirstOrDefault();

                                    if (!String.IsNullOrEmpty(Settings.SqlProcGatePass) && pd.Status == (int)StatusConstants.Submitted && !pd.GatePassHeaderId.HasValue)
                                    {

                                        SqlParameter SqlParameterUserId = new SqlParameter("@Id", item);
                                        IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                        if (pd.GatePassHeaderId == null)
                                        {
                                            SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                            DocDate.SqlDbType = SqlDbType.DateTime;
                                            SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
                                            SqlParameter DocType = new SqlParameter("@DocTypeId", GatePassDocTypeID);
                                            GatePassHeader GPHeader = new GatePassHeader();
                                            GPHeader.CreatedBy = User.Identity.Name;
                                            GPHeader.CreatedDate = DateTime.Now;
                                            GPHeader.DivisionId = pd.DivisionId;
                                            GPHeader.DocDate = DateTime.Now.Date;
                                            GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                            GPHeader.DocTypeId = GatePassDocTypeID;
                                            GPHeader.ModifiedBy = User.Identity.Name;
                                            GPHeader.ModifiedDate = DateTime.Now;
                                            GPHeader.Remark = pd.Remark;
                                            GPHeader.PersonId = pd.JobWorkerId;
                                            GPHeader.SiteId = pd.SiteId;
                                            GPHeader.GodownId = pd.GodownId;
                                            GPHeader.GatePassHeaderId = PK++;
                                            GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                                            GPHeader.ReferenceDocId = pd.JobReturnHeaderId;
                                            GPHeader.ReferenceDocNo = pd.DocNo;
                                            GPHeader.ObjectState = Model.ObjectState.Added;
                                            db.GatePassHeader.Add(GPHeader);

                                            //new GatePassHeaderService(_unitOfWork).Create(GPHeader);


                                            foreach (GatePassGeneratedViewModel GatepassLine in GatePasses)
                                            {
                                                GatePassLine Gline = new GatePassLine();
                                                Gline.CreatedBy = User.Identity.Name;
                                                Gline.CreatedDate = DateTime.Now;
                                                Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                                Gline.ModifiedBy = User.Identity.Name;
                                                Gline.ModifiedDate = DateTime.Now;
                                                Gline.Product = GatepassLine.ProductName;
                                                Gline.Qty = GatepassLine.Qty;
                                                Gline.Specification = GatepassLine.Specification;
                                                Gline.UnitId = GatepassLine.UnitId;

                                                // new GatePassLineService(_unitOfWork).Create(Gline);
                                                Gline.ObjectState = Model.ObjectState.Added;
                                                db.GatePassLine.Add(Gline);
                                            }

                                            pd.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                            StockHead.GatePassHeaderId = GPHeader.GatePassHeaderId;


                                            pd.ObjectState = Model.ObjectState.Modified;
                                            db.JobReturnHeader.Add(pd);

                                            StockHead.ObjectState = Model.ObjectState.Modified;
                                            db.StockHeader.Add(StockHead);

                                            JobHeaderIds += pd.StockHeaderId + ", ";
                                        }

                                        db.SaveChanges();
                                    }

                                }
                            }
                            else
                                TempData["CSEXC"] += ExceptionMsg;
                        }
                    }


                    //_unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = GatePassDocTypeID,
                    DocId = 0,
                    ActivityType = (int)ActivityTypeContants.Added,
                    Narration = "GatePass created for StockHeaders " + JobHeaderIds,
                }));

                return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Gate passes generated successfully");

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }




        public ActionResult DeleteGatePass(int Id)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (Id > 0)
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                try
                {

                    var pd = db.JobReturnHeader.Find(Id);

                    #region DocTypeTimeLineValidation
                    try
                    {

                        TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(pd), DocumentTimePlanTypeConstants.GatePassCancel, User.Identity.Name, out ExceptionMsg, out Continue);

                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        TimePlanValidation = false;
                    }

                    if (!TimePlanValidation && !Continue)
                        throw new Exception(ExceptionMsg);
                    #endregion

                    var StockHead = (from p in db.StockHeader
                                     where p.GatePassHeaderId == pd.GatePassHeaderId
                                     select p).FirstOrDefault();

                    var GatePass = db.GatePassHeader.Find(pd.GatePassHeaderId);

                    //LogList.Add(new LogTypeViewModel
                    //{
                    //    ExObj = GatePass,
                    //});

                    //var GatePassLines = (from p in db.GatePassLine
                    //                     where p.GatePassHeaderId == GatePass.GatePassHeaderId
                    //                     select p).ToList();

                    //foreach (var item in GatePassLines)
                    //{
                    //    LogList.Add(new LogTypeViewModel
                    //    {
                    //        ExObj = item,
                    //    });
                    //    item.ObjectState = Model.ObjectState.Deleted;
                    //    db.GatePassLine.Remove(item);
                    //}
                    pd.Status = (int)StatusConstants.Modified;
                    pd.GatePassHeaderId = null;
                    pd.ModifiedBy = User.Identity.Name;
                    pd.ModifiedDate = DateTime.Now;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobReturnHeader.Add(pd);


                    if (StockHead != null)
                    {
                        StockHead.GatePassHeaderId = null;
                        StockHead.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(StockHead);
                    }

                    GatePass.Status = (int)StatusConstants.Cancel;
                    GatePass.ObjectState = Model.ObjectState.Modified;
                    db.GatePassHeader.Add(GatePass);

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    db.SaveChanges();

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = GatePass.DocTypeId,
                        DocId = GatePass.GatePassHeaderId,
                        ActivityType = (int)ActivityTypeContants.Deleted,
                        DocNo = GatePass.DocNo,
                        DocDate = GatePass.DocDate,
                        xEModifications = Modifications,
                        DocStatus = GatePass.Status,
                    }));

                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Gate pass Deleted successfully");

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

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

                        var pd = db.JobReturnHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobReturnHeaderId,
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

                            if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
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
            var Query = _JobReturnHeaderService.GetCustomPerson(filter, searchTerm);
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
        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {   
            Msg = "";
            int Stockline = (new JobReturnLineService(_unitOfWork).GetLineListForIndex(id)).Count();
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
