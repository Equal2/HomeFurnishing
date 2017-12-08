using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Model.ViewModel;
using Core.Common;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class PurchaseGoodsReceiptSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPurchaseGoodsReceiptSettingService _PurchaseGoodsReceiptSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PurchaseGoodsReceiptSettingController(IPurchaseGoodsReceiptSettingService PurchaseGoodsReceiptSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PurchaseGoodsReceiptSettingService = PurchaseGoodsReceiptSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;


            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        private void PrepareViewBag()
        {
            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
        }
        // GET: /PurchaseGoodsReceiptSettingMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {

            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(id, DivisionId, SiteId);
            PrepareViewBag();
            if (settings == null)
            {
                PurchaseGoodsReceiptSettingsViewModel vm = new PurchaseGoodsReceiptSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                PurchaseGoodsReceiptSettingsViewModel temp = AutoMapper.Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PurchaseGoodsReceiptSettingsViewModel vm)
        {
            PurchaseGoodsReceiptSetting pt = AutoMapper.Mapper.Map<PurchaseGoodsReceiptSettingsViewModel, PurchaseGoodsReceiptSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.PurchaseGoodsReceiptSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PurchaseGoodsReceiptSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseGoodsReceiptSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "PurchaseGoodsReceiptHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseGoodsReceiptSetting temp = _PurchaseGoodsReceiptSettingService.Find(pt.PurchaseGoodsReceiptSettingId);

                    PurchaseGoodsReceiptSetting ExRec = Mapper.Map<PurchaseGoodsReceiptSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;                    
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;                    
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.UnitConversionForId = pt.UnitConversionForId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PurchaseGoodsReceiptSettingService.Update(temp);

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
                        PrepareViewBag();
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseGoodsReceiptSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "PurchaseGoodsReceiptHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("Create", vm);
        }

        public ActionResult CreatePurchaseGoodsReturn(int id)//DocTypeId
        {

            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new PurchaseGoodsReceiptSettingService(_unitOfWork).GetPurchaseGoodsReceiptSettingForDocument(id, DivisionId, SiteId);
            PrepareViewBag();
            if (settings == null)
            {
                PurchaseGoodsReceiptSettingsViewModel vm = new PurchaseGoodsReceiptSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreatePurchaseGoodsReturn", vm);
            }
            else
            {
                PurchaseGoodsReceiptSettingsViewModel temp = AutoMapper.Mapper.Map<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreatePurchaseGoodsReturn", temp);
            }


        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostPurchaseGoodsReturn(PurchaseGoodsReceiptSettingsViewModel vm)
        {
            PurchaseGoodsReceiptSetting pt = AutoMapper.Mapper.Map<PurchaseGoodsReceiptSettingsViewModel, PurchaseGoodsReceiptSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.PurchaseGoodsReceiptSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PurchaseGoodsReceiptSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreatePurchaseGoodsReturn", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PurchaseGoodsReceiptSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "PurchaseGoodsReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PurchaseGoodsReceiptSetting temp = _PurchaseGoodsReceiptSettingService.Find(pt.PurchaseGoodsReceiptSettingId);

                    PurchaseGoodsReceiptSetting ExRec = Mapper.Map<PurchaseGoodsReceiptSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.UnitConversionForId = pt.UnitConversionForId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PurchaseGoodsReceiptSettingService.Update(temp);

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
                        PrepareViewBag();
                        return View("CreatePurchaseGoodsReturn", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PurchaseGoodsReceiptSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "PurchaseGoodsReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("CreatePurchaseGoodsReturn", vm);
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
