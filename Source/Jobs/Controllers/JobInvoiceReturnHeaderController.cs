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
using System.Configuration;
using System.Data.SqlClient;
using System.Xml.Linq;
using DocumentEvents;
using JobInvoiceReturnDocumentEvents;
using CustomEventArgs;
using Reports.Controllers;
using Reports.Reports;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobInvoiceReturnHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobInvoiceReturnHeaderService _JobInvoiceReturnHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobInvoiceReturnHeaderController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceReturnHeaderService = new JobInvoiceReturnHeaderService(db);
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!JobInvoiceReturnEvents.Initialized)
            {
                JobInvoiceReturnEvents Obj = new JobInvoiceReturnEvents();
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

        // GET: /JobInvoiceReturnHeaderMaster/

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

            var JobInvoiceReturnHeader = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobInvoiceReturnHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }

        void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.ReasonList = new ReasonService(_unitOfWork).FindByDocumentType(id).ToList();
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);
            if(settings !=null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
                ViewBag.SqlProcGatePass = settings.SqlProcGatePass;
            }
        }

        // GET: /JobInvoiceReturnHeaderMaster/Create

        public ActionResult Create(int id)//DocuentTypeId
        {
            PrepareViewBag(id);
            JobInvoiceReturnHeaderViewModel vm = new JobInvoiceReturnHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            if (DocType != null)
            {
                vm.Nature = DocType.Nature ?? TransactionNatureConstants.Return;
            }

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobInvoiceReturn", "JobInvoiceSettings", new { id = id }).Warning("Please create Job invoice return settings");
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

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                vm.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            if (settings != null)
            {
                vm.SalesTaxGroupPersonId = settings.SalesTaxGroupPersonId;
            }

            vm.DocTypeId = id;
            //vm.ProcessId = settings.ProcessId;

            if ((settings.isVisibleProcessHeader ?? false) == false)
            {
                vm.ProcessId = settings.ProcessId;
            }

            vm.DocDate = DateTime.Now;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobInvoiceReturnHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobInvoiceReturnHeaderViewModel vm)
        {
            JobInvoiceReturnHeader pt = AutoMapper.Mapper.Map<JobInvoiceReturnHeaderViewModel, JobInvoiceReturnHeader>(vm);

            if (vm.Nature == TransactionNatureConstants.Return)
            {
                if (vm.GodownId <= 0)
                    ModelState.AddModelError("GodownId", "The Godown field is required");
            }

            if (vm.Remark == null || vm.Remark == "")
                ModelState.AddModelError("Remark", "Remark field is required");

            if (vm.ReasonId == null || vm.ReasonId == 0)
                ModelState.AddModelError("Reason", "Reason field is required");

            if (vm.JobWorkerId != null && vm.JobWorkerId != 0)
            {
                SiteDivisionSettings SiteDivisionSettings = new SiteDivisionSettingsService(_unitOfWork).GetSiteDivisionSettings(vm.SiteId, vm.DivisionId, vm.DocDate);
                if (SiteDivisionSettings != null)
                {
                    if (SiteDivisionSettings.IsApplicableGST == true)
                    {
                        if (vm.SalesTaxGroupPersonId == 0 || vm.SalesTaxGroupPersonId == null)
                        {
                            ModelState.AddModelError("", "Sales Tax Group Person is not defined for party, it is required.");
                        }
                    }
                }
            }


            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (vm.JobInvoiceReturnHeaderId <= 0)
                    BeforeSave = JobInvoiceReturnDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceReturnDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.JobInvoiceReturnHeaderId <= 0)
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
                if (vm.JobInvoiceReturnHeaderId <= 0)
                {
                    if (vm.Nature == TransactionNatureConstants.Return)
                    {
                        JobReturnHeader GoodsRet = Mapper.Map<JobInvoiceReturnHeaderViewModel, JobReturnHeader>(vm);

                        GoodsRet.DocTypeId = vm.JobInvoiceSettings.JobReturnDocTypeId ?? 0;
                        GoodsRet.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReturnHeaders", GoodsRet.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
                        GoodsRet.CreatedDate = DateTime.Now;
                        GoodsRet.ModifiedDate = DateTime.Now;
                        GoodsRet.CreatedBy = User.Identity.Name;
                        GoodsRet.ModifiedBy = User.Identity.Name;
                        GoodsRet.Status = (int)StatusConstants.System;
                        GoodsRet.ObjectState = Model.ObjectState.Added;
                        db.JobReturnHeader.Add(GoodsRet);
                        pt.JobReturnHeaderId = GoodsRet.JobReturnHeaderId;
                        //new JobReturnHeaderService(_unitOfWork).Create(GoodsRet);
                    }


                    //pt.CalculateDiscountOnRate = vm.CalculateDiscountOnRate;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    //_JobInvoiceReturnHeaderService.Create(pt);
                    db.JobInvoiceReturnHeader.Add(pt);

                    try
                    {
                        JobInvoiceReturnDocEvents.onHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
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
                        JobInvoiceReturnDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobInvoiceReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    //return Edit(pt.JobInvoiceReturnHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = pt.JobInvoiceReturnHeaderId }).Success("Data saved Successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceReturnHeader temp = _JobInvoiceReturnHeaderService.Find(pt.JobInvoiceReturnHeaderId);

                    JobInvoiceReturnHeader ExRec = new JobInvoiceReturnHeader();
                    ExRec = Mapper.Map<JobInvoiceReturnHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    //temp.CurrencyId = pt.CurrencyId;
                    temp.Remark = pt.Remark;
                    temp.JobWorkerId = pt.JobWorkerId;
                    temp.DocNo = pt.DocNo;
                    temp.ReasonId = pt.ReasonId;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.DocDate = pt.DocDate;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_JobInvoiceReturnHeaderService.Update(temp);
                    db.JobInvoiceReturnHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    if (temp.JobReturnHeaderId.HasValue)
                    {
                        JobReturnHeader GoodsRet = new JobReturnHeaderService(_unitOfWork).Find(temp.JobReturnHeaderId ?? 0);

                        JobReturnHeader ExRecR = new JobReturnHeader();
                        ExRecR = Mapper.Map<JobReturnHeader>(GoodsRet);

                        GoodsRet.DocDate = temp.DocDate;
                        //GoodsRet.DocNo = temp.DocNo;
                        GoodsRet.OrderById = (int)vm.OrderById;
                        GoodsRet.ReasonId = temp.ReasonId;
                        GoodsRet.JobWorkerId = temp.JobWorkerId;
                        GoodsRet.Remark = temp.Remark;
                        GoodsRet.GodownId = (int)vm.GodownId;
                        //GoodsRet.Status = temp.Status;

                        GoodsRet.ObjectState = Model.ObjectState.Modified;
                        //new JobReturnHeaderService(_unitOfWork).Update(GoodsRet);
                        db.JobReturnHeader.Add(GoodsRet);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecR,
                            Obj = GoodsRet,
                        });

                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceReturnDocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
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
                        return View("Create", vm);
                    }

                    try
                    {
                        JobInvoiceReturnDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(temp.JobInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = vm.DocTypeId }).Success("Data saved successfully");
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


        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            JobInvoiceReturnHeaderViewModel pt = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnHeader(id);

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

            PrepareViewBag(pt.DocTypeId);
            //pt.GodownId = new JobReturnHeaderService(_unitOfWork).Find(pt.JobReturnHeaderId ?? 0).GodownId;

            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }
            //Job Order Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobInvoiceReturn", "JobInvoiceSettings", new { id = pt.DocTypeId }).Warning("Please create Job Invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobInvoiceReturnHeaderId,
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
            JobInvoiceReturnHeader header = _JobInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobInvoiceReturnHeader header = _JobInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobInvoiceReturnHeader header = _JobInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobInvoiceReturnHeader header = _JobInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult DetailInformation(int id)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail" });
        }

        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType)
        {

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            JobInvoiceReturnHeaderViewModel pt = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnHeader(id);
            //pt.GodownId = new JobReturnHeaderService(_unitOfWork).Find(pt.JobReturnHeaderId ?? 0).GodownId;
            //Job Order Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateJobInvoiceReturn", "JobInvoiceSettings", new { id = pt.DocTypeId }).Warning("Please create Job Invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
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
                    DocId = pt.JobInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            JobInvoiceReturnHeader s = db.JobInvoiceReturnHeader.Find(id);
            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            

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
                BeforeSave = JobInvoiceReturnDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            JobInvoiceReturnHeader pd = _JobInvoiceReturnHeaderService.Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    int ActivityType;
                    pd.ReviewBy = null;
                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    if (pd.JobReturnHeaderId.HasValue)
                    {

                        JobReturnHeader GoodsRet = new JobReturnHeaderService(_unitOfWork).Find(pd.JobReturnHeaderId ?? 0);

                        //GoodsRet.Status = pd.Status;

                        JobInvoiceSettings Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                        if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                        {
                            if (String.IsNullOrEmpty(Settings.SqlProcGatePass))
                                throw new Exception("Gate pass Procedure is not Registered");

                            SqlParameter SqlParameterUserId = new SqlParameter("@Id", GoodsRet.JobReturnHeaderId);
                            IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                            if (GoodsRet.GatePassHeaderId == null)
                            {
                                SqlParameter DocDate = new SqlParameter("@DocDate", pd.DocDate);
                                DocDate.SqlDbType = SqlDbType.DateTime;
                                SqlParameter Godown = new SqlParameter("@GodownId", GoodsRet.GodownId);
                                SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId);
                                GatePassHeader GPHeader = new GatePassHeader();
                                GPHeader.CreatedBy = User.Identity.Name;
                                GPHeader.CreatedDate = DateTime.Now;
                                GPHeader.DivisionId = pd.DivisionId;
                                GPHeader.DocDate = pd.DocDate;
                                GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                                GPHeader.ModifiedBy = User.Identity.Name;
                                GPHeader.ModifiedDate = DateTime.Now;
                                GPHeader.Remark = pd.Remark;
                                GPHeader.PersonId = pd.JobWorkerId;
                                GPHeader.SiteId = pd.SiteId;
                                GPHeader.GodownId = GoodsRet.GodownId;
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

                                GoodsRet.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }
                            else
                            {
                                List<GatePassLine> LineList = (from p in db.GatePassLine
                                                               where p.GatePassHeaderId == GoodsRet.GatePassHeaderId
                                                               select p).ToList();

                                foreach (var ittem in LineList)
                                {
                                    ittem.ObjectState = Model.ObjectState.Deleted;
                                    db.GatePassLine.Remove(ittem);
                                    //new GatePassLineService(_unitOfWork).Delete(ittem);
                                }

                                GatePassHeader GPHeader = new GatePassHeaderService(_unitOfWork).Find(GoodsRet.GatePassHeaderId ?? 0);

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

                                GoodsRet.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }

                            GoodsRet.ObjectState = Model.ObjectState.Modified;
                            db.JobReturnHeader.Add(GoodsRet);
                            //new JobReturnHeaderService(_unitOfWork).Update(GoodsRet);

                        }
                    }

                    //_JobInvoiceReturnHeaderService.Update(pd);


                    #region "Ledger Posting"

                    LedgerHeaderViewModel LedgerHeaderViewModel = new LedgerHeaderViewModel();

                    LedgerHeaderViewModel.LedgerHeaderId = pd.LedgerHeaderId ?? 0;
                    LedgerHeaderViewModel.DocTypeId = pd.DocTypeId;
                    LedgerHeaderViewModel.DocDate = pd.DocDate;
                    LedgerHeaderViewModel.DocNo = pd.DocNo;
                    LedgerHeaderViewModel.DivisionId = pd.DivisionId;
                    LedgerHeaderViewModel.SiteId = pd.SiteId;
                    LedgerHeaderViewModel.Narration = "";
                    LedgerHeaderViewModel.Remark = pd.Remark;
                    LedgerHeaderViewModel.ExchangeRate = pd.ExchangeRate;
                    LedgerHeaderViewModel.CreatedBy = pd.CreatedBy;
                    LedgerHeaderViewModel.CreatedDate = DateTime.Now.Date;
                    LedgerHeaderViewModel.ModifiedBy = pd.ModifiedBy;
                    LedgerHeaderViewModel.ModifiedDate = DateTime.Now.Date;

                    IEnumerable<JobInvoiceReturnHeaderCharge> JobInvoiceReturnHeaderCharges = from H in db.JobInvoiceReturnHeaderCharge where H.HeaderTableId == Id select H;
                    IEnumerable<JobInvoiceReturnLineCharge> JobInvoiceReturnLineCharges = from L in db.JobInvoiceReturnLineCharge where L.HeaderTableId == Id select L;

                    try
                    {
                        new CalculationService(_unitOfWork).LedgerPostingDB(ref LedgerHeaderViewModel, JobInvoiceReturnHeaderCharges, JobInvoiceReturnLineCharges, ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return RedirectToAction("Detail", new { id = Id, IndexType = IndexType, transactionType = "submit" });
                    }


                    if (pd.LedgerHeaderId == null)
                    {
                        pd.LedgerHeaderId = LedgerHeaderViewModel.LedgerHeaderId;
                        //_JobInvoiceReturnHeaderService.Update(pd);

                    }

                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceReturnHeader.Add(pd);

                    #endregion
                    try
                    {
                        JobInvoiceReturnDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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
                        JobInvoiceReturnDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.JobInvoiceReturnHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobInvoiceReturnHeaders", "JobInvoiceReturnHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Job Invioce Return " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Job Invioce Return " + pd.DocNo + " submitted successfully.");

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Job Invoice Return " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Job Invioce Return " + pd.DocNo + " submitted successfully.");
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
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReturnDocEvents.beforeHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            JobInvoiceReturnHeader pd = _JobInvoiceReturnHeaderService.Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.JobInvoiceReturnHeader.Add(pd);

                if (pd.JobReturnHeaderId.HasValue)
                {
                    JobReturnHeader GoodsRet = new JobReturnHeaderService(_unitOfWork).Find(pd.JobReturnHeaderId ?? 0);
                    GoodsRet.ObjectState = Model.ObjectState.Modified;
                    db.JobReturnHeader.Add(GoodsRet);
                }

                try
                {
                    JobInvoiceReturnDocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
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
                    JobInvoiceReturnDocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    JobInvoiceReturnHeader HEader = _JobInvoiceReturnHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobInvoiceReturnHeaders", "JobInvoiceReturnHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _JobInvoiceReturnHeaderService.GetJobInvoiceReturnPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
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
            JobInvoiceReturnHeader JobInvoiceReturnHeader = db.JobInvoiceReturnHeader.Find(id);
            if (JobInvoiceReturnHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, JobInvoiceReturnHeader.DocTypeId, JobInvoiceReturnHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobInvoiceReturnHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                BeforeSave = JobInvoiceReturnDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
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
                int? StockHeaderId = 0;

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = db.JobInvoiceReturnHeader.Find(vm.id);
                int? JobReturnHeaderId = temp.JobReturnHeaderId;

                try
                {
                    JobInvoiceReturnDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobInvoiceReturnHeader>(temp),
                });

                var line = (from p in db.JobInvoiceReturnLine
                            where p.JobInvoiceReturnHeaderId == vm.id
                            select p).ToList();

                new JobInvoiceLineStatusService(db).DeleteJobInvoiceQtyOnReturnMultiple(temp.JobInvoiceReturnHeaderId, ref db);

                var LedgerHeaders = (from p in db.LedgerHeader
                                     where p.LedgerHeaderId == temp.LedgerHeaderId
                                     select p).FirstOrDefault();

                if (LedgerHeaders != null)
                {
                    var LedgerLines = (from p in db.Ledger
                                       where p.LedgerHeaderId == LedgerHeaders.LedgerHeaderId
                                       select p).ToList();

                    foreach (var item in LedgerLines)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.Ledger.Remove(item);
                    }

                    LedgerHeaders.ObjectState = Model.ObjectState.Deleted;
                    db.LedgerHeader.Remove(LedgerHeaders);
                }

                var InvRetLineIds = line.Select(m => m.JobInvoiceReturnLineId).ToArray();

                var InvRetLineCharges = (from p in db.JobInvoiceReturnLineCharge
                                         where InvRetLineIds.Contains(p.LineTableId)
                                         select p).ToList();

                foreach (var item in InvRetLineCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceReturnLineCharge.Remove(item);
                }

                var headercharges = (from p in db.JobInvoiceReturnHeaderCharge
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();

                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobInvoiceReturnLine>(item),
                    });

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceReturnLine.Remove(item);
                }


                foreach (var item in headercharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceReturnHeaderCharge.Remove(item);
                }

                temp.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceReturnHeader.Remove(temp);


                List<int> StockIdList = new List<int>();
                List<int> StockProcessIdList = new List<int>();

                if (JobReturnHeaderId.HasValue)
                {
                    var JobReturn = db.JobReturnHeader.Find(JobReturnHeaderId ?? 0);

                    int? GatePassHeaderId = JobReturn.GatePassHeaderId;

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReturnHeader>(JobReturn),
                    });

                    StockHeaderId = JobReturn.StockHeaderId;

                    var GoodsLines = (from p in db.JobReturnLine
                                      where p.JobReturnHeaderId == JobReturn.JobReturnHeaderId
                                      select p).ToList();

                    int[] ReceiveLineIds = GoodsLines.Select(m => m.JobReceiveLineId).ToArray();

                    var ReceiveRecords = (from p in db.JobReceiveLine
                                          where ReceiveLineIds.Contains(p.JobReceiveLineId)
                                          select p).ToList();

                    var BarCodesIDs = ReceiveRecords.Select(m => m.ProductUidId).ToArray();

                    var ProductUidRecords = (from p in db.ProductUid
                                             where BarCodesIDs.Contains(p.ProductUIDId)
                                             select p).ToList();

                    foreach (var item in GoodsLines)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = Mapper.Map<JobReturnLine>(item),
                        });

                        if (item.StockId != null)
                        {
                            StockAdj Adj = (from L in db.StockAdj
                                            where L.StockOutId == item.StockId
                                            select L).FirstOrDefault();

                            if (Adj != null)
                            {
                                Adj.ObjectState = Model.ObjectState.Deleted;
                                db.StockAdj.Remove(Adj);
                            }

                            StockIdList.Add((int)item.StockId);
                        }

                        if (item.StockProcessId  != null)
                        {
                            StockProcessIdList.Add((int)item.StockProcessId);
                        }

                        int ReceiveLineId = item.JobReceiveLineId;

                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobReturnLine.Remove(item);

                        var jobreceiveLine = ReceiveRecords.Where(m => m.JobReceiveLineId == ReceiveLineId).FirstOrDefault();

                        if (jobreceiveLine.ProductUidId.HasValue)
                        {
                            //Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues(jobreceiveLine.ProductUidId.Value, "Job Return-" + temp.JobReturnHeaderId.ToString());

                            ProductUid ProductUid = ProductUidRecords.Where(m => m.ProductUIDId == jobreceiveLine.ProductUidId).FirstOrDefault();

                            if (!(item.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && item.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || JobReturn.SiteId == 17)
                            {
                                if ((JobReturn.DocNo != ProductUid.LastTransactionDocNo || JobReturn.DocTypeId != ProductUid.LastTransactionDocTypeId))
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

                    new StockProcessService(_unitOfWork).DeleteStockProcessDBMultiple(StockProcessIdList, ref db, true);

                    JobReturn.ObjectState = Model.ObjectState.Deleted;
                    db.JobReturnHeader.Remove(JobReturn);


                    if (StockHeaderId != null)
                    {
                        var StockHead = db.StockHeader.Find(StockHeaderId);
                        StockHead.ObjectState = Model.ObjectState.Deleted;
                        db.StockHeader.Remove(StockHead);
                    }

                    if (GatePassHeaderId.HasValue)
                    {

                        var GatePassHeader = db.GatePassHeader.Find(GatePassHeaderId);

                        var GatePassLines = (from p in db.GatePassLine
                                             where p.GatePassHeaderId == GatePassHeader.GatePassHeaderId
                                             select p).ToList();

                        foreach (var item in GatePassLines)
                        {
                            item.ObjectState = Model.ObjectState.Deleted;
                            db.GatePassLine.Remove(item);
                        }

                        GatePassHeader.ObjectState = Model.ObjectState.Deleted;
                        db.GatePassHeader.Remove(GatePassHeader);

                    }


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
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    JobInvoiceReturnDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.JobInvoiceReturnHeaderId,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobInvoiceReturnHeaders", "JobInvoiceReturnHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobInvoiceReturnHeaders", "JobInvoiceReturnHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private ActionResult PrintOut(int id, string SqlProcForPrint)
        {
            string query;
            query = SqlProcForPrint;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);

        }

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            //if (!Dt.ReportMenuId.HasValue)
            //    throw new Exception("Report Menu not configured in document types");
            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Job Invoice Return Report&DocTypeId=" + id.ToString());


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

                        var pd = db.JobInvoiceReturnHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobInvoiceReturnHeaderId,
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
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                                PdfStream.Add(Pdf);
                            }
                            else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                            {
                                Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

                                PdfStream.Add(Pdf);
                            }
                            else
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

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        public int PendingToSubmitCount(int id)
        {
            return (_JobInvoiceReturnHeaderService.GetJobInvoiceReturnPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_JobInvoiceReturnHeaderService.GetJobInvoiceReturnPendingToReview(id, User.Identity.Name)).Count();
        }




        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;

                var Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string JobReturnHeaderIds = "";

                try
                {
                    if (!string.IsNullOrEmpty(Settings.SqlProcGatePass))
                        foreach (var item in Ids.Split(',').Select(Int32.Parse))
                        {

                            var pd = db.JobInvoiceReturnHeader.Find(item);

                            if ((pd.Status == (int)StatusConstants.Submitted) && pd.JobReturnHeaderId.HasValue)
                            {

                                var GoodsRet = db.JobReturnHeader.Find(pd.JobReturnHeaderId);

                                if (!GoodsRet.GatePassHeaderId.HasValue)
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
                                        SqlParameter SqlParameterUserId = new SqlParameter("@Id", item);
                                        IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                        if (pd.JobWorkerId != null)
                                        {
                                            if (GoodsRet.GatePassHeaderId == null)
                                            {
                                                SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                                DocDate.SqlDbType = SqlDbType.DateTime;
                                                SqlParameter Godown = new SqlParameter("@GodownId", GoodsRet.GodownId);
                                                SqlParameter DocType = new SqlParameter("@DocTypeId", GatePassDocTypeID);
                                                GatePassHeader GPHeader = new GatePassHeader();
                                                GPHeader.CreatedBy = User.Identity.Name;
                                                GPHeader.CreatedDate = DateTime.Now;
                                                GPHeader.DivisionId = GoodsRet.DivisionId;
                                                GPHeader.DocDate = DateTime.Now.Date;
                                                GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                                GPHeader.DocTypeId = GatePassDocTypeID;
                                                GPHeader.ModifiedBy = User.Identity.Name;
                                                GPHeader.ModifiedDate = DateTime.Now;
                                                GPHeader.Remark = GoodsRet.Remark;
                                                GPHeader.PersonId = (int)pd.JobWorkerId;
                                                GPHeader.SiteId = GoodsRet.SiteId;
                                                GPHeader.GodownId = (int)GoodsRet.GodownId;
                                                GPHeader.GatePassHeaderId = PK++;
                                                GPHeader.ObjectState = Model.ObjectState.Added;
                                                db.GatePassHeader.Add(GPHeader);

                                                //new GatePassHeaderService(_unitOfWork).Create(GPHeader);


                                                foreach (GatePassGeneratedViewModel GPLine in GatePasses)
                                                {
                                                    GatePassLine Gline = new GatePassLine();
                                                    Gline.CreatedBy = User.Identity.Name;
                                                    Gline.CreatedDate = DateTime.Now;
                                                    Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                                    Gline.ModifiedBy = User.Identity.Name;
                                                    Gline.ModifiedDate = DateTime.Now;
                                                    Gline.Product = GPLine.ProductName;
                                                    Gline.Qty = GPLine.Qty;
                                                    Gline.Specification = GPLine.Specification;
                                                    Gline.UnitId = GPLine.UnitId;

                                                    //new GatePassLineService(_unitOfWork).Create(Gline);
                                                    Gline.ObjectState = Model.ObjectState.Added;
                                                    db.GatePassLine.Add(Gline);
                                                }

                                                GoodsRet.GatePassHeaderId = GPHeader.GatePassHeaderId;


                                                GoodsRet.ObjectState = Model.ObjectState.Modified;
                                                db.JobReturnHeader.Add(GoodsRet);

                                                JobReturnHeaderIds += pd.JobReturnHeaderId + ", ";
                                            }

                                        }
                                        db.SaveChanges();

                                    }
                                    else
                                        TempData["CSEXC"] += ExceptionMsg;

                                }
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
                    ActivityType = (int)ActivityTypeContants.Added,
                    Narration = "GatePass created for Return Headers " + JobReturnHeaderIds,
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

                    var pd = db.JobInvoiceReturnHeader.Find(Id);
                    var GoodsRet = db.JobReturnHeader.Find(pd.JobReturnHeaderId);

                    #region DocTypeTimeLineValidation
                    try
                    {
                        DocumentUniqueId dui = Mapper.Map<DocumentUniqueId>(pd);
                        dui.GatePassHeaderId = GoodsRet.GatePassHeaderId;
                        TimePlanValidation = DocumentValidation.ValidateDocument(dui, DocumentTimePlanTypeConstants.GatePassCancel, User.Identity.Name, out ExceptionMsg, out Continue);

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

                    if (pd.JobReturnHeaderId.HasValue)
                    {

                        var GatePass = db.GatePassHeader.Find(GoodsRet.GatePassHeaderId);

                        if (GatePass.Status != (int)StatusConstants.Submitted)
                        {
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
                            GoodsRet.Status = (int)StatusConstants.Modified;
                            GoodsRet.GatePassHeaderId = null;
                            GoodsRet.ModifiedBy = User.Identity.Name;
                            GoodsRet.ModifiedDate = DateTime.Now;
                            GoodsRet.ObjectState = Model.ObjectState.Modified;
                            db.JobReturnHeader.Add(GoodsRet);

                            pd.Status = (int)StatusConstants.Modified;
                            pd.ModifiedBy = User.Identity.Name;
                            pd.ModifiedDate = DateTime.Now;
                            pd.ObjectState = Model.ObjectState.Modified;
                            db.JobInvoiceReturnHeader.Add(pd);

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
                        else
                            throw new Exception("Gatepass cannot be deleted because it is already submitted");

                    }


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

        public ActionResult Import(int id)//Document Type Id
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);

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

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter, int? filter2)//DocTypeId
        {
            var Query = _JobInvoiceReturnHeaderService.GetCustomPerson(filter, searchTerm, filter2);
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
