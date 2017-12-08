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
using PackingDocumentEvents;
using CustomEventArgs;


namespace Jobs.Controllers
{
    [Authorize]
    public class PackingHeaderController : System.Web.Mvc.Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private bool EventException = false;
        
        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        IPackingHeaderService _PackingHeaderService;
        IActivityLogService _ActivityLogService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public PackingHeaderController(IPackingHeaderService PackingHeaderService, IActivityLogService ActivityLogService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PackingHeaderService = PackingHeaderService;
            _ActivityLogService = ActivityLogService;
            _unitOfWork = unitOfWork;
            _exception = exec;
            if (!PackingEvents.Initialized)
            {
                PackingEvents Obj = new PackingEvents();
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
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(id,DivisionId,SiteId);
            if(settings !=null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
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
            IQueryable<PackingHeaderIndexViewModel> p = _PackingHeaderService.GetPackingHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(p);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _PackingHeaderService.GetPackingHeaderListPendingToSubmit(id, User.Identity.Name);

            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _PackingHeaderService.GetPackingHeaderListPendingToReview(id, User.Identity.Name);
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTR";
            return View("Index", PendingtoReview);
        }



        public ActionResult Create(int id)//DocTypeId
        {
            PackingHeaderViewModel vm = new PackingHeaderViewModel();

            vm.DocDate = DateTime.Now.Date;
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.DocTypeId = id;
            vm.CreatedDate = DateTime.Now;

            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PackingSetting", new { id = id }).Warning("Please create Sale Dispatch settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }


            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, settings.ProcessId, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }



            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
            ViewBag.Mode = "Add";
            PrepareViewBag(id);

            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".PackingHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);
            return View("Create", vm);
        }


       



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(PackingHeaderViewModel vm)
        {

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.PackingHeaderId <= 0)
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
                if (vm.PackingHeaderId == 0)
                {
                    PackingHeader Packingheader = Mapper.Map<PackingHeaderViewModel, PackingHeader>(vm);

                    Packingheader.BuyerId = vm.BuyerId;
                    Packingheader.CreatedBy = User.Identity.Name;
                    Packingheader.ModifiedBy = User.Identity.Name;
                    Packingheader.CreatedDate = DateTime.Now;
                    Packingheader.ModifiedDate = DateTime.Now;
                    Packingheader.ObjectState = Model.ObjectState.Added;
                    _PackingHeaderService.Create(Packingheader);





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
                        DocTypeId = Packingheader.DocTypeId,
                        DocId = Packingheader.PackingHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = Packingheader.DocNo,
                        DocDate = Packingheader.DocDate,
                        DocStatus = Packingheader.Status,
                    }));


                     return RedirectToAction("Modify", new { id = Packingheader.PackingHeaderId }).Success("Data saved Successfully");
                } 
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();


                    PackingHeader Packingheader = _PackingHeaderService.Find(vm.PackingHeaderId);

                    PackingHeader ExRec = Mapper.Map<PackingHeader>(Packingheader);


                    StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find(Packingheader.StockHeaderId ?? 0);


                    int status = Packingheader.Status;

                    if (Packingheader.Status != (int)StatusConstants.Drafted)
                    {
                        Packingheader.Status = (int)StatusConstants.Modified;
                    }


                    Packingheader.DocNo = vm.DocNo;
                    Packingheader.DocDate = vm.DocDate;
                    Packingheader.BuyerId = vm.BuyerId;
                    Packingheader.GodownId = vm.GodownId;
                    Packingheader.Remark = vm.Remark;
                    Packingheader.ModifiedDate = DateTime.Now;
                    Packingheader.ModifiedBy = User.Identity.Name;
                    _PackingHeaderService.Update(Packingheader);


                    if (StockHeader != null)
                    {
                        StockHeader.DocDate = vm.DocDate;
                        StockHeader.PersonId = vm.BuyerId;
                        StockHeader.GodownId = vm.GodownId;
                        StockHeader.Remark = vm.Remark;
                        StockHeader.ModifiedDate = DateTime.Now;
                        StockHeader.ModifiedBy = User.Identity.Name;
                        new StockHeaderService(_unitOfWork).Update(StockHeader);
                    }

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = Packingheader,
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
                        DocTypeId = Packingheader.DocTypeId,
                        DocId = Packingheader.PackingHeaderId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        DocNo = Packingheader.DocNo,
                        xEModifications = Modifications,
                        DocDate = Packingheader.DocDate,
                        DocStatus = Packingheader.Status,
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
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        private ActionResult Edit(int id, string IndexType)
        {
            
            ViewBag.IndexStatus = IndexType;
            PackingHeader PackingHeader = _PackingHeaderService.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, PackingHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {

                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PackingHeader), DocumentTimePlanTypeConstants.Modify, User.Identity.Name, out ExceptionMsg, out Continue);

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

            PackingHeaderViewModel vm = new PackingHeaderViewModel();

            vm = Mapper.Map<PackingHeader, PackingHeaderViewModel>(PackingHeader);
            vm.BuyerId = PackingHeader.BuyerId;
            vm.GodownId = PackingHeader.GodownId;
            
            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PackingSetting", new { id = PackingHeader.DocTypeId }).Warning("Please create Sale Dispatch settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (settings != null)
            {
                vm.ProcessId = settings.ProcessId;
            }


            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
            PrepareViewBag(PackingHeader.DocTypeId);
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = PackingHeader.DocTypeId,
                    DocId = PackingHeader.PackingHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = PackingHeader.DocNo,
                    DocDate = PackingHeader.DocDate,
                    DocStatus = PackingHeader.Status,
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

            PackingHeader PackingHeader = _PackingHeaderService.Find(id);


            PackingHeaderViewModel vm = new PackingHeaderViewModel();
            vm = Mapper.Map<PackingHeader, PackingHeaderViewModel>(PackingHeader);
            vm.BuyerId = PackingHeader.BuyerId;
            vm.GodownId = PackingHeader.GodownId;


            //Getting Settings
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(PackingHeader.DocTypeId, PackingHeader.DivisionId, PackingHeader.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "PackingSetting", new { id = id }).Warning("Please create Sale Dispatch settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            vm.PackingSettings = Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
            PrepareViewBag(PackingHeader.DocTypeId);

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = PackingHeader.DocTypeId,
                    DocId = PackingHeader.PackingHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = PackingHeader.DocNo,
                    DocDate = PackingHeader.DocDate,
                    DocStatus = PackingHeader.Status,
                }));

            return View("Create", vm);
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            PackingHeader header = _PackingHeaderService.Find(id);
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

            PackingHeader PackingHeader = db.PackingHeader.Find(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, PackingHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(PackingHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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

                PackingHeader Sd = (from H in db.PackingHeader where H.PackingHeaderId == vm.id select H).FirstOrDefault();


                var PackingLine = (from L in db.PackingLine where L.PackingHeaderId == vm.id select L).ToList();

                
                List<int> StockIdList = new List<int>();
                int cnt = 0;
                foreach (var item in PackingLine)
                {
                    if (item.StockIssueId != null)
                    {
                        StockIdList.Add((int)item.StockIssueId);
                    }

                    cnt = cnt + 1;
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
                db.PackingHeader.Attach(Sd);
                db.PackingHeader.Remove(Sd);



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
                    DocId = Sd.PackingHeaderId,
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
            PackingHeader s = db.PackingHeader.Find(id);
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
                BeforeSave = PackingDocEvents.beforeHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
            }
            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                TempData["CSEXC"] += message;
                EventException = true;
            }

            if (!BeforeSave)
                TempData["CSEXC"] += "Falied validation before submit.";


            PackingHeader Dh = _PackingHeaderService.Find(Id);

            if (ModelState.IsValid && BeforeSave && !EventException)
            {
                if (User.Identity.Name == Dh.ModifiedBy || UserRoles.Contains("Admin"))
                {

                    Dh.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    Dh.ReviewBy = null;

                    PackingSetting Settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(Dh.DocTypeId, Dh.DivisionId, Dh.SiteId);

                    _PackingHeaderService.Update(Dh);

                    
                    try
                    {
                        PackingDocEvents.onHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
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
                        PackingDocEvents.afterHeaderSubmitEvent(this, new SaleEventArgs(Id), ref db);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        TempData["CSEXC"] += message;
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = Dh.DocTypeId,
                        DocId = Dh.PackingHeaderId,
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



        public ActionResult Review(int id, string IndexType, string TransactionType)
        {
            return RedirectToAction("Detail", new { id = id, IndexType = IndexType, transactionType = string.IsNullOrEmpty(TransactionType) ? "review" : TransactionType });
        }


        [HttpPost, ActionName("Detail")]
        [MultipleButton(Name = "Command", Argument = "Review")]
        public ActionResult Reviewed(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            PackingHeader Dh = _PackingHeaderService.Find(Id);

            if (ModelState.IsValid)
            {
                Dh.ReviewCount = (Dh.ReviewCount ?? 0) + 1;
                Dh.ReviewBy += User.Identity.Name + ", ";

                _PackingHeaderService.Update(Dh);

                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = Dh.DocTypeId,
                    DocId = Dh.PackingHeaderId,
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
            return (_PackingHeaderService.GetPackingHeaderListPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_PackingHeaderService.GetPackingHeaderListPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult NextPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PackingHeaders", "PackingHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.PackingHeaders", "PackingHeaderId", PrevNextConstants.Prev);
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

                var Settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = db.PackingHeader.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.PackingHeaderId,
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
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted || header.Status == (int)StatusConstants.Import)
            {
                var SEttings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified)
            {
                var SEttings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            PackingHeader header = _PackingHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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

            PackingSetting SEttings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(Dt.DocumentTypeId, (int)System.Web.HttpContext.Current.Session["DivisionId"], (int)System.Web.HttpContext.Current.Session["SiteId"]);

            Dictionary<int, string> DefaultValue = new Dictionary<int, string>();

            //if (!Dt.ReportMenuId.HasValue)
            //    throw new Exception("Report Menu not configured in document types");

            if (!Dt.ReportMenuId.HasValue)
                return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/GridReport/GridReportLayout/?MenuName=Packing Report&DocTypeId=" + id.ToString());


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
            var Query = _PackingHeaderService.GetCustomPerson(filter, searchTerm);
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
            int PackingLine = (new PackingLineService(_unitOfWork).GetPackingLineListForIndex(id)).Count();
            if (PackingLine == 0)
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
