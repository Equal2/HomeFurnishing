using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.SqlClient;
using System.Configuration;
using System.Xml.Linq;
using PurchaseGoodsReturnDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Reports.Reports;
using Model.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseGoodsReturnHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseGoodsReturnHeaderService _PurchaseGoodsReturnHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseGoodsReturnHeaderController(IPurchaseGoodsReturnHeaderService PurchaseGoodsReturnHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseGoodsReturnHeaderService = PurchaseGoodsReturnHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PurchaseGoodsReturnEvents.Initialized)
            {
                PurchaseGoodsReturnEvents Obj = new PurchaseGoodsReturnEvents();
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

        // GET: /PurchaseGoodsReturnHeaderMaster/

        public ActionResult Index(int id, string IndexType)//DocumentTypeID
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            var PurchaseGoodsReturnHeader = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(PurchaseGoodsReturnHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }

        void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.ReasonList = new ReasonService(_unitOfWork).FindByDocumentType(id).ToList();
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
        }

        // GET: /PurchaseGoodsReturnHeaderMaster/Create

        public ActionResult Create(int id)//DocuentTypeId
        {
            PrepareViewBag(id);
            PurchaseGoodsReturnHeaderViewModel vm = new PurchaseGoodsReturnHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;
            //Getting Settings
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseGoodsReturn", "PurchaseGoodsReceiptSetting", new { id = id }).Warning("Please create goods return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            vm.PurchaseGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseGoodsReturnHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);

            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseGoodsReturnHeaderViewModel vm)
        {
            PurchaseGoodsReturnHeader pt = AutoMapper.Mapper.Map<PurchaseGoodsReturnHeaderViewModel, PurchaseGoodsReturnHeader>(vm);

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (vm.PurchaseGoodsReturnHeaderId <= 0)
                    BeforeSave = PurchaseGoodsReturnDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseGoodsReturnDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseGoodsReturnHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.PurchaseGoodsReturnHeaderId <= 0)
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
                if (vm.PurchaseGoodsReturnHeaderId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReturnHeader.Add(pt);
                    //_PurchaseGoodsReturnHeaderService.Create(pt);

                    try
                    {
                        PurchaseGoodsReturnDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(pt.PurchaseGoodsReturnHeaderId, EventModeConstants.Add), ref db);
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
                        PurchaseGoodsReturnDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(pt.PurchaseGoodsReturnHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseGoodsReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    //return Edit(pt.PurchaseGoodsReturnHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = pt.PurchaseGoodsReturnHeaderId });
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseGoodsReturnHeader temp = db.PurchaseGoodsReturnHeader.Find(pt.PurchaseGoodsReturnHeaderId);

                    PurchaseGoodsReturnHeader ExRec = new PurchaseGoodsReturnHeader();
                    ExRec = Mapper.Map<PurchaseGoodsReturnHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted || temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    temp.Remark = pt.Remark;
                    temp.SupplierId = pt.SupplierId;
                    temp.DocNo = pt.DocNo;
                    temp.ReasonId = pt.ReasonId;
                    temp.DocDate = pt.DocDate;
                    temp.GodownId = pt.GodownId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(temp);
                    //_PurchaseGoodsReturnHeaderService.Update(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseGoodsReturnDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseGoodsReturnHeaderId, EventModeConstants.Edit), ref db);
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
                        return View("Create", pt);
                    }

                    try
                    {
                        PurchaseGoodsReturnDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseGoodsReturnHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseGoodsReturnHeaderId,
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
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        private ActionResult Edit(int id, string IndexType)
        {
            ViewBag.IndexStatus = IndexType;
            PurchaseGoodsReturnHeaderViewModel pt = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnHeader(id);

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
                return RedirectToAction("CreatePurchaseGoodsReturn", "PurchaseGoodsReceiptSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase goods return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
            PrepareViewBag(pt.DocTypeId);
          
            ViewBag.Mode = "Edit";
            return View("Create", pt);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            PurchaseGoodsReturnHeader header = _PurchaseGoodsReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PurchaseGoodsReturnHeader header = _PurchaseGoodsReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            PurchaseGoodsReturnHeader header = _PurchaseGoodsReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PurchaseGoodsReturnHeader header = _PurchaseGoodsReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified )
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

            PurchaseGoodsReturnHeaderViewModel pt = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnHeader(id);
            PrepareViewBag(pt.DocTypeId);

            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseGoodsReturn", "PurchaseGoodsReceiptSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase goods return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseGoodsReceiptSettings = Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);

            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            #region DocTypeTimeLineValidation

            PurchaseGoodsReturnHeader s = db.PurchaseGoodsReturnHeader.Find(id);

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
                BeforeSave = PurchaseGoodsReturnDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseGoodsReturnHeader pd = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    int ActivityType;
                    pd.ReviewBy = null;
                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    PurchaseGoodsReceiptSetting Settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                    if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                    {
                        if (String.IsNullOrEmpty(Settings.SqlProcGatePass))
                            throw new Exception("Gate pass Procedure is not Created");

                        SqlParameter SqlParameterUserId = new SqlParameter("@Id", Id);
                        IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                        if (pd.GatePassHeaderId == null)
                        {
                            SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                            DocDate.SqlDbType = SqlDbType.DateTime;
                            SqlParameter Godown = new SqlParameter("@GodownId", pd.GodownId);
                            SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId);
                            GatePassHeader GPHeader = new GatePassHeader();
                            GPHeader.CreatedBy = User.Identity.Name;
                            GPHeader.CreatedDate = DateTime.Now;
                            GPHeader.DivisionId = pd.DivisionId;
                            GPHeader.DocDate = DateTime.Now.Date;
                            GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                            GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                            GPHeader.ModifiedBy = User.Identity.Name;
                            GPHeader.ModifiedDate = DateTime.Now;
                            GPHeader.Remark = pd.Remark;
                            GPHeader.PersonId = pd.SupplierId;
                            GPHeader.SiteId = pd.SiteId;
                            GPHeader.GodownId = pd.GodownId;
                            GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                            GPHeader.ReferenceDocId = pd.PurchaseGoodsReturnHeaderId;
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

                            GatePassHeader GPHeader = db.GatePassHeader.Find(pd.GatePassHeaderId ?? 0);

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


                    pd.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(pd);
                    //_PurchaseGoodsReturnHeaderService.Update(pd);

                    try
                    {
                        PurchaseGoodsReturnDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
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
                        PurchaseGoodsReturnDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PurchaseGoodsReturnHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));


                    NotifyUser(Id, ActivityTypeContants.Submitted);



                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseGoodsReturnHeaders", "PurchaseGoodsReturnHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {

                            var PendingtoSubmitCount = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " submitted successfully.");

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " submitted successfully.");

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
                BeforeSave = PurchaseGoodsReturnDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseGoodsReturnHeader pd = db.PurchaseGoodsReturnHeader.Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;

                db.PurchaseGoodsReturnHeader.Add(pd);

                try
                {
                    PurchaseGoodsReturnDocEvents.onHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
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
                    PurchaseGoodsReturnDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PurchaseGoodsReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                NotifyUser(Id, ActivityTypeContants.Submitted);

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    PurchaseGoodsReturnHeader HEader = _PurchaseGoodsReturnHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.PurchaseGoodsReturnHeaders", "PurchaseGoodsReturnHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " Reviewd successfully.");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " Reviewd successfully.");

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " Reviewd successfully.");
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Return " + pd.DocNo + " Reviewd successfully.");
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
            PurchaseGoodsReturnHeader PurchaseGoodsReturnHeader = db.PurchaseGoodsReturnHeader.Find(id);
            if (PurchaseGoodsReturnHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation

            bool TimePlanValidation = true;
            string ExceptionMsg = "";
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PurchaseGoodsReturnHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                BeforeSave = PurchaseGoodsReturnDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
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
                int? StockHeaderId = 0;
                int? GatePassHeadrId;
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = db.PurchaseGoodsReturnHeader.Find(vm.id);

                try
                {
                    PurchaseGoodsReturnDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseGoodsReturnHeader>(temp),
                });

                StockHeaderId = temp.StockHeaderId;
                GatePassHeadrId = temp.GatePassHeaderId;

                var line = (from p in db.PurchaseGoodsReturnLine
                            where p.PurchaseGoodsReturnHeaderId == vm.id
                            select p).ToList();

                var GoodsReceiptLineIds = line.Select(m => m.PurchaseGoodsReceiptLineId).ToArray();

                var GoodsRecRecords = (from p in db.PurchaseGoodsReceiptLine
                                       where GoodsReceiptLineIds.Contains(p.PurchaseGoodsReceiptLineId)
                                       select p).ToList();

                var ProdUids = GoodsRecRecords.Select(m => m.ProductUidId).ToArray();

                var ProductUidRecords = (from p in db.ProductUid
                                         where ProdUids.Contains(p.ProductUIDId)
                                         select p).ToList();

                List<int> StockIdList = new List<int>();

                new PurchaseOrderLineStatusService(_unitOfWork).DeletePurchaseQtyOnReturnMultiple(temp.PurchaseGoodsReturnHeaderId, ref db);

                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseGoodsReturnLine>(item),
                    });

                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    var Productuid = GoodsRecRecords.Where(m => m.PurchaseGoodsReceiptLineId == item.PurchaseGoodsReceiptLineId).FirstOrDefault().ProductUidId;

                    if (Productuid != null && Productuid != 0)
                    {

                        ProductUid ProductUid = ProductUidRecords.Where(m => m.ProductUIDId == Productuid).FirstOrDefault();

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
                    db.PurchaseGoodsReturnLine.Remove(item);
                    //new PurchaseGoodsReturnLineService(_unitOfWork).Delete(item.PurchaseGoodsReturnLineId);
                }

                foreach (var item in StockIdList)
                {
                    new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                }


                temp.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseGoodsReturnHeader.Remove(temp);

                //_PurchaseGoodsReturnHeaderService.Delete(vm.id);

                if (StockHeaderId != null)
                {
                    var StockHead = db.StockHeader.Find(StockHeaderId);
                    StockHead.ObjectState = Model.ObjectState.Deleted; ;
                    db.StockHeader.Remove(StockHead);
                    //new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
                }


                if (GatePassHeadrId.HasValue && GatePassHeadrId.Value > 0)
                {


                    var GatePassHEader = db.GatePassHeader.Find(GatePassHeadrId.Value);

                    var GatePassLines = (from p in db.GatePassLine
                                         where p.GatePassHeaderId == GatePassHEader.GatePassHeaderId
                                         select p).ToList();


                    foreach (var item in GatePassLines)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.GatePassLine.Remove(item);
                        //new GatePassLineService(_unitOfWork).Delete(item.GatePassLineId);
                    }

                    GatePassHEader.ObjectState = Model.ObjectState.Deleted;
                    db.GatePassHeader.Remove(GatePassHEader);
                    //new GatePassHeaderService(_unitOfWork).Delete(GatePassHEader);

                }


                //IEnumerable<PurchaseGoodsReturnLine> PurchaseGoodsReturnLineList = new PurchaseGoodsReturnHeaderService(_unitOfWork).GetPurchaseGoodsReturnLineList(vm.id);          

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
                    PurchaseGoodsReturnDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseGoodsReturnHeaders", "PurchaseGoodsReturnHeaderId", PrevNextConstants.Next);
            return Edit(nextId,"");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseGoodsReturnHeaders", "PurchaseGoodsReturnHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId,"");
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

                        var pd = db.PurchaseGoodsReturnHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PurchaseGoodsReturnHeaderId,
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


        public ActionResult Import(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            PurchaseGoodsReturnHeaderViewModel vm = new PurchaseGoodsReturnHeaderViewModel();

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
            PurchaseGoodsReturnHeader Header = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(Id);
            PurchaseGoodsReceiptSetting PurchaseGoodsReturnSettings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
            DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
            DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);

            new NotifyUserController(_unitOfWork).SendEmailMessage(Id, ActivityType, DocEmailContentSettings, PurchaseGoodsReturnSettings.SqlProcDocumentPrint);
            new NotifyUserController(_unitOfWork).SendNotificationMessage(Id, ActivityType, DocNotificationContentSettings, User.Identity.Name);
            new NotifyUserController(_unitOfWork).SendSmsMessage(Id, ActivityType, DocSmsContentSettings);

        }


        public int PendingToSubmitCount(int id)
        {
            return (_PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PurchaseGoodsReturnHeaderService.GetPurchaseGoodsReturnPendingToReview(id, User.Identity.Name)).Count();
        }




        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;

                var Settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string PurchaseGoodsReturnHeaderIds = "";

                try
                {
                    if (!string.IsNullOrEmpty(Settings.SqlProcGatePass))
                        foreach (var item in Ids.Split(',').Select(Int32.Parse))
                        {

                            var pd = db.PurchaseGoodsReturnHeader.Find(item);

                            if ((pd.Status == (int)StatusConstants.Submitted) && !pd.GatePassHeaderId.HasValue)
                            {

                                SqlParameter SqlParameterUserId = new SqlParameter("@Id", item);
                                IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                if (pd.SupplierId != null)
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
                                        GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                        GPHeader.DocTypeId = GatePassDocTypeID;
                                        GPHeader.ModifiedBy = User.Identity.Name;
                                        GPHeader.ModifiedDate = DateTime.Now;
                                        GPHeader.Remark = pd.Remark;
                                        GPHeader.PersonId = (int)pd.SupplierId;
                                        GPHeader.SiteId = pd.SiteId;
                                        GPHeader.GodownId = (int)pd.GodownId;
                                        GPHeader.ReferenceDocTypeId = pd.DocTypeId;
                                        GPHeader.ReferenceDocId = pd.PurchaseGoodsReturnHeaderId;
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
                                        db.PurchaseGoodsReturnHeader.Add(pd);

                                        PurchaseGoodsReturnHeaderIds += pd.PurchaseGoodsReturnHeaderId + ", ";
                                    }

                                }
                                db.SaveChanges();
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
                    Narration = "GatePass created for Goods Return " + PurchaseGoodsReturnHeaderIds,
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

                    var pd = db.PurchaseGoodsReturnHeader.Find(Id);

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
                        db.PurchaseGoodsReturnHeader.Add(pd);

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

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _PurchaseGoodsReturnHeaderService.GetCustomPerson(filter, searchTerm);
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
