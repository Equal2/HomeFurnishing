using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Model.ViewModels;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using System.Configuration;
using Presentation;
using Model.ViewModel;
using System.Xml.Linq;
using StockProcessTransferDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Reports;
using Reports.Controllers;

namespace Jobs.Controllers
{
    [Authorize]
    public class StockProcessTransferController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IStockHeaderService _StockHeaderService;
        IStockLineService _StockLineService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public StockProcessTransferController(IStockHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IStockLineService Stock)
        {
            _StockHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            _StockLineService = Stock;
            if (!StockProcessTransferEvents.Initialized)
            {
                StockProcessTransferEvents Obj = new StockProcessTransferEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /StockHeader/

        public ActionResult DocumentIndex(int id)//DocumentTypeId
        {
            var p = new DocumentTypeService(_unitOfWork).Find(id);

            return RedirectToAction("DocumentTypeIndex", new { id = p.DocumentCategoryId });
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
            IQueryable<StockHeaderViewModel> p = _StockHeaderService.GetStockHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }
        public ActionResult Index_PendingToSubmit(int id)
        {
            IQueryable<StockHeaderViewModel> p = _StockHeaderService.GetStockHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<StockHeaderViewModel> p = _StockHeaderService.GetStockHeaderListPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", p);
        }

        private void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
        }

        // GET: /StockHeader/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            StockHeaderViewModel p = new StockHeaderViewModel();

            p.DocDate = DateTime.Now;
            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForProcessTransfer", "StockHeaderSettings", new { id = id }).Warning("Please create Process Transfer settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            p.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            p.ProcessId = settings.ProcessId;

            PrepareViewBag(id);

            p.DocTypeId = id;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".StockHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            ViewBag.Mode = "Add";
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(StockHeaderViewModel svm)
        {

            #region BeforeSave
            bool BeforeSave = true;

            try
            {
                if (svm.StockHeaderId <= 0)
                    BeforeSave = StockProcessTransferDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = StockProcessTransferDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);
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

                if (svm.StockHeaderId <= 0)
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Create, User.Identity.Name, out ExceptionMsg, out Continue);
                else
                    TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(svm), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            StockHeader s = Mapper.Map<StockHeaderViewModel, StockHeader>(svm);


            if (svm.StockHeaderSettings != null)
            {
                if (svm.StockHeaderSettings.isMandatoryHeaderCostCenter == true && (svm.CostCenterId <= 0 || svm.CostCenterId == null))
                {
                    ModelState.AddModelError("CostCenterId", "The CostCenter field is required");
                }
                if (svm.StockHeaderSettings.isMandatoryMachine == true && (svm.MachineId <= 0 || svm.MachineId == null))
                {
                    ModelState.AddModelError("MachineId", "The Machine field is required");
                }
            }

            if (ModelState.IsValid && BeforeSave && !EventException && (TimePlanValidation || Continue))
            {

                #region CreateRecord
                if (svm.StockHeaderId <= 0)
                {

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Status = (int)StatusConstants.Drafted;
                    //_StockHeaderService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;

                    db.StockHeader.Add(s);

                    try
                    {
                        StockProcessTransferDocEvents.onHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                    try
                    {
                        StockProcessTransferDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = s.DocTypeId,
                        DocId = s.StockHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = s.DocNo,
                        DocDate = s.DocDate,
                        DocStatus = s.Status,
                    }));                  

                    return RedirectToAction("Modify", "StockProcessTransfer", new { Id = s.StockHeaderId }).Success("Data saved successfully");

                }
                #endregion

                #region EditRecord
                else
                {
                    bool GodownChanged = false;
                    bool DocDateChanged = false;
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeader temp = _StockHeaderService.Find(s.StockHeaderId);

                    GodownChanged = (temp.GodownId == s.GodownId) ? false : true;
                    DocDateChanged = (temp.DocDate == s.DocDate) ? false : true;

                    StockHeader ExRec = new StockHeader();
                    ExRec = Mapper.Map<StockHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    { temp.Status = (int)StatusConstants.Modified;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    }


                    temp.DocDate = s.DocDate;
                    temp.DocNo = s.DocNo;
                    temp.CostCenterId = s.CostCenterId;
                    temp.MachineId = s.MachineId;
                    temp.PersonId = s.PersonId;
                    temp.ProcessId = s.ProcessId;
                    temp.GodownId = s.GodownId;
                    temp.Remark = s.Remark;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    //_StockHeaderService.Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(temp);

                    if (GodownChanged || DocDateChanged)
                        new StockService(_unitOfWork).UpdateStockGodownId(temp.StockHeaderId, temp.GodownId, temp.DocDate, db);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        StockProcessTransferDocEvents.onHeaderSaveEvent(this, new StockEventArgs(temp.StockHeaderId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.id = svm.DocTypeId;
                        return View("Create", svm);
                    }

                    try
                    {
                        StockProcessTransferDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("Index", new { id = svm.DocTypeId }).Success("Data saved successfully");

                }
                #endregion

            }
            PrepareViewBag(svm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        // GET: /StockHeader/Edit/5
        [HttpGet]
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;

            StockHeaderViewModel s = _StockHeaderService.GetStockHeader(id);

            if (s == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(s), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForProcessTransfer", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Process Transfer settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            ViewBag.Mode = "Edit";
            PrepareViewBag(s.DocTypeId);

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.StockHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

            return View("Create", s);
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", IndexType = IndexType });
        }

        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType)
        {

            //Saving ViewBag Data::

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            StockHeaderViewModel s = _StockHeaderService.GetStockHeader(id);

            //Job Order Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null)
            {
                return RedirectToAction("CreateForProcessTransfer", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
            }

            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.StockHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));

            return View("Create", s);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }


        // GET: /PurchaseOrderHeader/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockHeader StockHeader = _StockHeaderService.Find(id);
            if (StockHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, StockHeader.DocTypeId, StockHeader.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(StockHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };
            return PartialView("_Reason", rvm);
        }



        // POST: /PurchaseOrderHeader/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = StockProcessTransferDocEvents.beforeHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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

                //first find the Purchase Order Object based on the ID. (sience this object need to marked to be deleted IE. ObjectState.Deleted)
                StockHeader StockHeader = db.StockHeader.Find(vm.id);

                try
                {
                    StockProcessTransferDocEvents.onHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                StockHeader ExRec = new StockHeader();
                ExRec = Mapper.Map<StockHeader>(StockHeader);
                StockHeader Rec = new StockHeader();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<StockHeader>(ExRec),
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var StockLine = (from p in db.StockLine
                                 where p.StockHeaderId == vm.id
                                 select p).ToList();


                List<int> StockIdList = new List<int>();
                List<int> StockProcessIdList = new List<int>();

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in StockLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<StockLine>(item),
                    });

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    if (item.StockProcessId != null)
                    {
                        StockProcessIdList.Add((int)item.StockProcessId);
                    }
                    if (item.FromStockProcessId != null)
                    {
                        StockProcessIdList.Add((int)item.FromStockProcessId);
                    }


