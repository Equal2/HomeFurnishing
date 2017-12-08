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
using Jobs.Helpers;
using AutoMapper;
using System.Configuration;
using Model.ViewModels;
using System.Xml.Linq;
using Reports.Controllers;
using Reports.Reports;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDeliveryOrderCancelHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        private bool EventException = false;
        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleDeliveryOrderCancelHeaderService _SaleDeliveryOrderCancelHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleDeliveryOrderCancelHeaderController(ISaleDeliveryOrderCancelHeaderService SaleDeliveryOrderCancelHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleDeliveryOrderCancelHeaderService = SaleDeliveryOrderCancelHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            //if (!SaleDeliveryOrderCancelEvents.Initialized)
            //{
            //    SaleDeliveryOrderCancelEvents Obj = new SaleDeliveryOrderCancelEvents();
            //}
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

        // GET: /SaleDeliveryOrderCancelHeaderMaster/

        public ActionResult Index(int id, string IndexType)
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            var SaleDeliveryOrderCancelHeader = _SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(SaleDeliveryOrderCancelHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelPendingToReview(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }


        public void PrepareViewBag(int id)//DocumentTypeId
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList(TransactionDocCategoryConstants.SaleDeliveryOrderCancel).ToList();
        }


        // GET: /SaleDeliveryOrderCancelHeaderMaster/Create

        public ActionResult Create(int id)//DocumentTypeId
        {
            PrepareViewBag(id);
            SaleDeliveryOrderCancelHeaderViewModel vm = new SaleDeliveryOrderCancelHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateSaleDeliveryOrderCancel", "SaleDeliveryOrderSettings", new { id = id }).Warning("Please create SaleDelivery Order Cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            vm.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);
            vm.DocDate = DateTime.Now;
            vm.DocTypeId = id;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleDeliveryOrderCancelHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleDeliveryOrderCancelHeaderViewModel vm)
        {
            #region BeforeSave
            bool BeforeSave = true;

            //try
            //{

            //    if (vm.SaleDeliveryOrderCancelHeaderId <= 0)
            //        BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeHeaderSaveEvent(this, new SaleDeliveryEventArgs(vm.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Add), ref db);
            //    else
            //        BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeHeaderSaveEvent(this, new SaleDeliveryEventArgs(vm.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Edit), ref db);

            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}


            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before save"; 
            #endregion

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.SaleDeliveryOrderCancelHeaderId <= 0)
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

            SaleDeliveryOrderCancelHeader pt = AutoMapper.Mapper.Map<SaleDeliveryOrderCancelHeaderViewModel, SaleDeliveryOrderCancelHeader>(vm);

            if (vm.BuyerId == 0)
            {
                ModelState.AddModelError("SupplierId", "The Supplier field is required");
            }

            if (ModelState.IsValid && BeforeSave && !EventException && (TimePlanValidation || Continue))
            {
                #region CreateRecord
                if (vm.SaleDeliveryOrderCancelHeaderId <= 0)
                {
                    pt.Status = (int)StatusConstants.Drafted;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    db.SaleDeliveryOrderCancelHeader.Add(pt);
                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.onHeaderSaveEvent(this, new SaleDeliveryEventArgs(pt.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //    EventException = true;
                    //}
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
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.afterHeaderSaveEvent(this, new SaleDeliveryEventArgs(pt.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Add), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}
                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleDeliveryOrderCancelHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    return RedirectToAction("Modify", new { id = pt.SaleDeliveryOrderCancelHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleDeliveryOrderCancelHeader temp = _SaleDeliveryOrderCancelHeaderService.Find(pt.SaleDeliveryOrderCancelHeaderId);

                    SaleDeliveryOrderCancelHeader ExRec = new SaleDeliveryOrderCancelHeader();
                    ExRec = Mapper.Map<SaleDeliveryOrderCancelHeader>(temp);

                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted && temp.Status != (int)StatusConstants.Import)
                        temp.Status = (int)StatusConstants.Modified;

                    temp.DocDate = pt.DocDate;
                    temp.DocNo = pt.DocNo;
                    temp.ReasonId = pt.ReasonId;
                    temp.Remark = pt.Remark;
                    temp.SiteId = pt.SiteId;
                    temp.BuyerId = pt.BuyerId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    //_SaleDeliveryOrderCancelHeaderService.Update(temp);
                    db.SaleDeliveryOrderCancelHeader.Add(temp);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.onHeaderSaveEvent(this, new SaleDeliveryEventArgs(temp.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Edit), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //    EventException = true;
                    //}

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
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", pt);
                    }

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.afterHeaderSaveEvent(this, new SaleDeliveryEventArgs(temp.SaleDeliveryOrderCancelHeaderId, EventModeConstants.Edit), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}
                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleDeliveryOrderCancelHeaderId,
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

            SaleDeliveryOrderCancelHeader pt = _SaleDeliveryOrderCancelHeaderService.Find(id);

            if (pt == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, pt.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            SaleDeliveryOrderCancelHeaderViewModel temp = AutoMapper.Mapper.Map<SaleDeliveryOrderCancelHeader, SaleDeliveryOrderCancelHeaderViewModel>(pt);

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
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateSaleDeliveryOrderCancel", "SaleDeliveryOrderSettings", new { id = pt.DocTypeId }).Warning("Please create SaleDelivery Order Cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);
            PrepareViewBag(pt.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.SaleDeliveryOrderCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", temp);
        }

        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Approve(int id, string IndexType)
        {
            SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
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

            SaleDeliveryOrderCancelHeader pt = _SaleDeliveryOrderCancelHeaderService.Find(id);
            SaleDeliveryOrderCancelHeaderViewModel temp = AutoMapper.Mapper.Map<SaleDeliveryOrderCancelHeader, SaleDeliveryOrderCancelHeaderViewModel>(pt);

            //Job Order Settings
            var settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateSaleDeliveryOrderCancel", "SaleDeliveryOrderSettings", new { id = pt.DocTypeId }).Warning("Please create SaleDelivery Order Cancel settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            temp.SaleDeliveryOrderSettings = Mapper.Map<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>(settings);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.SaleDeliveryOrderCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));

            return View("Create", temp);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            SaleDeliveryOrderCancelHeader s = db.SaleDeliveryOrderCancelHeader.Find(id);
            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation

            

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
            //try
            //{
            //    BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeHeaderSubmitEvent(this, new SaleDeliveryEventArgs(Id), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            SaleDeliveryOrderCancelHeader pd = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {                
                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;
                    pd.ReviewBy = null;
                    pd.ObjectState = Model.ObjectState.Modified;

                    db.SaleDeliveryOrderCancelHeader.Add(pd);

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.onHeaderSubmitEvent(this, new SaleDeliveryEventArgs(Id), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //    EventException = true;
                    //}

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

                    //try
                    //{
                    //    SaleDeliveryOrderCancelDocEvents.afterHeaderSubmitEvent(this, new SaleDeliveryEventArgs(Id), ref db);
                    //}
                    //catch (Exception ex)
                    //{
                    //    string message = _exception.HandleException(ex);
                    //    TempData["CSEXC"] += message;
                    //}

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.SaleDeliveryOrderCancelHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.SaleDeliveryOrderCancelHeaders", "SaleDeliveryOrderCancelHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = _SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType }).Success("SaleDelivery Order Cancel " + pd.DocNo + " submitted successfully.");
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("SaleDelivery Order Cancel " + pd.DocNo + " submitted successfully.");

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("SaleDelivery Order Cancel" + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("SaleDelivery Order Cancel " + pd.DocNo + " submitted successfully.");
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
            //try
            //{
            //    BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeHeaderReviewEvent(this, new SaleDeliveryEventArgs(Id), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";

            SaleDeliveryOrderCancelHeader pd = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                pd.ObjectState = Model.ObjectState.Modified;
                db.SaleDeliveryOrderCancelHeader.Add(pd);

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.onHeaderReviewEvent(this, new SaleDeliveryEventArgs(Id), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //    EventException = true;
                //}

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


                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.afterHeaderReviewEvent(this, new SaleDeliveryEventArgs(Id), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.SaleDeliveryOrderCancelHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));
             
                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    SaleDeliveryOrderCancelHeader HEader = _SaleDeliveryOrderCancelHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.SaleDeliveryOrderCancelHeaders", "SaleDeliveryOrderCancelHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
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
            SaleDeliveryOrderCancelHeader SaleDeliveryOrderCancelHeader = db.SaleDeliveryOrderCancelHeader.Find(id);
            if (SaleDeliveryOrderCancelHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, SaleDeliveryOrderCancelHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleDeliveryOrderCancelHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
            //try
            //{
            //    BeforeSave = SaleDeliveryOrderCancelDocEvents.beforeHeaderDeleteEvent(this, new SaleDeliveryEventArgs(vm.id), ref db);
            //}
            //catch (Exception ex)
            //{
            //    string message = _exception.HandleException(ex);
            //    TempData["CSEXC"] += message;
            //    EventException = true;
            //}

            if (!BeforeSave)
                TempData["CSEXC"] += "Failed validation before delete";

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                var temp = db.SaleDeliveryOrderCancelHeader.Find(vm.id);

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.onHeaderDeleteEvent(this, new SaleDeliveryEventArgs(vm.id), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //    EventException = true;
                //}

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = Mapper.Map<SaleDeliveryOrderCancelHeader>(temp),
                });

                //var lines = new SaleDeliveryOrderCancelLineService(_unitOfWork).GetSaleDeliveryOrderCancelLineForHeader(vm.id);

                var lines = (from p in db.SaleDeliveryOrderCancelLine
                             where p.SaleDeliveryOrderCancelHeaderId == vm.id
                             select p).ToList();

                //new SaleDeliveryOrderLineStatusService(_unitOfWork).DeleteSaleDeliveryQtyOnCancelMultiple(vm.id, ref db);

                foreach (var item in lines)
                {
                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = Mapper.Map<SaleDeliveryOrderCancelLine>(item),
                    });

                    item.ObjectState = Model.ObjectState.Deleted;
                    db.SaleDeliveryOrderCancelLine.Remove(item);
                    //new SaleDeliveryOrderCancelLineService(_unitOfWork).Delete(item.SaleDeliveryOrderCancelLineId);
                }


                //_SaleDeliveryOrderCancelHeaderService.Delete(vm.id);
                temp.ObjectState = Model.ObjectState.Deleted;
                db.SaleDeliveryOrderCancelHeader.Remove(temp);

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

                //try
                //{
                //    SaleDeliveryOrderCancelDocEvents.afterHeaderDeleteEvent(this, new SaleDeliveryEventArgs(vm.id), ref db);
                //}
                //catch (Exception ex)
                //{
                //    string message = _exception.HandleException(ex);
                //    TempData["CSEXC"] += message;
                //}

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = temp.DocTypeId,
                    DocId = temp.SaleDeliveryOrderCancelHeaderId,
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleDeliveryOrderCancelHeaders", "SaleDeliveryOrderCancelHeaderId", PrevNextConstants.Next);
            return Edit(nextId,"");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleDeliveryOrderCancelHeaders", "SaleDeliveryOrderCancelHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId,"");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult PrintOut(int id, string SqlProcForPrint)
        {
            String query = SqlProcForPrint;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);
        }

        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = db.JobOrderHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.JobOrderHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Import || pd.Status == (int)StatusConstants.Modified)
                        {
                            //LogAct(item.ToString());
                            Pdf = drp.DirectDocumentPrint("", User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
                        {
                            Pdf = drp.DirectDocumentPrint("", User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else
                        {
                            Pdf = drp.DirectDocumentPrint("", User.Identity.Name, item);
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

        //[HttpGet]
        //public ActionResult Print(int id)
        //{
        //    SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
        //    if (header.Status == (int)StatusConstants.Drafted)
        //    {
        //        var SEttings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
        //        if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint))
        //            throw new Exception("Document Print Not Configured");
        //        else
        //            return PrintOut(id, SEttings.SqlProcDocumentPrint);
        //    }
        //    else
        //        return HttpNotFound();
        //}

        //[HttpGet]
        //public ActionResult PrintAfter_Submit(int id)
        //{
        //    SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
        //    if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
        //    {
        //        var SEttings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
        //        if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint))
        //            throw new Exception("Document Print Not Configured");
        //        else
        //            return PrintOut(id, SEttings.SqlProcDocumentPrint_AfterSubmit);
        //    }
        //    else
        //        return HttpNotFound();
        //}

        //[HttpGet]
        //public ActionResult PrintAfter_Approve(int id)
        //{

        //    SaleDeliveryOrderCancelHeader header = _SaleDeliveryOrderCancelHeaderService.Find(id);
        //    if (header.Status == (int)StatusConstants.Approved)
        //    {
        //        var SEttings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
        //        if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint_AfterApprove))
        //            throw new Exception("Document Print Not Configured");
        //        else
        //            return PrintOut(id, SEttings.SqlProcDocumentPrint_AfterApprove);
        //    }
        //    else
        //        return HttpNotFound();
        //}

        //private void NotifyUser(int Id, ActivityTypeContants ActivityType)
        //{
        //    SaleDeliveryOrderCancelHeader Header = new SaleDeliveryOrderCancelHeaderService(_unitOfWork).Find(Id);
        //    SaleDeliveryOrderSettings SaleDeliveryOrderCancelSettings = new SaleDeliveryOrderSettingsService(_unitOfWork).GetSaleDeliveryOrderSettingsForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId);

        //    DocEmailContent DocEmailContentSettings = new DocEmailContentService(_unitOfWork).GetDocEmailContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    DocNotificationContent DocNotificationContentSettings = new DocNotificationContentService(_unitOfWork).GetDocNotificationContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);
        //    DocSmsContent DocSmsContentSettings = new DocSmsContentService(_unitOfWork).GetDocSmsContentForDocument(Header.DocTypeId, Header.DivisionId, Header.SiteId, ActivityType);

        //    new NotifyUserController(_unitOfWork).SendEmailMessage(Id, ActivityType, DocEmailContentSettings, SaleDeliveryOrderCancelSettings.SqlProcDocumentPrint);
        //    new NotifyUserController(_unitOfWork).SendNotificationMessage(Id, ActivityType, DocNotificationContentSettings, User.Identity.Name);
        //    new NotifyUserController(_unitOfWork).SendSmsMessage(Id, ActivityType, DocSmsContentSettings);
        //}

        public int PendingToSubmitCount(int id)
        {
            return (_SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleDeliveryOrderCancelHeaderService.GetSaleDeliveryOrderCancelPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDeliveryOrderCancelHeaderService.GetCustomPerson(filter, searchTerm);
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
