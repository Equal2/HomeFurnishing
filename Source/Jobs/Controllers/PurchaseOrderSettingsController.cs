using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseOrderSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseOrderSettingService _PurchaseOrderSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseOrderSettingController(IPurchaseOrderSettingService PurchaseOrderSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseOrderSettingService = PurchaseOrderSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /PurchaseOrderSettingMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                PurchaseOrderSettingsViewModel vm = new PurchaseOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                PurchaseOrderSettingsViewModel temp = AutoMapper.Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseOrderSettingsViewModel vm)
        {
            PurchaseOrderSetting pt = AutoMapper.Mapper.Map<PurchaseOrderSettingsViewModel, PurchaseOrderSetting>(vm);

            if (!pt.CalculationId.HasValue)
                ModelState.AddModelError("CalculationId", "The Calculation field is required");

            if (ModelState.IsValid)
            {

                if (vm.PurchaseOrderSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PurchaseOrderSettingService.Create(pt);

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseOrderSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        //PrepareViewBag();
                        return View("Create", vm);
                    }


                    return RedirectToAction("Index", "PurchaseOrderHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseOrderSetting temp = _PurchaseOrderSettingService.Find(pt.PurchaseOrderSettingId);

                    PurchaseOrderSetting ExRec = Mapper.Map<PurchaseOrderSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.TermsAndConditions = pt.TermsAndConditions;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.isAllowedWithoutQuotation = pt.isAllowedWithoutQuotation;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PurchaseOrderSettingService.Update(temp);

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
                        //PrepareViewBag();
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseOrderSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));


                    return RedirectToAction("Index", "PurchaseOrderHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("Create", vm);
        }

        // GET: /PurchaseOrderSettingMaster/Create

        public ActionResult CreatePurchaseOrderCancel(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                PurchaseOrderSettingsViewModel vm = new PurchaseOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreatePurchaseOrderCancel", vm);
            }
            else
            {
                PurchaseOrderSettingsViewModel temp = AutoMapper.Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreatePurchaseOrderCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostPurchaseOrderCancel(PurchaseOrderSettingsViewModel vm)
        {
            PurchaseOrderSetting pt = AutoMapper.Mapper.Map<PurchaseOrderSettingsViewModel, PurchaseOrderSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.PurchaseOrderSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PurchaseOrderSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("CreatePurchaseOrderCancel", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseOrderSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "PurchaseOrderCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseOrderSetting temp = _PurchaseOrderSettingService.Find(pt.PurchaseOrderSettingId);

                    PurchaseOrderSetting ExRec = Mapper.Map<PurchaseOrderSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.TermsAndConditions = pt.TermsAndConditions;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.isAllowedWithoutQuotation = pt.isAllowedWithoutQuotation;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PurchaseOrderSettingService.Update(temp);

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
                        return View("CreatePurchaseOrderCancel", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseOrderSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "PurchaseOrderCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("CreatePurchaseOrderCancel", vm);
        }




        // GET: /PurchaseOrderSettingMaster/Create

        public ActionResult CreatePurchaseOrderAmendment(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new PurchaseOrderSettingService(_unitOfWork).GetPurchaseOrderSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                PurchaseOrderSettingsViewModel vm = new PurchaseOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreatePurchaseOrderAmendment", vm);
            }
            else
            {
                PurchaseOrderSettingsViewModel temp = AutoMapper.Mapper.Map<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreatePurchaseOrderAmendment", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostPurchaseOrderAmendment(PurchaseOrderSettingsViewModel vm)
        {
            PurchaseOrderSetting pt = AutoMapper.Mapper.Map<PurchaseOrderSettingsViewModel, PurchaseOrderSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.PurchaseOrderSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PurchaseOrderSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        //PrepareViewBag();
                        return View("CreatePurchaseOrderAmendment", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseOrderSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "PurchaseOrderAmendmentHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseOrderSetting temp = _PurchaseOrderSettingService.Find(pt.PurchaseOrderSettingId);

                    PurchaseOrderSetting ExRec = Mapper.Map<PurchaseOrderSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.TermsAndConditions = pt.TermsAndConditions;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.isAllowedWithoutQuotation = pt.isAllowedWithoutQuotation;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PurchaseOrderSettingService.Update(temp);

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
                        return View("CreatePurchaseOrderAmendment", pt);

                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseOrderSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "PurchaseOrderAmendmentHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("CreatePurchaseOrderAmendment", vm);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
