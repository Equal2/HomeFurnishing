using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Model.ViewModel;
using System.Xml.Linq;
using RateConversionDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;



namespace Jobs.Controllers
{
    [Authorize]
    public class RateConversionHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IStockHeaderService _StockHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public RateConversionHeaderController(IStockHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!RateConversionEvents.Initialized)
            {
                RateConversionEvents Obj = new RateConversionEvents();
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
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PersonName = _StockHeaderService.GetPersonName(id);
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
            int Cid = new DocumentTypeService(_unitOfWork).Find(id).DocumentCategoryId;
            ViewBag.DocTypeList = new DocumentTypeService(_unitOfWork).FindByDocumentCategory(Cid).ToList();
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
                return RedirectToAction("CreateForRateConversion", "StockHeaderSettings", new { id = id }).Warning("Please create Rate Conversion settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            p.ProcessId = settings.ProcessId;

            PrepareViewBag(id);

            p.DocTypeId = id;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".StockHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            ViewBag.Mode = "Add";
            return View(p);
        }

        [HttpGet]
        public ActionResult Print(int id)
        {
            StockHeader s = _StockHeaderService.Find(id);
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);
            String query = settings.SqlProcDocumentPrint;
            if (string.IsNullOrEmpty(settings.SqlProcDocumentPrint))
                throw new Exception("Document Print Not Configured");

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);

        }


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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(StockHeaderViewModel svm)
        {

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (svm.StockHeaderId <= 0)
                    BeforeSave = RateConversionDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = RateConversionDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);
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
                        RateConversionDocEvents.onHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", svm);
                    }

                    try
                    {
                        RateConversionDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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

                    return RedirectToAction("Modify", "RateConversionHeader", new { Id = s.StockHeaderId }).Success("Data saved successfully");

                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeader temp = _StockHeaderService.Find(s.StockHeaderId);


                    StockHeader ExRec = new StockHeader();
                    ExRec = Mapper.Map<StockHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


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
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_StockHeaderService.Update(temp);
                    db.StockHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        RateConversionDocEvents.onHeaderSaveEvent(this, new StockEventArgs(temp.StockHeaderId, EventModeConstants.Edit), ref db);
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
                        PrepareViewBag(svm.DocTypeId);
                        ViewBag.id = svm.DocTypeId;
                        return View("Create", svm);
                    }

                    try
                    {
                        RateConversionDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Edit), ref db);
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
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            StockHeaderViewModel s = _StockHeaderService.GetStockHeader(id);

            if (s == null)
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

            //Job Order Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForRateConversion", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Rate conversion settings");
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

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForRateConversion", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Rate conversion settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
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
                BeforeSave = RateConversionDocEvents.beforeHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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
                //StockHeader StockHeader = _StockHeaderService.Find(vm.id);
                StockHeader StockHeader = (from p in db.StockHeader
                                           where p.StockHeaderId == vm.id
                                           select p).FirstOrDefault();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<StockHeader>(StockHeader),
                });


                //Then find all the Purchase Order Header Line associated with the above ProductType.
                //var StockLine = new StockLineService(_unitOfWork).GetStockLineforDelete(vm.id);
                var StockLine = (from p in db.StockLine
                                 where p.StockHeaderId == vm.id
                                 select p).ToList();

                var CostCenterAmounts = (from p in StockLine
                                         where p.CostCenterId != null
                                         group p by p.CostCenterId into g
                                         select g).ToList();

                List<int> StockProcessIdList = new List<int>();

                try
                {
                    RateConversionDocEvents.onHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in StockLine)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<StockLine>(item),
                    });


                    if (item.StockProcessId != null)
                    {
                        StockProcessIdList.Add((int)item.StockProcessId);
                    }


                    new StockLineService(_unitOfWork).Delete(item);
                }


                foreach (var item in StockProcessIdList)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB(item, ref db, true);
                }

                var ExisitingLedgers = (from p in db.Ledger
                                        where p.LedgerHeaderId == StockHeader.LedgerHeaderId
                                        select p).ToList();

                foreach (var item in ExisitingLedgers)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.Ledger.Remove(item);
                    //new LedgerService(_unitOfWork).Delete(item);
                }


                LedgerHeader LH = (from p in db.LedgerHeader
                                   where p.LedgerHeaderId == StockHeader.LedgerHeaderId
                                   select p).FirstOrDefault();

                if (LH != null)
                {
                    LH.ObjectState = Model.ObjectState.Deleted;
                    db.LedgerHeader.Remove(LH);
                }

                // Now delete the Purhcase Order Header
                //new StockHeaderService(_unitOfWork).Delete(StockHeader);
                StockHeader.ObjectState = Model.ObjectState.Deleted;
                db.StockHeader.Remove(StockHeader);



                //CostCenterStatus Modifications
                var CostCenterIds = CostCenterAmounts.Select(m => m.Key).ToArray();

                var CostCenterStatus = (from p in db.CostCenterStatus
                                        where CostCenterIds.Contains(p.CostCenterId)
                                        select p).ToList();


                foreach (var item in CostCenterStatus)
                {

                    var CCAmounts = CostCenterAmounts.Where(m => m.Key == item.CostCenterId).FirstOrDefault();

                    if (CostCenterAmounts != null)
                    {
                        if (CCAmounts.Sum(m => m.Amount) > 0)
                            item.AmountDr = (item.AmountDr ?? 0) - CCAmounts.Sum(m => m.Amount);
                        else if (CCAmounts.Sum(m => m.Amount) < 0)
                            item.AmountDr = (item.AmountDr ?? 0) - CCAmounts.Sum(m => m.Amount);

                        item.ObjectState = Model.ObjectState.Modified;
                        db.CostCenterStatus.Add(item);

                    }

                }


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
                    RateConversionDocEvents.afterHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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
            #region DocTypeTimeLineValidation

            StockHeader s = db.StockHeader.Find(id);

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
                BeforeSave = RateConversionDocEvents.beforeHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            StockHeader StokHeader = new StockHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == StokHeader.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    int ActivityType;


                    StokHeader.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    StokHeader.ReviewBy = null;

                    var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(StokHeader.DocTypeId, StokHeader.DivisionId, StokHeader.SiteId);

                    if (Settings.isPostedInLedger == true)
                    {

                        #region LedgerBegin
                        ////LedgerPosting::::::::::::

                        //string Nature = new DocumentTypeService(_unitOfWork).Find(pd.DocTypeId).Nature;
                        int LedgerAccountId = new LedgerAccountService(_unitOfWork).GetLedgerAccountByPersondId(StokHeader.PersonId.Value).LedgerAccountId;
                        LedgerHeader Header = new LedgerHeader();

                        if (StokHeader.LedgerHeaderId == null)
                        {

                            Header.CreatedBy = User.Identity.Name;
                            Header.CreatedDate = DateTime.Now;
                            Header.DivisionId = StokHeader.DivisionId;
                            Header.SiteId = StokHeader.SiteId;
                            Header.DivisionId = StokHeader.DivisionId;
                            Header.DocDate = StokHeader.DocDate;
                            Header.DocHeaderId = StokHeader.StockHeaderId;
                            Header.DocNo = StokHeader.DocNo;
                            Header.DocTypeId = StokHeader.DocTypeId;
                            Header.LedgerAccountId = LedgerAccountId;
                            Header.ProcessId = Settings.ProcessId;
                            Header.ModifiedBy = User.Identity.Name;
                            Header.ModifiedDate = DateTime.Now;
                            Header.Remark = StokHeader.Remark;
                            Header.Status = StokHeader.Status;
                            Header.ObjectState = Model.ObjectState.Added;
                            db.LedgerHeader.Add(Header);
                            //new LedgerHeaderService(_unitOfWork).Create(Header);
                            StokHeader.LedgerHeaderId = Header.LedgerHeaderId;

                        }
                        else
                            Header = new LedgerHeaderService(_unitOfWork).Find(StokHeader.LedgerHeaderId.Value);



                        //Deleting ExistingLedgers and LedgerAdjs
                        var ExisitingLedgers = (from p in db.Ledger
                                                where p.LedgerHeaderId == StokHeader.LedgerHeaderId
                                                select p).ToList();

                        foreach (var item in ExisitingLedgers)
                        {
                            item.ObjectState = Model.ObjectState.Deleted;
                            db.Ledger.Remove(item);
                            //new LedgerService(_unitOfWork).Delete(item);
                        }





                        var StockLine = (from p in db.StockLine
                                         where p.StockHeaderId == Id
                                         select p).ToList();

                        var Group = (from p in StockLine
                                     group p by p.CostCenterId
                                         into g
                                         select g).ToList();

                        if (StockLine.Count() > 0)
                        {

                            foreach (var item in Group)
                            {

                                decimal Amount = item.Sum(m => m.Amount);

                                if (Amount > 0)
                                {

                                    //Postingcontra LedgerRecord;;
                                    Ledger Ledger = new Ledger();

                                    Ledger.AmtDr = Amount;

                                    Ledger.LedgerHeaderId = Header.LedgerHeaderId;
                                    Ledger.LedgerAccountId = LedgerAccountId;
                                    Ledger.CostCenterId = item.Key;
                                    Ledger.ContraLedgerAccountId = Settings.AdjLedgerAccountId;
                                    Ledger.ObjectState = Model.ObjectState.Added;
                                    db.Ledger.Add(Ledger);
                                    //new LedgerService(_unitOfWork).Create(Ledger);



                                    //Postingcontra LedgerRecord;;
                                    Ledger ContraLedger = new Ledger();

                                    ContraLedger.AmtCr = Amount;

                                    ContraLedger.LedgerHeaderId = Header.LedgerHeaderId;
                                    ContraLedger.LedgerAccountId = Settings.AdjLedgerAccountId.Value;
                                    ContraLedger.ContraLedgerAccountId = LedgerAccountId;
                                    ContraLedger.ObjectState = Model.ObjectState.Added;
                                    //new LedgerService(_unitOfWork).Create(ContraLedger);
                                    db.Ledger.Add(ContraLedger);

                                }

                                else
                                {

                                    //Postingcontra LedgerRecord;;
                                    Ledger Ledger = new Ledger();

                                    Ledger.AmtDr = Math.Abs(Amount);

                                    Ledger.LedgerHeaderId = Header.LedgerHeaderId;
                                    Ledger.LedgerAccountId = Settings.AdjLedgerAccountId.Value;
                                    Ledger.ContraLedgerAccountId = LedgerAccountId;
                                    Ledger.ObjectState = Model.ObjectState.Added;
                                    //new LedgerService(_unitOfWork).Create(Ledger);
                                    db.Ledger.Add(Ledger);


                                    //Postingcontra LedgerRecord;;
                                    Ledger ContraLedger = new Ledger();

                                    ContraLedger.AmtCr = Math.Abs(Amount);

                                    ContraLedger.LedgerHeaderId = Header.LedgerHeaderId;
                                    ContraLedger.LedgerAccountId = LedgerAccountId;
                                    ContraLedger.CostCenterId = item.Key;
                                    ContraLedger.ContraLedgerAccountId = Settings.AdjLedgerAccountId.Value;
                                    ContraLedger.ObjectState = Model.ObjectState.Added;
                                    //new LedgerService(_unitOfWork).Create(ContraLedger);
                                    db.Ledger.Add(ContraLedger);


                                }



                            }


                        }



                        #endregion

                    }

                    //_StockHeaderService.Update(StokHeader);
                    StokHeader.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(StokHeader);

                    try
                    {
                        RateConversionDocEvents.onHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                        return RedirectToAction("Index", new { id = StokHeader.DocTypeId });
                    }


                    try
                    {
                        RateConversionDocEvents.afterHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = StokHeader.DocTypeId,
                        DocId = StokHeader.StockHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = StokHeader.DocNo,
                        DocDate = StokHeader.DocDate,
                        DocStatus = StokHeader.Status,
                    }));

                    string ReturnUrl = System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + "/" + "RateConversionHeader" + "/" + "Index" + "/" + StokHeader.DocTypeId + "?IndexType=" + IndexType;

                    #region "For Calling Customise Menu"
                    int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

                    var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(StokHeader.DocTypeId, DivisionId, SiteId);

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


                    return RedirectToAction("Index", new { id = StokHeader.DocTypeId, IndexType = IndexType }).Success("Record Submitted successfully");
                }
                else
                    return RedirectToAction("Index", new { id = StokHeader.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + StokHeader.ModifiedBy + " only.");

            }

            return View();
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
                BeforeSave = RateConversionDocEvents.beforeHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
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
                //_StockHeaderService.Update(pd);

                try
                {
                    RateConversionDocEvents.onHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                //_unitOfWork.Save();
                db.SaveChanges();

                try
                {
                    RateConversionDocEvents.afterHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
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

                //SendEmail_POReviewd(Id);
                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Reviewed Successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in Reviewing.");
        }


        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(DocTypeId, DivisionId, SiteId);

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

        public int PendingToSubmitCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToReview(id, User.Identity.Name)).Count();
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
