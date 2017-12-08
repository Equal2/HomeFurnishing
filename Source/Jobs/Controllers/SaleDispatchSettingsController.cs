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
    public class SaleDispatchSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleDispatchSettingService _SaleDispatchSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleDispatchSettingController(ISaleDispatchSettingService SaleDispatchSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleDispatchSettingService = SaleDispatchSettingService;
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
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.GodownList = new GodownService(_unitOfWork).GetGodownList(SiteId).ToList();
            ViewBag.DealUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
        }
        // GET: /SaleDispatchSettingMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {

            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(id, DivisionId, SiteId);
            PrepareViewBag();
            if (settings == null)
            {
                SaleDispatchSettingsViewModel vm = new SaleDispatchSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                SaleDispatchSettingsViewModel temp = AutoMapper.Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleDispatchSettingsViewModel vm)
        {
            SaleDispatchSetting pt = AutoMapper.Mapper.Map<SaleDispatchSettingsViewModel, SaleDispatchSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.SaleDispatchSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleDispatchSettingService.Create(pt);

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
                        DocId = pt.SaleDispatchSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "SaleDispatchHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleDispatchSetting temp = _SaleDispatchSettingService.Find(pt.SaleDispatchSettingId);

                    SaleDispatchSetting ExRec = Mapper.Map<SaleDispatchSetting>(temp);

                    temp.isVisibleProductUid = pt.isVisibleProductUid;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleBaleNo = pt.isVisibleBaleNo;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleForSaleOrder = pt.isVisibleForSaleOrder;
                    temp.isVisibleWeight = pt.isVisibleWeight;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isVisibleFreeQty = pt.isVisibleFreeQty;

                    temp.IsMandatoryStockIn = pt.IsMandatoryStockIn;
                    

                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;                    
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.ShipMethodId = pt.ShipMethodId;
                    temp.GodownId = pt.GodownId;
                    temp.ProcessId = pt.ProcessId;
                    temp.DeliveryTermsId = pt.DeliveryTermsId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleDispatchSettingService.Update(temp);

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
                        DocId = temp.SaleDispatchSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "SaleDispatchHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("Create", vm);
        }

        public ActionResult CreateSaleDispatchReturn(int id)//DocTypeId
        {

            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new SaleDispatchSettingService(_unitOfWork).GetSaleDispatchSettingForDocument(id, DivisionId, SiteId);
            PrepareViewBag();
            if (settings == null)
            {
                SaleDispatchSettingsViewModel vm = new SaleDispatchSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateSaleDispatchReturn", vm);
            }
            else
            {
                SaleDispatchSettingsViewModel temp = AutoMapper.Mapper.Map<SaleDispatchSetting, SaleDispatchSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateSaleDispatchReturn", temp);
            }


        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostSaleDispatchReturn(SaleDispatchSettingsViewModel vm)
        {
            SaleDispatchSetting pt = AutoMapper.Mapper.Map<SaleDispatchSettingsViewModel, SaleDispatchSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.SaleDispatchSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleDispatchSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateSaleDispatchReturn", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleDispatchSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "SaleDispatchReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleDispatchSetting temp = _SaleDispatchSettingService.Find(pt.SaleDispatchSettingId);

                    SaleDispatchSetting ExRec = Mapper.Map<SaleDispatchSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.isVisibleProductUid = pt.isVisibleProductUid;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleFreeQty = pt.isVisibleFreeQty;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.ProcessId = pt.ProcessId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleDispatchSettingService.Update(temp);

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
                        return View("CreateSaleDispatchReturn", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleDispatchSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "SaleDispatchReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("CreateSaleDispatchReturn", vm);
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
