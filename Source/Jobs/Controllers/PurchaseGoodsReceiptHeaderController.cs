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
using PurchaseGoodsReceiptDocumentEvents;
using DocumentEvents;
using CustomEventArgs;
using Reports.Controllers;
using Reports.Reports;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseGoodsReceiptHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseGoodsReceiptHeaderService _PurchaseGoodsReceiptHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseGoodsReceiptHeaderController(IPurchaseGoodsReceiptHeaderService PurchaseGoodsReceiptHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseGoodsReceiptHeaderService = PurchaseGoodsReceiptHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PurchaseGoodsReceiptEvents.Initialized)
            {
                PurchaseGoodsReceiptEvents Obj = new PurchaseGoodsReceiptEvents();
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

        public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();

        }

        // GET: /PurchaseGoodsReceiptHeaderMaster/

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
            var PurchaseGoodsReceiptHeader = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(PurchaseGoodsReceiptHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }

        // GET: /PurchaseGoodsReceiptHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            PurchaseGoodsReceiptHeaderViewModel vm = new PurchaseGoodsReceiptHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseGoodsReceiptSetting", new { id = id }).Warning("Please create Purchase receipt settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            vm.PurchaseGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            //vm.DocNo = _PurchaseGoodsReceiptHeaderService.GetMaxDocNo();
            vm.DocDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.UnitConversionForId = settings.UnitConversionForId;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseGoodsReceiptHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            PrepareViewBag(id);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseGoodsReceiptHeaderViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (vm.PurchaseGoodsReceiptHeaderId <= 0)
                    BeforeSave = PurchaseGoodsReceiptDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReceiptHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseGoodsReceiptDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReceiptHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.PurchaseGoodsReceiptHeaderId <= 0)
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
                if (vm.PurchaseGoodsReceiptHeaderId <= 0)
                {

                    PurchaseGoodsReceiptHeader header = new PurchaseGoodsReceiptHeader();
                    header = Mapper.Map<PurchaseGoodsReceiptHeaderViewModel, PurchaseGoodsReceiptHeader>(vm);
                    header.CreatedBy = User.Identity.Name;
                    header.CreatedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;

                    header.ObjectState = Model.ObjectState.Added;
                    //_PurchaseGoodsReceiptHeaderService.Create(header);

                    db.PurchaseGoodsReceiptHeader.Add(header);

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(header.PurchaseGoodsReceiptHeaderId, EventModeConstants.Add), ref db);
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
                        PurchaseGoodsReceiptDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(header.PurchaseGoodsReceiptHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.PurchaseGoodsReceiptHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));

                    //return Edit(header.PurchaseGoodsReceiptHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = header.PurchaseGoodsReceiptHeaderId }).Success("Data saved successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseGoodsReceiptHeader temp = _PurchaseGoodsReceiptHeaderService.Find(vm.PurchaseGoodsReceiptHeaderId);

                    PurchaseGoodsReceiptHeader ExRec = new PurchaseGoodsReceiptHeader();
                    ExRec = Mapper.Map<PurchaseGoodsReceiptHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                    {
                        temp.Status = (int)StatusConstants.Modified;
                    }

                    temp.DocNo = vm.DocNo;
                    temp.DocDate = vm.DocDate;
                    temp.SupplierId = vm.SupplierId;
                    temp.UnitConversionForId = vm.UnitConversionForId;
                    temp.PurchaseWaybillId = vm.PurchaseWaybillId;
                    temp.SupplierDocNo = vm.SupplierDocNo;
                    temp.GodownId = vm.GodownId;
                    temp.SupplierDocDate = vm.SupplierDocDate;
                    temp.RoadPermitFormId = vm.RoadPermitFormId;
                    temp.Remark = vm.Remark;

                    vm.GateInId = vm.GateInId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_PurchaseGoodsReceiptHeaderService.Update(temp);
                    db.PurchaseGoodsReceiptHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });


                    if (temp.StockHeaderId != null)
                    {
                        StockHeader S = db.StockHeader.Find(temp.StockHeaderId);
                        S.DocTypeId = temp.DocTypeId;
                        S.DocDate = temp.DocDate;
                        S.DocNo = temp.DocNo;
                        S.DivisionId = temp.DivisionId;
                        S.SiteId = temp.SiteId;
                        S.PersonId = temp.SupplierId;
                        S.GodownId = temp.GodownId;
                        S.Remark = temp.Remark;
                        S.Status = temp.Status;
                        S.ModifiedBy = temp.ModifiedBy;
                        S.ModifiedDate = temp.ModifiedDate;
                        S.ObjectState = Model.ObjectState.Modified;
                        db.StockHeader.Add(S);
                        //new StockHeaderService(_unitOfWork).UpdateStockHeader(S);
                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseGoodsReceiptHeaderId, EventModeConstants.Edit), ref db);
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
                        PurchaseGoodsReceiptDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseGoodsReceiptHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseGoodsReceiptHeaderId,
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
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;

            PurchaseGoodsReceiptHeaderViewModel pt = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptHeader(id);

            if (pt == null)
            {
                return HttpNotFound();
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

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseGoodsReceiptSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase receipt settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
            PrepareViewBag(pt.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseGoodsReceiptHeaderId,
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
            PurchaseGoodsReceiptHeader header = _PurchaseGoodsReceiptHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PurchaseGoodsReceiptHeader header = _PurchaseGoodsReceiptHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            PurchaseGoodsReceiptHeader header = _PurchaseGoodsReceiptHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PurchaseGoodsReceiptHeader header = _PurchaseGoodsReceiptHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, int? DocLineId)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", DocLineId = DocLineId });
        }


        [Authorize]
        public ActionResult Detail(int id, string IndexType, string transactionType, int? DocLineId)
        {
            if (DocLineId.HasValue)
                ViewBag.DocLineId = DocLineId;

            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            PurchaseGoodsReceiptHeaderViewModel pt = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptHeader(id);
            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseGoodsReceiptSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase receipt settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseGoodsReceiptHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));


            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            bool Continue = true;

            PurchaseGoodsReceiptHeader s = db.PurchaseGoodsReceiptHeader.Find(id);

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
                BeforeSave = PurchaseGoodsReceiptDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseGoodsReceiptHeader pd = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    int ActivityType;

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReceiptHeader.Add(pd);

                    try
                    {
                        PurchaseGoodsReceiptDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
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
                        PurchaseGoodsReceiptDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PurchaseGoodsReceiptHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    NotifyUser(Id, ActivityTypeContants.Submitted);

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseGoodsReceiptHeaders", "PurchaseGoodsReceiptHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Receipt " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Receipt " + pd.DocNo + " submitted successfully.");

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Receipt " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Order " + pd.DocNo + " submitted successfully.");

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
                BeforeSave = PurchaseGoodsReceiptDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseGoodsReceiptHeader pd = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {


                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.PurchaseGoodsReceiptHeader.Add(pd);

                try
                {
                    PurchaseGoodsReceiptDocEvents.onHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
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
                    PurchaseGoodsReceiptDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PurchaseGoodsReceiptHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                NotifyUser(Id, ActivityTypeContants.Approved);


                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    PurchaseGoodsReceiptHeader HEader = _PurchaseGoodsReceiptHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.PurchaseGoodsReceiptHeaders", "PurchaseGoodsReceiptHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewd Successfully");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewd Successfully");

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Record Reviewd Successfully");
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Reviewd Successfully");
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
            PurchaseGoodsReceiptHeader PurchaseGoodsReceiptHeader = _PurchaseGoodsReceiptHeaderService.Find(id);
            if (PurchaseGoodsReceiptHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PurchaseGoodsReceiptHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseGoodsReceiptDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
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

                int? StockHeaderId = 0;

                var temp = db.PurchaseGoodsReceiptHeader.Find(vm.id);

                try
                {
                    PurchaseGoodsReceiptDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseGoodsReceiptHeader>(temp),
                });

                StockHeaderId = temp.StockHeaderId;

                var line = (from p in db.PurchaseGoodsReceiptLine
                            where p.PurchaseGoodsReceiptHeaderId == vm.id
                            select p).ToList();

                var ProdUids = line.Select(m => m.ProductUidId).ToArray();

                var ProduUidRecords = (from p in db.ProductUid
                                       where ProdUids.Contains(p.ProductUIDId)
                                       select p).ToList();

                List<int> StockIdList = new List<int>();

                new PurchaseOrderLineStatusService(_unitOfWork).DeletePurchaseQtyOnReceiveMultiple(temp.PurchaseGoodsReceiptHeaderId, ref db);

                foreach (var item in line)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseGoodsReceiptLine>(item),
                    });

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    //PurchaseGoodsReceiptLine PurchaseGoodsReceiptLine = new PurchaseGoodsReceiptLineService(_unitOfWork).Find(item.PurchaseGoodsReceiptLineId);

                    var Productuid = item.ProductUidId;

                    if (Productuid != null && Productuid != 0)
                    {
                        ProductUid ProductUid = ProduUidRecords.Where(m => m.ProductUIDId == Productuid).FirstOrDefault();

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

                        //new ProductUidService(_unitOfWork).Update(ProductUid);
                        ProductUid.ObjectState = Model.ObjectState.Modified;
                        db.ProductUid.Add(ProductUid);

                    }

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseGoodsReceiptLine.Remove(item);

                    //new PurchaseGoodsReceiptLineService(_unitOfWork).Delete(item.PurchaseGoodsReceiptLineId);
                }

                foreach (var item in StockIdList)
                {
                    new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                }


                //_PurchaseGoodsReceiptHeaderService.Delete(vm.id);
                temp.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReceiptHeader.Remove(temp);


                if (StockHeaderId != null)
                {
                    var StockHead = db.StockHeader.Find(StockHeaderId);
                    StockHead.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeader.Remove(StockHead);
                    //new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
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
                    PurchaseGoodsReceiptDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.PurchaseGoodsReceiptHeaderId,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseGoodsReceiptHeaders", "PurchaseGoodsReceiptHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseGoodsReceiptHeaders", "PurchaseGoodsReceiptHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
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

                var Settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.PurchaseGoodsReceiptHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PurchaseGoodsReceiptHeaderId,
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

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        public JsonResult GetPendingWayBills(int id)//SupplierId
        {
            return Json(new { success = true });
        }

        public JsonResult GetPendingGateIns(int supplierid)
        {
            return Json(new GateInService(_unitOfWork).GetPendingGateIns(supplierid).ToList());
        }

        public JsonResult GetPendingRoadWayBills(int supplierid)
        {
            return Json(new GateInService(_unitOfWork).GetPendingGateIns(supplierid).ToList());
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



        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            PurchaseGoodsReceiptHeaderViewModel vm = new PurchaseGoodsReceiptHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(id, vm.DivisionId, vm.SiteId);

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

        private void NotifyUser(int Id, ActivityTypeContants ActivityType)
        {
            PurchaseGoodsReceiptHeader Header = new PurchaseGoodsReceiptHeaderService(_unitOfWork).Find(Id);
            PurchaseGoodsReceiptSetting PurchaseGoodsReceiptSettings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
            DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
            DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);

            new NotifyUserController(_unitOfWork).SendEmailMessage(Id, ActivityType, DocEmailContentSettings, PurchaseGoodsReceiptSettings.SqlProcDocumentPrint);
            new NotifyUserController(_unitOfWork).SendNotificationMessage(Id, ActivityType, DocNotificationContentSettings, User.Identity.Name);
            new NotifyUserController(_unitOfWork).SendSmsMessage(Id, ActivityType, DocSmsContentSettings);

        }

        public int PendingToSubmitCount(int id)
        {
            return (_PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PurchaseGoodsReceiptHeaderService.GetPurchaseGoodsReceiptPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _PurchaseGoodsReceiptHeaderService.GetCustomPerson(filter, searchTerm);
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

    }
}
