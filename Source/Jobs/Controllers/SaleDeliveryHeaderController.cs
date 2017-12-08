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
using SaleDeliveryDocumentEvents;
using CustomEventArgs;


namespace Jobs.Controllers
{
    [Authorize]
    public class SaleDeliveryHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;
        
        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleDeliveryHeaderService _SaleDeliveryHeaderService;
        IPackingHeaderService _PackingHeaderService;        
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public SaleDeliveryHeaderController(ISaleDeliveryHeaderService SaleDeliveryHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec, IPackingHeaderService PackingHeaderService)
        {
            _SaleDeliveryHeaderService = SaleDeliveryHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            _PackingHeaderService = PackingHeaderService;
            if (!SaleDeliveryEvents.Initialized)
            {
                SaleDeliveryEvents Obj = new SaleDeliveryEvents();
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
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(id,DivisionId,SiteId);
            if(settings !=null)
            {
                ViewBag.WizardId = settings.WizardMenuId;
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
            IQueryable<SaleDeliveryHeaderIndexViewModel> p = _SaleDeliveryHeaderService.GetSaleDeliveryHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _SaleDeliveryHeaderService.GetSaleDeliveryHeaderListPendingToSubmit(id, User.Identity.Name);

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _SaleDeliveryHeaderService.GetSaleDeliveryHeaderListPendingToReview(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }



        public ActionResult Create(int id)//DocTypeId
        {
            SaleDeliveryHeaderViewModel vm = new SaleDeliveryHeaderViewModel();

            vm.DocDate = DateTime.Now.Date;
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DocTypeId = id;
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDeliverySetting", new { id = id }).Warning("Please create Sale Delivery settings");
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
                vm.ProcessId = settings.ProcessId;
            }

            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            ViewBag.Mode = "Add";
            PrepareViewBag(id);

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleDeliveryHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            return View("Create", vm);
        }


       



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(SaleDeliveryHeaderViewModel vm)
        {

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.SaleDeliveryHeaderId <= 0)
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
                if (vm.SaleDeliveryHeaderId == 0)
                {
                    SaleDeliveryHeader SaleDeliveryheader = Mapper.Map<SaleDeliveryHeaderViewModel, SaleDeliveryHeader>(vm);

                    SaleDeliveryheader.CreatedDate = DateTime.Now;
                    SaleDeliveryheader.ModifiedDate = DateTime.Now;
                    SaleDeliveryheader.CreatedBy = User.Identity.Name;
                    SaleDeliveryheader.ModifiedBy = User.Identity.Name;
                    SaleDeliveryheader.Status = (int)StatusConstants.Drafted;
                    _SaleDeliveryHeaderService.Create(SaleDeliveryheader);


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
                        DocTypeId = SaleDeliveryheader.DocTypeId,
                        DocId = SaleDeliveryheader.SaleDeliveryHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = SaleDeliveryheader.DocNo,
                        DocDate = SaleDeliveryheader.DocDate,
                        DocStatus = SaleDeliveryheader.Status,
                    }));


