using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.SqlClient;
using System.Xml.Linq;
using DocumentEvents;
using CustomEventArgs;
using StockIssueDocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;
using Jobs.Controllers;



namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class StockIssueHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();

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

        public StockIssueHeaderController(IStockHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!StockIssueEvents.Initialized)
            {
                StockIssueEvents Obj = new StockIssueEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /StockHeader

        [HttpGet]
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
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id,DivisionId, SiteId);
            if(settings !=null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
                ViewBag.SqlProcGatePass = settings.SqlProcGatePass;
            }

        }

        // GET: /StockHeader/Create
        [HttpGet]
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
                return RedirectToAction("Create", "StockHeaderSettings", new { id = id }).Warning("Please create Material Issue settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            if (settings.isVisibleProcessHeader ?? false == false)
            {
                p.ProcessId = settings.ProcessId;
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
                    BeforeSave = StockIssueDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref context);
                else
                    BeforeSave = StockIssueDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref context);
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
                    //_StockHeaderService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;
                    context.StockHeader.Add(s);

                    try
                    {
                        StockIssueDocEvents.onHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref context);
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

                        context.SaveChanges();
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
                        StockIssueDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref context);
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

                    return RedirectToAction("Modify", "StockIssueHeader", new { Id = s.StockHeaderId }).Success("Data saved successfully");

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
                    //_StockHeaderService.Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    context.StockHeader.Add(temp);

                    //if (GodownChanged)
                    //    new StockService(_unitOfWork).UpdateStockGodownId(temp.StockHeaderId, temp.GodownId, context);


                    IEnumerable<Stock> stocklist = new StockService(_unitOfWork).GetStockForStockHeaderId(temp.StockHeaderId);

                    foreach (Stock item in stocklist)
                    {
                        Stock Stock = new StockService(_unitOfWork).Find(item.StockId);


                        if (GodownChanged == true)
                        {
                            Stock.GodownId = (int)temp.GodownId;
                        }

                        if (DocDateChanged == true)
                        {
                            Stock.DocDate = temp.DocDate;
                        }

                        Stock.ObjectState = Model.ObjectState.Modified;
                        context.Stock.Add(Stock);


                        if (item.ProductUidId != null && item.ProductUidId != 0)
                        {
                            ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)item.ProductUidId);

                            if (DocDateChanged == true)
                            {
                                ProductUid.LastTransactionDocDate = temp.DocDate;
                            }

                            ProductUid.ObjectState = Model.ObjectState.Modified;
                            context.ProductUid.Add(ProductUid);
                        }
                    }


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        StockIssueDocEvents.onHeaderSaveEvent(this, new StockEventArgs(temp.StockHeaderId, EventModeConstants.Edit), ref context);
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

                        context.SaveChanges();
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
                        StockIssueDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Edit), ref context);
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
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
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

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
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
                return RedirectToAction("Create", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
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
        [HttpGet]
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
                return RedirectToAction("Create", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
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
        [HttpGet]
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
                BeforeSave = StockIssueDocEvents.beforeHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref context);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            StockHeader StockHeader = (from p in context.StockHeader
                                       where p.StockHeaderId == vm.id
                                       select p).FirstOrDefault();

            var GatePassHeader = (from p in context.GatePassHeader
                                  where p.GatePassHeaderId == StockHeader.GatePassHeaderId
                                  select p).FirstOrDefault();

            if (GatePassHeader != null && GatePassHeader.Status == (int)StatusConstants.Submitted)
            {
                BeforeSave = false;
                TempData["CSEXC"] += "Cannot delete record because gatepass is submitted.";
            }


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                try
                {
                    StockIssueDocEvents.onHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref context);
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
                    ExObj = ExRec,
                    Obj = Rec,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                //var StockLine = new StockLineService(_unitOfWork).GetStockLineforDelete(vm.id);

                var StockLine = (from p in context.StockLine
                                 where p.StockHeaderId == vm.id
                                 select p).ToList();

                var ProductUids = StockLine.Select(m => m.ProductUidId).ToArray();

                var ProdUidRecords = (from p in context.ProductUid
                                      where ProductUids.Contains(p.ProductUIDId)
                                      select p).ToList();


                List<int> StockIdList = new List<int>();
                List<int> StockProcessIdList = new List<int>();

                var StockProcessIds = (from p in context.StockProcess
                                       where p.StockHeaderId == vm.id
                                       select p).ToList();

                new RequisitionLineStatusService(_unitOfWork).DeleteRequisitionQtyOnIssueMultiple(StockHeader.StockHeaderId, ref context);

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in StockLine)
                {
                    StockLine ExRecLine = new StockLine();
                    ExRecLine = Mapper.Map<StockLine>(item);
                    StockLine RecLine = new StockLine();

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
                        Obj = RecLine,
                    });

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    if (item.StockProcessId != null)
                    {
                        StockProcessIdList.Add((int)item.StockProcessId);
                    }

                    if (item.ProductUidId != null && item.ProductUidId > 0)
                    {
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
                        context.ProductUid.Add(ProductUid);
                    }

                    var JobOrderBomMaterialIssueList = (from L in context.JobOrderBomMaterialIssue where L.StockLineId == item.StockLineId select L).ToList();
                    foreach (var JobOrderBomMaterialIssueItem in JobOrderBomMaterialIssueList)
                    {
                        JobOrderBomMaterialIssueItem.ObjectState = Model.ObjectState.Deleted;
                        context.JobOrderBomMaterialIssue.Remove(JobOrderBomMaterialIssueItem);
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    context.StockLine.Remove(item);
                }

                foreach (var item in StockProcessIds)
                {
                    StockProcessIdList.Add((int)item.StockProcessId);
                }

                foreach (var item in StockIdList)
                {
                    StockAdj Adj = (from L in context.StockAdj
                                    where L.StockOutId == item
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        Adj.ObjectState = Model.ObjectState.Deleted;
                        context.StockAdj.Remove(Adj);
                    }

                    new StockService(_unitOfWork).DeleteStockDB(item, ref context, true);
                }

                foreach (var item in StockProcessIdList)
                {
                    new StockProcessService(_unitOfWork).DeleteStockProcessDB(item, ref context, true);
                }

                var GatePassHeaderId = StockHeader.GatePassHeaderId;

                StockHeader.ObjectState = Model.ObjectState.Deleted;
                context.StockHeader.Remove(StockHeader);

                if (GatePassHeaderId.HasValue)
                {

                    var GatePassLines = (from p in context.GatePassLine
                                         where p.GatePassHeaderId == GatePassHeaderId
                                         select p).ToList();

                    foreach (var item in GatePassLines)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        context.GatePassLine.Remove(item);
                    }

                    GatePassHeader.ObjectState = Model.ObjectState.Deleted;

                    context.GatePassHeader.Remove(GatePassHeader);


                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                //Commit the DB
                try
                {
                    if (EventException)
                        throw new Exception();
                    context.SaveChanges();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    return PartialView("_Reason", vm);
                }

                try
                {
                    StockIssueDocEvents.afterHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref context);
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

            StockHeader s = context.StockHeader.Find(id);

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
                BeforeSave = StockIssueDocEvents.beforeHeaderSubmitEvent(this, new StockEventArgs(Id), ref context);
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

                    if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                    {

                        if (!String.IsNullOrEmpty(Settings.SqlProcGatePass))
                        {

                            SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
                            IEnumerable<GatePassGeneratedViewModel> GatePasses = context.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                            if (pd.PersonId != null)
                            {
                                if (pd.GatePassHeaderId == null)
                                {
                                    int g= new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                                    SqlParameter DocDate = new SqlParameter("@DocDate", pd.DocDate);
                                    DocDate.SqlDbType = SqlDbType.DateTime;
                                    SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
                                    SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId);
                                    GatePassHeader GPHeader = new GatePassHeader();
                                    GPHeader.CreatedBy = User.Identity.Name;
                                    GPHeader.CreatedDate = DateTime.Now;
                                    GPHeader.DivisionId = pd.DivisionId;
                                    GPHeader.DocDate = pd.DocDate;
                                    GPHeader.DocNo = context.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                    GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                                    GPHeader.ModifiedBy = User.Identity.Name;
                                    GPHeader.ModifiedDate = DateTime.Now;
                                    GPHeader.Remark = pd.Remark;
                                    GPHeader.PersonId = (int)pd.PersonId;
                                    GPHeader.SiteId = pd.SiteId;
                                    GPHeader.GodownId = (int)pd.GodownId;
                                    GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                                    GPHeader.ReferenceDocId = pd.StockHeaderId;
                                    GPHeader.ReferenceDocNo = pd.DocNo;
                                    GPHeader.ObjectState = Model.ObjectState.Added;
                                    context.GatePassHeader.Add(GPHeader);

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

                                        //new GatePassLineService(_unitOfWork).Create(Gline);
                                        Gline.ObjectState = Model.ObjectState.Added;
                                        context.GatePassLine.Add(Gline);
                                    }

                                    pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                                }
                                else
                                {
                                    //List<GatePassLine> LineList = new GatePassLineService(_unitOfWork).GetGatePassLineList(pd.GatePassHeaderId ?? 0).ToList();

                                    var LineList = (from p in context.GatePassLine
                                                    where p.GatePassHeaderId == pd.GatePassHeaderId
                                                    select p).ToList();

                                    foreach (var ittem in LineList)
                                    {
                                        ittem.ObjectState = Model.ObjectState.Deleted;
                                        context.GatePassLine.Remove(ittem);
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
                                        context.GatePassLine.Add(Gline);

                                        //new GatePassLineService(_unitOfWork).Create(Gline);
                                    }
                                    pd.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                }
                            }
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

                    context.StockHeader.Add(pd);
                    //_StockHeaderService.Update(pd);

                    //_unitOfWork.Save();

                    try
                    {
                        StockIssueDocEvents.onHeaderSubmitEvent(this, new StockEventArgs(Id), ref context);
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

                        context.SaveChanges();
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
                        StockIssueDocEvents.afterHeaderSubmitEvent(this, new StockEventArgs(Id), ref context);
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


        [HttpGet]
        public ActionResult Print(int id)
        {
            StockHeader s = _StockHeaderService.Find(id);
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);
            String query = settings.SqlProcDocumentPrint;
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
                BeforeSave = StockIssueDocEvents.beforeHeaderReviewEvent(this, new StockEventArgs(Id), ref context);
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
                context.StockHeader.Add(pd);

                try
                {
                    StockIssueDocEvents.onHeaderReviewEvent(this, new StockEventArgs(Id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                context.SaveChanges();

                try
                {
                    StockIssueDocEvents.afterHeaderReviewEvent(this, new StockEventArgs(Id), ref context);
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
                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in Reviewing.");
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

        public int PendingToSubmitCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_StockHeaderService.GetStockHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.StockHeaders", "StockHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.StockHeaders", "StockHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }



        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;

                var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string StockHeaderIds = "";

                try
                {
                    if (!string.IsNullOrEmpty(Settings.SqlProcGatePass))
                        foreach (var item in Ids.Split(',').Select(Int32.Parse))
                        {
                            TimePlanValidation = true;

                            var pd = context.StockHeader.Find(item);

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

                            if (!pd.GatePassHeaderId.HasValue)
                            {
                                if ((TimePlanValidation || Continue))
                                {
                                    if ((pd.Status == (int)StatusConstants.Submitted) && !pd.GatePassHeaderId.HasValue)
                                    {

                                        SqlParameter SqlParameterUserId = new SqlParameter("@Id", item);
                                        IEnumerable<GatePassGeneratedViewModel> GatePasses = context.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                        if (pd.PersonId != null)
                                        {
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
                                                GPHeader.DocNo = context.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                                GPHeader.DocTypeId = GatePassDocTypeID;
                                                GPHeader.ModifiedBy = User.Identity.Name;
                                                GPHeader.ModifiedDate = DateTime.Now;
                                                GPHeader.Remark = pd.Remark;
                                                GPHeader.PersonId = (int)pd.PersonId;
                                                GPHeader.SiteId = pd.SiteId;
                                                GPHeader.GodownId = (int)pd.GodownId;
                                                GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                                                GPHeader.ReferenceDocId = pd.StockHeaderId;
                                                GPHeader.ReferenceDocNo = pd.DocNo;
                                                GPHeader.GatePassHeaderId = PK++;
                                                GPHeader.ObjectState = Model.ObjectState.Added;
                                                context.GatePassHeader.Add(GPHeader);

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
                                                    context.GatePassLine.Add(Gline);
                                                }

                                                pd.GatePassHeaderId = GPHeader.GatePassHeaderId;


                                                pd.ObjectState = Model.ObjectState.Modified;
                                                context.StockHeader.Add(pd);

                                                StockHeaderIds += pd.StockHeaderId + ", ";
                                            }
                                            //else
                                            //{
                                            //    //List<GatePassLine> LineList = new GatePassLineService(_unitOfWork).GetGatePassLineList(pd.GatePassHeaderId ?? 0).ToList();

                                            //    var LineList = (from p in context.GatePassLine
                                            //                    where p.GatePassHeaderId == pd.GatePassHeaderId
                                            //                    select p).ToList();

                                            //    foreach (var ittem in LineList)
                                            //    {
                                            //        ittem.ObjectState = Model.ObjectState.Deleted;
                                            //        context.GatePassLine.Remove(ittem);
                                            //        //new GatePassLineService(_unitOfWork).Delete(ittem);
                                            //    }

                                            //    GatePassHeader GPHeader = new GatePassHeaderService(_unitOfWork).Find(pd.GatePassHeaderId ?? 0);

                                            //    foreach (GatePassGeneratedViewModel GPLine in GatePasses)
                                            //    {
                                            //        GatePassLine Gline = new GatePassLine();
                                            //        Gline.CreatedBy = User.Identity.Name;
                                            //        Gline.CreatedDate = DateTime.Now;
                                            //        Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                            //        Gline.ModifiedBy = User.Identity.Name;
                                            //        Gline.ModifiedDate = DateTime.Now;
                                            //        Gline.Product = GPLine.ProductName;
                                            //        Gline.Qty = GPLine.Qty;
                                            //        Gline.Specification = GPLine.Specification;
                                            //        Gline.UnitId = GPLine.UnitId;
                                            //        Gline.ObjectState = Model.ObjectState.Added;
                                            //        context.GatePassLine.Add(Gline);

                                            //        //new GatePassLineService(_unitOfWork).Create(Gline);
                                            //    }
                                            //    pd.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                            //}
                                        }
                                        context.SaveChanges();
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
                    ActivityType = (int)ActivityTypeContants.Added,
                    Narration = "GatePass created for StockHeaders " + StockHeaderIds,
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

                    var pd = context.StockHeader.Find(Id);

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

                    var GatePass = context.GatePassHeader.Find(pd.GatePassHeaderId);

                    if (GatePass.Status != (int)StatusConstants.Submitted)
                    {

                        pd.Status = (int)StatusConstants.Modified;
                        pd.GatePassHeaderId = null;
                        pd.ModifiedBy = User.Identity.Name;
                        pd.ModifiedDate = DateTime.Now;
                        pd.IsGatePassPrinted = false;
                        pd.ObjectState = Model.ObjectState.Modified;
                        context.StockHeader.Add(pd);


                        GatePass.Status = (int)StatusConstants.Cancel;
                        GatePass.ObjectState = Model.ObjectState.Modified;
                        context.GatePassHeader.Add(GatePass);

                        XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                        context.SaveChanges();

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
                        int Copies = 1;
                        int AdditionalCopies = Settings.NoOfPrintCopies ?? 0;
                        bool UpdateGatePassPrint = true;
                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = context.StockHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.StockHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        do
                        {
                            byte[] Pdf;

                            if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified || pd.Status == (int)StatusConstants.Import)
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

                            if (UpdateGatePassPrint && !(pd.IsGatePassPrinted ?? false))
                            {
                                if (pd.GatePassHeaderId.HasValue)
                                {
                                    pd.IsGatePassPrinted = true;
                                    pd.ObjectState = Model.ObjectState.Modified;
                                    context.StockHeader.Add(pd);
                                    context.SaveChanges();

                                    UpdateGatePassPrint = false;
                                    Copies = AdditionalCopies;
                                    if (Copies > 0)
                                        continue;
                                }
                            }

                            Copies--;

                        } while (Copies > 0);
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
            var Query = _StockHeaderService.GetCustomPerson(filter, searchTerm, filter2);
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
            int Stockline = (new StockLineService(_unitOfWork).GetStockLineListForIndex(id)).Count();
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
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
