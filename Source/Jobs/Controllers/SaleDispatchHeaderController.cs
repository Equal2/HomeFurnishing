using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModels;
using System.Configuration;
using AutoMapper;
using Jobs.Helpers;
using Model.ViewModel;
using Reports.Controllers;
using System.Xml.Linq;
using System.Data.SqlClient;
using Reports.Reports;
using System.Data;
using SaleDispatchDocumentEvents;
using CustomEventArgs;


namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDispatchHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;
        
        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleDispatchHeaderService _SaleDispatchHeaderService;
        IPackingHeaderService _PackingHeaderService;        
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SaleDispatchHeaderController(ISaleDispatchHeaderService SaleDispatchHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IPackingHeaderService PackingHeaderService)
        {
            _SaleDispatchHeaderService = SaleDispatchHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            _PackingHeaderService = PackingHeaderService;
            if (!SaleDispatchEvents.Initialized)
            {
                SaleDispatchEvents Obj = new SaleDispatchEvents();
            }

             UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        private void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.DealUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(id,DivisionId,SiteId);
            if(settings !=null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
                ViewBag.SqlProcGatePass = settings.SqlProcGatePass;
            }
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

        public ActionResult Index(int id, string IndexType)//DocTypeId 
        {
            if (IndexType == "PTS")
            {
                return RedirectToAction("Index_PendingToSubmit", new { id });
            }
            else if (IndexType == "PTR")
            {
                return RedirectToAction("Index_PendingToReview", new { id });
            }
            IQueryable<SaleDispatchHeaderIndexViewModel> p = _SaleDispatchHeaderService.GetSaleDispatchHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _SaleDispatchHeaderService.GetSaleDispatchHeaderListPendingToSubmit(id, User.Identity.Name);

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _SaleDispatchHeaderService.GetSaleDispatchHeaderListPendingToReview(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }



        public ActionResult Create(int id)//DocTypeId
        {
            SaleDispatchHeaderViewModel vm = new SaleDispatchHeaderViewModel();

            vm.DocDate = DateTime.Now.Date;
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DocTypeId = id;
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDispatchSetting", new { id = id }).Warning("Please create Sale Dispatch settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }


            if (settings != null)
            {
                vm.ShipMethodId = settings.ShipMethodId;
                vm.DeliveryTermsId = settings.DeliveryTermsId;
                vm.GodownId = settings.GodownId;
                vm.ProcessId = settings.ProcessId;
            }

            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
            ViewBag.Mode = "Add";
            PrepareViewBag(id);

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleDispatchHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            return View("Create", vm);
        }


       



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(SaleDispatchHeaderViewModel vm)
        {

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.SaleDispatchHeaderId <= 0)
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

            if (ModelState.IsValid && (TimePlanValidation || Continue))
            {
                #region CreateRecord
                if (vm.SaleDispatchHeaderId == 0)
                {
                    PackingHeader packingHeder = Mapper.Map<SaleDispatchHeaderViewModel, PackingHeader>(vm);
                    SaleDispatchHeader saledispatchheader = Mapper.Map<SaleDispatchHeaderViewModel, SaleDispatchHeader>(vm);

                    packingHeder.BuyerId = vm.SaleToBuyerId;
                    packingHeder.CreatedBy = User.Identity.Name;
                    packingHeder.ModifiedBy = User.Identity.Name;
                    packingHeder.CreatedDate = DateTime.Now;
                    packingHeder.ModifiedDate = DateTime.Now;
                    packingHeder.ObjectState = Model.ObjectState.Added;
                    _PackingHeaderService.Create(packingHeder);


                    saledispatchheader.PackingHeaderId = packingHeder.PackingHeaderId;
                    saledispatchheader.CreatedDate = DateTime.Now;
                    saledispatchheader.ModifiedDate = DateTime.Now;
                    saledispatchheader.CreatedBy = User.Identity.Name;
                    saledispatchheader.ModifiedBy = User.Identity.Name;
                    saledispatchheader.Status = (int)StatusConstants.Drafted;
                    _SaleDispatchHeaderService.Create(saledispatchheader);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }

                     LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = saledispatchheader.DocTypeId,
                        DocId = saledispatchheader.SaleDispatchHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = saledispatchheader.DocNo,
                        DocDate = saledispatchheader.DocDate,
                        DocStatus = saledispatchheader.Status,
                    }));


                     return RedirectToAction("Modify", new { id = saledispatchheader.SaleDispatchHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    SaleDispatchHeader saledispatchheader = _SaleDispatchHeaderService.Find(vm.SaleDispatchHeaderId);

                    SaleDispatchHeader ExRec = Mapper.Map<SaleDispatchHeader>(saledispatchheader);


                    StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(saledispatchheader.StockHeaderId ?? 0);

                    PackingHeader packingHeader = _PackingHeaderService.Find(saledispatchheader.PackingHeaderId.Value);

                    int status = saledispatchheader.Status;

                    if (saledispatchheader.Status != (int)StatusConstants.Drafted)
                    {
                        saledispatchheader.Status = (int)StatusConstants.Modified;
                        packingHeader.Status = (int)StatusConstants.Modified;
                    }


                    saledispatchheader.DocNo = vm.DocNo;
                    saledispatchheader.DocDate = vm.DocDate;
                    saledispatchheader.SaleToBuyerId = vm.SaleToBuyerId;
                    saledispatchheader.Remark = vm.Remark;
                    saledispatchheader.ModifiedDate = DateTime.Now;
                    saledispatchheader.ModifiedBy = User.Identity.Name;
                    _SaleDispatchHeaderService.Update(saledispatchheader);


                    if (StockHeader != null)
                    {
                        StockHeader.DocDate = vm.DocDate;
                        StockHeader.PersonId = vm.SaleToBuyerId;
                        StockHeader.GodownId = vm.GodownId;
                        StockHeader.Remark = vm.Remark;
                        StockHeader.ModifiedDate = DateTime.Now;
                        StockHeader.ModifiedBy = User.Identity.Name;
                        new StockHeaderService(_unitOfWork).Update(StockHeader);
                    }

                    


                    packingHeader.BuyerId = vm.SaleToBuyerId;
                    packingHeader.DocDate = vm.DocDate;
                    packingHeader.DocNo = vm.DocNo;
                    packingHeader.GodownId = vm.GodownId;
                    packingHeader.Remark = vm.Remark;
                    packingHeader.ObjectState = Model.ObjectState.Modified;
                    packingHeader.ModifiedDate = DateTime.Now;
                    packingHeader.ModifiedBy = User.Identity.Name;

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = saledispatchheader,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = saledispatchheader.DocTypeId,
                        DocId = saledispatchheader.SaleDispatchHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = saledispatchheader.DocNo,
                        xEModifications = Modifications,
                        DocDate = saledispatchheader.DocDate,
                        DocStatus = saledispatchheader.Status,
                    }));

                    return RedirectToAction("Index", new { id = vm.DocTypeId }).Success("Data saved successfully");
                } 
                #endregion

            }
            PrepareViewBag(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }


        [HttpGet]
        public ActionResult Modify(int id, string IndexType)
        {
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
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

                    var pd = db.SaleDispatchHeader.Find(Id);

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
                        pd.GatePassHeaderId = null;
                        pd.Status = (int)StatusConstants.Modified;
                        pd.ModifiedBy = User.Identity.Name;
                        pd.ModifiedDate = DateTime.Now;
                       // pd.IsGatePassPrinted = false;
                        pd.ObjectState = Model.ObjectState.Modified;
                        db.SaleDispatchHeader.Add(pd);

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

        private ActionResult Edit(int id, string IndexType)
        {
            
            ViewBag.IndexStatus = IndexType;
            SaleDispatchHeader DispactchHeader = _SaleDispatchHeaderService.Find(id);
            PackingHeader packingHeader = _PackingHeaderService.Find(DispactchHeader.PackingHeaderId.Value);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DispactchHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(DispactchHeader), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            SaleDispatchHeaderViewModel vm = new SaleDispatchHeaderViewModel();

            vm = Mapper.Map<SaleDispatchHeader, SaleDispatchHeaderViewModel>(DispactchHeader);
            vm.SaleToBuyerId = DispactchHeader.SaleToBuyerId;
            vm.DeliveryTermsId = DispactchHeader.DeliveryTermsId;
            vm.GodownId = packingHeader.GodownId;
            if (DispactchHeader.GatePassHeaderId >0)
            {
                var GatePass = (from G in db.GatePassHeader
                                where G.GatePassHeaderId == DispactchHeader.GatePassHeaderId
                                select new SaleDispatchHeaderViewModel
                                {
                                    GatePassDocNo = G.DocNo,
                                    GatePassDocDate = G.DocDate
                                }).FirstOrDefault();
                vm.GatePassDocNo = GatePass.GatePassDocNo;
                vm.GatePassDocDate = GatePass.GatePassDocDate;                
            }
            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(DispactchHeader.DocTypeId, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDispatchSetting", new { id = DispactchHeader.DocTypeId }).Warning("Please create Sale Dispatch settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (settings != null)
            {
                vm.ProcessId = settings.ProcessId;
            }


            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
            PrepareViewBag(DispactchHeader.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = DispactchHeader.DocTypeId,
                    DocId = DispactchHeader.SaleDispatchHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = DispactchHeader.DocNo,
                    DocDate = DispactchHeader.DocDate,
                    DocStatus = DispactchHeader.Status,
                }));

            return View("Create", vm);
        }

        [HttpGet]
        public ActionResult DetailInformation(int id, string IndexType)
        {
            return RedirectToAction("Detail", new { id = id, transactionType = "detail", IndexType = IndexType });
        }

        [Authorize]
        public ActionResult Detail(int id, string transactionType, string IndexType)
        {
            ViewBag.transactionType = transactionType;
            ViewBag.IndexStatus = IndexType;

            SaleDispatchHeader DispactchHeader = _SaleDispatchHeaderService.Find(id);
            PackingHeader packingHeader = _PackingHeaderService.Find(DispactchHeader.PackingHeaderId.Value);


            SaleDispatchHeaderViewModel vm = new SaleDispatchHeaderViewModel();
            vm = Mapper.Map<SaleDispatchHeader, SaleDispatchHeaderViewModel>(DispactchHeader);
            vm.SaleToBuyerId = DispactchHeader.SaleToBuyerId;
            vm.DeliveryTermsId = DispactchHeader.DeliveryTermsId;
            vm.GodownId = packingHeader.GodownId;


            if (DispactchHeader.GatePassHeaderId > 0)
            {
                var GatePass = (from G in db.GatePassHeader
                                where G.GatePassHeaderId == DispactchHeader.GatePassHeaderId
                                select new SaleDispatchHeaderViewModel
                                {
                                    GatePassDocNo = G.DocNo,
                                    GatePassDocDate = G.DocDate
                                }).FirstOrDefault();
                vm.GatePassDocNo = GatePass.GatePassDocNo;
                vm.GatePassDocDate = GatePass.GatePassDocDate;
            }


            //Getting Settings
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(DispactchHeader.DocTypeId, DispactchHeader.DivisionId, DispactchHeader.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDispatchSetting", new { id = id }).Warning("Please create Sale Dispatch settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.SaleDispatchSettings = Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
            PrepareViewBag(DispactchHeader.DocTypeId);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = DispactchHeader.DocTypeId,
                    DocId = DispactchHeader.SaleDispatchHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = DispactchHeader.DocNo,
                    DocDate = DispactchHeader.DocDate,
                    DocStatus = DispactchHeader.Status,
                }));

            return View("Create", vm);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Remove(id);
            else
                return HttpNotFound();
        }


        private ActionResult Remove(int id)
        {
            ReasonViewModel rvm = new ReasonViewModel()
            {
                id = id,
            };

            SaleDispatchHeader SaleDispatchHeader = db.SaleDispatchHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, SaleDispatchHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleDispatchHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

            return PartialView("_Reason", rvm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ReasonViewModel vm)
        {
            if (ModelState.IsValid)
            {

                db.Configuration.AutoDetectChangesEnabled = false;

                SaleDispatchHeader Sd = (from H in db.SaleDispatchHeader where H.SaleDispatchHeaderId == vm.id select H).FirstOrDefault();
                PackingHeader Ph = (from H in db.PackingHeader where H.PackingHeaderId == Sd.PackingHeaderId select H).FirstOrDefault();




                var GatePassHEader = (from p in db.GatePassHeader
                                      where p.GatePassHeaderId == Sd.GatePassHeaderId
                                      select p).FirstOrDefault();

                if (Sd.GatePassHeaderId.HasValue)
                {
                    var GatePassLines = (from p in db.GatePassLine
                                         where p.GatePassHeaderId == GatePassHEader.GatePassHeaderId
                                         select p).ToList();


                    foreach (var item in GatePassLines)
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.GatePassLine.Remove(item);
                    }

                    GatePassHEader.ObjectState = Model.ObjectState.Deleted;
                    db.GatePassHeader.Remove(GatePassHEader);
                }


                var SaleDispatchLine = (from L in db.SaleDispatchLine where L.SaleDispatchHeaderId == vm.id select L).ToList();

                var PackingLine = (from L in db.PackingLine where L.PackingHeaderId == Ph.PackingHeaderId select L).ToList();

                List<int> StockIdList = new List<int>();
                int cnt = 0;
                foreach (var item in SaleDispatchLine)
                {
                    if (item.StockId != null)
                    {
                        StockIdList.Add((int)item.StockId);
                    }

                    cnt = cnt + 1;
                    try
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.SaleDispatchLine.Attach(item);
                        db.SaleDispatchLine.Remove(item);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }
                }

                foreach (var item in PackingLine)
                {
                    try
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.PackingLine.Attach(item);
                        db.PackingLine.Remove(item);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }

                }

                foreach (var item in StockIdList)
                {
                    if (item != null)
                    {
                        StockAdj Adj = (from L in db.StockAdj
                                        where L.StockOutId == item
                                        select L).FirstOrDefault();

                        if (Adj != null)
                        {
                            new StockAdjService(_unitOfWork).Delete(Adj);
                        }

                        new StockService(_unitOfWork).DeleteStockDB((int)item, ref db, true);
                    }
                }

                int? StockHeaderId = null;
                StockHeaderId = Sd.StockHeaderId;



                Sd.ObjectState = Model.ObjectState.Deleted;
                db.SaleDispatchHeader.Attach(Sd);
                db.SaleDispatchHeader.Remove(Sd);

                Ph.ObjectState = Model.ObjectState.Deleted;
                db.PackingHeader.Attach(Ph);
                db.PackingHeader.Remove(Ph);

                if (StockHeaderId != null)
                {
                    StockHeader StockHeader = (from H in db.StockHeader where H.StockHeaderId == StockHeaderId select H).FirstOrDefault();
                    StockHeader.ObjectState = Model.ObjectState.Deleted;
                    db.StockHeader.Attach(StockHeader);
                    db.StockHeader.Remove(StockHeader);
                }



                
                


                //Commit the DB
                try
                {
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    db.Configuration.AutoDetectChangesEnabled = true;
                    TempData["CSEXC"] += message;
                    PrepareViewBag(Sd.DocTypeId);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Sd.DocTypeId,
                    DocId = Sd.SaleDispatchHeaderId,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    DocNo = Sd.DocNo,
                    DocDate = Sd.DocDate,
                    DocStatus = Sd.Status,
                }));

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }





        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            SaleDispatchHeader s = db.SaleDispatchHeader.Find(id);
            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, s.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Submit") == false)
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
            //int SaleAc = 6650;
            int ActivityType;


            bool BeforeSave = true;
            try
            {
                BeforeSave = SaleDispatchDocEvents.beforeHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";


            SaleDispatchHeader Dh = _SaleDispatchHeaderService.Find(Id);
            PackingHeader Ph = _PackingHeaderService.Find(Dh.PackingHeaderId.Value);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == Dh.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    Dh.Status = (int)StatusConstants.Submitted;
                    Ph.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    Dh.ReviewBy = null;
                    Ph.ReviewBy = null;





                    SaleDispatchSetting Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(Dh.DocTypeId, Dh.DivisionId, Dh.SiteId);

                    if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                    {

                        if (!String.IsNullOrEmpty(Settings.SqlProcGatePass))
                        {
                            SqlParameter SqlParameterId = new SqlParameter("@Id", Dh.SaleDispatchHeaderId);
                            IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterId).ToList();

                            if (Dh.GatePassHeaderId == null)
                            {
                                SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                DocDate.SqlDbType = SqlDbType.DateTime;
                                SqlParameter Godown = new SqlParameter("@GodownId", Ph.GodownId);
                                SqlParameter DocType = new SqlParameter("@DocTypeId", new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.GatePass).DocumentTypeId);
                                GatePassHeader GPHeader = new GatePassHeader();
                                GPHeader.CreatedBy = User.Identity.Name;
                                GPHeader.CreatedDate = DateTime.Now;
                                GPHeader.DivisionId = Dh.DivisionId;
                                GPHeader.DocDate = DateTime.Now.Date;
                                GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                GPHeader.DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.GatePass).DocumentTypeId;
                                GPHeader.ModifiedBy = User.Identity.Name;
                                GPHeader.ModifiedDate = DateTime.Now;
                                GPHeader.Remark = Dh.Remark;
                                GPHeader.PersonId = Dh.SaleToBuyerId;
                                GPHeader.SiteId = Dh.SiteId;
                                GPHeader.GodownId = Ph.GodownId ;

                                GPHeader.ObjectState = Model.ObjectState.Added;
                                //db.GatePassHeader.Add(GPHeader);
                                new GatePassHeaderService(_unitOfWork).Create(GPHeader);

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
                                    //db.GatePassLine.Add(Gline);
                                    new GatePassLineService(_unitOfWork).Create(Gline);
                                }

                                Dh.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }
                            else
                            {
                                //List<GatePassLine> LineList = new GatePassLineService(_unitOfWork).GetGatePassLineList(pd.GatePassHeaderId ?? 0).ToList();

                                List<GatePassLine> LineList = (from p in db.GatePassLine
                                                               where p.GatePassHeaderId == Dh.GatePassHeaderId
                                                               select p).ToList();

                                foreach (var ittem in LineList)
                                {

                                    ittem.ObjectState = Model.ObjectState.Deleted;
                                    //db.GatePassLine.Remove(ittem);

                                    new GatePassLineService(_unitOfWork).Delete(ittem);
                                }

                                GatePassHeader GPHeader = new GatePassHeaderService(_unitOfWork).Find(Dh.GatePassHeaderId ?? 0);

                                GPHeader.PersonId = Dh.SaleToBuyerId;
                                GPHeader.GodownId = Ph.GodownId;

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
                                    //db.GatePassLine.Add(Gline);
                                    new GatePassLineService(_unitOfWork).Create(Gline);
                                }

                                new GatePassHeaderService(_unitOfWork).Update(GPHeader);
                                Dh.GatePassHeaderId = GPHeader.GatePassHeaderId;

                            }
                        }

                    }



                    _SaleDispatchHeaderService.Update(Dh);
                    _PackingHeaderService.Update(Ph);




                    
                    
                    
                     try
                    {
                        SaleDispatchDocEvents.onHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
                    }
                     catch (Exception ex)
                     {
                         string message = _exception.HandleException(ex);
                         TempData["CSEXC"] += message;
                         EventException = true;
                     }


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                        return RedirectToAction("Index", new { id = Dh.DocTypeId });
                    }


                    try
                    {
                        SaleDispatchDocEvents.afterHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dh.SaleDispatchHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = Dh.DocNo,
                        DocDate = Dh.DocDate,
                        DocStatus = Dh.Status,
                    }));

                    return RedirectToAction("Index", new { id = Dh.DocTypeId, IndexType = IndexType }).Success("Record submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = Dh.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + Dh.ModifiedBy + " only.");
            }

            return View();
        }



        public ActionResult GenerateGatePass(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                int PK = 0;               
                var Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string SaleDispatchIds = "";
                try
                {
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {
                        TimePlanValidation = true;

                       
                        SaleDispatchHeader Dh = _SaleDispatchHeaderService.Find(item);
                        PackingHeader Ph = _PackingHeaderService.Find(Dh.PackingHeaderId.Value);
                        if (!Dh.GatePassHeaderId.HasValue)
                        {
                            #region DocTypeTimeLineValidation
                            try
                            {

                                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(Dh), DocumentTimePlanTypeConstants.GatePassCreate, User.Identity.Name, out ExceptionMsg, out Continue);

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
                                if (!String.IsNullOrEmpty(Settings.SqlProcGatePass) && Dh.Status == (int)StatusConstants.Submitted && !Dh.GatePassHeaderId.HasValue)
                                {

                                    SqlParameter SqlParameterUserId = new SqlParameter("@Id",Dh.SaleDispatchHeaderId);
                                    IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                    if (Dh.GatePassHeaderId == null)
                                    {
                                        SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                        DocDate.SqlDbType = SqlDbType.DateTime;
                                        SqlParameter Godown = new SqlParameter("@GodownId", Ph.GodownId);
                                        SqlParameter DocType = new SqlParameter("@DocTypeId", GatePassDocTypeID);
                                        GatePassHeader GPHeader = new GatePassHeader();
                                        GPHeader.CreatedBy = User.Identity.Name;
                                        GPHeader.CreatedDate = DateTime.Now;
                                        GPHeader.DivisionId = Dh.DivisionId;
                                        GPHeader.DocDate = DateTime.Now.Date;
                                        GPHeader.DocNo = db.Database.SqlQuery<string>("Web.GetNewDocNoGatePass @DocTypeId, @DocDate, @GodownId ", DocType, DocDate, Godown).FirstOrDefault();
                                        GPHeader.DocTypeId = GatePassDocTypeID;
                                        GPHeader.ModifiedBy = User.Identity.Name;
                                        GPHeader.ModifiedDate = DateTime.Now;
                                        GPHeader.Remark = Dh.Remark;
                                        GPHeader.PersonId =Dh.SaleToBuyerId;
                                        GPHeader.SiteId = Dh.SiteId;
                                        GPHeader.GodownId = Ph.GodownId;
                                        GPHeader.GatePassHeaderId = PK++;
                                        GPHeader.ReferenceDocTypeId = Dh.DocTypeId;
                                        GPHeader.ReferenceDocId =Dh.SaleDispatchHeaderId;
                                        GPHeader.ReferenceDocNo = Dh.DocNo;
                                        GPHeader.ObjectState = Model.ObjectState.Added;
                                        db.GatePassHeader.Add(GPHeader);
                                        //new GatePassHeaderService(_unitOfWork).Create(GPHeader);                                   



                                        foreach (GatePassGeneratedViewModel GatepassLine in GatePasses)
                                        {
                                            GatePassLine Gline = new GatePassLine();
                                            Gline.CreatedBy = User.Identity.Name;
                                            Gline.CreatedDate = DateTime.Now;
                                            Gline.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                            Gline.ModifiedBy = User.Identity.Name;
                                            Gline.ModifiedDate = DateTime.Now;
                                            Gline.Product = GatepassLine.ProductName;
                                            Gline.Qty = GatepassLine.Qty;
                                            Gline.Specification = GatepassLine.Specification;
                                            Gline.UnitId = GatepassLine.UnitId;
                                            // new GatePassLineService(_unitOfWork).Create(Gline);
                                            Gline.ObjectState = Model.ObjectState.Added;
                                            db.GatePassLine.Add(Gline);
                                        }

                                        Dh.GatePassHeaderId = GPHeader.GatePassHeaderId;
                                        Dh.ObjectState = Model.ObjectState.Modified;
                                        db.SaleDispatchHeader.Add(Dh);
                                        SaleDispatchIds += Dh.SaleDispatchHeaderId+ ", ";
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
                    Narration = "GatePass created for Sale Dispatch " + SaleDispatchIds,
                }));

                if (string.IsNullOrEmpty((string)TempData["CSEXC"]))
                    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet).Success("Gate passes generated successfully");
                else
                    return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            SaleDispatchHeader Dh = _SaleDispatchHeaderService.Find(Id);
            PackingHeader Ph = _PackingHeaderService.Find(Dh.PackingHeaderId.Value);

            if (ModelState.IsValid)
            {
                Dh.ReviewCount = (Dh.ReviewCount ?? 0) + 1;
                Dh.ReviewBy += User.Identity.Name + ", ";

                Ph.ReviewCount = (Ph.ReviewCount ?? 0) + 1;
                Ph.ReviewBy += User.Identity.Name + ", ";

                _SaleDispatchHeaderService.Update(Dh);
                _PackingHeaderService.Update(Ph);

                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Dh.DocTypeId,
                    DocId = Dh.SaleDispatchHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = Dh.DocNo,
                    DocDate = Dh.DocDate,
                    DocStatus = Dh.Status,
                }));

                return RedirectToAction("Index", new { id = Dh.DocTypeId, IndexType = IndexType }).Success("Record reviewed successfully.");
            }

            return RedirectToAction("Index", new { id = Dh.DocTypeId, IndexType = IndexType }).Warning("Error in reviewing.");
        }

        

        public int PendingToSubmitCount(int id)
        {
            return (_SaleDispatchHeaderService.GetSaleDispatchHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleDispatchHeaderService.GetSaleDispatchHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }



        public JsonResult GetPersonLedgerBalance(int PersonId)
        {
            Decimal Balance = 0;
            SqlParameter SqlParameterPersonId = new SqlParameter("@PersonId", PersonId);

            PersonLedgerBalance PersonLedgerBalance = db.Database.SqlQuery<PersonLedgerBalance>("" + System.Configuration.ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetPersonLedgerBalance @PersonId", SqlParameterPersonId).FirstOrDefault();
            if (PersonLedgerBalance != null)
            {
                Balance = PersonLedgerBalance.Balance;
            }

            return Json(Balance);
        }

        public JsonResult GetPersonDetail(int PersonId)
        {
            var PersonDetail = (from B in db.BusinessEntity
                                where B.PersonID == PersonId
                                select new
                                {
                                    CreditDays = B.CreaditDays ?? 0,
                                    CreditLimit = B.CreaditLimit ?? 0
                                }).FirstOrDefault();

            return Json(PersonDetail);
        }

        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleDispatchHeaders", "SaleDispatchHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleDispatchHeaders", "SaleDispatchHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        //public ActionResult GeneratePrints(string Ids, int DocTypeId)
        //{

        //    if (!string.IsNullOrEmpty(Ids))
        //    {
        //        int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        //        int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

        //        var Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(DocTypeId, DivisionId, SiteId);

        //        try
        //        {

        //            List<byte[]> PdfStream = new List<byte[]>();
        //            foreach (var item in Ids.Split(',').Select(Int32.Parse))
        //            {

        //                DirectReportPrint drp = new DirectReportPrint();

        //                var pd = db.SaleDispatchHeader.Find(item);

        //                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
        //                {
        //                    DocTypeId = pd.DocTypeId,
        //                    DocId = pd.SaleDispatchHeaderId,
        //                    ActivityType = (int)ActivityTypeContants.Print,
        //                    DocNo = pd.DocNo,
        //                    DocDate = pd.DocDate,
        //                    DocStatus = pd.Status,
        //                }));

        //                byte[] Pdf;

        //                if (pd.Status == (int)StatusConstants.Drafted || pd.Status == (int)StatusConstants.Modified || pd.Status == (int)StatusConstants.Import)
        //                {
        //                    //LogAct(item.ToString());
        //                    Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint, User.Identity.Name, item);

        //                    PdfStream.Add(Pdf);
        //                }
        //                else if (pd.Status == (int)StatusConstants.Submitted || pd.Status == (int)StatusConstants.ModificationSubmitted)
        //                {
        //                    Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

        //                    PdfStream.Add(Pdf);
        //                }
        //                else if (pd.Status == (int)StatusConstants.Approved)
        //                {
        //                    Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterApprove, User.Identity.Name, item);
        //                    PdfStream.Add(Pdf);
        //                }

        //            }

        //            PdfMerger pm = new PdfMerger();

        //            byte[] Merge = pm.MergeFiles(PdfStream);

        //            if (Merge != null)
        //                return File(Merge, "application/pdf");

        //        }

        //        catch (Exception ex)
        //        {
        //            string message = _exception.HandleException(ex);
        //            return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(new { success = "Success" }, JsonRequestBehavior.AllowGet);

        //    }
        //    return Json(new { success = "Error", data = "No Records Selected." }, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {
            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(DocTypeId, DivisionId, SiteId);

                if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DocTypeId, Settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "GeneratePrints") == false)
                {
                    return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
                }

                string ReportSql = "";

                if (Settings.DocumentPrintReportHeaderId.HasValue)
                    ReportSql = db.ReportHeader.Where((m) => m.ReportHeaderId == Settings.DocumentPrintReportHeaderId).FirstOrDefault().ReportSQL;

                try
                {

                    List<byte[]> PdfStream = new List<byte[]>();
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {

                        DirectReportPrint drp = new DirectReportPrint();
                        var pd = db.SaleDispatchHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.SaleDispatchHeaderId,
                            ActivityType = (int)ActivityTypeContants.Print,
                            DocNo = pd.DocNo,
                            DocDate = pd.DocDate,
                            DocStatus = pd.Status,
                        }));

                        byte[] Pdf;

                        if (!string.IsNullOrEmpty(ReportSql))
                        {
                            Pdf = drp.rsDirectDocumentPrint(ReportSql, User.Identity.Name, item);
                            PdfStream.Add(Pdf);
                        }
                        else
                        {

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
        private ActionResult PrintOut(int id, string SqlProcForPrint)
        {
            String query = SqlProcForPrint;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);
        }

        [HttpGet]
        public ActionResult Print(int id)
        {
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
            {
                var SEttings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
            {
                var SEttings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            SaleDispatchHeader header = _SaleDispatchHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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

            SaleDispatchSetting SEttings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

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
                //ReportLine Process = new ReportLineService(_unitOfWork).GetReportLineByName("Process", header.ReportHeaderId);
                //if (Process != null)
                //    DefaultValue.Add(Process.ReportLineId, ((int)SEttings.ProcessId).ToString());
            }

            TempData["ReportLayoutDefaultValues"] = DefaultValue;

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleDispatchHeaderService.GetCustomPerson(filter, searchTerm);
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
            int SaleDispatchLine = (new SaleDispatchLineService(_unitOfWork).GetSaleDispatchLineListForIndex(id)).Count();
            if (SaleDispatchLine == 0)
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
