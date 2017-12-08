using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.SqlClient;
using System.Xml.Linq;
using StockExchangeDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Reports;
using Reports.Controllers;



namespace Jobs.Controllers
{
    [Authorize]
    public class StockExchangeController : System.Web.Mvc.Controller
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

        public StockExchangeController(IStockHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IStockLineService Stock)
        {
            _StockHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            _StockLineService = Stock;
            if (!StockExchangeEvents.Initialized)
            {
                StockExchangeEvents Obj = new StockExchangeEvents();
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
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id,DivisionId,SiteId);
            if(settings !=null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
                ViewBag.SqlProcGatePass = settings.SqlProcGatePass;
            }
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
                return RedirectToAction("CreateForExchange", "StockHeaderSettings", new { id = id }).Warning("Please create Material Exchange settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            p.ProcessId = settings.ProcessId;

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                p.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

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
            StockHeader s = Mapper.Map<StockHeaderViewModel, StockHeader>(svm);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (svm.StockHeaderId <= 0)
                    BeforeSave = StockExchangeDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = StockExchangeDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);
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
                    s.ObjectState = Model.ObjectState.Added;
                    db.StockHeader.Add(s);
                    //_StockHeaderService.Create(s);

                    try
                    {
                        StockExchangeDocEvents.onHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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
                        StockExchangeDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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

                    return RedirectToAction("Modify", "StockExchange", new { Id = s.StockHeaderId }).Success("Data saved successfully");

                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeader temp = _StockHeaderService.Find(s.StockHeaderId);

                    StockHeader ExRec = new StockHeader();
                    ExRec = Mapper.Map<StockHeader>(temp);

                    if (temp.Status != (int)StatusConstants.Drafted)
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
                    db.StockHeader.Add(temp);
                    //_StockHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);


                    try
                    {
                        StockExchangeDocEvents.onHeaderSaveEvent(this, new StockEventArgs(temp.StockHeaderId, EventModeConstants.Edit), ref db);
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
                        StockExchangeDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Edit), ref db);
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

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, s.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

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
                return RedirectToAction("CreateForExchange", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
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


        [HttpGet]
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
                return RedirectToAction("CreateForExchange", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
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
                BeforeSave = StockExchangeDocEvents.beforeHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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
                StockHeader StockHeader = (from p in db.StockHeader
                                           where p.StockHeaderId == vm.id
                                           select p).FirstOrDefault();

                try
                {
                    StockExchangeDocEvents.onHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                StockHeader ExRec = new StockHeader();
                ExRec = Mapper.Map<StockHeader>(StockHeader);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                var StockLine = (from p in db.StockLine
                                 where p.StockHeaderId == vm.id
                                 select p).ToList();


                List<int> StockIdList = new List<int>();
                List<int> StockProcessIdList = new List<int>();

                var StockProcessIds = (from p in db.StockProcess
                                       where p.StockHeaderId == vm.id
                                       select p).ToList();

                var ProdUids = StockLine.Select(m => m.ProductUidId).ToArray();

                var ProdUidRecords = (from p in db.ProductUid
                                      where ProdUids.Contains(p.ProductUIDId)
                                      select p).ToList();

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in StockLine)
                {
                    StockLine ExRecLine = new StockLine();
                    ExRecLine = Mapper.Map<StockLine>(item);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
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

                    if (item.StockProcessId != null)
                    {
                        StockProcessIdList.Add((int)item.StockProcessId);
                    }

                    if (item.ProductUidId != null && item.ProductUidId > 0)
                    {
                        //ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)item.ProductUidId, "Stock Head-" + StockHeader.StockHeaderId.ToString());

                        ProductUid ProductUid = ProdUidRecords.Where(m => m.ProductUIDId == item.ProductUidId).FirstOrDefault();

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

                        //new ProductUidService(_unitOfWork).Update(ProductUid);
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.StockLine.Remove(item);

                    //new StockLineService(_unitOfWork).Delete(item);
                }

                foreach (var item in StockProcessIds)
                {
                    StockProcessIdList.Add((int)item.StockProcessId);
                }

                //foreach (var item in StockIdList)
                //{
                //    new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                //}

                new StockService(_unitOfWork).DeleteStockDBMultiple(StockIdList, ref db, true);

                foreach (var item in StockProcessIdList)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB(item, ref db, true);
                }

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
                    StockExchangeDocEvents.afterHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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
                BeforeSave = StockExchangeDocEvents.beforeHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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

                    StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                    if (String.IsNullOrEmpty(Settings.SqlProcGatePass))
                        throw new Exception("Gate pass Procedure is not Created");

                    SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
                    IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                    if (pd.PersonId != null)
                    {
                        if (pd.GatePassHeaderId == null)
                        {
                            SqlParameter DocDate = new SqlParameter("@DocDate", pd.DocDate);
                            DocDate.SqlDbType = SqlDbType.DateTime;
                            SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
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
                            GPHeader.PersonId = (int)pd.PersonId;
                            GPHeader.SiteId = pd.SiteId;
                            GPHeader.GodownId = (int)pd.GodownId;
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



                    if (!String.IsNullOrEmpty(Settings.SqlProcStockProcessPost))
                    {
                        string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
                        using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                        {
                            sqlConnection.Open();

                            using (SqlCommand cmd = new SqlCommand("" + Settings.SqlProcStockProcessPost))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = sqlConnection;
                                cmd.Parameters.AddWithValue("@StockHeaderId", pd.StockHeaderId);
                                cmd.CommandTimeout = 1000;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }


                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(pd);
                    //_StockHeaderService.Update(pd);

                    try
                    {
                        StockExchangeDocEvents.onHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
                    }

                    try
                    {
                        StockExchangeDocEvents.afterHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                BeforeSave = StockExchangeDocEvents.beforeHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
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
                    StockExchangeDocEvents.onHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
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
                    StockExchangeDocEvents.afterHeaderReviewEvent(this, new StockEventArgs(Id), ref db);
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




        public int PendingToSubmitCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }




















        // // // // StockExchange Line

        public ActionResult _ForExchange(int id, int sid)
        {
            RequisitionFiltersForExchange vm = new RequisitionFiltersForExchange();
            StockHeader Header = new StockHeaderService(_unitOfWork).Find(id);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(Header.DocTypeId);
            vm.StockHeaderId = id;
            vm.PersonId = sid;
            return PartialView("_Filters", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _FilterPost(RequisitionFiltersForExchange vm)
        {
            List<StockExchangeLineViewModel> temp = _StockLineService.GetRequisitionsForExchange(vm).ToList();

            StockExchangeMasterDetailModel svm = new StockExchangeMasterDetailModel();
            svm.StockLineViewModel = temp;
            //Getting Settings           
            var Header = new StockHeaderService(_unitOfWork).Find(vm.StockHeaderId);
            svm.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId));
            return PartialView("_Results", svm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ResultsPost(StockExchangeMasterDetailModel vm)
        {
            int Cnt = 0;
            int pk = 0;

            int IssSerial = _StockLineService.GetMaxSr(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);

            StockHeader Header = new StockHeaderService(_unitOfWork).Find(vm.StockLineViewModel.FirstOrDefault().StockHeaderId);

            StockHeaderSettings Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            bool BeforeSave = true;
            try
            {
                BeforeSave = StockExchangeDocEvents.beforeLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                ModelState.AddModelError("", "Validation failed before save");

            //decimal Qty = vm.StockLineViewModel.Where(m => m.Rate > 0).Sum(m => m.Qty);


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                foreach (var item in vm.StockLineViewModel.Where(m => m.QtyIss > 0))
                {
                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                    if (item.QtyIss > 0)
                    {
                        StockLine line = new StockLine();

                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = Header.StockHeaderId;
                        }
                        else
                        {
                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = Header.StockHeaderId;
                        StockViewModel.DocLineId = line.StockLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.PersonId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.HeaderProcessId = Header.ProcessId;
                        StockViewModel.GodownId = (int)Header.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = item.ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = item.CostCenterId;
                        StockViewModel.Qty_Iss = item.QtyIss;
                        StockViewModel.Qty_Rec = 0;
                        StockViewModel.Rate = item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = item.Specification;
                        StockViewModel.Dimension1Id = item.Dimension1Id;
                        StockViewModel.Dimension2Id = item.Dimension2Id;
                        StockViewModel.Dimension3Id = item.Dimension3Id;
                        StockViewModel.Dimension4Id = item.Dimension4Id;
                        StockViewModel.ProductUidId = item.ProductUidId;
                        StockViewModel.CreatedBy = User.Identity.Name;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        if (Cnt == 0)
                        {
                            Header.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                        line.StockId = StockViewModel.StockId;




                        if (Settings.isPostedInStockProcess ?? false)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessViewModel.StockHeaderId = 0;
                            }
                            StockProcessViewModel.StockProcessId = -Cnt;
                            StockProcessViewModel.DocHeaderId = Header.StockHeaderId;
                            StockProcessViewModel.DocLineId = line.StockLineId;
                            StockProcessViewModel.DocTypeId = Header.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                            StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                            StockProcessViewModel.DocNo = Header.DocNo;
                            StockProcessViewModel.DivisionId = Header.DivisionId;
                            StockProcessViewModel.SiteId = Header.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = Header.PersonId;
                            StockProcessViewModel.ProductId = item.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = Header.GodownId;
                            StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                            StockProcessViewModel.GodownId = (int)Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = item.CostCenterId;
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = item.QtyIss;
                            StockProcessViewModel.Rate = item.Rate;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = item.Specification;
                            StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = item.Dimension4Id;
                            StockProcessViewModel.ProductUidId = item.ProductUidId;
                            StockProcessViewModel.CreatedBy = User.Identity.Name;
                            StockProcessViewModel.CreatedDate = DateTime.Now;
                            StockProcessViewModel.ModifiedBy = User.Identity.Name;
                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                string message = StockProcessPostingError;
                                ModelState.AddModelError("", message);
                                return PartialView("_Results", vm);
                            }

                            line.StockProcessId = StockProcessViewModel.StockProcessId;
                        }


                        line.StockHeaderId = item.StockHeaderId;
                        line.RequisitionLineId = item.RequisitionLineId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.Qty = item.QtyIss;
                        line.CostCenterId = item.CostCenterId;
                        line.DocNature = StockNatureConstants.Issue;
                        line.Rate = item.Rate ?? 0;
                        line.Amount = (line.Qty * line.Rate);
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.StockLineId = pk;
                        line.Sr = IssSerial++;
                        line.ObjectState = Model.ObjectState.Added;
                        db.StockLine.Add(line);
                        //_StockLineService.Create(line);
                        pk++;
                        Cnt = Cnt + 1;
                    }

                }



                foreach (var item in vm.StockLineViewModel.Where(m => m.QtyRec > 0))
                {
                    //if (item.Qty > 0 &&  ((Settings.isMandatoryRate.HasValue && Settings.isMandatoryRate == true )? item.Rate > 0 : 1 == 1))
                    if (item.QtyRec > 0)
                    {
                        StockLine line = new StockLine();

                        StockViewModel StockViewModel = new StockViewModel();

                        if (Cnt == 0)
                        {
                            StockViewModel.StockHeaderId = Header.StockHeaderId;
                        }
                        else
                        {
                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)
                            {
                                StockViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else
                            {
                                StockViewModel.StockHeaderId = -1;
                            }
                        }

                        StockViewModel.StockId = -Cnt;
                        StockViewModel.DocHeaderId = Header.StockHeaderId;
                        StockViewModel.DocLineId = line.StockLineId;
                        StockViewModel.DocTypeId = Header.DocTypeId;
                        StockViewModel.StockHeaderDocDate = Header.DocDate;
                        StockViewModel.StockDocDate = Header.DocDate;
                        StockViewModel.DocNo = Header.DocNo;
                        StockViewModel.DivisionId = Header.DivisionId;
                        StockViewModel.SiteId = Header.SiteId;
                        StockViewModel.CurrencyId = null;
                        StockViewModel.PersonId = Header.PersonId;
                        StockViewModel.ProductId = item.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = Header.GodownId;
                        StockViewModel.HeaderProcessId = Header.ProcessId;
                        StockViewModel.GodownId = (int)Header.GodownId;
                        StockViewModel.Remark = Header.Remark;
                        StockViewModel.Status = Header.Status;
                        StockViewModel.ProcessId = Header.ProcessId;
                        StockViewModel.LotNo = null;
                        StockViewModel.CostCenterId = item.CostCenterId;
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = item.QtyRec;
                        StockViewModel.Rate = item.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = item.Specification;
                        StockViewModel.Dimension1Id = item.Dimension1Id;
                        StockViewModel.Dimension2Id = item.Dimension2Id;
                        StockViewModel.Dimension3Id = item.Dimension3Id;
                        StockViewModel.Dimension4Id = item.Dimension4Id;
                        StockViewModel.ProductUidId = item.ProductUidId;
                        StockViewModel.CreatedBy = User.Identity.Name;
                        StockViewModel.CreatedDate = DateTime.Now;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            string message = StockPostingError;
                            ModelState.AddModelError("", message);
                            return PartialView("_Results", vm);
                        }

                        if (Cnt == 0)
                        {
                            Header.StockHeaderId = StockViewModel.StockHeaderId;
                        }
                        line.StockId = StockViewModel.StockId;




                        if (Settings.isPostedInStockProcess ?? false)
                        {
                            StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();

                            if (Header.StockHeaderId != null && Header.StockHeaderId != 0)//If Transaction Header Table Has Stock Header Id Then It will Save Here.
                            {
                                StockProcessViewModel.StockHeaderId = (int)Header.StockHeaderId;
                            }
                            else if (Cnt > 0)//If function will only post in stock process then after first iteration of loop the stock header id will go -1
                            {
                                StockProcessViewModel.StockHeaderId = -1;
                            }
                            else//If function will only post in stock process then this statement will execute.For Example Job consumption.
                            {
                                StockProcessViewModel.StockHeaderId = 0;
                            }
                            StockProcessViewModel.StockProcessId = -Cnt;
                            StockProcessViewModel.DocHeaderId = Header.StockHeaderId;
                            StockProcessViewModel.DocLineId = line.StockLineId;
                            StockProcessViewModel.DocTypeId = Header.DocTypeId;
                            StockProcessViewModel.StockHeaderDocDate = Header.DocDate;
                            StockProcessViewModel.StockProcessDocDate = Header.DocDate;
                            StockProcessViewModel.DocNo = Header.DocNo;
                            StockProcessViewModel.DivisionId = Header.DivisionId;
                            StockProcessViewModel.SiteId = Header.SiteId;
                            StockProcessViewModel.CurrencyId = null;
                            StockProcessViewModel.PersonId = Header.PersonId;
                            StockProcessViewModel.ProductId = item.ProductId;
                            StockProcessViewModel.HeaderFromGodownId = null;
                            StockProcessViewModel.HeaderGodownId = Header.GodownId;
                            StockProcessViewModel.HeaderProcessId = Header.ProcessId;
                            StockProcessViewModel.GodownId = (int)Header.GodownId;
                            StockProcessViewModel.Remark = Header.Remark;
                            StockProcessViewModel.Status = Header.Status;
                            StockProcessViewModel.ProcessId = Header.ProcessId;
                            StockProcessViewModel.LotNo = null;
                            StockProcessViewModel.CostCenterId = item.CostCenterId;
                            StockProcessViewModel.Qty_Iss = item.QtyRec;
                            StockProcessViewModel.Qty_Rec = 0;
                            StockProcessViewModel.Rate = item.Rate;
                            StockProcessViewModel.ExpiryDate = null;
                            StockProcessViewModel.Specification = item.Specification;
                            StockProcessViewModel.Dimension1Id = item.Dimension1Id;
                            StockProcessViewModel.Dimension2Id = item.Dimension2Id;
                            StockProcessViewModel.Dimension3Id = item.Dimension3Id;
                            StockProcessViewModel.Dimension4Id = item.Dimension4Id;
                            StockProcessViewModel.ProductUidId = item.ProductUidId;
                            StockProcessViewModel.CreatedBy = User.Identity.Name;
                            StockProcessViewModel.CreatedDate = DateTime.Now;
                            StockProcessViewModel.ModifiedBy = User.Identity.Name;
                            StockProcessViewModel.ModifiedDate = DateTime.Now;

                            string StockProcessPostingError = "";
                            StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                            if (StockProcessPostingError != "")
                            {
                                string message = StockProcessPostingError;
                                ModelState.AddModelError("", message);
                                return PartialView("_Results", vm);
                            }

                            line.StockProcessId = StockProcessViewModel.StockProcessId;
                        }


                        line.StockHeaderId = item.StockHeaderId;
                        line.RequisitionLineId = item.RequisitionLineId;
                        line.ProductId = item.ProductId;
                        line.Dimension1Id = item.Dimension1Id;
                        line.Dimension2Id = item.Dimension2Id;
                        line.Dimension3Id = item.Dimension3Id;
                        line.Dimension4Id = item.Dimension4Id;
                        line.Specification = item.Specification;
                        line.Qty = item.QtyRec;
                        line.CostCenterId = item.CostCenterId;
                        line.DocNature = StockNatureConstants.Receive;
                        line.Rate = item.Rate ?? 0;
                        line.Amount = (line.Qty * line.Rate);
                        line.CreatedDate = DateTime.Now;
                        line.ModifiedDate = DateTime.Now;
                        line.CreatedBy = User.Identity.Name;
                        line.ModifiedBy = User.Identity.Name;
                        line.StockLineId = pk;
                        line.Sr = IssSerial++;
                        line.ObjectState = Model.ObjectState.Added;
                        db.StockLine.Add(line);
                        //_StockLineService.Create(line);
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

                Header.ObjectState = Model.ObjectState.Modified;
                db.StockHeader.Add(Header);

                try
                {
                    StockExchangeDocEvents.onLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                //new StockHeaderService(_unitOfWork).Update(Header);
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
                    StockExchangeDocEvents.afterLineSaveBulkEvent(this, new StockEventArgs(vm.StockLineViewModel.FirstOrDefault().StockHeaderId), ref db);
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
        public JsonResult ReceiveIndex(int id)
        {
            var p = _StockLineService.GetStockLineListForIssueReceive(id, false).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public JsonResult IssueIndex(int id)
        {
            var p = _StockLineService.GetStockLineListForIssueReceive(id, true).ToList();
            return Json(p, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRequisitions(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new RequisitionHeaderService(_unitOfWork).GetRequisitionsForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new RequisitionHeaderService(_unitOfWork).GetRequisitionsForExchange(filter, searchTerm).Count();

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


        public JsonResult GetCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var Query=new RequisitionHeaderService(_unitOfWork).GetCostCentersForExchange(filter, searchTerm);

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

        }

        public JsonResult GetProducts(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new RequisitionHeaderService(_unitOfWork).GetProductsForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new RequisitionHeaderService(_unitOfWork).GetProductsForExchange(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetDimension1(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new RequisitionHeaderService(_unitOfWork).GetDimension1ForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new RequisitionHeaderService(_unitOfWork).GetDimension1ForExchange(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetDimension2(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new RequisitionHeaderService(_unitOfWork).GetDimension2ForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new RequisitionHeaderService(_unitOfWork).GetDimension2ForExchange(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        public JsonResult GetDimension3(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new RequisitionHeaderService(_unitOfWork).GetDimension3ForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new RequisitionHeaderService(_unitOfWork).GetDimension3ForExchange(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        public JsonResult GetDimension4(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = new RequisitionHeaderService(_unitOfWork).GetDimension4ForExchange(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new RequisitionHeaderService(_unitOfWork).GetDimension4ForExchange(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

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
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
            {
                var SEttings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
            {
                var SEttings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint_AfterSubmit))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, SEttings.SqlProcDocumentPrint_AfterSubmit);
            }
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Report(int id)
        {
            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).Find(id);

            StockHeaderSettings SEttings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

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
        public ActionResult CreateLine(int id, bool Issue)
        {
            return _Create(id, Issue);
        }

        [HttpGet]
        public ActionResult CreateLineAfter_Submit(int id, bool Issue)
        {
            return _Create(id, Issue);
        }

        public ActionResult _Create(int Id, bool Issue) //Id ==>Sale Order Header Id
        {
            StockHeader H = new StockHeaderService(_unitOfWork).Find(Id);
            StockLineViewModel s = new StockLineViewModel();

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(H.DocTypeId, H.DivisionId, H.SiteId);
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(H.DocTypeId);
            s.PersonId = H.PersonId;
            s.StockHeaderId = H.StockHeaderId;
            s.GodownId = H.GodownId;
            s.Issue = Issue;
            ViewBag.Status = H.Status;

            ViewBag.LineMode = "Create";

            return PartialView("_Create", s);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(StockLineViewModel svm)
        {
            StockHeader temp = new StockHeaderService(_unitOfWork).Find(svm.StockHeaderId);
            StockLine s = Mapper.Map<StockLineViewModel, StockLine>(svm);

            if (svm.StockHeaderSettings != null)
            {
                if (svm.StockHeaderSettings.isMandatoryProcessLine == true && (svm.FromProcessId <= 0 || svm.FromProcessId == null))
                {
                    ModelState.AddModelError("FromProcessId", "The Process field is required");
                }
                if (svm.StockHeaderSettings.isMandatoryRate == true && svm.Rate <= 0)
                {
                    ModelState.AddModelError("Rate", "The Rate field is required");
                }
                if (svm.StockHeaderSettings.isMandatoryLineCostCenter == true && !svm.CostCenterId.HasValue)
                {
                    ModelState.AddModelError("CostCenterId", "The Cost Center field is required");
                }

            }

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (svm.StockLineId <= 0)
                    BeforeSave = StockExchangeDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = StockExchangeDocEvents.beforeLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);
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

            if (svm.ProductId <= 0)
                ModelState.AddModelError("ProductId", "The Product field is required");

            if (svm.StockLineId <= 0)
            {
                ViewBag.LineMode = "Create";
            }
            else
            {
                ViewBag.LineMode = "Edit";
            }

            if (ModelState.IsValid && !EventException)
            {

                if (svm.StockLineId <= 0)
                {
                    StockViewModel StockViewModel = new StockViewModel();
                    StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                    //Posting in Stock
                    StockViewModel.StockHeaderId = temp.StockHeaderId;
                    StockViewModel.DocHeaderId = temp.StockHeaderId;
                    StockViewModel.DocLineId = s.StockLineId;
                    StockViewModel.DocTypeId = temp.DocTypeId;
                    StockViewModel.StockHeaderDocDate = temp.DocDate;
                    StockViewModel.StockDocDate = temp.DocDate;
                    StockViewModel.DocNo = temp.DocNo;
                    StockViewModel.DivisionId = temp.DivisionId;
                    StockViewModel.SiteId = temp.SiteId;
                    StockViewModel.CurrencyId = null;

                    StockViewModel.PersonId = temp.PersonId;
                    StockViewModel.ProductId = s.ProductId;
                    StockViewModel.HeaderFromGodownId = temp.FromGodownId;
                    StockViewModel.HeaderGodownId = temp.GodownId;
                    StockViewModel.GodownId = temp.GodownId ?? 0;
                    StockViewModel.LotNo = s.LotNo;
                    StockViewModel.CostCenterId = (s.CostCenterId);
                    StockViewModel.HeaderProcessId = temp.ProcessId;

                    //if (svm.Issue)
                    //    StockViewModel.ProcessId = s.FromProcessId;
                    //else
                    //    StockViewModel.ProcessId = temp.ProcessId;

                    StockViewModel.ProcessId = s.FromProcessId;
                    if (svm.Issue)
                    {
                        StockViewModel.Qty_Iss = s.Qty;
                        StockViewModel.Qty_Rec = 0;
                    }
                    else
                    {
                        StockViewModel.Qty_Iss = 0;
                        StockViewModel.Qty_Rec = s.Qty;
                    }

                    StockViewModel.Rate = s.Rate;
                    StockViewModel.ExpiryDate = null;
                    StockViewModel.Specification = s.Specification;
                    StockViewModel.Dimension1Id = s.Dimension1Id;
                    StockViewModel.Dimension2Id = s.Dimension2Id;
                    StockViewModel.Dimension3Id = s.Dimension3Id;
                    StockViewModel.Dimension4Id = s.Dimension4Id;
                    StockViewModel.Remark = s.Remark;
                    StockViewModel.ProductUidId = svm.ProductUidId;
                    StockViewModel.Status = temp.Status;
                    StockViewModel.CreatedBy = temp.CreatedBy;
                    StockViewModel.CreatedDate = DateTime.Now;
                    StockViewModel.ModifiedBy = temp.ModifiedBy;
                    StockViewModel.ModifiedDate = DateTime.Now;

                    string StockPostingError = "";
                    StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                    if (StockPostingError != "")
                    {
                        ModelState.AddModelError("", StockPostingError);
                        return PartialView("_Create", svm);
                    }

                    s.StockId = StockViewModel.StockId;

                    if (svm.Issue)
                    {
                        if (svm.StockInId != null)
                        {
                            StockAdj Adj_IssQty = new StockAdj();
                            Adj_IssQty.StockInId = (int)svm.StockInId;
                            Adj_IssQty.StockOutId = (int)s.StockId;
                            Adj_IssQty.DivisionId = temp.DivisionId;
                            Adj_IssQty.SiteId = temp.SiteId;
                            Adj_IssQty.AdjustedQty = s.Qty;
                            Adj_IssQty.ObjectState = Model.ObjectState.Added;
                            db.StockAdj.Add(Adj_IssQty);
                            //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                        }

                        s.StockInId = svm.StockInId;
                    }



                    if (svm.StockHeaderSettings.isPostedInStockProcess)
                    {
                        StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

                        StockProcessViewModel.StockHeaderId = StockHeader.StockHeaderId;
                        StockProcessViewModel.StockHeaderId = temp.StockHeaderId;
                        StockProcessViewModel.DocHeaderId = temp.StockHeaderId;
                        StockProcessViewModel.DocLineId = s.StockLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.PersonId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = temp.FromGodownId;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = s.LotNo;
                        StockProcessViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);

                        if (svm.Issue)
                        {
                            StockProcessViewModel.Qty_Iss = 0;
                            StockProcessViewModel.Qty_Rec = s.Qty;
                        }
                        else
                        {
                            StockProcessViewModel.Qty_Iss = s.Qty;
                            StockProcessViewModel.Qty_Rec = 0;
                        }

                        StockProcessViewModel.Rate = s.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = s.Specification;
                        StockProcessViewModel.Dimension1Id = s.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = s.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = s.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = s.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.CreatedBy = temp.CreatedBy;
                        StockProcessViewModel.CreatedDate = DateTime.Now;
                        StockProcessViewModel.ModifiedBy = temp.ModifiedBy;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }

                        s.StockProcessId = StockProcessViewModel.StockProcessId;
                    }

                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;

                    if (svm.Issue)
                        s.DocNature = StockNatureConstants.Issue;
                    else
                        s.DocNature = StockNatureConstants.Receive;

                    s.ModifiedBy = User.Identity.Name;
                    s.ProductUidId = svm.ProductUidId;
                    s.Sr = _StockLineService.GetMaxSr(s.StockHeaderId);


                    //StockHeader header = new StockHeaderService(_unitOfWork).Find(s.StockHeaderId);
                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedBy = User.Identity.Name;
                        temp.ModifiedDate = DateTime.Now;
                        db.StockHeader.Add(temp);
                    }



                    if (svm.ProductUidId != null && svm.ProductUidId > 0)
                    {

                        ProductUid Produid = new ProductUidService(_unitOfWork).Find(svm.ProductUidId ?? 0);

                        s.ProductUidCurrentGodownId = Produid.CurrenctGodownId;
                        s.ProductUidCurrentProcessId = Produid.CurrenctProcessId;
                        s.ProductUidLastTransactionDocDate = Produid.LastTransactionDocDate;
                        s.ProductUidLastTransactionDocId = Produid.LastTransactionDocId;
                        s.ProductUidLastTransactionDocNo = Produid.LastTransactionDocNo;
                        s.ProductUidLastTransactionDocTypeId = Produid.LastTransactionDocTypeId;
                        s.ProductUidLastTransactionPersonId = Produid.LastTransactionPersonId;
                        s.ProductUidStatus = Produid.Status;

                        Produid.LastTransactionDocId = temp.StockHeaderId;
                        Produid.LastTransactionDocNo = temp.DocNo;
                        Produid.LastTransactionDocTypeId = temp.DocTypeId;
                        Produid.LastTransactionDocDate = temp.DocDate;
                        Produid.LastTransactionPersonId = temp.PersonId;
                        Produid.CurrenctGodownId = null;
                        Produid.Status = ProductUidStatusConstants.Issue;

                        //UpdateProductUIdStat.MapGodownIssue(temp, ref Produid);

                        Produid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(Produid);
                        //new ProductUidService(_unitOfWork).Update(Produid);

                    }

                    s.ObjectState = Model.ObjectState.Added;
                    //_StockLineService.Create(s);
                    db.StockLine.Add(s);

                    try
                    {
                        StockExchangeDocEvents.onLineSaveEvent(this, new StockEventArgs(svm.StockHeaderId, s.StockLineId, EventModeConstants.Add), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderId,
                        DocLineId = s.StockLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = temp.DocNo,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));

                    return RedirectToAction("_Create", new { id = svm.StockHeaderId, Issue = svm.Issue });

                }


                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
                    int status = temp.Status;
                    StockLine templine = _StockLineService.Find(s.StockLineId);

                    StockLine ExRec = new StockLine();
                    ExRec = Mapper.Map<StockLine>(templine);



                    if (templine.StockId != null)
                    {
                        StockViewModel StockViewModel = new StockViewModel();
                        StockViewModel.StockHeaderId = temp.StockHeaderId;
                        StockViewModel.StockId = templine.StockId ?? 0;
                        StockViewModel.DocHeaderId = templine.StockHeaderId;
                        StockViewModel.DocLineId = templine.StockLineId;
                        StockViewModel.DocTypeId = temp.DocTypeId;
                        StockViewModel.StockHeaderDocDate = temp.DocDate;
                        StockViewModel.StockDocDate = temp.DocDate;
                        StockViewModel.DocNo = temp.DocNo;
                        StockViewModel.DivisionId = temp.DivisionId;
                        StockViewModel.SiteId = temp.SiteId;
                        StockViewModel.CurrencyId = null;

                        StockViewModel.PersonId = temp.PersonId;
                        StockViewModel.ProductId = s.ProductId;
                        StockViewModel.HeaderFromGodownId = null;
                        StockViewModel.HeaderGodownId = temp.GodownId;
                        StockViewModel.GodownId = temp.GodownId ?? 0;

                        StockViewModel.LotNo = templine.LotNo;
                        StockViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);


                        StockViewModel.ProcessId = templine.FromProcessId;
                        StockViewModel.HeaderProcessId = temp.ProcessId;
                        StockViewModel.Qty_Iss = s.Qty;
                        StockViewModel.Qty_Rec = 0;


                        StockViewModel.Rate = templine.Rate;
                        StockViewModel.ExpiryDate = null;
                        StockViewModel.Specification = templine.Specification;
                        StockViewModel.Dimension1Id = templine.Dimension1Id;
                        StockViewModel.Dimension2Id = templine.Dimension2Id;
                        StockViewModel.Dimension3Id = templine.Dimension3Id;
                        StockViewModel.Dimension4Id = templine.Dimension4Id;
                        StockViewModel.Remark = s.Remark;
                        StockViewModel.Status = temp.Status;
                        StockViewModel.ProductUidId = svm.ProductUidId;                        
                        StockViewModel.CreatedBy = templine.CreatedBy;
                        StockViewModel.CreatedDate = templine.CreatedDate;
                        StockViewModel.ModifiedBy = User.Identity.Name;
                        StockViewModel.ModifiedDate = DateTime.Now;

                        string StockPostingError = "";
                        StockPostingError = new StockService(_unitOfWork).StockPostDB(ref StockViewModel, ref db);

                        if (StockPostingError != "")
                        {
                            ModelState.AddModelError("", StockPostingError);
                            return PartialView("_Create", svm);
                        }
                    }




                    if (templine.StockProcessId != null)
                    {
                        StockProcessViewModel StockProcessViewModel = new StockProcessViewModel();
                        StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(temp.StockHeaderId);

                        StockProcessViewModel.StockHeaderId = StockHeader.StockHeaderId;
                        StockProcessViewModel.StockProcessId = templine.StockProcessId ?? 0;
                        StockProcessViewModel.DocHeaderId = templine.StockHeaderId;
                        StockProcessViewModel.DocLineId = templine.StockLineId;
                        StockProcessViewModel.DocTypeId = temp.DocTypeId;
                        StockProcessViewModel.StockHeaderDocDate = temp.DocDate;
                        StockProcessViewModel.StockProcessDocDate = temp.DocDate;
                        StockProcessViewModel.DocNo = temp.DocNo;
                        StockProcessViewModel.DivisionId = temp.DivisionId;
                        StockProcessViewModel.SiteId = temp.SiteId;
                        StockProcessViewModel.CurrencyId = null;
                        StockProcessViewModel.HeaderProcessId = temp.ProcessId;
                        StockProcessViewModel.PersonId = temp.PersonId;
                        StockProcessViewModel.ProductId = s.ProductId;
                        StockProcessViewModel.HeaderFromGodownId = null;
                        StockProcessViewModel.HeaderGodownId = temp.GodownId;
                        StockProcessViewModel.GodownId = temp.GodownId ?? 0;
                        StockProcessViewModel.ProcessId = temp.ProcessId;
                        StockProcessViewModel.LotNo = templine.LotNo;
                        StockProcessViewModel.CostCenterId = (s.CostCenterId == null ? temp.CostCenterId : s.CostCenterId);

                        StockProcessViewModel.Qty_Iss = 0;
                        StockProcessViewModel.Qty_Rec = s.Qty;

                        StockProcessViewModel.Rate = templine.Rate;
                        StockProcessViewModel.ExpiryDate = null;
                        StockProcessViewModel.Specification = templine.Specification;
                        StockProcessViewModel.Dimension1Id = templine.Dimension1Id;
                        StockProcessViewModel.Dimension2Id = templine.Dimension2Id;
                        StockProcessViewModel.Dimension3Id = templine.Dimension3Id;
                        StockProcessViewModel.Dimension4Id = templine.Dimension4Id;
                        StockProcessViewModel.Remark = s.Remark;
                        StockProcessViewModel.ProductUidId = svm.ProductUidId;
                        StockProcessViewModel.Status = temp.Status;
                        StockProcessViewModel.CreatedBy = templine.CreatedBy;
                        StockProcessViewModel.CreatedDate = templine.CreatedDate;
                        StockProcessViewModel.ModifiedBy = User.Identity.Name;
                        StockProcessViewModel.ModifiedDate = DateTime.Now;

                        string StockProcessPostingError = "";
                        StockProcessPostingError = new StockProcessService(_unitOfWork).StockProcessPostDB(ref StockProcessViewModel, ref db);

                        if (StockProcessPostingError != "")
                        {
                            ModelState.AddModelError("", StockProcessPostingError);
                            return PartialView("_Create", svm);
                        }
                    }


                    if (svm.Issue)
                    {
                        StockAdj Adj = (from L in db.StockAdj
                                        where L.StockOutId == templine.StockId
                                        select L).FirstOrDefault();

                        if (Adj != null)
                        {
                            Adj.ObjectState = Model.ObjectState.Deleted;
                            db.StockAdj.Remove(Adj);
                            //new StockAdjService(_unitOfWork).Delete(Adj);
                        }

                        if (svm.StockInId != null)
                        {
                            StockAdj Adj_IssQty = new StockAdj();
                            Adj_IssQty.StockInId = (int)svm.StockInId;
                            Adj_IssQty.StockOutId = (int)templine.StockId;
                            Adj_IssQty.DivisionId = temp.DivisionId;
                            Adj_IssQty.SiteId = temp.SiteId;
                            Adj_IssQty.AdjustedQty = svm.Qty;
                            Adj_IssQty.ObjectState = Model.ObjectState.Added;
                            db.StockAdj.Add(Adj_IssQty);
                            //new StockAdjService(_unitOfWork).Create(Adj_IssQty);
                        }
                    }



                    templine.ProductId = s.ProductId;
                    templine.ProductUidId = s.ProductUidId;
                    templine.RequisitionLineId = s.RequisitionLineId;
                    templine.Specification = s.Specification;
                    templine.Dimension1Id = s.Dimension1Id;
                    templine.Dimension2Id = s.Dimension2Id;
                    templine.Dimension3Id = s.Dimension3Id;
                    templine.Dimension4Id = s.Dimension4Id;
                    templine.CostCenterId = s.CostCenterId;
                    templine.DocNature = StockNatureConstants.Issue;
                    templine.Rate = s.Rate;
                    templine.Amount = s.Amount;
                    templine.LotNo = s.LotNo;
                    templine.FromProcessId = s.FromProcessId;
                    templine.Remark = s.Remark;
                    templine.Qty = s.Qty;
                    templine.Remark = s.Remark;

                    templine.ModifiedDate = DateTime.Now;
                    templine.ModifiedBy = User.Identity.Name;
                    //_StockLineService.Update(templine);
                    templine.ObjectState = Model.ObjectState.Modified;
                    db.StockLine.Add(templine);

                    if (templine.RequisitionLineId.HasValue)
                        new RequisitionLineStatusService(_unitOfWork).UpdateRequisitionQtyOnIssue(templine.RequisitionLineId.Value, templine.StockLineId, temp.DocDate, templine.Qty, ref db, true);

                    if (temp.Status != (int)StatusConstants.Drafted)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                        temp.ModifiedDate = DateTime.Now;
                        temp.ModifiedBy = User.Identity.Name;
                    }
                    //new StockHeaderService(_unitOfWork).Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(temp);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = templine,
                    });


                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        StockExchangeDocEvents.onLineSaveEvent(this, new StockEventArgs(temp.StockHeaderId, templine.StockLineId, EventModeConstants.Edit), ref db);
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
                        return PartialView("_Create", svm);
                    }

                    //Saving the Activity Log

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = templine.StockHeaderId,
                        DocLineId = templine.StockLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = temp.DocNo,
                        xEModifications = Modifications,
                        DocDate = temp.DocDate,
                        DocStatus = temp.Status,
                    }));                   
                    //End of Saving the Activity Log

                    return Json(new { success = true });
                }

            }
            return PartialView("_Create", svm);
        }


        public JsonResult GetLineCostCenters(string searchTerm, int pageSize, int pageNum, int filter)//filter:PersonId
        {
            var temp = _StockLineService.GetCostCentersForLine(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _StockLineService.GetCostCentersForLine(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetCustomProducts(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {

            var temp = _StockLineService.GetCustomProducts(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = _StockLineService.GetCustomProducts(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public JsonResult GetProductDetailJson(int ProductId, int StockId)
        {
            ProductViewModel product = new ProductService(_unitOfWork).GetProduct(ProductId);

            return Json(new { ProductId = product.ProductId, StandardCost = product.StandardCost, UnitId = product.UnitId, UnitName = product.UnitName, Specification = product.ProductSpecification });
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



        public ActionResult DeleteGatePass(int Id)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (Id > 0)
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                try
                {

                    var pd = db.StockHeader.Find(Id);

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

                    var GatePass = db.GatePassHeader.Find(pd.GatePassHeaderId);

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
                        pd.GatePassHeaderId = null;
                        pd.Status = (int)StatusConstants.Modified;
                        pd.ModifiedBy = User.Identity.Name;
                        pd.ModifiedDate = DateTime.Now;
                        pd.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(pd);

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

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Gate pass Deleted successfully");

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetExcessStock(int ProductId, int? Dim1, int? Dim2, int? ProcId, string Lot, int MaterialIssueId, string ProcName)
        {
            return Json(_StockLineService.GetExcessStock(ProductId, Dim1, Dim2, ProcId, Lot, MaterialIssueId, ProcName), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            StockHeaderViewModel vm = new StockHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, vm.DivisionId, vm.SiteId);

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

        public JsonResult GetProductPrevProcess(int ProductId, int GodownId, int DocTypeId)
        {
            ProductPrevProcess ProductPrevProcess = new ProductService(_unitOfWork).FGetProductPrevProcess(ProductId, GodownId, DocTypeId);
            List<ProductPrevProcess> ProductPrevProcessJson = new List<ProductPrevProcess>();

            if (ProductPrevProcess != null)
            {
                string ProcessName = new ProcessService(_unitOfWork).Find((int)ProductPrevProcess.ProcessId).ProcessName;
                ProductPrevProcessJson.Add(new ProductPrevProcess()
                {
                    ProcessId = ProductPrevProcess.ProcessId,
                    ProcessName = ProcessName
                });
                return Json(ProductPrevProcessJson);
            }
            else
            {
                return null;
            }

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

        public JsonResult GetStockInDetailJson(int StockInId)
        {
            var temp = (from p in db.ViewStockInBalance
                        join S in db.Stock on p.StockInId equals S.StockId into StockTable
                        from StockTab in StockTable.DefaultIfEmpty()
                        join pt in db.Product on p.ProductId equals pt.ProductId into ProductTable
                        from ProductTab in ProductTable.DefaultIfEmpty()
                        join D1 in db.Dimension1 on p.Dimension1Id equals D1.Dimension1Id into Dimension1Table
                        from Dimension1Tab in Dimension1Table.DefaultIfEmpty()
                        join D2 in db.Dimension2 on p.Dimension2Id equals D2.Dimension2Id into Dimension2Table
                        from Dimension2Tab in Dimension2Table.DefaultIfEmpty()
                        join D3 in db.Dimension3 on p.Dimension3Id equals D3.Dimension3Id into Dimension3Table
                        from Dimension3Tab in Dimension3Table.DefaultIfEmpty()
                        join D4 in db.Dimension4 on p.Dimension4Id equals D4.Dimension4Id into Dimension4Table
                        from Dimension4Tab in Dimension4Table.DefaultIfEmpty()
                        where p.StockInId == StockInId
                        select new
                        {
                            ProductId = p.ProductId,
                            ProductName = ProductTab.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension1Name = Dimension1Tab.Dimension1Name,
                            Dimension2Id = p.Dimension2Id,
                            Dimension2Name = Dimension2Tab.Dimension2Name,
                            Dimension3Id = p.Dimension3Id,
                            Dimension3Name = Dimension3Tab.Dimension3Name,
                            Dimension4Id = p.Dimension4Id,
                            Dimension4Name = Dimension4Tab.Dimension4Name,
                            BalanceQty = p.BalanceQty,
                            LotNo = p.LotNo,
                            ProcessId = StockTab.ProcessId,
                            ProcessName = StockTab.Process.ProcessName
                        }).FirstOrDefault();

            if (temp != null)
            {
                return Json(temp);
            }
            else
            {
                return null;
            }
        }

        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";
            int Issue = (_StockLineService.GetStockLineListForIssueReceive(id, true)).Count();
            int Receive = (_StockLineService.GetStockLineListForIssueReceive(id, false)).Count();
            if (Issue == 0)
            {
                Msg = "Add Record For Issue Product. <br />";
            }
            
            if(Receive==0)
            {
                Msg += "Add Record for Receive Product";
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
