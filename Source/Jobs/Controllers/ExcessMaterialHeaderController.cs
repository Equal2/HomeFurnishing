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
using DocumentEvents;
using CustomEventArgs;
using ExcessMaterialDocumentEvents;
using Reports.Reports;
using Reports.Controllers;
using Model.ViewModels;



namespace Jobs.Controllers
{
    [Authorize]
    public class ExcessMaterialHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IExcessMaterialHeaderService _ExcessMaterialHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public ExcessMaterialHeaderController(IExcessMaterialHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ExcessMaterialHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!ExcessMaterialEvents.Initialized)
            {
                ExcessMaterialEvents Obj = new ExcessMaterialEvents();
            }
            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /ExcessMaterialHeader

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
            IQueryable<ExcessMaterialHeaderViewModel> p = _ExcessMaterialHeaderService.GetExcessMaterialHeaderList(id, User.Identity.Name);
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
            IQueryable<ExcessMaterialHeaderViewModel> p = _ExcessMaterialHeaderService.GetExcessMaterialHeaderListPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", p);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            IQueryable<ExcessMaterialHeaderViewModel> p = _ExcessMaterialHeaderService.GetExcessMaterialHeaderListPendingToReview(id, User.Identity.Name);
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
            var settings = new ExcessMaterialSettingsService(_unitOfWork, context).GetExcessMaterialSettingsForDocument(id,DivisionId,SiteId);
            if(settings !=null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;                
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }

        }

        // GET: /ExcessMaterialHeader/Create
        [HttpGet]
        public ActionResult Create(int id)//DocumentTypeId
        {
            ExcessMaterialHeaderViewModel p = new ExcessMaterialHeaderViewModel();


            if (System.Web.HttpContext.Current.Session["ExcessMaterialDocDate"] == null)
                p.DocDate = DateTime.Now;
            else
                p.DocDate = (DateTime)System.Web.HttpContext.Current.Session["ExcessMaterialDocDate"];

            p.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            p.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            p.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new ExcessMaterialSettingsService(_unitOfWork,context).GetExcessMaterialSettingsForDocument(id, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "ExcessMaterialSettings", new { id = id }).Warning("Please create Material Issue settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.ExcessMaterialSettings = Mapper.Map<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>(settings);
            p.ProcessId = settings.ProcessId;

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                p.GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            PrepareViewBag(id);

            p.DocTypeId = id;
            p.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".ExcessMaterialHeaders", p.DocTypeId, p.DocDate, p.DivisionId, p.SiteId);
            ViewBag.Mode = "Add";
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ExcessMaterialHeaderViewModel svm)
        {
            ExcessMaterialHeader s = Mapper.Map<ExcessMaterialHeaderViewModel, ExcessMaterialHeader>(svm);

            System.Web.HttpContext.Current.Session["ExcessMaterialDocDate"] = svm.DocDate;

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (svm.ExcessMaterialHeaderId <= 0)
                    BeforeSave = ExcessMaterialDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.ExcessMaterialHeaderId, EventModeConstants.Add), ref context);
                else
                    BeforeSave = ExcessMaterialDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.ExcessMaterialHeaderId, EventModeConstants.Edit), ref context);
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

                if (svm.ExcessMaterialHeaderId <= 0)
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
                if (svm.ExcessMaterialHeaderId <= 0)
                {
                    s.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".ExcessMaterialHeaders", s.DocTypeId, s.DocDate, s.DivisionId, s.SiteId);
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.Status = (int)StatusConstants.Drafted;
                    //_ExcessMaterialHeaderService.Create(s);
                    s.ObjectState = Model.ObjectState.Added;
                    context.ExcessMaterialHeader.Add(s);

                    try
                    {
                        ExcessMaterialDocEvents.onHeaderSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, EventModeConstants.Add), ref context);
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
                        ExcessMaterialDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, EventModeConstants.Add), ref context);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = s.DocTypeId,
                        DocId = s.ExcessMaterialHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = s.DocNo,
                        DocDate = s.DocDate,
                        DocStatus = s.Status,
                    }));       
                   
                    return RedirectToAction("Modify", "ExcessMaterialHeader", new { Id = s.ExcessMaterialHeaderId }).Success("Data saved successfully");

                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ExcessMaterialHeader temp = _ExcessMaterialHeaderService.Find(s.ExcessMaterialHeaderId);

                    ExcessMaterialHeader ExRec = new ExcessMaterialHeader();
                    ExRec = Mapper.Map<ExcessMaterialHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    temp.DocDate = s.DocDate;
                    temp.DocNo = s.DocNo;
                    temp.PersonId = s.PersonId;
                    temp.ProcessId = s.ProcessId;
                    temp.GodownId = s.GodownId;
                    temp.Remark = s.Remark;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    context.ExcessMaterialHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        ExcessMaterialDocEvents.onHeaderSaveEvent(this, new StockEventArgs(temp.ExcessMaterialHeaderId, EventModeConstants.Edit), ref context);
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
                        ExcessMaterialDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.ExcessMaterialHeaderId, EventModeConstants.Edit), ref context);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.ExcessMaterialHeaderId,
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
            ExcessMaterialHeader header = _ExcessMaterialHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            ExcessMaterialHeader header = _ExcessMaterialHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            ExcessMaterialHeader header = _ExcessMaterialHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            ExcessMaterialHeader header = _ExcessMaterialHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Remove(id);
            else
                return HttpNotFound();
        }


        // GET: /ExcessMaterialHeader/Edit/5
        [HttpGet]
        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            
            ExcessMaterialHeaderViewModel s = _ExcessMaterialHeaderService.GetExcessMaterialHeader(id);

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
            var settings = new ExcessMaterialSettingsService(_unitOfWork,context).GetExcessMaterialSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "ExcessMaterialSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            s.ExcessMaterialSettings = Mapper.Map<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>(settings);
           
            ViewBag.Mode = "Edit";
            PrepareViewBag(s.DocTypeId);

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.ExcessMaterialHeaderId,
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

            ExcessMaterialHeaderViewModel s = _ExcessMaterialHeaderService.GetExcessMaterialHeader(id);

            //Job Order Settings
            var settings = new ExcessMaterialSettingsService(_unitOfWork, context).GetExcessMaterialSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null)
            {
                return RedirectToAction("Create", "ExcessMaterialSettings", new { id = s.DocTypeId }).Warning("Please create Material Issue settings");
            }

            s.ExcessMaterialSettings = Mapper.Map<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>(settings);

            PrepareViewBag(s.DocTypeId);
            if (s == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = s.DocTypeId,
                    DocId = s.ExcessMaterialHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = s.DocNo,
                    DocDate = s.DocDate,
                    DocStatus = s.Status,
                }));


            return View("Create", s);
        }


        // GET: /PurchaseOrderHeader/Delete/5
        [HttpGet]
        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExcessMaterialHeader ExcessMaterialHeader = _ExcessMaterialHeaderService.Find(id);
            if (ExcessMaterialHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(ExcessMaterialHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                BeforeSave = ExcessMaterialDocEvents.beforeHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref context);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            ExcessMaterialHeader ExcessMaterialHeader = (from p in context.ExcessMaterialHeader
                                       where p.ExcessMaterialHeaderId == vm.id
                                       select p).FirstOrDefault();

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                try
                {
                    ExcessMaterialDocEvents.onHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }


                ExcessMaterialHeader ExRec = new ExcessMaterialHeader();
                ExRec = Mapper.Map<ExcessMaterialHeader>(ExcessMaterialHeader);
                ExcessMaterialHeader Rec = new ExcessMaterialHeader();

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = ExRec,
                    Obj = Rec,
                });

                //Then find all the Purchase Order Header Line associated with the above ProductType.
                //var ExcessMaterialLine = new ExcessMaterialLineService(_unitOfWork).GetExcessMaterialLineforDelete(vm.id);

                var ExcessMaterialLine = (from p in context.ExcessMaterialLine
                                 where p.ExcessMaterialHeaderId == vm.id
                                 select p).ToList();

                //var ProductUids = ExcessMaterialLine.Select(m => m.ProductUidId).ToArray();

                //var ProdUidRecords = (from p in context.ProductUid
                //                      where ProductUids.Contains(p.ProductUIDId)
                //                      select p).ToList();


                //new RequisitionLineStatusService(_unitOfWork).DeleteRequisitionQtyOnIssueMultiple(ExcessMaterialHeader.ExcessMaterialHeaderId, ref context);

                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in ExcessMaterialLine)
                {
                    ExcessMaterialLine ExRecLine = new ExcessMaterialLine();
                    ExRecLine = Mapper.Map<ExcessMaterialLine>(item);
                    ExcessMaterialLine RecLine = new ExcessMaterialLine();

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
                        Obj = RecLine,
                    });

                    //if (item.ProductUidId != null && item.ProductUidId > 0)
                    //{
                    //    ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)item.ProductUidId, "ExcessMaterial Head-" + ExcessMaterialHeader.ExcessMaterialHeaderId.ToString());

                    //    ProductUid ProductUid = ProdUidRecords.Where(m => m.ProductUIDId == item.ProductUidId).FirstOrDefault();

                    //    ProductUid.LastTransactionDocDate = ProductUidDetail.LastTransactionDocDate;
                    //    ProductUid.LastTransactionDocId = ProductUidDetail.LastTransactionDocId;
                    //    ProductUid.LastTransactionDocNo = ProductUidDetail.LastTransactionDocNo;
                    //    ProductUid.LastTransactionDocTypeId = ProductUidDetail.LastTransactionDocTypeId;
                    //    ProductUid.LastTransactionPersonId = ProductUidDetail.LastTransactionPersonId;
                    //    ProductUid.CurrenctGodownId = ProductUidDetail.CurrenctGodownId;
                    //    ProductUid.CurrenctProcessId = ProductUidDetail.CurrenctProcessId;
                    //    ProductUid.ObjectState = Model.ObjectState.Modified;
                    //    context.ProductUid.Add(ProductUid);

                    //}

                    item.ObjectState = Model.ObjectState.Deleted;
                    context.ExcessMaterialLine.Remove(item);
                }

                ExcessMaterialHeader.ObjectState = Model.ObjectState.Deleted;
                context.ExcessMaterialHeader.Remove(ExcessMaterialHeader);

                //if (GatePassHeaderId.HasValue)
                //{

                //    var GatePassLines = (from p in context.GatePassLine
                //                         where p.GatePassHeaderId == GatePassHeaderId
                //                         select p).ToList();

                //    foreach (var item in GatePassLines)
                //    {
                //        item.ObjectState = Model.ObjectState.Deleted;
                //        context.GatePassLine.Remove(item);
                //    }

                //    GatePassHeader.ObjectState = Model.ObjectState.Deleted;

                //    context.GatePassHeader.Remove(GatePassHeader);

                //}

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);               

                //Commit the DB
                try
                {
                    if (EventException)
                    { throw new Exception(); }
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
                    ExcessMaterialDocEvents.afterHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = ExcessMaterialHeader.DocTypeId,
                    DocId = ExcessMaterialHeader.ExcessMaterialHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = ExcessMaterialHeader.DocNo,
                    xEModifications = Modifications,
                    DocDate = ExcessMaterialHeader.DocDate,
                    DocStatus = ExcessMaterialHeader.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }


        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            ExcessMaterialHeader s = context.ExcessMaterialHeader.Find(id);

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
                BeforeSave = ExcessMaterialDocEvents.beforeHeaderSubmitEvent(this, new StockEventArgs(Id), ref context);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            ExcessMaterialHeader pd = new ExcessMaterialHeaderService(_unitOfWork,context).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    int ActivityType;

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    ExcessMaterialSettings Settings = new ExcessMaterialSettingsService(_unitOfWork, context).GetExcessMaterialSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;

                    context.ExcessMaterialHeader.Add(pd);

                    try
                    {
                        ExcessMaterialDocEvents.onHeaderSubmitEvent(this, new StockEventArgs(Id), ref context);
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
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = pd.DocTypeId });
                    }


                    try
                    {
                        ExcessMaterialDocEvents.afterHeaderSubmitEvent(this, new StockEventArgs(Id), ref context);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.ExcessMaterialHeaderId,
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
            ExcessMaterialHeader s = _ExcessMaterialHeaderService.Find(id);
            var settings = new ExcessMaterialSettingsService(_unitOfWork, context).GetExcessMaterialSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);
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
                BeforeSave = ExcessMaterialDocEvents.beforeHeaderReviewEvent(this, new StockEventArgs(Id), ref context);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before Review.";

            ExcessMaterialHeader pd = new ExcessMaterialHeaderService(_unitOfWork, context).Find(Id);

            if (ModelState.IsValid && BeforeSave)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                context.ExcessMaterialHeader.Add(pd);

                try
                {
                    ExcessMaterialDocEvents.onHeaderReviewEvent(this, new StockEventArgs(Id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                context.SaveChanges();

                try
                {
                    ExcessMaterialDocEvents.afterHeaderReviewEvent(this, new StockEventArgs(Id), ref context);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.ExcessMaterialHeaderId,
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
            ExcessMaterialHeaderViewModel vm = new ExcessMaterialHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new ExcessMaterialSettingsService(_unitOfWork, context).GetExcessMaterialSettingsForDocument(id, vm.DivisionId, vm.SiteId);

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
            return (_ExcessMaterialHeaderService.GetExcessMaterialHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_ExcessMaterialHeaderService.GetExcessMaterialHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        [HttpGet]
        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.ExcessMaterialHeaders", "ExcessMaterialHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.ExcessMaterialHeaders", "ExcessMaterialHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }



   
        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new ExcessMaterialSettingsService(_unitOfWork, context).GetExcessMaterialSettingsForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = context.ExcessMaterialHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.ExcessMaterialHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

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

        public JsonResult GetDocNo(DateTime Date, int DocTypeId, int SiteId, int DivisionId)
        {
            return Json(new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".ExcessMaterialHeaders", DocTypeId, Date, DivisionId, SiteId), JsonRequestBehavior.AllowGet);
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
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