                    item.ObjectState = Model.ObjectState.Deleted;
                    db.StockLine.Remove(item);
                    //new StockLineService(_unitOfWork).Delete(item);
                }

                new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);

                new StockProcessService(_unitOfWork).DeleteStockProcessDBMultiple(StockProcessIdList, ref db, true);

                // Now delete the Purhcase Order Header
                //new StockHeaderService(_unitOfWork).Delete(StockHeader);
                StockHeader.ObjectState = Model.ObjectState.Deleted;
                db.StockHeader.Remove(StockHeader);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);



                //Commit the DB
                try
                {
                    if (EventException)
                        throw new Exception();
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
                    StockProcessTransferDocEvents.afterHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = StockHeader.DocTypeId,
                    DocId = StockHeader.StockHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = StockHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = StockHeader.DocDate,
                    DocStatus = StockHeader.Status,
                }));             


                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }


        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            StockHeader s = db.StockHeader.Find(id);
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
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = StockProcessTransferDocEvents.beforeHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            StockHeader pd = new StockHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    int ActivityType;

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    //_StockHeaderService.Update(pd);
                    //_unitOfWork.Save();
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(pd);

                    try
                    {
                        StockProcessTransferDocEvents.onHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
                    }

                    try
                    {
                        StockProcessTransferDocEvents.afterHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.StockHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));                  

                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Submitted Successfully");
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
                BeforeSave = StockProcessTransferDocEvents.beforeHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before Review.";

            StockHeader pd = new StockHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.StockHeader.Add(pd);

                try
                {
                    StockProcessTransferDocEvents.onHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                db.SaveChanges();

                try
                {
                    StockProcessTransferDocEvents.afterHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.StockHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));              

                //SendEmail_POApproved(Id);
                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Reviewed Successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in Reviewing.");
        }



        public int PendingToReviewCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        public int PendingToSubmitCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }















        // // // // StockProcessTransfer Line

        public ActionResult _ForProcessTransfer(int id, int sid, int ProcessId, int? CostCenterId)
        {
            FiltersForProcessTransfer vm = new FiltersForProcessTransfer();
            if (CostCenterId.HasValue && CostCenterId.Value > 0)
                vm.FromCostCenterId = CostCenterId.Value;
            vm.StockHeaderId = id;
            vm.PersonId = sid;
            vm.ProcessId = ProcessId;
            //vm.FromCostCenterId = FromCostCenterId;
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(FiltersForProcessTransfer vm)
        {

            if (vm.FromCostCenterId <= 0 || vm.ToCostCenterId <= 0)
            {
                ModelState.AddModelError("FromCostCenterId", "From Costcenter field is required");
                ModelState.AddModelError("ToCostCenterId", "To Costcenter field is required");
            }

            if (ModelState.IsValid)
            {
                List<StockExchangeLineViewModel> temp = _StockLineService.GetProcessTransfersForFilters(vm).ToList();

                StockExchangeMasterDetailModel svm = new StockExchangeMasterDetailModel();
                svm.StockLineViewModel = temp;
                //Getting Settings           
                var Header = new StockHeaderService(_unitOfWork).Find(vm.StockHeaderId);
                svm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId));
                return PartialView("_Results", svm);
            }
            else
                return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(StockExchangeMasterDetailModel vm)
        {
            int Cnt = 0;
            int pk = 0;


            StockHeader Header = new StockHeaderService(_unitOfWork).Find(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);

            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            Header.CostCenterId = vm.StockLineViewModel.FirstOrDefault().CostCenterId;

            bool BeforeSave = true;
            try
            {
                BeforeSave = StockProcessTransferDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                var ToCosterName = db.CostCenter.Find(vm.StockLineViewModel.FirstOrDefault().ToCostCenterId);

                foreach (var item in vm.StockLineViewModel.Where(m => m.Qty != 0))
                {
                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                    if (item.Qty != 0)
                    {
                        StockLine line = new StockLine();

                        StockProcessViewModel StockProcessIssueViewModel = new StockProcessViewModel();

                        if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessIssueViewModel.StockHeaderId = (int)Header.StockHeaderId;
                        }
                        else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                        {
                            StockProcessIssueViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessIssueViewModel.StockHeaderId = 0;
                        }
                        StockProcessIssueViewModel.StockProcessId = -Cnt;
                        StockProcessIssueViewModel.DocHeaderId = Header.StockHeaderId;
                        StockProcessIssueViewModel.DocLineId = line.StockLineId;
                        StockProcessIssueViewModel.DocTypeId = Header.DocTypeId;
                        StockProcessIssueViewModel.StockHeaderDocDate = Header.DocDate;
                        StockProcessIssueViewModel.StockProcessDocDate = Header.DocDate;
                        StockProcessIssueViewModel.DocNo = Header.DocNo;
                        StockProcessIssueViewModel.DivisionId = Header.DivisionId;
                        StockProcessIssueViewModel.SiteId = Header.SiteId;
                        StockProcessIssueViewModel.CurrencyId = null;
                        StockProcessIssueViewModel.PersonId = Header.PersonId;
                        StockProcessIssueViewModel.ProductId = item.ProductId;
                        StockProcessIssueViewModel.HeaderFromGodownId = null;
                        StockProcessIssueViewModel.HeaderGodownId = Header.GodownId;
                        StockProcessIssueViewModel.HeaderProcessId = Header.ProcessId;
                        StockProcessIssueViewModel.GodownId = Header.GodownId;
                        StockProcessIssueViewModel.Remark = "To Costcenter " + ToCosterName.CostCenterName  + " " + Header.Remark;
                        StockProcessIssueViewModel.Status = Header.Status;
                        StockProcessIssueViewModel.ProcessId = item.ProcessId;
                        StockProcessIssueViewModel.LotNo = null;
                        StockProcessIssueViewModel.CostCenterId = Header.CostCenterId;
                        StockProcessIssueViewModel.Qty_Iss = item.Qty > 0 ? item.Qty : 0;
                        StockProcessIssueViewModel.Qty_Rec = item.Qty < 0 ? Math.Abs(item.Qty) : 0;
                        StockProcessIssueViewModel.Rate = item.Rate;
                        StockProcessIssueViewModel.ExpiryDate = null;
                        StockProcessIssueViewModel.Specification = item.Specification;
                        StockProcessIssueViewModel.Dimension1Id = item.Dimension1Id;
                        StockProcessIssueViewModel.Dimension2Id = item.Dimension2Id;
                        StockProcessIssueViewModel.Dimension3Id = item.Dimension3Id;
                        StockProcessIssueViewModel.Dimension4Id = item.Dimension4Id;
                        StockProcessIssueViewModel.CreatedBy = User.Identity.Name;
                        StockProcessIssueViewModel.CreatedDate = DateTime.Now;
                        StockProcessIssueViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessIssueViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessIssueViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            string message = StockProcessPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }


                        line.StockProcessId = StockProcessIssueViewModel.StockProcessId;

                        Cnt = Cnt + 1;


                        StockProcessViewModel StockProcessReceiveViewModel = new StockProcessViewModel();

                        if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                        {
                            StockProcessReceiveViewModel.StockHeaderId = (int)Header.StockHeaderId;
                        }
                        else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                        {
                            StockProcessReceiveViewModel.StockHeaderId = -1;
                        }
                        else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                        {
                            StockProcessReceiveViewModel.StockHeaderId = 0;
                        }
                        StockProcessReceiveViewModel.StockProcessId = -Cnt;
                        StockProcessReceiveViewModel.DocHeaderId = Header.StockHeaderId;
                        StockProcessReceiveViewModel.DocLineId = line.StockLineId;
                        StockProcessReceiveViewModel.DocTypeId = Header.DocTypeId;
                        StockProcessReceiveViewModel.StockHeaderDocDate = Header.DocDate;
                        StockProcessReceiveViewModel.StockProcessDocDate = Header.DocDate;
                        StockProcessReceiveViewModel.DocNo = Header.DocNo;
                        StockProcessReceiveViewModel.DivisionId = Header.DivisionId;
                        StockProcessReceiveViewModel.SiteId = Header.SiteId;
                        StockProcessReceiveViewModel.CurrencyId = null;
                        StockProcessReceiveViewModel.PersonId = Header.PersonId;
                        StockProcessReceiveViewModel.ProductId = item.ProductId;
                        StockProcessReceiveViewModel.HeaderFromGodownId = null;
                        StockProcessReceiveViewModel.HeaderGodownId = Header.GodownId;
                        StockProcessReceiveViewModel.HeaderProcessId = Header.ProcessId;
                        StockProcessReceiveViewModel.GodownId = Header.GodownId;
                        StockProcessReceiveViewModel.Remark = "From Costcenter " + item.CostCenterName  + " " + Header.Remark;
                        StockProcessReceiveViewModel.Status = Header.Status;
                        StockProcessReceiveViewModel.ProcessId = Header.ProcessId;
                        StockProcessReceiveViewModel.LotNo = null;
                        StockProcessReceiveViewModel.CostCenterId = item.ToCostCenterId;
                        StockProcessReceiveViewModel.Qty_Iss = item.Qty < 0 ? Math.Abs(item.Qty) : 0;
                        StockProcessReceiveViewModel.Qty_Rec = item.Qty > 0 ? item.Qty : 0;
                        StockProcessReceiveViewModel.Rate = item.Rate;
                        StockProcessReceiveViewModel.ExpiryDate = null;
                        StockProcessReceiveViewModel.Specification = item.Specification;
                        StockProcessReceiveViewModel.Dimension1Id = item.Dimension1Id;
                        StockProcessReceiveViewModel.Dimension2Id = item.Dimension2Id;
                        StockProcessReceiveViewModel.Dimension3Id = item.Dimension3Id;
                        StockProcessReceiveViewModel.Dimension4Id = item.Dimension4Id;
                        StockProcessReceiveViewModel.CreatedBy = User.Identity.Name;
                        StockProcessReceiveViewModel.CreatedDate = DateTime.Now;
                        StockProcessReceiveViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessReceiveViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingReceiveError = "";
                        StockProcessPostingReceiveError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessReceiveViewModel, ref db);

                        if (StockProcessPostingReceiveError != "")
                        {
                            string message = StockProcessPostingReceiveError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        line.FromStockProcessId = StockProcessReceiveViewModel.StockProcessId;


                        line.StockHeaderId = item.StockHeaderId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.CostCenterId = item.ToCostCenterId;
                        line.Qty = item.Qty;
                        line.DocNature = StockNatureConstants.Transfer;
                        line.Rate = item.Rate ?? 0;
                        line.Amount = (line.Qty * line.Rate);
                        line.FromProcessId = item.ProcessId;
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.StockLineId = pk;
                        line.ObjectState = Model.ObjectState.Added;
                        //_StockLineService.Create(line);
                        db.StockLine.Add(line);
                        pk++;
                        Cnt = Cnt + 1;

                    }

                }

                if (Header.Status != (int)StatusConstants.Drafted)
                {
                    Header.Status = (int)StatusConstants.Modified;
                    Header.ModifiedBy = User.Identity.Name;
                    Header.ModifiedDate = DateTime.Now;
                }
                //new StockHeaderService(_unitOfWork).Update(Header);

                Header.ObjectState = Model.ObjectState.Modified;
                db.StockHeader.Add(Header);

                try
                {
                    StockProcessTransferDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
                    return PartialView("_Results", vm);
                }

                try
                {
                    StockProcessTransferDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Header.DocTypeId,
                    DocId = Header.StockHeaderId,
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
        public JsonResult TransferIndex(int id)
        {
            var p = _StockLineService.GetStockLineListForTransfer(id).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }


        public JsonResult GetCostCentersForDocType(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {

            var StockHead = db.StockHeader.Find(filter);

            var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(StockHead.DocTypeId, StockHead.DivisionId, StockHead.SiteId);

            var Query = _StockLineService.GetCostCentersForProcessTransfer(StockHead.SiteId, StockHead.DivisionId, Settings.filterContraDocTypes, searchTerm, StockHead.PersonId);

            var temp = Query.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //return Json(new {  Data=Data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCostCentersForProcessTransfer(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {

            var Header = new StockHeaderService(_unitOfWork).Find(filter);

            var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            var temp = _StockLineService.GetPendingCCFromStockProcBal(Header.StockHeaderId, null, Header.ProcessId.Value, Header.PersonId.Value, searchTerm).ToList().Select(m => new ComboBoxResult { id = m.Id.ToString(), text = m.PropFirst }).ToList();

            var count = temp.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //return Json(new {  Data=Data }, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetProducts(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        //{
        //    var temp = new RequisitionHeaderService(_unitOfWork).GetProductsForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

        //    var count = new RequisitionHeaderService(_unitOfWork).GetProductsForExchange(filter, searchTerm).Count();

        //    ComboBoxPagedResult Data = new ComboBoxPagedResult();
        //    Data.Results = temp;
        //    Data.Total = count;

        //    return new JsonpResult
        //    {
        //        Data = Data,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };

        //    //return Json(new {  Data=Data }, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetDimension1(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        //{
        //    var temp = new RequisitionHeaderService(_unitOfWork).GetDimension1ForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

        //    var count = new RequisitionHeaderService(_unitOfWork).GetDimension1ForExchange(filter, searchTerm).Count();

        //    ComboBoxPagedResult Data = new ComboBoxPagedResult();
        //    Data.Results = temp;
        //    Data.Total = count;

        //    return new JsonpResult
        //    {
        //        Data = Data,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };

        //    //return Json(new {  Data=Data }, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult GetDimension2(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        //{
        //    var temp = new RequisitionHeaderService(_unitOfWork).GetDimension2ForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

        //    var count = new RequisitionHeaderService(_unitOfWork).GetDimension2ForExchange(filter, searchTerm).Count();

        //    ComboBoxPagedResult Data = new ComboBoxPagedResult();
        //    Data.Results = temp;
        //    Data.Total = count;

        //    return new JsonpResult
        //    {
        //        Data = Data,
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };

        //    //return Json(new {  Data=Data }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

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

                var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(DocTypeId, DivisionId, SiteId);

                if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DocTypeId, Settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "GeneratePrints") == false)
                {
                    return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
                }

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.StockHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.StockHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified)
                        {
                            //LogAct(item.ToString());
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                        {
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else
                        {
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
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
            var Query = _StockHeaderService.GetCustomPerson(filter, searchTerm);
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
