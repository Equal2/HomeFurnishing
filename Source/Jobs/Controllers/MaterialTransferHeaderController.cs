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
using System.Xml.Linq;
using MaterialTransferDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using System.Data.SqlClient;
using Reports.Reports;
using Reports.Controllers;



namespace Jobs.Controllers
{
    [Authorize]
    public class MaterialTransferHeaderController : System.Web.Mvc.Controller
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

        public MaterialTransferHeaderController(IStockHeaderService PurchaseOrderHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockHeaderService = PurchaseOrderHeaderService;
            _ActivityLogService = ActivityLogService;
            _exception = exec;
            _unitOfWork = unitOfWork;
            if (!MaterialTransferEvents.Initialized)
            {
                MaterialTransferEvents Obj = new MaterialTransferEvents();
            }

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /StockHeader/
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
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            if (settings!=null)
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

            List<DocumentTypeHeaderAttributeViewModel> tem = new DocumentTypeService(_unitOfWork).GetDocumentTypeHeaderAttribute(id).ToList();
            p.DocumentTypeHeaderAttributes = tem;


            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, p.DivisionId, p.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForMaterialTransfer", "StockHeaderSettings", new { id = id }).Warning("Please create Material Transfer settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            p.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                p.FromGodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];

            PrepareViewBag(id);

            p.DocTypeId = id;

            p.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(p.DocTypeId);



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
                    BeforeSave = MaterialTransferDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = MaterialTransferDocEvents.beforeHeaderSaveEvent(this, new StockEventArgs(svm.StockHeaderId, EventModeConstants.Edit), ref db);
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

            if (!svm.PersonId.HasValue)
                ModelState.AddModelError("PersonId", "The Person field is required.");

            if (!svm.GodownId.HasValue)
                ModelState.AddModelError("GodownId", "The To Godown field is required.");

            if (svm.GodownId == svm.FromGodownId)
                ModelState.AddModelError("GodownId", "From Godown and To Godown can't be same.");


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


                    if (svm.DocumentTypeHeaderAttributes != null)
                    {
                        foreach (var Attributes in svm.DocumentTypeHeaderAttributes)
                        {
                            StockHeaderAttributes StockHeaderAttribute = (from A in db.StockHeaderAttributes
                                                                                where A.HeaderTableId == s.StockHeaderId
                                                                                && A.DocumentTypeHeaderAttributeId == Attributes.DocumentTypeHeaderAttributeId
                                                                                select A).FirstOrDefault();

                            if (StockHeaderAttribute != null)
                            {
                                StockHeaderAttribute.Value = Attributes.Value;
                                StockHeaderAttribute.ObjectState = Model.ObjectState.Modified;
                                db.StockHeaderAttributes.Add(StockHeaderAttribute);
                            }
                            else
                            {
                                StockHeaderAttributes HeaderAttribute = new StockHeaderAttributes()
                                {
                                    HeaderTableId = s.StockHeaderId,
                                    Value = Attributes.Value,
                                    DocumentTypeHeaderAttributeId = Attributes.DocumentTypeHeaderAttributeId,
                                };
                                HeaderAttribute.ObjectState = Model.ObjectState.Added;
                                db.StockHeaderAttributes.Add(HeaderAttribute);
                            }
                        }
                    }

