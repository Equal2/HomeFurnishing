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
using System.Configuration;
using System.Data.SqlClient;
using System.Xml.Linq;
using DocumentEvents;
using PurchaseInvoiceReturnDocumentEvents;
using CustomEventArgs;
using Reports.Controllers;
using Reports.Reports;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseInvoiceReturnHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseInvoiceReturnHeaderService _PurchaseInvoiceReturnHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseInvoiceReturnHeaderController(IPurchaseInvoiceReturnHeaderService PurchaseInvoiceReturnHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseInvoiceReturnHeaderService = PurchaseInvoiceReturnHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PurchaseInvoiceReturnEvents.Initialized)
            {
                PurchaseInvoiceReturnEvents Obj = new PurchaseInvoiceReturnEvents();
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

        // GET: /PurchaseInvoiceReturnHeaderMaster/

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

            var PurchaseInvoiceReturnHeader = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(PurchaseInvoiceReturnHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnPendingToReview(id, User.Identity.Name);
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
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList().ToList();
        }

        // GET: /PurchaseInvoiceReturnHeaderMaster/Create

        public ActionResult Create(int id)//DocuentTypeId
        {
            PrepareViewBag(id);
            PurchaseInvoiceReturnHeaderViewModel vm = new PurchaseInvoiceReturnHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseInvoiceReturn", "PurchaseInvoiceSetting", new { id = id }).Warning("Please create Purchase invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            vm.CalculateDiscountOnRate = settings.CalculateDiscountOnRate;
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseInvoiceReturnHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);

            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseInvoiceReturnHeaderViewModel vm)
        {
            PurchaseInvoiceReturnHeader pt = AutoMapper.Mapper.Map<PurchaseInvoiceReturnHeaderViewModel, PurchaseInvoiceReturnHeader>(vm);

            if (vm.GodownId <= 0)
                ModelState.AddModelError("GodownId", "The Godown field is required");

            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (vm.PurchaseInvoiceReturnHeaderId <= 0)
                    BeforeSave = PurchaseInvoiceReturnDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseInvoiceReturnDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.PurchaseInvoiceReturnHeaderId <= 0)
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
                if (vm.PurchaseInvoiceReturnHeaderId <= 0)
                {

                    PurchaseGoodsReturnHeader GoodsRet = Mapper.Map<PurchaseInvoiceReturnHeaderViewModel, PurchaseGoodsReturnHeader>(vm);

                    GoodsRet.DocTypeId = vm.PurchaseInvoiceSettings.DocTypeGoodsReturnId ?? 0;
                    GoodsRet.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseGoodsReturnHeaders", GoodsRet.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
                    GoodsRet.CreatedDate = DateTime.Now;
                    GoodsRet.ModifiedDate = DateTime.Now;
                    GoodsRet.CreatedBy = User.Identity.Name;
                    GoodsRet.ModifiedBy = User.Identity.Name;
                    GoodsRet.Status = (int)StatusConstants.System;
                    GoodsRet.ObjectState = Model.ObjectState.Added;
                    db.PurchaseGoodsReturnHeader.Add(GoodsRet);
                    //new PurchaseGoodsReturnHeaderService(_unitOfWork).Create(GoodsRet);

                    pt.CalculateDiscountOnRate = vm.CalculateDiscountOnRate;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.PurchaseGoodsReturnHeaderId = GoodsRet.PurchaseGoodsReturnHeaderId;
                    pt.ObjectState = Model.ObjectState.Added;
                    //_PurchaseInvoiceReturnHeaderService.Create(pt);
                    db.PurchaseInvoiceReturnHeader.Add(pt);

                    try
                    {
                        PurchaseInvoiceReturnDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(pt.PurchaseInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
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
                        PurchaseInvoiceReturnDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(pt.PurchaseInvoiceReturnHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseInvoiceReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    //return Edit(pt.PurchaseInvoiceReturnHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = pt.PurchaseInvoiceReturnHeaderId }).Success("Data saved Successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseInvoiceReturnHeader temp = _PurchaseInvoiceReturnHeaderService.Find(pt.PurchaseInvoiceReturnHeaderId);

                    PurchaseInvoiceReturnHeader ExRec = new PurchaseInvoiceReturnHeader();
                    ExRec = Mapper.Map<PurchaseInvoiceReturnHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;


                    temp.CurrencyId = pt.CurrencyId;
                    temp.SalesTaxGroupId = pt.SalesTaxGroupId;
                    temp.Remark = pt.Remark;
                    temp.SupplierId = pt.SupplierId;
                    temp.DocNo = pt.DocNo;
                    temp.ReasonId = pt.ReasonId;
                    temp.DocDate = pt.DocDate;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_PurchaseInvoiceReturnHeaderService.Update(temp);
                    db.PurchaseInvoiceReturnHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    if (temp.PurchaseGoodsReturnHeaderId.HasValue)
                    {
                        PurchaseGoodsReturnHeader GoodsRet = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(temp.PurchaseGoodsReturnHeaderId ?? 0);

                        PurchaseGoodsReturnHeader ExRecR = new PurchaseGoodsReturnHeader();
                        ExRecR = Mapper.Map<PurchaseGoodsReturnHeader>(GoodsRet);

                        GoodsRet.DocDate = temp.DocDate;
                        //GoodsRet.DocNo = temp.DocNo;
                        GoodsRet.ReasonId = temp.ReasonId;
                        GoodsRet.SupplierId = temp.SupplierId;
                        GoodsRet.Remark = temp.Remark;
                        GoodsRet.GodownId = vm.GodownId;
                        //GoodsRet.Status = temp.Status;

                        GoodsRet.ObjectState = Model.ObjectState.Modified;
                        //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(GoodsRet);
                        db.PurchaseGoodsReturnHeader.Add(GoodsRet);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecR,
                            Obj = GoodsRet,
                        });

                    }

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseInvoiceReturnDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
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
                        PurchaseInvoiceReturnDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseInvoiceReturnHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseInvoiceReturnHeaderId,
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
            PurchaseInvoiceReturnHeaderViewModel pt = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnHeader(id);

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

            PrepareViewBag(pt.DocTypeId);
            pt.GodownId = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(pt.PurchaseGoodsReturnHeaderId ?? 0).GodownId;

            if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }
            //Job Order Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseInvoiceReturn", "PurchaseInvoiceSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseInvoiceReturnHeaderId,
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
            PurchaseInvoiceReturnHeader header = _PurchaseInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PurchaseInvoiceReturnHeader header = _PurchaseInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            PurchaseInvoiceReturnHeader header = _PurchaseInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PurchaseInvoiceReturnHeader header = _PurchaseInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
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

            PurchaseInvoiceReturnHeaderViewModel pt = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnHeader(id);
            pt.GodownId = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(pt.PurchaseGoodsReturnHeaderId ?? 0).GodownId;
            //Job Order Settings
            var settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreatePurchaseInvoiceReturn", "PurchaseInvoiceSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseInvoiceSettings = Mapper.Map<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>(settings);
            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseInvoiceReturnHeaderId,
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

            PurchaseInvoiceReturnHeader s = db.PurchaseInvoiceReturnHeader.Find(id);

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
                BeforeSave = PurchaseInvoiceReturnDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseInvoiceReturnHeader pd = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {

                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    pd.ReviewBy = null;
                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    if (pd.PurchaseGoodsReturnHeaderId.HasValue)
                    {

                        PurchaseGoodsReturnHeader GoodsRet = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(pd.PurchaseGoodsReturnHeaderId ?? 0);

                        //GoodsRet.Status = pd.Status;

                        PurchaseInvoiceSetting Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                        if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                        {
                            if (String.IsNullOrEmpty(Settings.SqlProcGatePass))
                                throw new Exception("Gate pass Procedure is not Registered");

                            SqlParameter SqlParameterUserId = new SqlParameter("@Id", GoodsRet.PurchaseGoodsReturnHeaderId);
                            IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                            if (GoodsRet.GatePassHeaderId == null)
                            {
                                SqlParameter DocDate = new SqlParameter("@DocDate", pd.DocDate);
                                DocDate.SqlDbType = SqlDbType.DateTime;
                                SqlParameter Godown = new SqlParameter("@GodownId", GoodsRet.GodownId);
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
                                GPHeader.PersonId = pd.SupplierId;
                                GPHeader.SiteId = pd.SiteId;
                                GPHeader.GodownId = GoodsRet.GodownId;
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

                                GoodsRet.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }
                            else
                            {
                                List<GatePassLine> LineList = (from p in db.GatePassLine
                                                               where p.GatePassHeaderId == GoodsRet.GatePassHeaderId
                                                               select p).ToList();

                                foreach (var ittem in LineList)
                                {
                                    ittem.ObjectState = Model.ObjectState.Deleted;
                                    db.GatePassLine.Remove(ittem);
                                    //new GatePassLineService(_unitOfWork).Delete(ittem);
                                }

                                GatePassHeader GPHeader = new GatePassHeaderService(_unitOfWork).Find(GoodsRet.GatePassHeaderId ?? 0);

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

                                GoodsRet.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }

                            GoodsRet.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseGoodsReturnHeader.Add(GoodsRet);
                            //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(GoodsRet);

                        }
                    }

                    //_PurchaseInvoiceReturnHeaderService.Update(pd);


                    #region "Ledger Posting"

                    LedgerHeaderViewModel LedgerHeaderViewModel = new LedgerHeaderViewModel();

                    LedgerHeaderViewModel.LedgerHeaderId = pd.LedgerHeaderId ?? 0;
                    LedgerHeaderViewModel.DocTypeId = pd.DocTypeId;
                    LedgerHeaderViewModel.DocDate = pd.DocDate;
                    LedgerHeaderViewModel.DocNo = pd.DocNo;
                    LedgerHeaderViewModel.DivisionId = pd.DivisionId;
                    LedgerHeaderViewModel.SiteId = pd.SiteId;
                    LedgerHeaderViewModel.Narration = "";
                    LedgerHeaderViewModel.Remark = pd.Remark;
                    LedgerHeaderViewModel.ExchangeRate = pd.ExchangeRate;
                    LedgerHeaderViewModel.CreatedBy = pd.CreatedBy;
                    LedgerHeaderViewModel.CreatedDate = DateTime.Now.Date;
                    LedgerHeaderViewModel.ModifiedBy = pd.ModifiedBy;
                    LedgerHeaderViewModel.ModifiedDate = DateTime.Now.Date;

                    IEnumerable<PurchaseInvoiceReturnHeaderCharge> PurchaseInvoiceReturnHeaderCharges = from H in db.PurchaseInvoiceReturnHeaderCharge where H.HeaderTableId == Id select H;
                    IEnumerable<PurchaseInvoiceReturnLineCharge> PurchaseInvoiceReturnLineCharges = from L in db.PurchaseInvoiceReturnLineCharge where L.HeaderTableId == Id select L;

                    try
                    {
                        new CalculationService(_unitOfWork).LedgerPostingDB(ref LedgerHeaderViewModel, PurchaseInvoiceReturnHeaderCharges, PurchaseInvoiceReturnLineCharges, ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return RedirectToAction("Detail", new { id = Id, IndexType = IndexType, transactionType = "submit" });
                    }


                    if (pd.LedgerHeaderId == null)
                    {
                        pd.LedgerHeaderId = LedgerHeaderViewModel.LedgerHeaderId;
                        //_PurchaseInvoiceReturnHeaderService.Update(pd);

                    }

                    pd.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseInvoiceReturnHeader.Add(pd);

                    #endregion
                    try
                    {
                        PurchaseInvoiceReturnDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
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
                        PurchaseInvoiceReturnDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PurchaseInvoiceReturnHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseInvoiceReturnHeaders", "PurchaseInvoiceReturnHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Invioce Return " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Invioce Return " + pd.DocNo + " submitted successfully.");

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Invoice Return " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Purchase Invioce Return " + pd.DocNo + " submitted successfully.");
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
                BeforeSave = PurchaseInvoiceReturnDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseInvoiceReturnHeader pd = new PurchaseInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.PurchaseInvoiceReturnHeader.Add(pd);

                if (pd.PurchaseGoodsReturnHeaderId.HasValue)
                {
                    PurchaseGoodsReturnHeader GoodsRet = new PurchaseGoodsReturnHeaderService(_unitOfWork).Find(pd.PurchaseGoodsReturnHeaderId ?? 0);
                    //GoodsRet.Status = pd.Status;
                    //new PurchaseGoodsReturnHeaderService(_unitOfWork).Update(GoodsRet);
                    GoodsRet.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseGoodsReturnHeader.Add(GoodsRet);
                }

                try
                {
                    PurchaseInvoiceReturnDocEvents.onHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
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
                    PurchaseInvoiceReturnDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PurchaseInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    PurchaseInvoiceReturnHeader HEader = _PurchaseInvoiceReturnHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.PurchaseInvoiceReturnHeaders", "PurchaseInvoiceReturnHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
                    }
                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType }).Success("Record Reviewed Successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record Reviewed Successfully.");
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
            PurchaseInvoiceReturnHeader PurchaseInvoiceReturnHeader = db.PurchaseInvoiceReturnHeader.Find(id);
            if (PurchaseInvoiceReturnHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PurchaseInvoiceReturnHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                BeforeSave = PurchaseInvoiceReturnDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
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

                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = db.PurchaseInvoiceReturnHeader.Find(vm.id);
                var PurchaseGoodsRetId = temp.PurchaseGoodsReturnHeaderId;

                try
                {
                    PurchaseInvoiceReturnDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseInvoiceReturnHeader>(temp),
                });

                var line = (from p in db.PurchaseInvoiceReturnLine
                            where p.PurchaseInvoiceReturnHeaderId == vm.id
                            select p).ToList();

                var InvRetLineIds = line.Select(m => m.PurchaseInvoiceReturnLineId).ToArray();

                var InvRetLineCharges = (from p in db.PurchaseInvoiceReturnLineCharge
                                         where InvRetLineIds.Contains(p.LineTableId)
                                         select p).ToList();

                foreach (var item in InvRetLineCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseInvoiceReturnLineCharge.Remove(item);
                }


                foreach (var item in line)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseInvoiceReturnLine>(item),
                    });

                    //var linecharges = new PurchaseInvoiceReturnLineChargeService(_unitOfWork).GetCalculationProductList(item.PurchaseInvoiceReturnLineId);

                    //foreach (var citem in linecharges)
                    //{
                    //    new PurchaseInvoiceReturnLineChargeService(_unitOfWork).Delete(citem.Id);
                    //}

                    //new PurchaseInvoiceReturnLineService(_unitOfWork).Delete(item.PurchaseInvoiceReturnLineId);
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseInvoiceReturnLine.Remove(item);
                }

                var headercharges = (from p in db.PurchaseInvoiceReturnHeaderCharge
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();

                foreach (var item in headercharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseInvoiceReturnHeaderCharge.Remove(item);
                    //new PurchaseInvoiceReturnHeaderChargeService(_unitOfWork).Delete(item.Id);
                }

                //_PurchaseInvoiceReturnHeaderService.Delete(vm.id);
                temp.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseInvoiceReturnHeader.Remove(temp);


                List<int> StockIdList = new List<int>();

                if (PurchaseGoodsRetId.HasValue)
                {
                    var Purchasegoodsreturn = db.PurchaseGoodsReturnHeader.Find(PurchaseGoodsRetId ?? 0);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseGoodsReturnHeader>(Purchasegoodsreturn),
                    });

                    StockHeaderId = Purchasegoodsreturn.StockHeaderId;

                    var GoodsLines = (from p in db.PurchaseGoodsReturnLine
                                      where p.PurchaseGoodsReturnHeaderId == Purchasegoodsreturn.PurchaseGoodsReturnHeaderId
                                      select p).ToList();

                    foreach (var item in GoodsLines)
                    {
                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = Mapper.Map<PurchaseGoodsReturnLine>(item),
                        });

                        if (item.StockId != null)
                        {
                            StockIdList.Add((int)item.StockId);
                        }
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PurchaseGoodsReturnLine.Remove(item);
                        //new PurchaseGoodsReturnLineService(_unitOfWork).Delete(item.PurchaseGoodsReturnLineId);

                    }

                    foreach (var item in StockIdList)
                    {
                        new StockService(_unitOfWork).DeleteStockDB(item, ref db, true);
                    }


                    //new PurchaseGoodsReturnHeaderService(_unitOfWork).Delete(Purchasegoodsreturn.PurchaseGoodsReturnHeaderId);
                    Purchasegoodsreturn.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseGoodsReturnHeader.Remove(Purchasegoodsreturn);


                    if (StockHeaderId != null)
                    {
                        var StockHead = db.StockHeader.Find(StockHeaderId);
                        StockHead.ObjectState = Model.ObjectState.Deleted;
                        db.StockHeader.Remove(StockHead);
                        //new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
                    }

                    if (Purchasegoodsreturn.GatePassHeaderId.HasValue)
                    {

                        var GatePassHeader = db.GatePassHeader.Find(Purchasegoodsreturn.GatePassHeaderId);

                        var GatePassLines = (from p in db.GatePassLine
                                             where p.GatePassHeaderId == GatePassHeader.GatePassHeaderId
                                             select p).ToList();

                        foreach (var item in GatePassLines)
                        {
                            item.ObjectState = Model.ObjectState.Deleted;
                            db.GatePassLine.Remove(item);
                            //new GatePassLineService(_unitOfWork).Delete(item.GatePassLineId);
                        }

                        GatePassHeader.ObjectState = Model.ObjectState.Deleted;
                        db.GatePassHeader.Remove(GatePassHeader);
                        //new GatePassHeaderService(_unitOfWork).Delete(GatePassHeader);

                    }


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
                    PurchaseInvoiceReturnDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.PurchaseInvoiceReturnHeaderId,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseInvoiceReturnHeaders", "PurchaseInvoiceReturnHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseInvoiceReturnHeaders", "PurchaseInvoiceReturnHeaderId", PrevNextConstants.Prev);
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

                var Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.PurchaseInvoiceReturnHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PurchaseInvoiceReturnHeaderId,
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


        public int PendingToSubmitCount(int id)
        {
            return (_PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PurchaseInvoiceReturnHeaderService.GetPurchaseInvoiceReturnPendingToReview(id, User.Identity.Name)).Count();
        }




        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;

                var Settings = new PurchaseInvoiceSettingService(_unitOfWork).GetPurchaseInvoiceSettingForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string PurchaseGoodsReturnHeaderIds = "";

                try
                {
                    if (!string.IsNullOrEmpty(Settings.SqlProcGatePass))
                        foreach (var item in Ids.Split(',').Select(Int32.Parse))
                        {

                            var pd = db.PurchaseInvoiceReturnHeader.Find(item);

                            if ((pd.Status == (int)StatusConstants.Submitted) && pd.PurchaseGoodsReturnHeaderId.HasValue)
                            {

                                var GoodsRet = db.PurchaseGoodsReturnHeader.Find(pd.PurchaseGoodsReturnHeaderId);

                                if (!GoodsRet.GatePassHeaderId.HasValue)
                                {
                                    SqlParameter SqlParameterUserId = new SqlParameter("@Id", item);
                                    IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                    if (pd.SupplierId != null)
                                    {
                                        if (GoodsRet.GatePassHeaderId == null)
                                        {
                                            SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                            DocDate.SqlDbType = SqlDbType.DateTime;
                                            SqlParameter Godown = new SqlParameter("@GodownId", GoodsRet.GodownId);
                                            SqlParameter DocType = new SqlParameter("@DocTypeId", GatePassDocTypeID);
                                            GatePassHeader GPHeader = new GatePassHeader();
                                            GPHeader.CreatedBy = User.Identity.Name;
                                            GPHeader.CreatedDate = DateTime.Now;
                                            GPHeader.DivisionId = GoodsRet.DivisionId;
                                            GPHeader.DocDate = DateTime.Now.Date;
                                            GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                            GPHeader.DocTypeId = GatePassDocTypeID;
                                            GPHeader.ModifiedBy = User.Identity.Name;
                                            GPHeader.ModifiedDate = DateTime.Now;
                                            GPHeader.Remark = GoodsRet.Remark;
                                            GPHeader.PersonId = (int)pd.SupplierId;
                                            GPHeader.SiteId = GoodsRet.SiteId;
                                            GPHeader.GodownId = (int)GoodsRet.GodownId;
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

                                            GoodsRet.GatePassHeaderId = GPHeader.GatePassHeaderId;


                                            GoodsRet.ObjectState = Model.ObjectState.Modified;
                                            db.PurchaseGoodsReturnHeader.Add(GoodsRet);

                                            PurchaseGoodsReturnHeaderIds += pd.PurchaseGoodsReturnHeaderId + ", ";
                                        }

                                    }
                                    db.SaveChanges();
                                }
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
                    Narration = "GatePass created for Goods Return" + PurchaseGoodsReturnHeaderIds,
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

                    var pd = db.PurchaseInvoiceReturnHeader.Find(Id);
                    var GoodsRet = db.PurchaseGoodsReturnHeader.Find(pd.PurchaseGoodsReturnHeaderId);

                    #region DocTypeTimeLineValidation
                    try
                    {
                        DocumentUniqueId dui = Mapper.Map<DocumentUniqueId>(pd);
                        dui.GatePassHeaderId = GoodsRet.GatePassHeaderId;
                        TimePlanValidation = DocumentValidation.ValidateDocument(dui, DocumentTimePlanTypeConstants.GatePassCancel, User.Identity.Name, out ExceptionMsg, out Continue);

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

                    if (pd.PurchaseGoodsReturnHeaderId.HasValue)
                    {

                        var GatePass = db.GatePassHeader.Find(GoodsRet.GatePassHeaderId);

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
                            //GoodsRet.Status = (int)StatusConstants.Modified;
                            GoodsRet.GatePassHeaderId = null;
                            GoodsRet.ModifiedBy = User.Identity.Name;
                            GoodsRet.ModifiedDate = DateTime.Now;
                            GoodsRet.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseGoodsReturnHeader.Add(GoodsRet);

                            pd.Status = (int)StatusConstants.Modified;
                            pd.ModifiedBy = User.Identity.Name;
                            pd.ModifiedDate = DateTime.Now;
                            pd.ObjectState = Model.ObjectState.Modified;
                            db.PurchaseInvoiceReturnHeader.Add(pd);

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