                     return RedirectToAction("Modify", new { id = SaleDeliveryheader.SaleDeliveryHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    SaleDeliveryHeader SaleDeliveryheader = _SaleDeliveryHeaderService.Find(vm.SaleDeliveryHeaderId);

                    SaleDeliveryHeader ExRec = Mapper.Map<SaleDeliveryHeader>(SaleDeliveryheader);




                    int status = SaleDeliveryheader.Status;

                    if (SaleDeliveryheader.Status != (int)StatusConstants.Drafted)
                    {
                        SaleDeliveryheader.Status = (int)StatusConstants.Modified;
                    }



                    SaleDeliveryheader.DocDate = vm.DocDate;
                    SaleDeliveryheader.SaleToBuyerId = vm.SaleToBuyerId;
                    SaleDeliveryheader.DeliverToPerson = vm.DeliverToPerson;
                    SaleDeliveryheader.DeliverToPersonReference = vm.DeliverToPersonReference;
                    SaleDeliveryheader.ShipToPartyAddress = vm.ShipToPartyAddress;
                    SaleDeliveryheader.Remark = vm.Remark;
                    SaleDeliveryheader.ModifiedDate = DateTime.Now;
                    SaleDeliveryheader.ModifiedBy = User.Identity.Name;
                    _SaleDeliveryHeaderService.Update(SaleDeliveryheader);






                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = SaleDeliveryheader,
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
                        DocTypeId = SaleDeliveryheader.DocTypeId,
                        DocId = SaleDeliveryheader.SaleDeliveryHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = SaleDeliveryheader.DocNo,
                        xEModifications = Modifications,
                        DocDate = SaleDeliveryheader.DocDate,
                        DocStatus = SaleDeliveryheader.Status,
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
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
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

                    var pd = db.SaleDeliveryHeader.Find(Id);

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
                        db.SaleDeliveryHeader.Add(pd);

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
            SaleDeliveryHeader DeliveryHeader = _SaleDeliveryHeaderService.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, DeliveryHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(DeliveryHeader), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            SaleDeliveryHeaderViewModel vm = new SaleDeliveryHeaderViewModel();

            vm = Mapper.Map<SaleDeliveryHeader, SaleDeliveryHeaderViewModel>(DeliveryHeader);
            vm.SaleToBuyerId = DeliveryHeader.SaleToBuyerId;
            if (DeliveryHeader.GatePassHeaderId >0)
            {
                var GatePass = (from G in db.GatePassHeader
                                where G.GatePassHeaderId == DeliveryHeader.GatePassHeaderId
                                select new SaleDeliveryHeaderViewModel
                                {
                                    GatePassDocNo = G.DocNo,
                                    GatePassDocDate = G.DocDate
                                }).FirstOrDefault();
                vm.GatePassDocNo = GatePass.GatePassDocNo;
                vm.GatePassDocDate = GatePass.GatePassDocDate;                
            }
            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DeliveryHeader.DocTypeId, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDeliverySetting", new { id = DeliveryHeader.DocTypeId }).Warning("Please create Sale Delivery settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (settings != null)
            {
                vm.ProcessId = settings.ProcessId;
            }


            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            PrepareViewBag(DeliveryHeader.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = DeliveryHeader.DocTypeId,
                    DocId = DeliveryHeader.SaleDeliveryHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = DeliveryHeader.DocNo,
                    DocDate = DeliveryHeader.DocDate,
                    DocStatus = DeliveryHeader.Status,
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

            SaleDeliveryHeader DeliveryHeader = _SaleDeliveryHeaderService.Find(id);


            SaleDeliveryHeaderViewModel vm = new SaleDeliveryHeaderViewModel();
            vm = Mapper.Map<SaleDeliveryHeader, SaleDeliveryHeaderViewModel>(DeliveryHeader);
            vm.SaleToBuyerId = DeliveryHeader.SaleToBuyerId;


            if (DeliveryHeader.GatePassHeaderId > 0)
            {
                var GatePass = (from G in db.GatePassHeader
                                where G.GatePassHeaderId == DeliveryHeader.GatePassHeaderId
                                select new SaleDeliveryHeaderViewModel
                                {
                                    GatePassDocNo = G.DocNo,
                                    GatePassDocDate = G.DocDate
                                }).FirstOrDefault();
                vm.GatePassDocNo = GatePass.GatePassDocNo;
                vm.GatePassDocDate = GatePass.GatePassDocDate;
            }


            //Getting Settings
            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DeliveryHeader.DocTypeId, DeliveryHeader.DivisionId, DeliveryHeader.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "SaleDeliverySetting", new { id = id }).Warning("Please create Sale Delivery settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.SaleDeliverySettings = Mapper.Map<SaleDeliverySetting, SaleDeliverySettingsViewModel>(settings);
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            PrepareViewBag(DeliveryHeader.DocTypeId);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = DeliveryHeader.DocTypeId,
                    DocId = DeliveryHeader.SaleDeliveryHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = DeliveryHeader.DocNo,
                    DocDate = DeliveryHeader.DocDate,
                    DocStatus = DeliveryHeader.Status,
                }));

            return View("Create", vm);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
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

            SaleDeliveryHeader SaleDeliveryHeader = db.SaleDeliveryHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, SaleDeliveryHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleDeliveryHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

                SaleDeliveryHeader Sd = (from H in db.SaleDeliveryHeader where H.SaleDeliveryHeaderId == vm.id select H).FirstOrDefault();


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


                var SaleDeliveryLine = (from L in db.SaleDeliveryLine where L.SaleDeliveryHeaderId == vm.id select L).ToList();


                foreach (var item in SaleDeliveryLine)
                {
                    try
                    {
                        item.ObjectState = Model.ObjectState.Deleted;
                        db.SaleDeliveryLine.Attach(item);
                        db.SaleDeliveryLine.Remove(item);
                    }
                    catch (Exception e)
                    {
                        string str = e.Message;
                    }
                }
                