                    try
                    {
                        MaterialTransferDocEvents.onHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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
                        MaterialTransferDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Add), ref db);
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

                    return RedirectToAction("Modify", "MaterialTransferHeader", new { Id = s.StockHeaderId }).Success("Data saved successfully");

                }
                #endregion

                #region EditRecord
                else
                {
                    bool GodownChanged = false;
                    bool FromGodownChanged = false;
                    bool DocDateChanged = false;
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeader temp = _StockHeaderService.Find(s.StockHeaderId);

                    GodownChanged = (temp.GodownId == s.GodownId) ? false : true;
                    FromGodownChanged = (temp.FromGodownId == s.FromGodownId) ? false : true;
                    DocDateChanged = (temp.DocDate == s.DocDate) ? false : true;

                    StockHeader ExRec = new StockHeader();
                    ExRec = Mapper.Map<StockHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    temp.DocDate = s.DocDate;
                    temp.DocNo = s.DocNo;
                    temp.PersonId = s.PersonId;
                    temp.ProcessId = s.ProcessId;
                    temp.GodownId = s.GodownId;
                    temp.FromGodownId = s.FromGodownId;
                    temp.Remark = s.Remark;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    //_StockHeaderService.Update(temp);
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(temp);


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    IEnumerable<Stock> stocklist = new StockService(_unitOfWork).GetStockForStockHeaderId(temp.StockHeaderId);

                    foreach (Stock item in stocklist)
                    {
                        Stock Stock = new StockService(_unitOfWork).Find(item.StockId);


                        if (GodownChanged == true)
                        {
                            if (item.Qty_Rec > 0)
                            {
                                Stock.GodownId = (int)temp.GodownId;
                            }
                        }

                        if (FromGodownChanged == true)
                        {
                            if (item.Qty_Iss > 0)
                            {
                                Stock.GodownId = (int)temp.FromGodownId;
                            }
                        }

                        if (DocDateChanged == true)
                        {
                            Stock.DocDate = temp.DocDate;
                        }

                        Stock.ObjectState = Model.ObjectState.Modified;
                        db.Stock.Add(Stock);


                        if (item.Qty_Rec > 0 && item.ProductUidId != null && item.ProductUidId != 0)
                        {
                            ProductUid ProductUid = new ProductUidService(_unitOfWork).Find((int)item.ProductUidId);

                            if (GodownChanged == true)
                            {
                                ProductUid.CurrenctGodownId = temp.GodownId;
                            }

                            if (DocDateChanged == true)
                            {
                                ProductUid.LastTransactionDocDate = temp.DocDate;
                            }


                            ProductUid.ObjectState = Model.ObjectState.Modified;
                            db.ProductUid.Add(ProductUid);
                        }
                    }


                    if (svm.DocumentTypeHeaderAttributes != null)
                    {
                        foreach (var Attributes in svm.DocumentTypeHeaderAttributes)
                        {

                            StockHeaderAttributes StockHeaderAttribute = (from A in db.StockHeaderAttributes
                                                                                where A.HeaderTableId == s.StockHeaderId
                                                                                && A.DocumentTypeHeaderAttributeId == Attributes.DocumentTypeHeaderAttributeId
                                                                                select A).FirstOrDefault();

                            if (StockHeaderAttribute != null)
                            {
                                StockHeaderAttribute.Value = Attributes.Value;
                                StockHeaderAttribute.ObjectState = Model.ObjectState.Modified;
                                db.StockHeaderAttributes.Add(StockHeaderAttribute);
                            }
                            else
                            {
                                StockHeaderAttributes HeaderAttribute = new StockHeaderAttributes()
                                {
                                    Value = Attributes.Value,
                                    HeaderTableId = s.StockHeaderId,
                                    DocumentTypeHeaderAttributeId = Attributes.DocumentTypeHeaderAttributeId,
                                };
                                HeaderAttribute.ObjectState = Model.ObjectState.Added;
                                db.StockHeaderAttributes.Add(HeaderAttribute);
                            }
                        }
                    }



                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        MaterialTransferDocEvents.onHeaderSaveEvent(this, new StockEventArgs(temp.StockHeaderId, EventModeConstants.Edit), ref db);
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
                        MaterialTransferDocEvents.afterHeaderSaveEvent(this, new StockEventArgs(s.StockHeaderId, EventModeConstants.Edit), ref db);
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


        // GET: /StockHeader/Edit/5
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

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForMaterialTransfer", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Transfer settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(s.DocTypeId);

            List<DocumentTypeHeaderAttributeViewModel> tem = _StockHeaderService.GetDocumentHeaderAttribute(id).ToList();
            s.DocumentTypeHeaderAttributes = tem;

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

            //Getting Settings
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(s.DocTypeId, s.DivisionId, s.SiteId);

            if (settings == null)
            {
                return RedirectToAction("CreateForMaterialTransfer", "StockHeaderSettings", new { id = s.DocTypeId }).Warning("Please create Material Transfer settings");
            }
            s.StockHeaderSettings = Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
            s.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(s.DocTypeId);

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
                BeforeSave = MaterialTransferDocEvents.beforeHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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
                var StockHeader = (from p in db.StockHeader
                                   where p.StockHeaderId == vm.id
                                   select p).FirstOrDefault();

                try
                {
                    MaterialTransferDocEvents.onHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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

                List<int> FromStockIdList = new List<int>();
                List<int> StockIdList = new List<int>();

                var ProdUids = StockLine.Select(m => m.ProductUidId).ToArray();

                var ProdUidRecords = (from p in db.ProductUid
                                      where ProdUids.Contains(p.ProductUIDId)
                                      select p).ToList();

                var attributes = (from A in db.StockHeaderAttributes where A.HeaderTableId == vm.id select A).ToList();

                foreach (var ite2 in attributes)
                {
                    ite2.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeaderAttributes.Remove(ite2);
                }


                //Mark ObjectState.Delete to all the Purchase Order Lines. 
                foreach (var item in StockLine)
                {

                    StockLine ExRecLine = new StockLine();
                    ExRecLine = Mapper.Map<StockLine>(item);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRecLine,
                    });

                    if (item.FromStockId != null)
                    {
                        FromStockIdList.Add((int)item.FromStockId);
                    }

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }
                    var Productuid = item.ProductUidId;
                    //new StockLineService(_unitOfWork).Delete(item);

                    if (Productuid != null && Productuid != 0)
                    {
                        //Service.ProductUidDetail ProductUidDetail = new ProductUidService(_unitOfWork).FGetProductUidLastValues((int)Productuid, "Stock Transfer-" + item.StockHeaderId.ToString());

                        ProductUid ProductUid = ProdUidRecords.Where(m => m.ProductUIDId == Productuid).FirstOrDefault();

                        ProductUid.LastTransactionDocDate = item.ProductUidLastTransactionDocDate;
                        ProductUid.LastTransactionDocId = item.ProductUidLastTransactionDocId;
                        ProductUid.LastTransactionDocNo = item.ProductUidLastTransactionDocNo;
                        ProductUid.LastTransactionDocTypeId = item.ProductUidLastTransactionDocTypeId;
                        ProductUid.LastTransactionPersonId = item.ProductUidLastTransactionPersonId;
                        ProductUid.CurrenctGodownId = item.ProductUidCurrentGodownId;
                        ProductUid.CurrenctProcessId = item.ProductUidCurrentProcessId;
                        ProductUid.Status = item.ProductUidStatus;

                        ProductUid.ModifiedBy = User.Identity.Name;
                        ProductUid.ModifiedDate = DateTime.Now;

                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(ProductUid);

                        //new ProductUidService(_unitOfWork).Update(ProductUid);
                        //ProductUid.ObjectState = Model.ObjectState.Modified;
                        //db.ProductUid.Add(ProductUid);

                        new StockUidService(_unitOfWork).DeleteStockUidForDocLineDB(item.StockHeaderId, StockHeader.DocTypeId, StockHeader.SiteId, StockHeader.DivisionId, ref db);
                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.StockLine.Remove(item);
                }



                foreach (var item in FromStockIdList)
                {
                    StockAdj Adj = (from L in db.StockAdj
                                    where L.StockOutId == item
                                    select L).FirstOrDefault();

                    if (Adj != null)
                    {
                        Adj.ObjectState = Model.ObjectState.Deleted;
                        db.StockAdj.Remove(Adj);
                    }

                    new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                }

                foreach (var item in StockIdList)
                {
                    new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
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
                    MaterialTransferDocEvents.afterHeaderDeleteEvent(this, new StockEventArgs(vm.id), ref db);
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
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue, string GenGatePass)
        {
            bool BeforeSave = true;
            try
            {
                BeforeSave = MaterialTransferDocEvents.beforeHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                    var ToGodown = db.Godown.Find(pd.GodownId);
                    var Settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);
                    int ActivityType;


                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    pd.ReviewBy = null;


                    if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                    {
                        if (!String.IsNullOrEmpty(Settings.SqlProcGatePass))
                        {

                            SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
                            IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                            if (pd.GatePassHeaderId == null)
                            {
                                SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                DocDate.SqlDbType = SqlDbType.DateTime;
                                SqlParameter Godown = new SqlParameter("@GodownId", pd.FromGodownId);
                                SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.GatePass).DocumentTypeId);
                                GatePassHeader GPHeader = new GatePassHeader();
                                GPHeader.CreatedBy = User.Identity.Name;
                                GPHeader.CreatedDate = DateTime.Now;
                                GPHeader.DivisionId = pd.DivisionId;
                                GPHeader.DocDate = DateTime.Now.Date;
                                GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.GatePass).DocumentTypeId;
                                GPHeader.ModifiedBy = User.Identity.Name;
                                GPHeader.ModifiedDate = DateTime.Now;
                                GPHeader.Remark = ToGodown != null ? "Transfer To: " + ToGodown.GodownName : "";
                                GPHeader.PersonId = pd.PersonId.Value;
                                GPHeader.SiteId = pd.SiteId;
                                GPHeader.GodownId = pd.FromGodownId ?? 0;
                                GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                                GPHeader.ReferenceDocId = pd.StockHeaderId;
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

                                    // new GatePassLineService(_unitOfWork).Create(Gline);
                                    Gline.ObjectState = Model.ObjectState.Added;
                                    db.GatePassLine.Add(Gline);
                                }

                                pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }
                            else
                            {
                                //List<GatePassLine> LineList = new GatePassLineService(_unitOfWork).GetGatePassLineList(pd.GatePassHeaderId ?? 0).ToList();

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

                                    //new GatePassLineService(_unitOfWork).Create(Gline);
                                    Gline.ObjectState = Model.ObjectState.Added;
                                    db.GatePassLine.Add(Gline);
                                }

                                pd.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }

                        }
                    }


                    //_StockHeaderService.Update(pd);
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.StockHeader.Add(pd);

                    try
                    {
                        MaterialTransferDocEvents.onHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                        MaterialTransferDocEvents.afterHeaderSubmitEvent(this, new StockEventArgs(Id), ref db);
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
                BeforeSave = MaterialTransferDocEvents.beforeHeaderApproveEvent(this, new StockEventArgs(Id), ref db);
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
                    MaterialTransferDocEvents.onHeaderApproveEvent(this, new StockEventArgs(Id), ref db);
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
                    MaterialTransferDocEvents.afterHeaderApproveEvent(this, new StockEventArgs(Id), ref db);
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
            //if (header.Status == (int)StatusConstants.Drafted)
            //{
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);


            if (string.IsNullOrEmpty(settings.SqlProcDocumentPrint))
                throw new Exception("Document Print Not Configured");
            else
                return PrintOut(id, settings.SqlProcDocumentPrint);
            //}
            //else
            //    return HttpNotFound();

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
        public ActionResult PrintAfter_Approve(int id)
        {
            StockHeader header = _StockHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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

            var SEttings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            //if (!Dt.ReportMenuId.HasValue)
            //    throw new Exception("Report Menu not configured in document types");

            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Stock Transfer Report&DocTypeId=" + id.ToString());


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


        public JsonResult GetGodowns(string searchTerm, int pageSize, int pageNum, int filter)
        {
            // var temp = _GodownService.GetGodownForContraSiteFilters(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var temp = new GodownService(_unitOfWork).GetGodownForContraSiteFilters(filter, searchTerm).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = new GodownService(_unitOfWork).GetGodownForContraSiteFilters(filter, searchTerm).Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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

                            var pd = db.StockHeader.Find(item);


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
                                    var ToGodown = db.Godown.Find(pd.GodownId);
                                    if ((pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted) && !pd.GatePassHeaderId.HasValue)
                                    {

                                        SqlParameter SqlParameterUserId = new SqlParameter("@Id", item);
                                        IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                        if (pd.PersonId != null)
                                        {
                                            if (pd.GatePassHeaderId == null)
                                            {
                                                SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                                DocDate.SqlDbType = SqlDbType.DateTime;
                                                SqlParameter Godown = new SqlParameter("@GodownId", pd.FromGodownId);
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
                                                GPHeader.Remark = ToGodown != null ? "Transfer To: " + ToGodown.GodownName : "";
                                                GPHeader.PersonId = (int)pd.PersonId;
                                                GPHeader.SiteId = pd.SiteId;
                                                GPHeader.GodownId = (int)pd.FromGodownId;
                                                GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                                                GPHeader.ReferenceDocId = pd.StockHeaderId;
                                                GPHeader.ReferenceDocNo = pd.DocNo;
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

                                                pd.GatePassHeaderId = GPHeader.GatePassHeaderId;


                                                pd.ObjectState = Model.ObjectState.Modified;
                                                db.StockHeader.Add(pd);

                                                StockHeaderIds += pd.StockHeaderId + ", ";
                                            }
                                        }
                                        db.SaveChanges();
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
                        pd.Status = (int)StatusConstants.Modified;
                        pd.GatePassHeaderId = null;
                        pd.ModifiedBy = User.Identity.Name;
                        pd.ModifiedDate = DateTime.Now;
                        pd.IsGatePassPrinted = false;
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
                        int Copies = 1;
                        int AdditionalCopies = Settings.NoOfPrintCopies ?? 0;
                        bool UpdateGatePassPrint = true;
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
                            else if (pd.Status == (int)StatusConstants.Approved)
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
                                    db.StockHeader.Add(pd);
                                    db.SaveChanges();

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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
