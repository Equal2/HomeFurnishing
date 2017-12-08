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
using System.Xml.Linq;
using Reports.Controllers;
using Model.ViewModels;
using Reports.Reports;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleInvoiceReturnHeaderController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISaleInvoiceReturnHeaderService _SaleInvoiceReturnHeaderService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleInvoiceReturnHeaderController(ISaleInvoiceReturnHeaderService SaleInvoiceReturnHeaderService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleInvoiceReturnHeaderService = SaleInvoiceReturnHeaderService;
            _unitOfWork = unitOfWork;
            _exception = exec;

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

        // GET: /SaleInvoiceReturnHeaderMaster/

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
            var SaleInvoiceReturnHeader = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnHeaderList(id, User.Identity.Name);
            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "All";
            return View(SaleInvoiceReturnHeader);
        }

        public ActionResult Index_PendingToSubmit(int id)
        {
            var PendingToSubmit = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnPendingToSubmit(id, User.Identity.Name);

            PrepareViewBag(id);
            ViewBag.PendingToSubmit = PendingToSubmitCount(id);
            ViewBag.PendingToReview = PendingToReviewCount(id);
            ViewBag.IndexStatus = "PTS";
            return View("Index", PendingToSubmit);
        }

        public ActionResult Index_PendingToReview(int id)
        {
            var PendingtoReview = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnPendingToReview(id, User.Identity.Name);
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

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.AdminSetting = UserRoles.Contains("Admin").ToString();
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(id,DivisionId,SiteId);
            if(settings !=null)
            {
                ViewBag.ImportMenuId = settings.ImportMenuId;
                ViewBag.SqlProcDocumentPrint = settings.SqlProcDocumentPrint;
                ViewBag.ExportMenuId = settings.ExportMenuId;
            }
        }

        // GET: /SaleInvoiceReturnHeaderMaster/Create

        public ActionResult Create(int id)//DocuentTypeId
        {
            PrepareViewBag(id);
            SaleInvoiceReturnHeaderViewModel vm = new SaleInvoiceReturnHeaderViewModel();
            vm.DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            vm.SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            vm.CreatedDate = DateTime.Now;

            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(id);
            if (DocType != null)
            {
                vm.Nature = DocType.Nature ?? TransactionNatureConstants.Return;
            }


            //Getting Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(id, vm.DivisionId, vm.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForReturn", "SaleInvoiceSetting", new { id = id }).Warning("Please create Sale invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, id, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Create") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }

            vm.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            vm.CalculateDiscountOnRate = settings.CalculateDiscountOnRate;
            vm.DocTypeId = id;
            vm.DocDate = DateTime.Now;
            vm.DocNo = new DocumentTypeService(_unitOfWork).FGetNewDocNo("DocNo", ConfigurationManager.AppSettings["DataBaseSchema"] + ".SaleInvoiceReturnHeaders", vm.DocTypeId, vm.DocDate, vm.DivisionId, vm.SiteId);

            if (settings != null)
            {
                vm.ProcessId = settings.ProcessId;
            }
            vm.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(vm.DocTypeId);
            ViewBag.Mode = "Add";
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleInvoiceReturnHeaderViewModel vm)
        {
            SaleInvoiceReturnHeader pt = AutoMapper.Mapper.Map<SaleInvoiceReturnHeaderViewModel, SaleInvoiceReturnHeader>(vm);

            if (vm.Nature == TransactionNatureConstants.Return)
            {
                if (vm.GodownId <= 0)
                    ModelState.AddModelError("GodownId", "The Godown field is required");
            }

            #region DocTypeTimeLineValidation

            try
            {

                if (vm.SaleInvoiceReturnHeaderId <= 0)
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
                if (vm.SaleInvoiceReturnHeaderId <= 0)
                {
                    if (vm.Nature == TransactionNatureConstants.Return)
                    {
                        SaleDispatchReturnHeader GoodsRet = Mapper.Map<SaleInvoiceReturnHeaderViewModel, SaleDispatchReturnHeader>(vm);
                        GoodsRet.DocTypeId = vm.SaleInvoiceSettings.DocTypeDispatchReturnId ?? 0;
                        GoodsRet.CreatedDate = DateTime.Now;
                        GoodsRet.ModifiedDate = DateTime.Now;
                        GoodsRet.CreatedBy = User.Identity.Name;
                        GoodsRet.ModifiedBy = User.Identity.Name;
                        GoodsRet.ObjectState = Model.ObjectState.Added;
                        new SaleDispatchReturnHeaderService(_unitOfWork).Create(GoodsRet);
                        pt.SaleDispatchReturnHeaderId = GoodsRet.SaleDispatchReturnHeaderId;
                    }

                    pt.CalculateDiscountOnRate = vm.CalculateDiscountOnRate;
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceReturnHeaderService.Create(pt);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Add";
                        return View("Create", vm);
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleInvoiceReturnHeaderId,
                        ActivityType = (int)ActivityTypeContants.Added,
                        DocNo = pt.DocNo,
                        DocDate = pt.DocDate,
                        DocStatus = pt.Status,
                    }));

                    //return Edit(pt.SaleInvoiceReturnHeaderId).Success("Data saved successfully");
                    return RedirectToAction("Modify", new { id = pt.SaleInvoiceReturnHeaderId }).Success("Data saved Successfully");
                }
                #endregion

                #region EditRecord
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleInvoiceReturnHeader temp = _SaleInvoiceReturnHeaderService.Find(pt.SaleInvoiceReturnHeaderId);

                    SaleInvoiceReturnHeader ExRec = new SaleInvoiceReturnHeader();
                    ExRec = Mapper.Map<SaleInvoiceReturnHeader>(temp);


                    int status = temp.Status;

                    if (temp.Status != (int)StatusConstants.Drafted)
                        temp.Status = (int)StatusConstants.Modified;


                    //temp.CurrencyId = pt.CurrencyId;
                    //temp.SalesTaxGroupId = pt.SalesTaxGroupId;
                    temp.Remark = pt.Remark;
                    temp.BuyerId = pt.BuyerId;
                    temp.DocNo = pt.DocNo;
                    temp.ReasonId = pt.ReasonId;
                    temp.DocDate = pt.DocDate;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleInvoiceReturnHeaderService.Update(temp);

                    if (temp.SaleDispatchReturnHeaderId.HasValue)
                    {
                        SaleDispatchReturnHeader GoodsRet = new SaleDispatchReturnHeaderService(_unitOfWork).Find(temp.SaleDispatchReturnHeaderId ?? 0);

                        GoodsRet.DocDate = temp.DocDate;
                        GoodsRet.DocNo = temp.DocNo;
                        GoodsRet.ReasonId = temp.ReasonId;
                        GoodsRet.BuyerId = temp.BuyerId;
                        GoodsRet.Remark = temp.Remark;
                        GoodsRet.GodownId = (int)vm.GodownId;
                        GoodsRet.Status = temp.Status;
                        GoodsRet.ObjectState = Model.ObjectState.Modified;
                        new SaleDispatchReturnHeaderService(_unitOfWork).Update(GoodsRet);


                        if (GoodsRet.StockHeaderId != null)
                        {
                            StockHeader StockHeader = new StockHeaderService(_unitOfWork).Find((int)GoodsRet.StockHeaderId);
                            if (StockHeader != null)
                            {
                                StockHeader.DocDate = temp.DocDate;
                                StockHeader.DocNo = temp.DocNo;
                                StockHeader.PersonId = temp.BuyerId;
                                StockHeader.Remark = temp.Remark;
                                StockHeader.GodownId = (int)vm.GodownId;
                                StockHeader.Status = temp.Status;
                                StockHeader.ObjectState = Model.ObjectState.Modified;
                                new StockHeaderService(_unitOfWork).Update(StockHeader);

                                IEnumerable<Stock> StockList = new StockService(_unitOfWork).GetStockForStockHeaderId(StockHeader.StockHeaderId);
                                foreach(Stock Stock in StockList)
                                {
                                    Stock.DocDate = temp.DocDate;
                                    Stock.GodownId = (int)vm.GodownId;
                                    Stock.ObjectState = Model.ObjectState.Modified;
                                    new StockService(_unitOfWork).Update(Stock);
                                }
                            }
                        }



                        SaleDispatchReturnHeader ExRecR = new SaleDispatchReturnHeader();
                        ExRecR = Mapper.Map<SaleDispatchReturnHeader>(GoodsRet);

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = ExRecR,
                            Obj = GoodsRet,
                        });

                    }


                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });
                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        ViewBag.Mode = "Edit";
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleInvoiceReturnHeaderId,
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

            SaleInvoiceReturnHeaderViewModel pt = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnHeader(id);

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, pt.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Edit") == false)
            {
                return View("~/Views/Shared/PermissionDenied.cshtml").Warning("You don't have permission to do this task.");
            }


            PrepareViewBag(pt.DocTypeId);
            //pt.GodownId = new SaleDispatchReturnHeaderService(_unitOfWork).Find(pt.SaleDispatchReturnHeaderId ?? 0).GodownId;

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

            if ((pt.Status == (int)StatusConstants.Drafted || pt.Status == (int)StatusConstants.Modified) && pt.ModifiedBy != User.Identity.Name && !UserRoles.Contains("Admin"))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType }).Danger("Record must be submitted before modification.");
            }
            else if ((!TimePlanValidation && !Continue))
            {
                return RedirectToAction("DetailInformation", new { id = id, IndexType = IndexType });
            }

            //Job Order Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForReturn", "SaleInvoiceSetting", new { id = pt.DocTypeId }).Warning("Please create Sale Invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            if (settings != null)
            {
                pt.ProcessId = settings.ProcessId;
            }

            if (pt == null)
            {
                return HttpNotFound();
            }
            ViewBag.Mode = "Edit";

            if (!(System.Web.HttpContext.Current.Request.UrlReferrer.PathAndQuery).Contains("Create"))
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.SaleInvoiceReturnHeaderId,
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
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ModifyAfter_Submit(int id, string IndexType)
        {
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Edit(id, IndexType);
            else
                return HttpNotFound();
        }


        [HttpGet]
        public ActionResult Delete(int id)
        {
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Submit(int id)
        {
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
                return Remove(id);
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteAfter_Approve(int id)
        {
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
                return Remove(id);
            else
                return HttpNotFound();
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

            SaleInvoiceReturnHeaderViewModel pt = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnHeader(id);
            //pt.GodownId = new SaleDispatchReturnHeaderService(_unitOfWork).Find(pt.SaleDispatchReturnHeaderId ?? 0).GodownId;
            //Job Order Settings
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(pt.DocTypeId, pt.DivisionId, pt.SiteId);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("CreateForReturn", "SaleInvoiceSetting", new { id = pt.DocTypeId }).Warning("Please create Sale Invoice return settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            pt.SaleInvoiceSettings = Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
            pt.DocumentTypeSettings = new DocumentTypeSettingsService(_unitOfWork).GetDocumentTypeSettingsForDocument(pt.DocTypeId);

            PrepareViewBag(pt.DocTypeId);
            if (pt == null)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrEmpty(transactionType) || transactionType == "detail")
                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pt.DocTypeId,
                    DocId = pt.SaleInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Detail,
                    DocNo = pt.DocNo,
                    DocDate = pt.DocDate,
                    DocStatus = pt.Status,
                }));


            return View("Create", pt);
        }




        public ActionResult Submit(int id, string IndexType, string TransactionType)
        {
            SaleInvoiceReturnHeader s = db.SaleInvoiceReturnHeader.Find(id);

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
        public ActionResult Submitted(int Id, string IndexType, string UserRemark, string IsContinue)
        {
            SaleInvoiceReturnHeader pd = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {
                int ActivityType;
                if (User.Identity.Name == pd.ModifiedBy || UserRoles.Contains("Admin"))
                {


                    pd.Status = (int)StatusConstants.Submitted;
                    ActivityType = (int)ActivityTypeContants.Submitted;

                    pd.ReviewBy = null;

                    if (pd.SaleDispatchReturnHeaderId.HasValue)
                    {

                        SaleDispatchReturnHeader GoodsRet = new SaleDispatchReturnHeaderService(_unitOfWork).Find(pd.SaleDispatchReturnHeaderId ?? 0);

                        GoodsRet.Status = pd.Status;



                        SaleInvoiceSetting Settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);

                        new SaleDispatchReturnHeaderService(_unitOfWork).Update(GoodsRet);

                    }

                    _SaleInvoiceReturnHeaderService.Update(pd);



                    #region "Ledger Posting"

                    LedgerHeaderViewModel LedgerHeaderViewModel = new LedgerHeaderViewModel();


                    SaleInvoiceSetting Settings1 = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(pd.DocTypeId, pd.DivisionId, pd.SiteId);


                    LedgerHeaderViewModel.LedgerHeaderId = pd.LedgerHeaderId ?? 0;
                    LedgerHeaderViewModel.ProcessId  = Settings1.ProcessId ;
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

                    IEnumerable<SaleInvoiceReturnHeaderCharge> SaleInvoiceReturnHeaderCharges = from H in db.SaleInvoiceReturnHeaderCharge where H.HeaderTableId == Id select H;
                    IEnumerable<SaleInvoiceReturnLineCharge> SaleInvoiceReturnLineCharges = from L in db.SaleInvoiceReturnLineCharge where L.HeaderTableId == Id select L;

                    try
                    {
                        new CalculationService(_unitOfWork).LedgerPosting(ref LedgerHeaderViewModel, SaleInvoiceReturnHeaderCharges, SaleInvoiceReturnLineCharges);
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return RedirectToAction("Detail", new { id = Id, transactionType = "submit" });
                    }


                    if (pd.LedgerHeaderId == null)
                    {
                        pd.LedgerHeaderId = LedgerHeaderViewModel.LedgerHeaderId;
                        _SaleInvoiceReturnHeaderService.Update(pd);
                    }

                    #endregion


                    _unitOfWork.Save();

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pd.DocTypeId,
                        DocId = pd.SaleInvoiceReturnHeaderId,
                        ActivityType = ActivityType,
                        UserRemark = UserRemark,
                        DocNo = pd.DocNo,
                        DocDate = pd.DocDate,
                        DocStatus = pd.Status,
                    }));

                    if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                    {

                        int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, pd.DocTypeId, User.Identity.Name, ForActionConstants.PendingToSubmit, "Web.SaleInvoiceReturnHeaders", "SaleInvoiceReturnHeaderId", PrevNextConstants.Next);

                        if (nextId == 0)
                        {


                            var PendingtoSubmitCount = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnPendingToSubmit(pd.DocTypeId, User.Identity.Name).Count();
                            if (PendingtoSubmitCount > 0)
                                return RedirectToAction("Index_PendingToSubmit", new { id = pd.DocTypeId, IndexType = IndexType });
                            else
                                return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType });

                        }

                        return RedirectToAction("Detail", new { id = nextId, TransactionType = "submitContinue", IndexType = IndexType }).Success("Sale Invoice Return " + pd.DocNo + " submitted successfully.");

                    }

                    else
                        return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Sale Invioce Return " + pd.DocNo + " submitted successfully.");
                }
                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Record can be submitted by user " + pd.ModifiedBy + " only.");
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
            SaleInvoiceReturnHeader pd = new SaleInvoiceReturnHeaderService(_unitOfWork).Find(Id);

            if (ModelState.IsValid)
            {

                pd.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                pd.ReviewBy += User.Identity.Name + ", ";

                _SaleInvoiceReturnHeaderService.Update(pd);

                if (pd.SaleDispatchReturnHeaderId.HasValue)
                {
                    SaleDispatchReturnHeader GoodsRet = new SaleDispatchReturnHeaderService(_unitOfWork).Find(pd.SaleDispatchReturnHeaderId ?? 0);

                    GoodsRet.ReviewCount = (pd.ReviewCount ?? 0) + 1;
                    GoodsRet.ReviewBy += User.Identity.Name + ", ";

                    new SaleDispatchReturnHeaderService(_unitOfWork).Update(GoodsRet);
                }

                _unitOfWork.Save();

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = pd.DocTypeId,
                    DocId = pd.SaleInvoiceReturnHeaderId,
                    ActivityType = (int)ActivityTypeContants.Reviewed,
                    UserRemark = UserRemark,
                    DocNo = pd.DocNo,
                    DocDate = pd.DocDate,
                    DocStatus = pd.Status,
                }));

                if (!string.IsNullOrEmpty(IsContinue) && IsContinue == "True")
                {
                    SaleInvoiceReturnHeader HEader = _SaleInvoiceReturnHeaderService.Find(Id);

                    int nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(Id, HEader.DocTypeId, User.Identity.Name, ForActionConstants.PendingToReview, "Web.SaleInvoiceReturnHeaders", "SaleInvoiceReturnHeaderId", PrevNextConstants.Next);
                    if (nextId == 0)
                    {
                        var PendingtoSubmitCount = _SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnPendingToReview(HEader.DocTypeId, User.Identity.Name).Count();
                        if (PendingtoSubmitCount > 0)
                            return RedirectToAction("Index_PendingToReview", new { id = HEader.DocTypeId, IndexType = IndexType });
                        else
                            return RedirectToAction("Index", new { id = HEader.DocTypeId, IndexType = IndexType });

                    }

                    ViewBag.PendingToReview = PendingToReviewCount(Id);
                    return RedirectToAction("Detail", new { id = nextId, transactionType = "ReviewContinue", IndexType = IndexType });
                }


                else
                    return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Success("Record reviewed successfully.");
            }

            return RedirectToAction("Index", new { id = pd.DocTypeId, IndexType = IndexType }).Warning("Error in reviewing.");
        }






        // GET: /ProductMaster/Delete/5

        private ActionResult Remove(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SaleInvoiceReturnHeader SaleInvoiceReturnHeader = db.SaleInvoiceReturnHeader.Find(id);
            if (SaleInvoiceReturnHeader == null)
            {
                return HttpNotFound();
            }

            if (new RolePermissionService(_unitOfWork).IsActionAllowed(UserRoles, SaleInvoiceReturnHeader.DocTypeId, null, this.ControllerContext.RouteData.Values["controller"].ToString(), "Remove") == false)
            {
                return PartialView("~/Views/Shared/PermissionDenied_Modal.cshtml").Warning("You don't have permission to do this task.");
            }

            #region DocTypeTimeLineValidation
            try
            {
                TimePlanValidation = DocumentValidation.ValidateDocument(Mapper.Map<DocumentUniqueId>(SaleInvoiceReturnHeader), DocumentTimePlanTypeConstants.Delete, User.Identity.Name, out ExceptionMsg, out Continue);
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
            if (ModelState.IsValid)
            {
                List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                int? StockHeaderId = 0;
                int? LedgerHeaderId = 0;

                var temp = _SaleInvoiceReturnHeaderService.Find(vm.id);
                var DispatchRetHeaderId = temp.SaleDispatchReturnHeaderId;

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                var line = new SaleInvoiceReturnLineService(_unitOfWork).GetLineListForIndex(vm.id);

                LedgerHeaderId = temp.LedgerHeaderId;

                foreach (var item in line)
                {

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = item,
                    });

                    var linecharges = new SaleInvoiceReturnLineChargeService(_unitOfWork).GetCalculationProductList(item.SaleInvoiceReturnLineId);

                    foreach (var citem in linecharges)
                    {
                        new SaleInvoiceReturnLineChargeService(_unitOfWork).Delete(citem.Id);
                    }

                    new SaleInvoiceReturnLineService(_unitOfWork).Delete(item.SaleInvoiceReturnLineId);
                }

                var headercharges = new SaleInvoiceReturnHeaderChargeService(_unitOfWork).GetCalculationFooterList(vm.id);

                foreach (var item in headercharges)
                {
                    new SaleInvoiceReturnHeaderChargeService(_unitOfWork).Delete(item.Id);
                }

                _SaleInvoiceReturnHeaderService.Delete(vm.id);

                if (LedgerHeaderId.HasValue)
                {
                    var LedgerList = new LedgerService(_unitOfWork).FindForLedgerHeader((int)LedgerHeaderId);

                    foreach(Ledger Ledger in LedgerList)
                    {
                        new LedgerService(_unitOfWork).Delete(Ledger.LedgerId);
                    }

                    new LedgerHeaderService(_unitOfWork).Delete((int)LedgerHeaderId);
                }


                List<int> StockIdList = new List<int>();

                if (DispatchRetHeaderId.HasValue)
                {
                    var SaleDispatchreturn = new SaleDispatchReturnHeaderService(_unitOfWork).Find(DispatchRetHeaderId ?? 0);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = SaleDispatchreturn,
                    });

                    StockHeaderId = SaleDispatchreturn.StockHeaderId;

                    var GoodsLines = new SaleDispatchReturnLineService(_unitOfWork).GetLineListForIndex(SaleDispatchreturn.SaleDispatchReturnHeaderId);

                    foreach (var item in GoodsLines)
                    {

                        LogList.Add(new LogTypeViewModel
                        {
                            ExObj = item,
                        });

                        if (item.StockId != null)
                        {
                            StockIdList.Add((int)item.StockId);
                        }
                        new SaleDispatchReturnLineService(_unitOfWork).Delete(item.SaleDispatchReturnLineId);

                    }

                    foreach (var item in StockIdList)
                    {
                        new StockService(_unitOfWork).DeleteStock(item);
                    }


                    new SaleDispatchReturnHeaderService(_unitOfWork).Delete(SaleDispatchreturn.SaleDispatchReturnHeaderId);


                    if (StockHeaderId != null)
                    {
                        new StockHeaderService(_unitOfWork).Delete((int)StockHeaderId);
                    }

                }

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
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
            var nextId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleInvoiceReturnHeaders", "SaleInvoiceReturnHeaderId", PrevNextConstants.Next);
            return Edit(nextId, "");
        }
        [HttpGet]
        public ActionResult PrevPage(int DocId, int DocTypeId)//CurrentHeaderId
        {
            var PrevId = new NextPrevIdService(_unitOfWork).GetNextPrevId(DocId, DocTypeId, User.Identity.Name, "", "Web.SaleInvoiceReturnHeaders", "SaleInvoiceReturnHeaderId", PrevNextConstants.Prev);
            return Edit(PrevId, "");
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        private ActionResult PrintOut(int id, string SqlProcForPrint)
        {
            string query;
            query = SqlProcForPrint;
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

        [HttpGet]
        public ActionResult Print(int id)
        {
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Drafted)
            {
                var SEttings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Submitted || header.Status == (int)StatusConstants.Modified || header.Status == (int)StatusConstants.ModificationSubmitted)
            {
                var SEttings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
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
            SaleInvoiceReturnHeader header = _SaleInvoiceReturnHeaderService.Find(id);
            if (header.Status == (int)StatusConstants.Approved)
            {
                var SEttings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(header.DocTypeId, header.DivisionId, header.SiteId);
                if (string.IsNullOrEmpty(SEttings.SqlProcDocumentPrint_AfterApprove))
                    throw new Exception("Document Print Not Configured");
                else
                    return PrintOut(id, SEttings.SqlProcDocumentPrint_AfterApprove);
            }
            else
                return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }


        public int PendingToSubmitCount(int id)
        {
            return (_SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnPendingToSubmit(id, User.Identity.Name)).Count();
        }

        public int PendingToReviewCount(int id)
        {
            return (_SaleInvoiceReturnHeaderService.GetSaleInvoiceReturnPendingToReview(id, User.Identity.Name)).Count();
        }

        public ActionResult GetCustomPerson(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SaleInvoiceReturnHeaderService.GetCustomPerson(filter, searchTerm);
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

        public bool Submitvalidation(int id, out string Msg)
        {
            Msg = "";
            int SaleInvoiceReturnLine = (new SaleInvoiceReturnLineService(_unitOfWork).GetLineListForIndex(id)).Count();
            if (SaleInvoiceReturnLine == 0)
            {
                Msg = "Add Line Record. <br />";
            }
            else
            {
                Msg = "";
            }
            return (string.IsNullOrEmpty(Msg));
        }

        public ActionResult GeneratePrints(string Ids, int DocTypeId)
        {

            if (!string.IsNullOrEmpty(Ids))
            {
                int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

                var Settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(DocTypeId, DivisionId, SiteId);

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

                        var pd = _SaleInvoiceReturnHeaderService.Find(item);

                        LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                        {
                            DocTypeId = pd.DocTypeId,
                            DocId = pd.SaleInvoiceReturnHeaderId,
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
