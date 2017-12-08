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
using System.Configuration;
using Jobs.Helpers;
using System.Xml.Linq;
using PurchaseOrderDocumentEvents;
using CustomEventArgs;
using DocumentEvents;
using Reports.Controllers;
using Reports.Reports;
using Model.ViewModels;


namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();


        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPurchaseOrderHeaderService _PurchaseOrderHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;


        public PurchaseOrderHeaderController(IPurchaseOrderHeaderService PurchaseOrderHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseOrderHeaderService = PurchaseOrderHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PurchaseOrderEvents.Initialized)
            {
                PurchaseOrderEvents Obj = new PurchaseOrderEvents();
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
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
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
            string name = User.Identity.Name;
            var PurchaseOrderHeader = _PurchaseOrderHeaderService.GetPurchaseOrderHeaderList(id, name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(PurchaseOrderHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PurchaseOrderHeaderService.GetPurchaseOrderPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PurchaseOrderHeaderService.GetPurchaseOrderPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        // GET: /PurchaseOrderHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeID
        {
            PurchaseOrderHeaderViewModel vm = new PurchaseOrderHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseOrderSetting", new { id = id }).Warning("Please create Purchase Order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            vm.TermsAndConditions = settings.TermsAndConditions;
            vm.CalculateDiscountOnRate = settings.CalculateDiscountOnRate;
            vm.DocTypeId = id;
            //vm.DocNo = _PurchaseOrderHeaderService.GetMaxDocNo();
            vm.DocDate = DateTime.Now;
            vm.DueDate = DateTime.Now;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PurchaseOrderHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            PrepareViewBag(id);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseOrderHeaderViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;
            try
            {
                if (vm.PurchaseOrderHeaderId <= 0)
                    BeforeSave = PurchaseOrderDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseOrderHeaderId, EventModeConstants.Add), ref db);
                else
                    BeforeSave = PurchaseOrderDocEvents.beforeHeaderSaveEvent(this, new PurchaseEventArgs(vm.PurchaseOrderHeaderId, EventModeConstants.Edit), ref db);
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

                if (vm.PurchaseOrderHeaderId <= 0)
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
                if (vm.PurchaseOrderHeaderId <= 0)
                {

                    PurchaseOrderHeader header = new PurchaseOrderHeader();
                    header = Mapper.Map<PurchaseOrderHeaderViewModel, PurchaseOrderHeader>(vm);
                    header.CalculateDiscountOnRate = vm.CalculateDiscountOnRate;
                    header.CreatedBy = User.Identity.Name;
                    header.ActualDueDate = vm.DueDate;
                    header.CreatedDate = DateTime.Now;
                    header.ModifiedBy = User.Identity.Name;
                    header.ModifiedDate = DateTime.Now;

                    header.ObjectState = Model.ObjectState.Added;
                    //_PurchaseOrderHeaderService.Create(header);
                    db.PurchaseOrderHeader.Add(header);

                    new PurchaseOrderHeaderStatusService(_unitOfWork).CreateHeaderStatus(header.PurchaseOrderHeaderId, ref db);

                    try
                    {
                        PurchaseOrderDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(header.PurchaseOrderHeaderId, EventModeConstants.Add), ref db);
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
                        PurchaseOrderDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(header.PurchaseOrderHeaderId, EventModeConstants.Add), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = header.DocTypeId,
                        DocId = header.PurchaseOrderHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = header.DocNo,
                        DocDate = header.DocDate,
                        DocStatus = header.Status,
                    }));

                    return RedirectToAction("Modify", new { id = header.PurchaseOrderHeaderId }).Success("Data saved successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseOrderHeader temp = _PurchaseOrderHeaderService.Find(vm.PurchaseOrderHeaderId);

                    PurchaseOrderHeader ExRec = new PurchaseOrderHeader();
                    ExRec = Mapper.Map<PurchaseOrderHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import) 
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocNo = vm.DocNo;
                    temp.DocTypeId = vm.DocTypeId;
                    temp.DocDate = vm.DocDate;
                    temp.DueDate = vm.DueDate;
                    temp.SupplierId = vm.SupplierId;
                    temp.CurrencyId = vm.CurrencyId;
                    temp.SalesTaxGroupPersonId = vm.SalesTaxGroupPersonId;
                    temp.DeliveryTermsId = vm.DeliveryTermsId;
                    temp.ShipMethodId = vm.ShipMethodId;
                    temp.ShipAddress = vm.ShipAddress;
                    temp.UnitConversionForId = vm.UnitConversionForId;
                    temp.TermsAndConditions = vm.TermsAndConditions;
                    temp.CreditDays = vm.CreditDays;
                    temp.Remark = vm.Remark;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_PurchaseOrderHeaderService.Update(temp);
                    db.PurchaseOrderHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        PurchaseOrderDocEvents.onHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseOrderHeaderId, EventModeConstants.Edit), ref db);
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
                        ViewBag.Mode = "Edit";
                        PrepareViewBag(vm.DocTypeId);
                        return View("Create", vm);
                    }

                    try
                    {
                        PurchaseOrderDocEvents.afterHeaderSaveEvent(this, new PurchaseEventArgs(temp.PurchaseOrderHeaderId, EventModeConstants.Edit), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseOrderHeaderId,
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

            PurchaseOrderHeaderViewModel pt = _PurchaseOrderHeaderService.GetPurchaseOrderHeader(id);

            PrepareViewBag(pt.DocTypeId);
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
            //Job Order Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PurchaseOrderSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Order settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseOrderHeaderId,
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
            PurchaseOrderHeader header = _PurchaseOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PurchaseOrderHeader header = _PurchaseOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            PurchaseOrderHeader header = _PurchaseOrderHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PurchaseOrderHeader header = _PurchaseOrderHeaderService.Find(id);
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

            PurchaseOrderHeaderViewModel pt = _PurchaseOrderHeaderService.GetPurchaseOrderHeader(id);

            //Job Order Settings
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null)
            {
                return RedirectToAction("Create", "PurchaseOrderSetting", new { id = pt.DocTypeId }).Warning("Please create Purchase Order settings");
            }
            pt.PurchaseOrderSettings = Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.PurchaseOrderHeaderId,
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

            PurchaseOrderHeader s = db.PurchaseOrderHeader.Find(id);

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
                BeforeSave = PurchaseOrderDocEvents.beforeHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseOrderHeader pd = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                int Cnt = 0;
                int CountUid = 0;

                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {
                    int ActivityType;

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    //_PurchaseOrderHeaderService.Update(pd);
                    //_unitOfWork.Save();
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;
                    db.PurchaseOrderHeader.Add(pd);


                    var Lines = (from p in db.PurchaseOrderLine
                                 where p.PurchaseOrderHeaderId == pd.PurchaseOrderHeaderId
                                 select p).ToList();

                    if(Lines.Count > 0)
                    {

                        decimal Qty = Lines.Where(m => m.ProductUidHeaderId == null).Sum(m => m.Qty);

                        List<string> uids = new PurchaseOrderLineService(_unitOfWork).GetProcGenProductUids(pd.DocTypeId, Qty, pd.DivisionId, pd.SiteId);

                        foreach (var item in Lines.Where(m => m.ProductUidHeaderId == null))
                        {
                            if (uids.Count > 0)
                            {
                                ProductUidHeader ProdUidHeader = new ProductUidHeader();

                                ProdUidHeader.ProductUidHeaderId = Cnt;
                                ProdUidHeader.ProductId = item.ProductId;
                                ProdUidHeader.Dimension1Id = item.Dimension1Id;
                                ProdUidHeader.Dimension2Id = item.Dimension2Id;
                                ProdUidHeader.GenDocId = pd.PurchaseOrderHeaderId;
                                ProdUidHeader.GenDocNo = pd.DocNo;
                                ProdUidHeader.GenDocTypeId = pd.DocTypeId;
                                ProdUidHeader.GenDocDate = pd.DocDate;
                                ProdUidHeader.GenPersonId = pd.SupplierId;
                                ProdUidHeader.CreatedBy = User.Identity.Name;
                                ProdUidHeader.CreatedDate = DateTime.Now;
                                ProdUidHeader.ModifiedBy = User.Identity.Name;
                                ProdUidHeader.ModifiedDate = DateTime.Now;
                                ProdUidHeader.ObjectState = Model.ObjectState.Added;
                                //new ProductUidHeaderService(_unitOfWork).Create(ProdUidHeader);
                                db.ProductUidHeader.Add(ProdUidHeader);

                                item.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;


                                int count = 0;
                                //foreach (string UidItem in uids)
                                for (int A = 0; A < item.Qty; A++)
                                {
                                    ProductUid ProdUid = new ProductUid();

                                    ProdUid.ProductUidHeaderId = ProdUidHeader.ProductUidHeaderId;
                                    //ProdUid.ProductUidName = UidItem;
                                    ProdUid.ProductUidName = uids[CountUid];
                                    ProdUid.ProductId = item.ProductId;
                                    ProdUid.IsActive = true;
                                    ProdUid.CreatedBy = User.Identity.Name;
                                    ProdUid.CreatedDate = DateTime.Now;
                                    ProdUid.ModifiedBy = User.Identity.Name;
                                    ProdUid.ModifiedDate = DateTime.Now;
                                    ProdUid.GenLineId = item.PurchaseOrderLineId;
                                    ProdUid.GenDocId = pd.PurchaseOrderHeaderId;
                                    ProdUid.GenDocNo = pd.DocNo;
                                    ProdUid.GenDocTypeId = pd.DocTypeId;
                                    ProdUid.GenDocDate = pd.DocDate;
                                    ProdUid.GenPersonId = pd.SupplierId;
                                    ProdUid.Dimension1Id = item.Dimension1Id;
                                    ProdUid.Dimension2Id = item.Dimension2Id;
                                    ProdUid.CurrenctProcessId = null;
                                    ProdUid.Status = ProductUidStatusConstants.Issue;
                                    ProdUid.LastTransactionDocId = pd.PurchaseOrderHeaderId;
                                    ProdUid.LastTransactionDocNo = pd.DocNo;
                                    ProdUid.LastTransactionDocTypeId = pd.DocTypeId;
                                    ProdUid.LastTransactionDocDate = pd.DocDate;
                                    ProdUid.LastTransactionPersonId = pd.SupplierId;
                                    ProdUid.LastTransactionLineId = item.PurchaseOrderLineId;
                                    ProdUid.ProductUIDId = count;
                                    ProdUid.ObjectState = Model.ObjectState.Added;
                                    //new ProductUidService(_unitOfWork).Create(ProdUid);
                                    db.ProductUid.Add(ProdUid);

                                    count++;
                                    CountUid++;
                                }
                                Cnt++;
                                item.ObjectState = Model.ObjectState.Modified;
                                db.PurchaseOrderLine.Add(item);
                                //new PurchaseOrderLineService(_unitOfWork).Update(item);
                            }
                        }


                    }
                    try
                    {
                        PurchaseOrderDocEvents.onHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
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
                        PurchaseOrderDocEvents.afterHeaderSubmitEvent(this, new PurchaseEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.PurchaseOrderHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    NotifyUser(Id, ActivityTypeContants.Submitted);

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {
                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.PurchaseOrderHeaders", "PurchaseOrderHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {
                            var PendingtoSubmitCount = _PurchaseOrderHeaderService.GetPurchaseOrderPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType });
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });
                        }
                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Purchase Order " + pd.DocNo + " submitted successfully.");
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

            ViewBag.PendingToReview = PendingToReviewCount(id);
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });

        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {

            bool BeforeSave = true;
            try
            {
                BeforeSave = PurchaseOrderDocEvents.beforeHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            PurchaseOrderHeader pd = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);


            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";
                pd.ObjectState = Model.ObjectState.Modified;
                db.PurchaseOrderHeader.Add(pd);

                try
                {
                    PurchaseOrderDocEvents.onHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
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
                    PurchaseOrderDocEvents.afterHeaderReviewEvent(this, new PurchaseEventArgs(Id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.PurchaseOrderHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                NotifyUser(Id, ActivityTypeContants.Approved);


                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    PurchaseOrderHeader HEader = _PurchaseOrderHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.PurchaseOrderHeaders", "PurchaseOrderHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _PurchaseOrderHeaderService.GetPurchaseOrderPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
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
            PurchaseOrderHeader PurchaseOrderHeader = _PurchaseOrderHeaderService.Find(id);
            if (PurchaseOrderHeader == null)
            {
                return HttpNotFound();
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PurchaseOrderHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
                BeforeSave = PurchaseOrderDocEvents.beforeHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            var temp = db.PurchaseOrderHeader.Find(vm.id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                try
                {
                    PurchaseOrderDocEvents.onHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    EventException = true;
                }

                string Exception = "";

                var line = (from p in db.PurchaseOrderLine
                            where p.PurchaseOrderHeaderId == vm.id
                            select p).ToList();

                var LineIds = line.Select(m => m.PurchaseOrderLineId).ToArray();

                var LineStatus = (from p in db.PurchaseOrderLineStatus
                                  where LineIds.Contains(p.PurchaseOrderLineId.Value)
                                  select p).ToList();

                var PurchaseOrderLineCharges = (from p in db.PurchaseOrderLineCharge
                                                where LineIds.Contains(p.LineTableId)
                                                select p).ToList();

                var ProdrUidHeaderIds = line.Where(m=>m.ProductUidHeaderId!=null).Select(m => m.ProductUidHeaderId).ToArray();

                var ProductUids = (from p in db.ProductUid
                                   where ProdrUidHeaderIds.Contains(p.ProductUidHeaderId)
                                   select p).ToList();

                foreach (var item in LineStatus)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderLineStatus.Remove(item);
                }

                foreach (var item in PurchaseOrderLineCharges)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderLineCharge.Remove(item);
                }

                foreach (var item in ProductUids)
                {
                    if (item.LastTransactionDocNo == temp.DocNo && item.LastTransactionDocTypeId == temp.DocTypeId)
                    {
                        //new ProductUidService(_unitOfWork).Delete(item);
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.ProductUid.Remove(item);
                    }
                    else
                    {
                        Exception = "Record Cannot be deleted as it is in use by other documents";
                        break;
                    }
                }

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<PurchaseOrderHeader>(temp),
                });

                foreach (var item in line)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<PurchaseOrderLine>(item),
                    });

                    //new PurchaseOrderLineStatusService(_unitOfWork).Delete(item.PurchaseOrderLineId);

                    //var linecharges = new PurchaseOrderLineChargeService(_unitOfWork).GetCalculationProductList(item.PurchaseOrderLineId);
                    //foreach (var citem in linecharges)
                    //    new PurchaseOrderLineChargeService(_unitOfWork).Delete(citem.Id);


                    //new PurchaseOrderLineService(_unitOfWork).Delete(item.PurchaseOrderLineId);

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderLine.Remove(item);

                }

                var headercharges = (from p in db.PurchaseOrderHeaderCharges
                                     where p.HeaderTableId == vm.id
                                     select p).ToList();



                foreach (var item in headercharges)
                {

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderHeaderCharges.Remove(item);
                    //new PurchaseOrderHeaderChargeService(_unitOfWork).Delete(item.Id);
                }
                //new PurchaseOrderHeaderStatusService(_unitOfWork).Delete(temp.PurchaseOrderHeaderId);

                var PurchaseOrderHeaderStatus = (from p in db.PurchaseOrderHeaderStatus
                                                 where p.PurchaseOrderHeaderId == vm.id
                                                 select p).FirstOrDefault();

                if (PurchaseOrderHeaderStatus != null)
                {
                    PurchaseOrderHeaderStatus.ObjectState = Model.ObjectState.Deleted;
                    db.PurchaseOrderHeaderStatus.Remove(PurchaseOrderHeaderStatus);
                }


                temp.ObjectState = Model.ObjectState.Deleted;
                db.PurchaseOrderHeader.Remove(temp);

                //_PurchaseOrderHeaderService.Delete(vm.id);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    if (!string.IsNullOrEmpty(Exception))
                        throw new Exception(Exception);

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
                    PurchaseOrderDocEvents.afterHeaderDeleteEvent(this, new PurchaseEventArgs(vm.id), ref db);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.PurchaseOrderHeaderId,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseOrderHeaders", "PurchaseOrderHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PurchaseOrderHeaders", "PurchaseOrderHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History(int id)
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");

        }

        [HttpGet]
        public ActionResult BarcodePrint(int id)
        {

            //return RedirectToAction("PrintBarCode", "Report_BarcodePrint", new { GenHeaderId = id });
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_BarcodePrint/PrintBarCode/?GenHeaderId=" + id + "&queryString=" + id);
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

                var Settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();

                        var pd = db.PurchaseOrderHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PurchaseOrderHeaderId,
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


        public ActionResult Action_OnSubmit(int Id, int DocTypeId)//DocId
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.OnSubmitMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.OnSubmitMenuId);

                    if (menuviewmodel != null)
                    {
                        if (!string.IsNullOrEmpty(menuviewmodel.URL))
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id);
                        }
                        else
                        {
                            return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { id = DocTypeId });
        }

        public ActionResult Action_OnApprove(int Id, int DocTypeId)//DocId
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(DocTypeId, DivisionId, SiteId);

            if (settings != null)
            {
                if (settings.OnApproveMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.OnApproveMenuId);

                    if (menuviewmodel != null)
                    {
                        if (!string.IsNullOrEmpty(menuviewmodel.URL))
                        {
                            return Redirect(System.Configuration.ConfigurationManager.AppSettings[menuviewmodel.URL] + "/" + menuviewmodel.ControllerName + "/" + menuviewmodel.ActionName + "/" + Id);
                        }
                        else
                        {
                            return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { id = Id });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { id = DocTypeId });
        }

        private void NotifyUser(int Id, ActivityTypeContants ActivityType)
        {
            PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
            PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

            DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
            DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
            DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);

            new NotifyUserController(_unitOfWork).SendEmailMessage(Id, ActivityType, DocEmailContentSettings, PurchaseOrderSettings.SqlProcDocumentPrint);
            new NotifyUserController(_unitOfWork).SendNotificationMessage(Id, ActivityType, DocNotificationContentSettings, User.Identity.Name);
            new NotifyUserController(_unitOfWork).SendSmsMessage(Id, ActivityType, DocSmsContentSettings);

        }

        //private void SendEmailMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocEmailContentSettings != null)
        //    {
        //        if (DocEmailContentSettings.ProcEmailContent != null && DocEmailContentSettings.ProcEmailContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<EmailContentViewModel> MailContent = db.Database.SqlQuery<EmailContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocEmailContentSettings.ProcEmailContent + " @Id", SqlParameterId);

        //            foreach (EmailContentViewModel item in MailContent)
        //            {
        //                if (DocEmailContentSettings.AttachmentTypes != null && DocEmailContentSettings.AttachmentTypes != "")
        //                {
        //                    string[] AttachmentTypeArr = DocEmailContentSettings.AttachmentTypes.Split(',');

        //                    for (int i = 0; i <= AttachmentTypeArr.Length - 1; i++)
        //                    {
        //                        if (item.FileNameList != "" && item.FileNameList != null) { item.FileNameList = item.FileNameList + ","; }
        //                        if (AttachmentTypeArr[i].ToUpper() == "PDF")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else if (AttachmentTypeArr[i].ToUpper() == "EXCEL")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.Excel, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else if (AttachmentTypeArr[i].ToUpper() == "WORD")
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.Word, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                        else
        //                        {
        //                            item.FileNameList = item.FileNameList + ReportFiles.CreateFiles(PurchaseOrderSettings.SqlProcDocumentPrint, Id.ToString(), ReportFileTypeConstants.PDF, (string)System.Configuration.ConfigurationManager.AppSettings["CustomizePath"]);
        //                        }
        //                    }
        //                    item.EmailBody = item.EmailBody.Replace("DomainName", (string)System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"]);
        //                }

        //                SendEmail.SendEmailMsg(item);
        //            }
        //        }
        //    }
        //}

        //private void SendNotificationMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocNotificationContentSettings != null)
        //    {
        //        if (DocNotificationContentSettings.ProcNotificationContent != null && DocNotificationContentSettings.ProcNotificationContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<NotificationContentViewModel> NotificationContent = db.Database.SqlQuery<NotificationContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocNotificationContentSettings.ProcNotificationContent + " @Id", SqlParameterId);

        //            foreach (NotificationContentViewModel item in NotificationContent)
        //            {
        //                Notification Note = new Notification();
        //                if (ActivityType == ActivityTypeContants.Submitted)
        //                {
        //                    Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderSubmitted;
        //                }
        //                else
        //                {
        //                    Note.NotificationSubjectId = (int)NotificationSubjectConstants.PurchaseOrderApproved;
        //                }
        //                Note.NotificationText = item.NotificationText;
        //                Note.NotificationUrl = item.NotificationUrl;
        //                Note.UrlKey = item.UrlKey;
        //                Note.ExpiryDate = item.ExpiryDate;
        //                Note.IsActive = true;
        //                Note.CreatedBy = User.Identity.Name;
        //                Note.ModifiedBy = User.Identity.Name;
        //                Note.CreatedDate = DateTime.Now;
        //                Note.ModifiedDate = DateTime.Now;
        //                new NotificationService(_unitOfWork).Create(Note);

        //                string[] UserNameArr = item.UserNameList.Split(',');

        //                foreach (string UserName in UserNameArr)
        //                {
        //                    NotificationUser NoteUser = new NotificationUser();
        //                    NoteUser.NotificationId = Note.NotificationId;
        //                    NoteUser.UserName = UserName;
        //                    new NotificationUserService(_unitOfWork).Create(NoteUser);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void SendSmsMessage(int Id, ActivityTypeContants ActivityType)
        //{
        //    PurchaseOrderHeader Header = new PurchaseOrderHeaderService(_unitOfWork).Find(Id);
        //    DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    PurchaseOrderSetting PurchaseOrderSettings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    if (DocSmsContentSettings != null)
        //    {
        //        if (DocSmsContentSettings.ProcSmsContent != null && DocSmsContentSettings.ProcSmsContent != "")
        //        {
        //            SqlParameter SqlParameterId = new SqlParameter("@Id", Id);

        //            IEnumerable<SmsContentViewModel> SmsContent = db.Database.SqlQuery<SmsContentViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + DocSmsContentSettings.ProcSmsContent + " @Id", SqlParameterId);

        //            foreach (SmsContentViewModel item in SmsContent)
        //            {

        //            }
        //        }
        //    }
        //}

        public int PendingToSubmitCount(int id)
        {
            return (_PurchaseOrderHeaderService.GetPurchaseOrderPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PurchaseOrderHeaderService.GetPurchaseOrderPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _PurchaseOrderHeaderService.GetCustomPerson(filter, searchTerm);
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
