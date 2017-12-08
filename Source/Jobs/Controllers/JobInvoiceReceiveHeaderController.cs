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
using System.Configuration;
using System.Xml.Linq;
using JobInvoiceReceiveDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobInvoiceReceiveHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IJobReceiveHeaderService _JobReceiveHeaderService;
        IJobInvoiceHeaderService _JobInvoiceHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobInvoiceReceiveHeaderController(IJobReceiveHeaderService JobReceiveHeaderService, IJobInvoiceHeaderService JobInvoiceHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveHeaderService = JobReceiveHeaderService;
            _JobInvoiceHeaderService = JobInvoiceHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!JobInvoiceReceiveEvents.Initialized)
            {
                JobInvoiceReceiveEvents Obj = new JobInvoiceReceiveEvents();
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

        void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);
            if (settings != null)
            {
                ViewBag.IsPostedInStock = settings.isPostedInStock;
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }

            ViewBag.id = id;
        }

        // GET: /JobInvoiceHeaderMaster/

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
            IQueryable<JobInvoiceHeaderViewModel> JobInvoiceHeader = _JobInvoiceHeaderService.GetJobInvoiceHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobInvoiceHeader);
        }


        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<JobInvoiceHeaderViewModel> p = _JobInvoiceHeaderService.GetJobInvoiceHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }
        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<JobInvoiceHeaderViewModel> p = _JobInvoiceHeaderService.GetJobInvoiceHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }



        // GET: /JobInvoiceHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            JobInvoiceHeaderViewModel vm = new JobInvoiceHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            List<DocumentTypeHeaderAttributeViewModel> tem = new DocumentTypeService(_unitOfWork).GetDocumentTypeHeaderAttribute(id).ToList();
            vm.DocumentTypeHeaderAttributes = tem;

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateInvoiceReceive", "JobInvoiceSettings", new { id = id }).Warning("Please create job Invoice Receive settings");
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

            if (vm.GodownId == null || vm.GodownId == 0)
            {
                Site Site = new SiteService(_unitOfWork).Find(vm.SiteId);
                if (Site != null)
                {
                    if (Site.DefaultGodownId != null)
                        vm.GodownId = (int)Site.DefaultGodownId;
                }

                if (vm.GodownId == null || vm.GodownId == 0)
                {
                    var SiteGodownList = (from G in db.Godown where G.SiteId == vm.SiteId select G).FirstOrDefault();
                    if (SiteGodownList != null)
                        vm.GodownId = (int)SiteGodownList.GodownId;
                }

                if (vm.GodownId == null || vm.GodownId == 0)
                {
                    var GodownList = db.Godown.FirstOrDefault();
                    if (GodownList != null)
                        vm.GodownId = (int)GodownList.GodownId;
                }
            }

            JobReceiveHeader LastReceive = (from H in db.JobReceiveHeader where H.DocTypeId == settings.JobReceiveDocTypeId && H.SiteId == settings.SiteId
                                            && H.DivisionId == settings.DivisionId
                                            orderby H.DocDate descending, H.JobReceiveHeaderId descending select H).FirstOrDefault();
            if (LastReceive != null)
            {
                if (LastReceive.JobReceiveById != null)
                {
                    vm.JobReceiveById = LastReceive.JobReceiveById;
                }
            }
            //else
            //{
            //    Employee Employee = new EmployeeService(_unitOfWork).GetEmployeeList().FirstOrDefault();
            //    if (Employee != null)
            //    {
            //        vm.JobReceiveById = Employee.PersonID;
            //    }
            //}


            if (settings != null)
            {
                vm.SalesTaxGroupPersonId = settings.SalesTaxGroupPersonId;
            }


            if (settings != null)
            {
                if (settings.CalculationId != null)
                {
                    var CalculationHeaderLedgerAccount = (from H in db.CalculationHeaderLedgerAccount where H.CalculationId == settings.CalculationId && H.DocTypeId == id && H.SiteId == vm.SiteId && H.DivisionId == vm.DivisionId select H).FirstOrDefault();
                    var CalculationLineLedgerAccount = (from H in db.CalculationLineLedgerAccount where H.CalculationId == settings.CalculationId && H.DocTypeId == id && H.SiteId == vm.SiteId && H.DivisionId == vm.DivisionId select H).FirstOrDefault();

                    if (CalculationHeaderLedgerAccount == null && CalculationLineLedgerAccount == null && UserRoles.Contains("SysAdmin"))
                    {
                        return RedirectToAction("Create", "CalculationHeaderLedgerAccount", null).Warning("Ledger posting settings is not defined for current site and division.");
                    }
                    else if (CalculationHeaderLedgerAccount == null && CalculationLineLedgerAccount == null && !UserRoles.Contains("SysAdmin"))
                    {
                        return View("~/Views/Shared/InValidSettings.cshtml").Warning("Ledger posting settings is not defined for current site and division.");
                    }
                }
            }


            //vm.ProcessId = settings.ProcessId;

            if ((settings.isVisibleProcessHeader ?? false) == false)
            {
                vm.ProcessId = settings.ProcessId;
            }

            vm.DocDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobInvoiceHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobInvoiceHeaderViewModel vm)
        {
            bool BeforeSave = true;
            JobInvoiceHeader pt = AutoMapper.Mapper.Map<JobInvoiceHeaderViewModel, JobInvoiceHeader>(vm);
            JobReceiveHeader pt2 = AutoMapper.Mapper.Map<JobInvoiceHeaderViewModel, JobReceiveHeader>(vm);

            var Settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, vm.DivisionId, vm.SiteId);

            if ((Settings.isVisibleGodown ?? false) == true)
            {
                if (vm.GodownId <= 0)
                    ModelState.AddModelError("GodownId", "The Godown field is required");
            }

            if ((Settings.isVisibleJobReceiveBy ?? false) == true)
            {
                if (vm.JobReceiveById <= 0)
                    ModelState.AddModelError("JobReceiveById", "The Job Receiveby field is required");
            }

            if ((Settings.isVisibleHeaderJobWorker ?? false) == true)
            {
                if (!vm.JobWorkerId.HasValue || vm.JobWorkerId.Value <= 0)
                    ModelState.AddModelError("JobWorkerId", "The JobWorker field is required");
            }

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
            try
            {

                if (vm.JobInvoiceHeaderId <= 0)
                    BeforeSave = JobInvoiceReceiveDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobInvoiceHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobInvoiceReceiveDocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobInvoiceHeaderId, EventModeConstants.Edit), ref db);

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

                if (vm.JobInvoiceHeaderId <= 0)
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
                if (vm.JobInvoiceHeaderId <= 0)
                {
                    if (vm.JobWorkerId != null && vm.JobWorkerId != 0)
                    {
                        if (Settings.JobReceiveDocTypeId != null)
                        {
                            pt2.DocTypeId = Settings.JobReceiveDocTypeId.Value;
                        }
                        else
                        {
                            pt2.DocTypeId = vm.DocTypeId;
                        }
                        pt2.CreatedBy = User.Identity.Name;
                        pt2.CreatedDate = DateTime.Now;
                        pt2.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveHeaders", pt2.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
                        pt2.ModifiedBy = User.Identity.Name;
                        pt2.ModifiedDate = DateTime.Now;
                        pt2.Status = (int)StatusConstants.System;
                        pt2.ObjectState = Model.ObjectState.Added;
                        db.JobReceiveHeader.Add(pt2);
                        pt.JobReceiveHeaderId = pt2.JobReceiveHeaderId;
                    }

                    pt.DivisionId = vm.DivisionId;
                    pt.SiteId = vm.SiteId;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    db.JobInvoiceHeader.Add(pt);
                    //_JobInvoiceHeaderService.Create(pt);     

                    if (vm.DocumentTypeHeaderAttributes != null)
                    {
                        foreach (var Attributes in vm.DocumentTypeHeaderAttributes)
                        {
                            JobInvoiceHeaderAttributes JobInvoiceHeaderAttribute = (from A in db.JobInvoiceHeaderAttributes
                                                                                    where A.HeaderTableId == pt.JobInvoiceHeaderId
                                                                                && A.DocumentTypeHeaderAttributeId == Attributes.DocumentTypeHeaderAttributeId
                                                                                    select A).FirstOrDefault();

                            if (JobInvoiceHeaderAttribute != null)
                            {
                                JobInvoiceHeaderAttribute.Value = Attributes.Value;
                                JobInvoiceHeaderAttribute.ObjectState = Model.ObjectState.Modified;
                                db.JobInvoiceHeaderAttributes.Add(JobInvoiceHeaderAttribute);
                            }
                            else
                            {
                                JobInvoiceHeaderAttributes HeaderAttribute = new JobInvoiceHeaderAttributes()
                                {
                                    HeaderTableId = pt.JobInvoiceHeaderId,
                                    Value = Attributes.Value,
                                    DocumentTypeHeaderAttributeId = Attributes.DocumentTypeHeaderAttributeId,
                                };
                                HeaderAttribute.ObjectState = Model.ObjectState.Added;
                                db.JobInvoiceHeaderAttributes.Add(HeaderAttribute);
                            }
                        }
                    }


                    try
                    {
                        JobInvoiceReceiveDocEvents.onHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceHeaderId, EventModeConstants.Add), ref db);
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
                        JobInvoiceReceiveDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(pt.JobInvoiceHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Modify", new { id = pt.JobInvoiceHeaderId }).Success("Data saved successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    bool GodownChanged = false;
                    bool DocDateChanged = false;
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceHeader temp = _JobInvoiceHeaderService.Find(pt.JobInvoiceHeaderId);                    

                    JobInvoiceHeader ExRec = new JobInvoiceHeader();
                    ExRec = Mapper.Map<JobInvoiceHeader>(temp);

                    if (temp.JobReceiveHeaderId != null)
                    {
                        JobReceiveHeader temp2 = _JobReceiveHeaderService.Find(temp.JobReceiveHeaderId.Value);
                        GodownChanged = (temp2.GodownId == vm.GodownId) ? false : true;
                        DocDateChanged = (temp2.DocDate == vm.DocDate) ? false : true;

                        JobReceiveHeader ExRecR = new JobReceiveHeader();
                        ExRecR = Mapper.Map<JobReceiveHeader>(temp2);

                        temp2.DocDate = vm.DocDate;
                        //temp2.DocNo = vm.DocNo;
                        temp2.JobWorkerDocNo = vm.JobWorkerDocNo;
                        temp2.ProcessId = vm.ProcessId;
                        temp2.JobWorkerId = vm.JobWorkerId.Value;
                        temp2.JobReceiveById = vm.JobReceiveById;
                        temp2.GodownId = vm.GodownId;
                        temp2.Remark = vm.Remark;
                        temp2.ModifiedDate = DateTime.Now;
                        temp2.ModifiedBy = User.Identity.Name;
                        temp2.ObjectState = Model.ObjectState.Modified;
                        db.JobReceiveHeader.Add(temp2);

                        if (GodownChanged)
                            new StockService(_unitOfWork).UpdateStockGodownId(temp2.StockHeaderId, temp2.GodownId, temp.DocDate, db);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecR,
                            Obj = temp2,
                        });


                        if (temp2.StockHeaderId != null)
                        {
                            StockHeader StockHeader = (from p in db.StockHeader
                                                       where p.StockHeaderId == temp2.StockHeaderId
                                                       select p).FirstOrDefault();

                            StockHeader.DocTypeId = temp2.DocTypeId;
                            StockHeader.DocDate = temp2.DocDate;
                            StockHeader.DocNo = temp2.DocNo;
                            StockHeader.DivisionId = temp2.DivisionId;
                            StockHeader.SiteId = temp2.SiteId;
                            StockHeader.ProcessId = temp2.ProcessId;
                            StockHeader.GodownId = temp2.GodownId;
                            StockHeader.Remark = temp2.Remark;
                            StockHeader.Status = temp2.Status;
                            StockHeader.ModifiedBy = temp2.ModifiedBy;
                            StockHeader.ModifiedDate = temp2.ModifiedDate;

                            StockHeader.ObjectState = Model.ObjectState.Modified;
                            db.StockHeader.Add(StockHeader);

                        }
                    }



                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        //temp2.Status = (int)StatusConstants.Modified;
                    }

                    temp.Remark = pt.Remark;
                    temp.DocNo = pt.DocNo;
                    temp.DocDate = pt.DocDate;
                    temp.JobWorkerId = pt.JobWorkerId;
                    temp.JobWorkerDocNo = pt.JobWorkerDocNo;
                    temp.GovtInvoiceNo = pt.GovtInvoiceNo;
                    temp.JobWorkerDocDate = pt.JobWorkerDocDate;
                    temp.ProcessId = pt.ProcessId;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.FinancierId = pt.FinancierId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.JobInvoiceHeader.Add(temp);
                    //_JobInvoiceHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });



                    if (vm.DocumentTypeHeaderAttributes != null)
                    {
                        foreach (var Attributes in vm.DocumentTypeHeaderAttributes)
                        {

                            JobInvoiceHeaderAttributes JobInvoiceHeaderAttribute = (from A in db.JobInvoiceHeaderAttributes
                                                                                    where A.HeaderTableId == temp.JobInvoiceHeaderId
                                                                                && A.DocumentTypeHeaderAttributeId == Attributes.DocumentTypeHeaderAttributeId
                                                                                    select A).FirstOrDefault();

                            if (JobInvoiceHeaderAttribute != null)
                            {
                                JobInvoiceHeaderAttribute.Value = Attributes.Value;
                                JobInvoiceHeaderAttribute.ObjectState = Model.ObjectState.Modified;
                                db.JobInvoiceHeaderAttributes.Add(JobInvoiceHeaderAttribute);
                            }
                            else
                            {
                                JobInvoiceHeaderAttributes HeaderAttribute = new JobInvoiceHeaderAttributes()
                                {
                                    Value = Attributes.Value,
                                    HeaderTableId = temp.JobInvoiceHeaderId,
                                    DocumentTypeHeaderAttributeId = Attributes.DocumentTypeHeaderAttributeId,
                                };
                                HeaderAttribute.ObjectState = Model.ObjectState.Added;
                                db.JobInvoiceHeaderAttributes.Add(HeaderAttribute);
                            }
                        }
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobInvoiceReceiveDocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobInvoiceHeaderId, EventModeConstants.Edit), ref db);
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
                        return View("Create", vm);
                    }

                    try
                    {
                        JobInvoiceReceiveDocEvents.afterHeaderSaveEvent(this, new JobEventArgs(temp.JobInvoiceHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = pt.DocNo,
                        xEModifications = Modifications,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
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
                            Messsages = Error.ErrorMessage  + System.Environment.NewLine;
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
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
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
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            JobInvoiceHeaderViewModel pt = _JobInvoiceHeaderService.GetJobInvoiceHeader(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, pt.DocTypeId, pt.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            string SiteName = db.Site.Find(pt.SiteId).SiteName;
            int LoginSiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            if (pt.SiteId != LoginSiteId)
                pt.LockReason = "Can't modify " + SiteName + " record.You have to login with " + SiteName;

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


            if (pt.JobReceiveHeaderId != null)
            {
                JobReceiveHeader Pt2 = _JobReceiveHeaderService.Find((int)pt.JobReceiveHeaderId);
                pt.GodownId = Pt2.GodownId;
                pt.JobReceiveById = Pt2.JobReceiveById;
            }


            //Job Order Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobInvoiceSettings", new { id = pt.DocTypeId }).Warning("Please create job Invoice settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            pt.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            List<DocumentTypeHeaderAttributeViewModel> tem = _JobInvoiceHeaderService.GetDocumentHeaderAttribute(id).ToList();
            pt.DocumentTypeHeaderAttributes = tem;

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
                    DocId = pt.JobInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }


        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType)
        {

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            JobInvoiceHeaderViewModel pt = _JobInvoiceHeaderService.GetJobInvoiceHeader(id);

            //Getting Settings
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            pt.JobInvoiceSettings = Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            pt.SiteName = db.Site.Find(pt.SiteId).SiteName;

            JobReceiveHeader Pt2 = _JobReceiveHeaderService.Find(pt.JobReceiveHeaderId.Value);
            pt.GodownId = Pt2.GodownId;
            pt.JobReceiveById = Pt2.JobReceiveById;

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }
            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            JobInvoiceHeader s = db.JobInvoiceHeader.Find(id);
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
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReceiveDocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";
            #endregion


            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                JobInvoiceHeader pd = new JobInvoiceHeaderService(_unitOfWork).Find(Id);
                JobReceiveHeader pd2 = _JobReceiveHeaderService.Find(pd.JobReceiveHeaderId.Value);

                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    //pd2.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    #region "Ledger Posting"

                    LedgerHeaderViewModel LedgerHeaderViewModel = new LedgerHeaderViewModel();

                    LedgerHeaderViewModel.LedgerHeaderId = pd.LedgerHeaderId ?? 0;
                    LedgerHeaderViewModel.DocHeaderId = pd.JobInvoiceHeaderId;
                    LedgerHeaderViewModel.DocTypeId = pd.DocTypeId;
                    LedgerHeaderViewModel.DocDate = pd.DocDate;
                    LedgerHeaderViewModel.DocNo = pd.DocNo;
                    LedgerHeaderViewModel.DivisionId = pd.DivisionId;
                    LedgerHeaderViewModel.SiteId = pd.SiteId;
                    LedgerHeaderViewModel.PartyDocNo = pd.JobWorkerDocNo;
                    LedgerHeaderViewModel.PartyDocDate = pd.JobWorkerDocDate;
                    LedgerHeaderViewModel.Narration = "";
                    LedgerHeaderViewModel.Remark = pd.Remark;
                    LedgerHeaderViewModel.CreatedBy = pd.CreatedBy;
                    LedgerHeaderViewModel.CreatedDate = DateTime.Now.Date;
                    LedgerHeaderViewModel.ModifiedBy = pd.ModifiedBy;
                    LedgerHeaderViewModel.ModifiedDate = DateTime.Now.Date;

                    IEnumerable<JobInvoiceHeaderCharge> JobInvoiceHeaderCharges = from H in db.JobInvoiceHeaderCharges where H.HeaderTableId == Id select H;
                    IEnumerable<JobInvoiceLineCharge> JobInvoiceLineCharges = from L in db.JobInvoiceLineCharge where L.HeaderTableId == Id select L;

                    try
                    {
                        new CalculationService(_unitOfWork).LedgerPostingDB(ref LedgerHeaderViewModel, JobInvoiceHeaderCharges, JobInvoiceLineCharges, ref db);
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
                        //_JobInvoiceHeaderService.Update(pd);
                    }

                    #endregion

                    pd.ReviewBy = null;
                    pd2.ReviewBy = null;

                    pd.ObjectState = Model.ObjectState.Modified;
                    pd2.ObjectState = Model.ObjectState.Modified;

                    db.JobInvoiceHeader.Add(pd);
                    db.JobReceiveHeader.Add(pd2);

                    try
                    {
                        JobInvoiceReceiveDocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        EventException = true;
                    }

                    db.SaveChanges();

                    try
                    {
                        JobInvoiceReceiveDocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.JobInvoiceHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveHeader" + "/" + "Index" + "/" + pd.DocTypeId;
                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobInvoiceHeaders", "JobInvoiceHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = PendingToSubmitCount(pd.DocTypeId);
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType });
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Invoice " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Invoice " + pd.DocNo + " submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
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

            if (ModelState.IsValid)
            {
                JobInvoiceHeader pd = new JobInvoiceHeaderService(_unitOfWork).Find(Id);
                JobReceiveHeader pd2 = _JobReceiveHeaderService.Find(pd.JobReceiveHeaderId.Value);

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd2.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd2.ReviewBy += User.Identity.Name + ", ";


                pd.ObjectState = Model.ObjectState.Modified;
                pd2.ObjectState = Model.ObjectState.Modified;

                db.JobInvoiceHeader.Add(pd);
                db.JobReceiveHeader.Add(pd2);

                try
                {
                    JobInvoiceReceiveDocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                db.SaveChanges();

                try
                {
                    JobInvoiceReceiveDocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobInvoiceHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));
                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    JobInvoiceHeader HEader = _JobInvoiceHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobInvoiceHeaders", "JobInvoiceHeaderId", PrevNextConstants.Next);
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
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
            }

            return View();
        }







        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobInvoiceHeader JobInvoiceHeader = db.JobInvoiceHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, JobInvoiceHeader.DocTypeId, JobInvoiceHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobInvoiceHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            if (JobInvoiceHeader == null)
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
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                BeforeSave = JobInvoiceReceiveDocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
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

                var temp = (from p in db.JobInvoiceHeader
                            where p.JobInvoiceHeaderId == vm.id
                            select p).FirstOrDefault();

                try
                {
                    JobInvoiceReceiveDocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobInvoiceHeader>(temp),
                });

                var attributes = (from A in db.JobInvoiceHeaderAttributes where A.HeaderTableId == vm.id select A).ToList();

                foreach (var ite2 in attributes)
                {
                    ite2.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceHeaderAttributes.Remove(ite2);
                }


                var temp2 = (from p in db.JobReceiveHeader
                             where p.JobReceiveHeaderId == temp.JobReceiveHeaderId
                             select p).FirstOrDefault();


                var line = (from p in db.JobInvoiceLine
                            where p.JobInvoiceHeaderId == vm.id
                            select p).ToList();

                new JobOrderLineStatusService(_unitOfWork).DeleteJobQtyOnInvoiceReceiveMultiple(temp.JobInvoiceHeaderId, ref db);

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
                        //new LedgerLineService(_unitOfWork).Delete(item.LedgerLineId );
                    }

                    LedgerHeaders.ObjectState = Model.ObjectState.Deleted;
                    db.LedgerHeader.Remove(LedgerHeaders);
                }

                var LineIds = line.Select(m => m.JobInvoiceLineId).ToArray();

                var LineCharges = (from p in db.JobInvoiceLineCharge
                                   where LineIds.Contains(p.LineTableId)
                                   select p).ToList();

                foreach (var item in LineCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceLineCharge.Remove(item);
                }

                foreach (var item in line)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobInvoiceLine>(item),
                    });

                    JobInvoiceLineStatus Status = new JobInvoiceLineStatus();
                    Status.JobInvoiceLineId = item.JobInvoiceLineId;
                    db.JobInvoiceLineStatus.Attach(Status);
                    Status.ObjectState = Model.ObjectState.Deleted;

                    db.JobInvoiceLineStatus.Remove(Status);

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceLine.Remove(item);
                }

                var headercharges = (from p in db.JobInvoiceHeaderCharges
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();

                foreach (var item in headercharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.JobInvoiceHeaderCharges.Remove(item);
                }

                temp.ObjectState = Model.ObjectState.Deleted;
                db.JobInvoiceHeader.Remove(temp);


                if (temp2 != null)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReceiveHeader>(temp2),
                    });

                    StockHeaderId = temp2.StockHeaderId;

                    var line2 = (from p in db.JobReceiveLine
                                 where p.JobReceiveHeaderId == temp2.JobReceiveHeaderId
                                 select p).ToList();

                    var JRLineIds = line2.Select(m => m.JobReceiveLineId).ToArray();

                    var JobReceiveLineStatusRecords = (from p in db.JobReceiveLineStatus
                                                       where JRLineIds.Contains(p.JobReceiveLineId ?? 0)
                                                       select p).ToList();

                    var ProductUids = line2.Select(m => m.ProductUidId).ToArray();

                    var BarCodeRecords = (from p in db.ProductUid
                                          where ProductUids.Contains(p.ProductUIDId)
                                          select p).ToList();

                    List<int> StockIdList = new List<int>();
                    List<int> StockProcessIdList = new List<int>();

                    foreach (var item in JobReceiveLineStatusRecords)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobReceiveLineStatus.Remove(item);
                    }


                    foreach (var item in line2)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = Mapper.Map<JobReceiveLine>(item),
                        });

                        if (item.StockId != null)
                        {
                            StockIdList.Add((int)item.StockId);
                        }

                        if (item.StockProcessId != null)
                        {
                            StockProcessIdList.Add((int)item.StockProcessId);
                        }

                        var Productuid = item.ProductUidId;



                        if (Productuid != null && Productuid != 0)
                        {
                            ProductUid ProductUid = BarCodeRecords.Where(m => m.ProductUIDId == Productuid).FirstOrDefault();

                            if (!(item.ProductUidLastTransactionDocNo == ProductUid.LastTransactionDocNo && item.ProductUidLastTransactionDocTypeId == ProductUid.LastTransactionDocTypeId) || temp.SiteId == 17)
                            {

                                if ((temp2.DocNo != ProductUid.LastTransactionDocNo || temp2.DocTypeId != ProductUid.LastTransactionDocTypeId))
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
                                if (!string.IsNullOrEmpty(ProductUid.ProcessesDone))
                                    ProductUid.ProcessesDone = ProductUid.ProcessesDone.Replace("|" + temp2.ProcessId.ToString() + "|", "");
                                ProductUid.ModifiedBy = User.Identity.Name;
                                ProductUid.ModifiedDate = DateTime.Now;

                                ProductUid.ObjectState = Model.ObjectState.Modified;
                                db.ProductUid.Add(ProductUid);

                                new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(item.JobReceiveHeaderId, temp2.DocTypeId, temp2.SiteId, temp2.DivisionId, ref db);

                            }
                        }


                        item.ObjectState = Model.ObjectState.Deleted;
                        db.JobReceiveLine.Remove(item);
                    }

                    var Boms = (from p in db.JobReceiveBom
                                where p.JobReceiveHeaderId == temp2.JobReceiveHeaderId
                                select p).ToList();

                    var StockProcessIds = Boms.Select(m => m.StockProcessId).ToArray();

                    var StockProcessRecords = (from p in db.StockProcess
                                               where StockProcessIds.Contains(p.StockProcessId)
                                               select p).ToList();

                    foreach (var item2 in Boms)
                    {
                        if (item2.StockProcessId != null)
                        {
                            var StockProcessRec = StockProcessRecords.Where(m => m.StockProcessId == item2.StockProcessId).FirstOrDefault();
                            StockProcessRec.ObjectState = Model.ObjectState.Deleted;
                            db.StockProcess.Remove(StockProcessRec);
                        }
                        item2.ObjectState = Model.ObjectState.Deleted;
                        db.JobReceiveBom.Remove(item2);
                    }

                    new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);

                    new StockProcessService(_unitOfWork).DeleteStockProcessDBMultiple(StockProcessIdList, ref db, true);

                    temp2.ObjectState = Model.ObjectState.Deleted;
                    db.JobReceiveHeader.Remove(temp2);

                    if (StockHeaderId != null)
                    {
                        var STockHeader = (from p in db.StockHeader
                                           where p.StockHeaderId == StockHeaderId
                                           select p).FirstOrDefault();
                        STockHeader.ObjectState = Model.ObjectState.Deleted;
                        db.StockHeader.Remove(STockHeader);
                    }
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
                    JobInvoiceReceiveDocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = vm.id,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobInvoiceHeaders", "JobInvoiceHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobInvoiceHeaders", "JobInvoiceHeaderId", PrevNextConstants.Prev);
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
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
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
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
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
            JobInvoiceHeader header = _JobInvoiceHeaderService.Find(id);
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

            //if (!Dt.ReportMenuId.HasValue)
            //    throw new Exception("Report Menu not configured in document types");

            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Job Invoice Report&DocTypeId=" + id.ToString());

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


        public int PendingToSubmitCount(int id)
        {
            return (_JobInvoiceHeaderService.GetJobInvoiceHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }
        public int PendingToReviewCount(int id)
        {
            return (_JobInvoiceHeaderService.GetJobInvoiceHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }


        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobInvoiceHeaderViewModel vm = new JobInvoiceHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, vm.DivisionId, vm.SiteId);

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

                        var pd = db.JobInvoiceHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobInvoiceHeaderId,
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

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter, int? filter2)//DocTypeId
        {
            var Query = _JobInvoiceHeaderService.GetCustomPerson(filter, searchTerm, filter2);
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

        public JsonResult GetJobWorkerDetailJson(int JobWorkerId)
        {
            return Json(new JobInvoiceHeaderService(_unitOfWork).GetJobWorkerDetail(JobWorkerId));
        }

        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";
            int Stockline = (new JobInvoiceLineService(_unitOfWork).GetLineListForIndex(id)).Count();
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
