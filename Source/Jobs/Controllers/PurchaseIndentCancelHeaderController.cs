using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core.Common;
using Model.Models;
using Data.Models;
using Service;
using Jobs.Helpers;
using Data.Infrastructure;
using Presentation.ViewModels;
using AutoMapper;
using System.Configuration;
using Presentation;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using PurchaseIndentCancelDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Reports.Reports;


namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseIndentCancelHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseIndentCancelHeaderService _PurchaseIndentCancelHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseIndentCancelHeaderController(IPurchaseIndentCancelHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseIndentCancelHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PurchaseIndentCancelEvents.Initialized)
            {
                PurchaseIndentCancelEvents Obj = new PurchaseIndentCancelEvents();
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

        // GET: /PurchaseIndentCancelHeader/

        public ActionResult Index(int id, string IndexType)//DocumentTypeIndex
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            IQueryable<PurchaseIndentCancelHeaderViewModel> p = _PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseIndentCancelHeaders", "PurchaseIndentCancelHeaderId", PrevNextConstants.Next);
            return Edit(nextId,"");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseIndentCancelHeaders", "PurchaseIndentCancelHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId,"");
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
        public ActionResult Remove()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.ReasonList = new ReasonService(_unitOfWork).FindByDocumentType(id).ToList();

        }

        // GET: /PurchaseIndentCancelHeader/Create

        public ActionResult Create(int id)//DocumentTypeIndex
        {
            PrepareViewBag(id);
            PurchaseIndentCancelHeaderViewModel s = new PurchaseIndentCancelHeaderViewModel();
            s.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            s.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            s.CreatedDate = DateTime.Now;
            //Getting Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(id, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseIndentCancel", "PurchaseIndentSetting", new { id = id }).Warning("Please create Purchase Indent cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            s.PurchaseIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);
            s.DocTypeId = id;
            s.DocDate = DateTime.Now;
            s.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseIndentCancelHeaders", s.DocTypeId, s.DocDate, s.DivisionId, s.SiteId);

            ViewBag.Mode = "Add";
            return View("Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderPost(PurchaseIndentCancelHeaderViewModel svm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (svm.PurchaseIndentCancelHeaderId <= 0)
                    BeforeSave = PurchaseIndentCancelDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(svm.PurchaseIndentCancelHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseIndentCancelDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(svm.PurchaseIndentCancelHeaderId, EventModeConstants.Edit), ref db);
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

                if (svm.PurchaseIndentCancelHeaderId <= 0)
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

            if (ModelState.IsValid && BeforeSave && !EventException && (TimePlanValidation || Continue))
            {

                #region CreateRecord
                if (svm.PurchaseIndentCancelHeaderId <= 0)
                {
                    PurchaseIndentCancelHeader s = new PurchaseIndentCancelHeader();
                    s.DocTypeId = svm.DocTypeId;
                    s.DocDate = svm.DocDate;
                    s.DocNo = svm.DocNo;
                    s.Remark = svm.Remark;
                    s.CreatedDate = DateTime.Now;
                    s.SiteId = svm.SiteId;
                    s.ReasonId = svm.ReasonId;
                    s.DivisionId = svm.DivisionId;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Status = (int)StatusConstants.Drafted;
                    //_PurchaseIndentCancelHeaderService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;
                    db.PurchaseIndentCancelHeader.Add(s);

                    try
                    {
                        PurchaseIndentCancelDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentCancelHeaderId, EventModeConstants.Add), ref db);
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
                        PrepareViewBag(svm.DocTypeId);
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                    try
                    {
                        PurchaseIndentCancelDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(s.PurchaseIndentCancelHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = s.DocTypeId,
                        DocId = s.PurchaseIndentCancelHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = s.DocNo,
                        DocDate = s.DocDate,
                        DocStatus = s.Status,
                    }));

                    //return Edit(s.PurchaseIndentCancelHeaderId).Success("Data saved Successfully");
                    return RedirectToAction("Modify", new { id = s.PurchaseIndentCancelHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StringBuilder logstring = new StringBuilder();
                    PurchaseIndentCancelHeader temp = _PurchaseIndentCancelHeaderService.Find(svm.PurchaseIndentCancelHeaderId);

                    PurchaseIndentCancelHeader ExRec = new PurchaseIndentCancelHeader();
                    ExRec = Mapper.Map<PurchaseIndentCancelHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocTypeId = svm.DocTypeId;
                    temp.DocDate = svm.DocDate;
                    temp.DocNo = svm.DocNo;
                    temp.Remark = svm.Remark;
                    temp.ReasonId = svm.ReasonId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentCancelHeader.Add(temp);
                    //_PurchaseIndentCancelHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseIndentCancelDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseIndentCancelHeaderId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(svm.DocTypeId);
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        ViewBag.Mode = "Edit";
                        return View("Create", svm);
                    }

                    try
                    {
                        PurchaseIndentCancelDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseIndentCancelHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseIndentCancelHeaderId,
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


        // GET: /PurchaseIndentCancelHeader/Edit/5
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            PurchaseIndentCancelHeader s = _PurchaseIndentCancelHeaderService.Find(id);
            PurchaseIndentCancelHeaderViewModel svm = Mapper.Map<PurchaseIndentCancelHeader, PurchaseIndentCancelHeaderViewModel>(s);

            if (svm == null)
            {
                return HttpNotFound();
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

            PrepareViewBag(s.DocTypeId);
                   
            //Job Order Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(s.DocTypeId, svm.DivisionId, svm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseIndentCancel", "PurchaseIndentSetting", new { id = s.DocTypeId }).Warning("Please create Purchase Indent cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            svm.PurchaseIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);
            ViewBag.Mode = "Edit";


            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.PurchaseIndentCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));


            return View("Create", svm);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            PurchaseIndentCancelHeader header = _PurchaseIndentCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PurchaseIndentCancelHeader header = _PurchaseIndentCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }
       

        [HttpGet]
        public ActionResult Delete(int id)
        {
            PurchaseIndentCancelHeader header = _PurchaseIndentCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PurchaseIndentCancelHeader header = _PurchaseIndentCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
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

            PurchaseIndentCancelHeader s = _PurchaseIndentCancelHeaderService.Find(id);
            PurchaseIndentCancelHeaderViewModel svm = Mapper.Map<PurchaseIndentCancelHeader, PurchaseIndentCancelHeaderViewModel>(s);
            PrepareViewBag(s.DocTypeId);

            //Job Order Settings
            var settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null)
            {
                return RedirectToAction("Create", "PurchaseIndentSetting", new { id = s.DocTypeId }).Warning("Please create Purchase Indent cancel settings");
            }
            svm.PurchaseIndentSettings = Mapper.Map<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>(settings);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.PurchaseIndentCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));


            return View("Create", svm);
        }


        // GET: /PurchaseOrderHeader/Delete/5

        private ActionResult Remove(int id)
        {
            PurchaseIndentCancelHeader PurchaseIndentCancelHeader = _PurchaseIndentCancelHeaderService.Find(id);
            if (PurchaseIndentCancelHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PurchaseIndentCancelHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseIndentCancelDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
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
                var PurchaseIndentCancelHeader = db.PurchaseIndentCancelHeader.Find(vm.id);

                try
                {
                    PurchaseIndentCancelDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseIndentCancelHeader>(PurchaseIndentCancelHeader),
                });

                int doctypeid = PurchaseIndentCancelHeader.DocTypeId;

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var Line = (from p in db.PurchaseIndentCancelLine
                            where p.PurchaseIndentCancelHeaderId == vm.id
                            select p).ToList();

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in Line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseIndentCancelLine>(item),
                    });

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseIndentCancelLine.Remove(item);

                    //new PurchaseIndentCancelLineService(_unitOfWork).Delete(item.PurchaseIndentCancelLineId);
                }

                // Now delete the Purhcase Order Header
                //new PurchaseIndentCancelHeaderService(_unitOfWork).Delete(vm.id);

                PurchaseIndentCancelHeader.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseIndentCancelHeader.Remove(PurchaseIndentCancelHeader);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                //Commit the DB
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
                    PurchaseIndentCancelDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = PurchaseIndentCancelHeader.DocTypeId,
                    DocId = PurchaseIndentCancelHeader.PurchaseIndentCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = PurchaseIndentCancelHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = PurchaseIndentCancelHeader.DocDate,
                    DocStatus = PurchaseIndentCancelHeader.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }



        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            PurchaseIndentCancelHeader s = db.PurchaseIndentCancelHeader.Find(id);

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
            PurchaseIndentCancelHeader pd = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(Id);
            int ActivityType;

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseIndentCancelDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
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
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    //_PurchaseIndentCancelHeaderService.Update(pd);
                    //_unitOfWork.Save();
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseIndentCancelHeader.Add(pd);

                    try
                    {
                        PurchaseIndentCancelDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
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
                        PurchaseIndentCancelDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PurchaseIndentCancelHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseIndentCancelHeaders", "PurchaseIndentCancelHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = _PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType });
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Indent Cancel " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Indent Cancel " + pd.DocNo + " submitted successfully.");
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
            PurchaseIndentCancelHeader pd = new PurchaseIndentCancelHeaderService(_unitOfWork).Find(Id);

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseIndentCancelDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
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

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;
                db.PurchaseIndentCancelHeader.Add(pd);

                try
                {
                    PurchaseIndentCancelDocEvents.onHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
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
                    PurchaseIndentCancelDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PurchaseIndentCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));


                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    PurchaseIndentCancelHeader HEader = _PurchaseIndentCancelHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.PurchaseIndentCancelHeaders", "PurchaseIndentCancelHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully"); ;

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Record Reviewed Successfully"); ;
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
        }

        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            if (!Dt.ReportMenuId.HasValue)
                throw new Exception("Report Menu not configured in document types");

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

                var Settings = new PurchaseIndentSettingService(_unitOfWork).GetPurchaseIndentSettingForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.PurchaseIndentCancelHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PurchaseIndentCancelHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

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
                        else
                        {
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
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

        public int PendingToSubmitCount(int id)
        {
            return (_PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PurchaseIndentCancelHeaderService.GetPurchaseIndentCancelPendingToReview(id, User.Identity.Name)).Count();
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
