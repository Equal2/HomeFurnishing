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
using JobReceiveQADocumentEvents;
using Reports.Reports;
using Model.ViewModels;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobReceiveQAHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IUnitOfWork _unitOfWork;
        IJobReceiveQAHeaderService _JobReceiveQAHeaderService;
        IExceptionHandlingService _exception;
        public JobReceiveQAHeaderController(IExceptionHandlingService exec, IUnitOfWork uow)
        {
            _unitOfWork = uow;
            _JobReceiveQAHeaderService = new JobReceiveQAHeaderService(db);
            _exception = exec;
            if (!JobReceiveQAEvents.Initialized)
            {
                JobReceiveQAEvents Obj = new JobReceiveQAEvents();
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
            var p = _JobReceiveQAHeaderService.FindByDocumentCategory(id).ToList();

            if (p != null)
            {
                if (p.Count == 1)
                    return RedirectToAction("Index", new { id = p.FirstOrDefault().DocumentTypeId });
            }

            return View("DocumentTypeList", p);
        }

        public void PrepareViewBag(int id)
        {
            ViewBag.Name = db.DocumentType.Find(id).DocumentTypeName;
            ViewBag.id = id;
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
             var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();            
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, DivisionId, SiteId);
            if(settings !=null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }
        }

        // GET: /JobReceiveQAHeaderMaster/

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
            var JobReceiveQAHeader = _JobReceiveQAHeaderService.GetJobReceiveQAHeaderList(id, User.Identity.Name);
            ViewBag.Name = db.DocumentType.Find(id).DocumentTypeName;
            ViewBag.id = id;
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(JobReceiveQAHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<JobReceiveQAHeaderViewModel> p = _JobReceiveQAHeaderService.GetJobReceiveQAHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }
        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<JobReceiveQAHeaderViewModel> p = _JobReceiveQAHeaderService.GetJobReceiveQAHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }
        // GET: /JobReceiveQAHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            JobReceiveQAHeaderViewModel vm = new JobReceiveQAHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQASettings", new { id = id }).Warning("Please create job Inspection settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            vm.ProcessId = settings.ProcessId;  
            vm.DocDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = _JobReceiveQAHeaderService.FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".JobReceiveQAHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
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
        public ActionResult Post(JobReceiveQAHeaderViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {

                if (vm.JobReceiveQAHeaderId <= 0)
                    BeforeSave = JobReceiveQADocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobReceiveQAHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = JobReceiveQADocEvents.beforeHeaderSaveEvent(this, new JobEventArgs(vm.JobReceiveQAHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.JobReceiveQAHeaderId <= 0)
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
                if (vm.JobReceiveQAHeaderId <= 0)
                {

                    JobReceiveQAHeader header = new JobReceiveQAHeader();
                    header = Mapper.Map<JobReceiveQAHeaderViewModel, JobReceiveQAHeader>(vm);

                    _JobReceiveQAHeaderService.Create(header, User.Identity.Name);

                    try
                    {
                        JobReceiveQADocEvents.onHeaderSaveEvent(this, new JobEventArgs(header.JobReceiveQAHeaderId, EventModeConstants.Add), ref db);
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
                        JobReceiveQADocEvents.afterHeaderSaveEvent(this, new JobEventArgs(header.JobReceiveQAHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.JobReceiveQAHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocDate = header.DocDate,
                        DocNo = header.DocNo,
                        DocStatus = header.Status,
                    }));


                    return RedirectToAction("Modify", new { id = header.JobReceiveQAHeaderId }).Success("Data saved successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveQAHeader temp = _JobReceiveQAHeaderService.Find(vm.JobReceiveQAHeaderId);

                    JobReceiveQAHeader ExRec = new JobReceiveQAHeader();
                    ExRec = Mapper.Map<JobReceiveQAHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocDate = vm.DocDate;
                    temp.DocNo = vm.DocNo;
                    temp.JobWorkerId = vm.JobWorkerId;
                    temp.QAById = vm.QAById;
                    temp.Remark = vm.Remark;

                    _JobReceiveQAHeaderService.Update(temp, User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        JobReceiveQADocEvents.onHeaderSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, EventModeConstants.Edit), ref db);
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
                        JobReceiveQADocEvents.afterHeaderSaveEvent(this, new JobEventArgs(temp.JobReceiveQAHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveQAHeaderId,
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
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
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
            JobReceiveQAHeaderViewModel pt = _JobReceiveQAHeaderService.GetJobReceiveQAHeader(id);

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
            //Job Inspection Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQASettings", new { id = pt.DocTypeId }).Warning("Please create job Inspection settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);

            ViewBag.Mode = "Edit";
            if ((System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.JobReceiveQAHeaderId,
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

            JobReceiveQAHeaderViewModel pt = _JobReceiveQAHeaderService.GetJobReceiveQAHeader(id);

            //Job Inspection Settings
            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "JobReceiveQASettings", new { id = pt.DocTypeId }).Warning("Please create job Inspection settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            pt.JobReceiveQASettings = Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);
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
                    DocId = pt.JobReceiveQAHeaderId,
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
            JobReceiveQAHeader JobReceiveQAHeader = _JobReceiveQAHeaderService.Find(id);

            if (JobReceiveQAHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, JobReceiveQAHeader.DocTypeId, JobReceiveQAHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(JobReceiveQAHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                BeforeSave = JobReceiveQADocEvents.beforeHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
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

                var temp = (from p in db.JobReceiveQAHeader
                            where p.JobReceiveQAHeaderId == vm.id
                            select p).FirstOrDefault();


                try
                {
                    JobReceiveQADocEvents.onHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<JobReceiveQAHeader>(temp),
                });

                int? StockHeaderId = 0;
                StockHeaderId = temp.StockHeaderId;
                List<int> StockIdList = new List<int>();

                if (StockHeaderId != null)
                {
                    var Stocks = new StockService(_unitOfWork).GetStockForStockHeaderId((int)StockHeaderId);
                    foreach (var item in Stocks)
                    {
                        if (item.StockId != null)
                        {
                            StockIdList.Add((int)item.StockId);
                        }
                        StockAdj StockAdj = (from L in db.StockAdj where L.StockOutId == item.StockId select L).FirstOrDefault();
                        if (StockAdj != null)
                        {
                            StockAdj.ObjectState = Model.ObjectState.Deleted;
                            db.StockAdj.Remove(StockAdj);
                        }
                    }
                }


                //var line = new JobReceiveQALineService(_unitOfWork).GetLineListForIndex(vm.id);
                var line = (from p in db.JobReceiveQALine
                            where p.JobReceiveQAHeaderId == vm.id
                            select p).ToList();

                new JobReceiveLineStatusService(_unitOfWork).DeleteJobReceiveQtyOnQAMultiple(temp.JobReceiveQAHeaderId, ref db);

                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<JobReceiveQALine>(item),
                    });



                    IEnumerable<JobReceiveQAAttribute> AttributeList = (from L in db.JobReceiveQAAttribute where L.JobReceiveQALineId == item.JobReceiveQALineId select L).ToList();
                    foreach (var Attribute in AttributeList)
                    {
                        if (Attribute.JobReceiveQAAttributeId != null)
                        {
                            //new JobReceiveQAAttributeService(_unitOfWork).Delete((int)Attribute.JobReceiveQAAttributeId);
                            //JobReceiveQAAttribute Temp = db.JobReceiveQAAttribute.Find(id);
                            Attribute.ObjectState = Model.ObjectState.Deleted;
                            db.JobReceiveQAAttribute.Remove(Attribute);
                        }
                    }

                    IEnumerable<JobReceiveQAPenalty> PenaltyList = (from L in db.JobReceiveQAPenalty where L.JobReceiveQALineId == item.JobReceiveQALineId select L).ToList();
                    foreach (var Penalty in PenaltyList)
                    {
                        if (Penalty.JobReceiveQAPenaltyId != null)
                        {
                            new JobReceiveQAPenaltyService(db, _unitOfWork).Delete((int)Penalty.JobReceiveQAPenaltyId);
                        }
                    }

                    JobReceiveQALineExtended QALineExtended = (from L in db.JobReceiveQALineExtended where L.JobReceiveQALineId == item.JobReceiveQALineId select L).FirstOrDefault();
                    if (QALineExtended != null)
                    {
                        QALineExtended.ObjectState = Model.ObjectState.Deleted;
                        db.JobReceiveQALineExtended.Remove(QALineExtended);
                    }


                    new JobReceiveQALineService(db,_unitOfWork).Delete(item);
                }



                new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);


                DeleteProdOrder(temp);

                _JobReceiveQAHeaderService.Delete(temp);

                //temp.ObjectState = Model.ObjectState.Deleted;
                //db.JobReceiveQAHeader.Remove(temp);
                ////_JobReceiveQAHeaderService.Delete(vm.id);               

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
                    JobReceiveQADocEvents.afterHeaderDeleteEvent(this, new JobEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.JobReceiveQAHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    DocDate = temp.DocDate,
                    DocNo = temp.DocNo,
                    DocStatus = temp.Status,
                }));


                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            JobReceiveQAHeader s = db.JobReceiveQAHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            
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
                BeforeSave = JobReceiveQADocEvents.beforeHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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

                JobReceiveQAHeader pd = _JobReceiveQAHeaderService.Find(Id);
                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    JobReceiveQASettings Settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                    if (Settings.isPostedInStock == true)
                    {
                        IEnumerable<Stock> StockList = (from L in db.Stock
                                                        where L.StockHeaderId == pd.StockHeaderId
                                                        select L).ToList();

                        foreach (var stockdet in StockList)
                        {
                            StockAdj Adj = (from L in db.StockAdj
                                            where L.StockOutId == stockdet.StockId
                                            select L).FirstOrDefault();
                        
                            if (Adj!= null)
                            {
                                new StockAdjService(_unitOfWork).Delete(Adj);
                            }
                            new StockService(_unitOfWork).Delete(stockdet, db);
                        }
                        int? StockHeaderId = new JobReceiveQALineService(db, _unitOfWork).StockPost(Id);
                        pd.StockHeaderId = StockHeaderId;
                    }

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.JobReceiveQAHeader.Add(pd);


                    
                    if (Settings.DocTypeProductionOrderId != null)
                    {
                        CreateProdOrder(pd, (int) Settings.DocTypeProductionOrderId);
                    }


                    try
                    {
                        JobReceiveQADocEvents.onHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
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
                        JobReceiveQADocEvents.afterHeaderSubmitEvent(this, new JobEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.JobReceiveQAHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocDate = pd.DocDate,
                        DocNo = pd.DocNo,
                        DocStatus = pd.Status,
                    }));

                    string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveQAHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.JobReceiveQAHeaders", "JobReceiveQAHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {
                            var PendingtoSubmitCount = PendingToSubmitCount(pd.DocTypeId);
                            if (PendingtoSubmitCount > 0)
                                //return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId });
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveQAHeader" + "/" + "Index_PendingToSubmit" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                            else
                                //return RedirectToAction("Index", new { id = pd.DocTypeId });
                                ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveQAHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;

                        }
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveQAHeader" + "/" + "Submit" + "/" + nextId + "?TransactionType=submitContinue&IndexType=" + IndexType;
                    }
                    else
                    {
                        ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "JobReceiveQAHeader" + "/" + "Index" + "/" + pd.DocTypeId + "?IndexType=" + IndexType;
                    }
                    return Redirect(ReturnUrl).Success("Record Submitted Successfully");
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
                BeforeSave = JobReceiveQADocEvents.beforeHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
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
                JobReceiveQAHeader pd = _JobReceiveQAHeaderService.Find(Id);

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.JobReceiveQAHeader.Add(pd);


                //_JobReceiveQAHeaderService.Update(pd);

                try
                {
                    JobReceiveQADocEvents.onHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
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
                    JobReceiveQADocEvents.afterHeaderReviewEvent(this, new JobEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }


                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.JobReceiveQAHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocDate = pd.DocDate,
                    DocNo = pd.DocNo,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    JobReceiveQAHeader HEader = _JobReceiveQAHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.JobReceiveQAHeaders", "JobReceiveQAHeaderId", PrevNextConstants.Next);
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobReceiveQAHeaders", "JobReceiveQAHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.JobReceiveQAHeaders", "JobReceiveQAHeaderId", PrevNextConstants.Prev);
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
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
            {
                var SEttings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(""))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, "");
            }
            else
                return HttpNotFound();

        }

        [HttpGet]
        public ActionResult PrintAfter_Submit(int id)
        {
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
            {
                var SEttings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(""))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, "");
            }
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult PrintAfter_Approve(int id)
        {
            JobReceiveQAHeader header = _JobReceiveQAHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(""))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, "");
            }
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = db.DocumentType.Find(id);

            JobReceiveQASettings SEttings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

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
                //ReportLine Process = new ReportLineService(_unitOfWork).GetReportLineByName("Process", header.ReportHeaderId);
                //if (Process != null)
                //    DefaultValue.Add(Process.ReportLineId, ((int)SEttings.ProcessId).ToString());
            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }


        public int PendingToSubmitCount(int id)
        {
            return (_JobReceiveQAHeaderService.GetJobReceiveQAHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_JobReceiveQAHeaderService.GetJobReceiveQAHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            JobReceiveQAHeaderViewModel vm = new JobReceiveQAHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, vm.DivisionId, vm.SiteId);

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

                var Settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = db.JobReceiveQAHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobReceiveQAHeaderId,
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
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(id, DivisionId, SiteId);

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
                        return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + menuviewmodel.RouteId + "?MenuId=" + menuviewmodel.MenuId);
                    }
                    else
                    {
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public void DeleteProdOrder(JobReceiveQAHeader Header)
        {
            IEnumerable<ProdOrderHeader> prodorderheaderlist = (from H in db.ProdOrderHeader where H.ReferenceDocId == Header.JobReceiveQAHeaderId && H.ReferenceDocTypeId == Header.DocTypeId select H).ToList();

            foreach (var prodorderheader in prodorderheaderlist)
            {
                IEnumerable<ProdOrderLine> ProdOrderLineList = (from L in db.ProdOrderLine where L.ProdOrderHeaderId == prodorderheader.ProdOrderHeaderId select L).ToList();

                foreach (var prodorderline in ProdOrderLineList)
                {
                    ProdOrderLineStatus prodorderlinestatus = (from L in db.ProdOrderLineStatus where L.ProdOrderLineId == prodorderline.ProdOrderLineId select L).FirstOrDefault();
                    if (prodorderlinestatus != null)
                    {
                        prodorderlinestatus.ObjectState = Model.ObjectState.Deleted;
                        db.ProdOrderLineStatus.Remove(prodorderlinestatus);
                    }

                    prodorderline.ObjectState = Model.ObjectState.Deleted;
                    db.ProdOrderLine.Remove(prodorderline);
                }

                ProdOrderHeaderStatus prodorderheaderstatus = (from L in db.ProdOrderHeaderStatus where L.ProdOrderHeaderId == prodorderheader.ProdOrderHeaderId select L).FirstOrDefault();
                if (prodorderheaderstatus != null)
                {
                    prodorderheaderstatus.ObjectState = Model.ObjectState.Deleted;
                    db.ProdOrderHeaderStatus.Remove(prodorderheaderstatus);
                }

                prodorderheader.ObjectState = Model.ObjectState.Deleted;
                db.ProdOrderHeader.Remove(prodorderheader);
            }
        }

        //public void CreateProdOrder(int JobReceiveQAHeaderId, int ProdOrderDocTypeId)
        public void CreateProdOrder(JobReceiveQAHeader Header, int ProdOrderDocTypeId)
        {
            //var Header = (from p in db.JobReceiveQAHeader
            //              where p.JobReceiveQAHeaderId == JobReceiveQAHeaderId
            //              select p).FirstOrDefault();

            DeleteProdOrder(Header);

            Decimal FailedQty = (from L in db.JobReceiveQALine
                                 where L.JobReceiveQAHeaderId == Header.JobReceiveQAHeaderId
                                 select new { FailQty = L.FailQty }).FirstOrDefault().FailQty;



            //var Header = (from H in db.JobReceiveQAHeader
            //              where H.JobReceiveQAHeaderId == JobReceiveQAHeaderId
            //              select new
            //              {
            //                  JobReceiveQAHeaderId = H.JobReceiveQAHeaderId,
            //                  DocTypeId = H.DocTypeId,
            //                  DocDate = H.DocDate,
            //                  DocNo = H.DocNo,
            //                  DivisionId = H.DivisionId,
            //                  SiteId = H.SiteId,
            //                  ProcessId = H.ProcessId,
            //                  Status = H.Status,
            //                  Remark = H.Remark,
            //                  CreatedBy = H.CreatedBy,
            //                  ModifiedBy = H.ModifiedBy
            //              }).FirstOrDefault();


            int DyeingProcessId = (from P in db.Process
                                   where P.ProcessName == ProcessConstants.Dyeing
                                   select new { ProcessId = P.ProcessId }).FirstOrDefault().ProcessId;

            if (FailedQty > 0)
            {
                int PersonId = (from L in db.JobReceiveQALine
                                join Rl in db.JobReceiveLine on L.JobReceiveLineId equals Rl.JobReceiveLineId
                                join Jol in db.JobOrderLine on Rl.JobOrderLineId equals Jol.JobOrderLineId
                                join Joh in db.JobOrderHeaderExtended on Jol.JobOrderHeaderId equals Joh.JobOrderHeaderId
                                where L.JobReceiveQAHeaderId == Header.JobReceiveQAHeaderId
                                select new { PersonId = Joh.PersonId }).FirstOrDefault().PersonId;


                ProdOrderHeader ProdOrderHeader = new ProdOrderHeader();
                ProdOrderHeader.ProdOrderHeaderId = -1;
                ProdOrderHeader.DocTypeId = ProdOrderDocTypeId;
                ProdOrderHeader.DocDate = Header.DocDate;
                ProdOrderHeader.DocNo = Header.DocNo;
                ProdOrderHeader.DivisionId = Header.DivisionId;
                ProdOrderHeader.SiteId = Header.SiteId;
                ProdOrderHeader.DueDate = Header.DocDate;
                ProdOrderHeader.ReferenceDocTypeId = Header.DocTypeId;
                ProdOrderHeader.ReferenceDocId = Header.JobReceiveQAHeaderId;
                ProdOrderHeader.Status = Header.Status;
                ProdOrderHeader.Remark = Header.Remark;
                ProdOrderHeader.BuyerId = PersonId;
                ProdOrderHeader.CreatedBy = Header.CreatedBy;
                ProdOrderHeader.CreatedDate = DateTime.Now;
                ProdOrderHeader.ModifiedBy = Header.ModifiedBy;
                ProdOrderHeader.ModifiedDate = DateTime.Now;
                ProdOrderHeader.LockReason = "Prod order automatically generated from Job QA.";
                ProdOrderHeader.ObjectState = Model.ObjectState.Added;
                db.ProdOrderHeader.Add(ProdOrderHeader);

                ProdOrderHeaderStatus ProdOrderHeaderStatus = new ProdOrderHeaderStatus();
                ProdOrderHeaderStatus.ProdOrderHeaderId = ProdOrderHeader.ProdOrderHeaderId;
                db.ProdOrderHeaderStatus.Add(ProdOrderHeaderStatus);


                IEnumerable<JobReceiveQALineViewModel> Line = (from L in db.JobReceiveQALine
                                                               where L.JobReceiveQAHeaderId == Header.JobReceiveQAHeaderId
                                                               select new JobReceiveQALineViewModel
                                                               {
                                                                   ProductId = L.JobReceiveLine.JobOrderLine.ProductId,
                                                                   Dimension1Id = L.JobReceiveLine.JobOrderLine.Dimension1Id,
                                                                   Dimension2Id = L.JobReceiveLine.JobOrderLine.Dimension2Id,
                                                                   Dimension3Id = L.JobReceiveLine.JobOrderLine.Dimension3Id,
                                                                   Dimension4Id = L.JobReceiveLine.JobOrderLine.Dimension4Id,
                                                                   JobReceiveQALineId = L.JobReceiveQALineId,
                                                                   FailQty = L.FailQty
                                                               }).ToList();



                int ProdOrderLineId = 0;
                foreach (var item in Line)
                {
                    ProdOrderLineId = ProdOrderLineId - 1;
                    ProdOrderLine ProdOrderLine = new ProdOrderLine();
                    ProdOrderLine.ProdOrderLineId = ProdOrderLineId;
                    ProdOrderLine.ProdOrderHeaderId = ProdOrderHeader.ProdOrderHeaderId;
                    ProdOrderLine.ProductId = item.ProductId;
                    ProdOrderLine.Dimension1Id = item.Dimension1Id;
                    ProdOrderLine.Dimension2Id = item.Dimension2Id;
                    ProdOrderLine.Dimension3Id = item.Dimension3Id;
                    ProdOrderLine.Dimension4Id = item.Dimension4Id;
                    ProdOrderLine.Specification = null;
                    ProdOrderLine.ProcessId = Header.ProcessId;
                    ProdOrderLine.ReferenceDocTypeId = ProdOrderHeader.DocTypeId;
                    ProdOrderLine.ReferenceDocLineId = item.JobReceiveLineId;
                    ProdOrderLine.Sr = item.Sr;
                    ProdOrderLine.Qty = item.FailQty;
                    ProdOrderLine.Remark = item.Remark;
                    ProdOrderLine.CreatedBy = ProdOrderHeader.CreatedBy;
                    ProdOrderLine.ModifiedBy = ProdOrderHeader.ModifiedBy;
                    ProdOrderLine.CreatedDate = DateTime.Now;
                    ProdOrderLine.ModifiedDate = DateTime.Now;
                    ProdOrderLine.LockReason = "Prod order automatically generated from recipe.";
                    ProdOrderLine.ObjectState = Model.ObjectState.Added;
                    db.ProdOrderLine.Add(ProdOrderLine);


                    ProdOrderLineStatus ProdOrderLineStatus = new ProdOrderLineStatus();
                    ProdOrderLineStatus.ProdOrderLineId = ProdOrderLine.ProdOrderLineId;
                    db.ProdOrderLineStatus.Add(ProdOrderLineStatus);
                }
            }
        }


        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _JobReceiveQAHeaderService.GetCustomPerson(filter, searchTerm);
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
            int Qaline = (from p in db.JobReceiveQALine
                     where p.JobReceiveQAHeaderId == id
                     select p).Count();
            if (Qaline == 0)
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