                Sd.ObjectState = Model.ObjectState.Deleted;
                db.SaleDeliveryHeader.Attach(Sd);
                db.SaleDeliveryHeader.Remove(Sd);






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
                    DocId = Sd.SaleDeliveryHeaderId,
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
            SaleDeliveryHeader s = db.SaleDeliveryHeader.Find(id);
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
                BeforeSave = SaleDeliveryDocEvents.beforeHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";


            SaleDeliveryHeader Dh = _SaleDeliveryHeaderService.Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == Dh.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    Dh.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    Dh.ReviewBy = null;



                    SaleDeliverySetting Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(Dh.DocTypeId, Dh.DivisionId, Dh.SiteId);

                    if (!string.IsNullOrEmpty(GenGatePass) && GenGatePass == "true")
                    {

                        if (!String.IsNullOrEmpty(Settings.SqlProcGatePass))
                        {
                            int GodownId = 0;
                            if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                            {
                                GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
                            }
                            var Dispatch = (from H in db.SaleDeliveryHeader
                                            join L in db.SaleDeliveryLine on H.SaleDeliveryHeaderId equals L.SaleDeliveryHeaderId into SaleDeliveryLineTable
                                            from SaleDeliveryLineTab in SaleDeliveryLineTable.DefaultIfEmpty()
                                            where H.SaleDeliveryHeaderId == Dh.SaleDeliveryHeaderId
                                            select new
                                            {
                                                GodownId = SaleDeliveryLineTab.SaleInvoiceLine.SaleDispatchLine.GodownId
                                            }).FirstOrDefault();
                            if (Dispatch != null)
                            {
                                GodownId = Dispatch.GodownId;
                            }

                            SqlParameter SqlParameterId = new SqlParameter("@Id", Dh.SaleDeliveryHeaderId);
                            IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterId).ToList();

                            if (Dh.GatePassHeaderId == null)
                            {
                                SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                DocDate.SqlDbType = SqlDbType.DateTime;
                                SqlParameter Godown = new SqlParameter("@GodownId", GodownId);
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
                                GPHeader.GodownId = GodownId;

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
                                GPHeader.GodownId = GodownId;

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



                    _SaleDeliveryHeaderService.Update(Dh);
                    
                    
                     try
                    {
                        SaleDeliveryDocEvents.onHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
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
                        SaleDeliveryDocEvents.afterHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dh.SaleDeliveryHeaderId,
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
                var Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DocTypeId, DivisionId, SiteId);
                var GatePassDocTypeID = new DocumentTypeService(_unitOfWork).FindByName(TransactionDocCategoryConstants.GatePass).DocumentTypeId;
                string SaleDeliveryIds = "";
                try
                {
                    foreach (var item in Ids.Split(',').Select(Int32.Parse))
                    {
                        TimePlanValidation = true;

                       
                        SaleDeliveryHeader Dh = _SaleDeliveryHeaderService.Find(item);
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
                                    int GodownId = 0;
                                    if (System.Web.HttpContext.Current.Session["DefaultGodownId"] != null)
                                    {
                                        GodownId = (int)System.Web.HttpContext.Current.Session["DefaultGodownId"];
                                    }

                                    var Dispatch = (from H in db.SaleDeliveryHeader
                                                    join L in db.SaleDeliveryLine on H.SaleDeliveryHeaderId equals L.SaleDeliveryHeaderId into SaleDeliveryLineTable
                                                    from SaleDeliveryLineTab in SaleDeliveryLineTable.DefaultIfEmpty()
                                                    where H.SaleDeliveryHeaderId == Dh.SaleDeliveryHeaderId
                                                    select new
                                                    {
                                                        GodownId = SaleDeliveryLineTab.SaleInvoiceLine.SaleDispatchLine.GodownId
                                                    }).FirstOrDefault();
                                    if (Dispatch != null)
                                    {
                                        GodownId = Dispatch.GodownId;
                                    }





                                    SqlParameter SqlParameterUserId = new SqlParameter("@Id",Dh.SaleDeliveryHeaderId);
                                    IEnumerable<GatePassGeneratedViewModel> GatePasses = db.Database.SqlQuery<GatePassGeneratedViewModel>(Settings.SqlProcGatePass + " @Id", SqlParameterUserId).ToList();

                                    if (Dh.GatePassHeaderId == null)
                                    {
                                        SqlParameter DocDate = new SqlParameter("@DocDate", DateTime.Now.Date);
                                        DocDate.SqlDbType = SqlDbType.DateTime;
                                        SqlParameter Godown = new SqlParameter("@GodownId", GodownId);
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
                                        GPHeader.GodownId = GodownId;
                                        GPHeader.GatePassHeaderId = PK++;
                                        GPHeader.ReferenceDocTypeId = Dh.DocTypeId;
                                        GPHeader.ReferenceDocId =Dh.SaleDeliveryHeaderId;
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
                                        db.SaleDeliveryHeader.Add(Dh);
                                        SaleDeliveryIds += Dh.SaleDeliveryHeaderId+ ", ";
                                    }

                                    db.SaveChanges();
                                }
                                else
                                {
                                    if (Dh.Status != (int)StatusConstants.Submitted)
                                    {
                                        string message = "Record must be submitted before generating gatepass.";
                                        return Json(new { success = "Error", data = message }, JsonRequestBehavior.AllowGet);
                                    }
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
                    Narration = "GatePass created for Sale Delivery " + SaleDeliveryIds,
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
            SaleDeliveryHeader Dh = _SaleDeliveryHeaderService.Find(Id);

            if (ModelState.IsValid)
            {
                Dh.ReviewCount = (Dh.ReviewCount ?? 0) + 1;
                Dh.ReviewBy += User.Identity.Name + ", ";


                _SaleDeliveryHeaderService.Update(Dh);


                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Dh.DocTypeId,
                    DocId = Dh.SaleDeliveryHeaderId,
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
            return (_SaleDeliveryHeaderService.GetSaleDeliveryHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleDeliveryHeaderService.GetSaleDeliveryHeaderListPendingToReview(id, User.Identity.Name)).Count();
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleDeliveryHeaders", "SaleDeliveryHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleDeliveryHeaders", "SaleDeliveryHeaderId", PrevNextConstants.Prev);
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


        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = db.SaleDeliveryHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.SaleDeliveryHeaderId,
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
                            Pdf = drp.DirectDocumentPrint(Settings.SqlProcDocumentPrint_AfterSubmit, User.Identity.Name, item);

                            PdfStream.Add(Pdf);
                        }
                        else if (pd.Status == (int)StatusConstants.Approved)
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
        private ActionResult PrintOut(int id, string SqlProcForPrint)
        {
            String query = SqlProcForPrint;
            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_DocumentPrint/DocumentPrint/?DocumentId=" + id + "&queryString=" + query);
        }

        [HttpGet]
        public ActionResult Print(int id)
        {
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
            {
                var SEttings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
            {
                var SEttings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            SaleDeliveryHeader header = _SaleDeliveryHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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

            SaleDeliverySetting SEttings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

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
            var Query = _SaleDeliveryHeaderService.GetCustomPerson(filter, searchTerm);
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

        public ActionResult Wizard(int id)//Document Type Id
        {
            //ControllerAction ca = new ControllerActionService(_unitOfWork).Find(id);
            SaleDeliveryHeaderViewModel vm = new SaleDeliveryHeaderViewModel();

            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new SaleDeliverySettingService(_unitOfWork).GetSaleDeliverySettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                if (settings.WizardMenuId != null)
                {
                    MenuViewModel menuviewmodel = new MenuService(_unitOfWork).GetMenu((int)settings.WizardMenuId);

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
                        return RedirectToAction(menuviewmodel.ActionName, menuviewmodel.ControllerName, new { MenuId = menuviewmodel.MenuId, id = menuviewmodel.RouteId });
                    }
                }
            }
            return RedirectToAction("Index", new { id = id });
        }

        public JsonResult GetBuyerDetailJson(int SaleToBuyerId)
        {
            var PersonAddress = (from P in db.Persons
                                 join Pa in db.PersonAddress on P.PersonID equals Pa.PersonId into PersonAddressTable
                                 from PersonAddressTab in PersonAddressTable.DefaultIfEmpty()
                                 where P.PersonID == SaleToBuyerId
                                 select new
                                 {
                                     PersonId = P.PersonID,
                                     Address = PersonAddressTab.Address
                                 }).FirstOrDefault();

            return Json(PersonAddress);
        }

        #region submitValidation
        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";
            int SaleDeliveryLine = (new SaleDeliveryLineService(_unitOfWork).GetSaleDeliveryLineListForIndex(id)).Count();
            if (SaleDeliveryLine == 0)
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
