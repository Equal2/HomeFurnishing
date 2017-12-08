using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Core.Common;
using Model.ViewModel;
using System.Xml.Linq;
using AutoMapper;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class SaleInvoiceSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleInvoiceSettingService _SaleInvoiceSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleInvoiceSettingController(ISaleInvoiceSettingService SaleInvoiceSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleInvoiceSettingService = SaleInvoiceSettingService;
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
                
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.GodownList = new GodownService(_unitOfWork).GetGodownList(SiteId).ToList();


            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();

            ViewBag.DealUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
        }


        // GET: /SaleInvoiceSettingMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //ViewBag.UnitConvForList = (from p in db.UnitConversonFor
            //                           select p).ToList();
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                SaleInvoiceSettingsViewModel vm = new SaleInvoiceSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                PrepareViewBag();
                return View("Create", vm);
            }
            else
            {
                SaleInvoiceSettingsViewModel temp = AutoMapper.Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                PrepareViewBag();
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleInvoiceSettingsViewModel vm)
        {
            SaleInvoiceSetting pt = AutoMapper.Mapper.Map<SaleInvoiceSettingsViewModel, SaleInvoiceSetting>(vm);

            if (pt.CalculationId <= 0)
                ModelState.AddModelError("CalculationId", "The Calculation field is required");

            if (ModelState.IsValid)
            {

                if (vm.SaleInvoiceSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceSettingService.Create(pt);

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
                        DocId = pt.SaleInvoiceSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "DirectSaleInvoiceHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleInvoiceSetting temp = _SaleInvoiceSettingService.Find(pt.SaleInvoiceSettingId);

                    SaleInvoiceSetting ExRec = Mapper.Map<SaleInvoiceSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.DocTypeDispatchReturnId = pt.DocTypeDispatchReturnId;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.DocTypePackingHeaderId = pt.DocTypePackingHeaderId;
                    temp.SaleDispatchDocTypeId = pt.SaleDispatchDocTypeId;
                    temp.SaleInvoiceReturnDocTypeId = pt.SaleInvoiceReturnDocTypeId;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleAgent = pt.isVisibleAgent;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleProductUid = pt.isVisibleProductUid;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleBaleNo = pt.isVisibleBaleNo;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.isVisiblePromoCode = pt.isVisiblePromoCode;
                    temp.isVisibleForSaleOrder = pt.isVisibleForSaleOrder;
                    temp.isVisibleWeight = pt.isVisibleWeight;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;
                    temp.isVisibleFreeQty = pt.isVisibleFreeQty;
                    temp.isVisibleRewardPoints = pt.isVisibleRewardPoints;
                    temp.isVisibleCreditDays = pt.isVisibleCreditDays;
                    temp.isVisibleTermsAndConditions = pt.isVisibleTermsAndConditions;
                    temp.isVisibleShipToPartyAddress = pt.isVisibleShipToPartyAddress;
                    temp.DoNotUpdateProductUidStatus = pt.DoNotUpdateProductUidStatus;
                    temp.CurrencyId = pt.CurrencyId;
                    temp.DeliveryTermsId = pt.DeliveryTermsId;
                    temp.ShipMethodId = pt.ShipMethodId;
                    temp.ProcessId = pt.ProcessId;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.GodownId = pt.GodownId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleInvoiceSettingService.Update(temp);

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
                        DocId = temp.SaleInvoiceSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "DirectSaleInvoiceHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("Create", vm);
        }




        public ActionResult CreateForReturn(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new SaleInvoiceSettingService(_unitOfWork).GetSaleInvoiceSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                SaleInvoiceSettingsViewModel vm = new SaleInvoiceSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForReturn", vm);
            }
            else
            {
                SaleInvoiceSettingsViewModel temp = AutoMapper.Mapper.Map<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForReturn", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostForReturn(SaleInvoiceSettingsViewModel vm)
        {
            SaleInvoiceSetting pt = AutoMapper.Mapper.Map<SaleInvoiceSettingsViewModel, SaleInvoiceSetting>(vm);

            if (pt.CalculationId <= 0)
                ModelState.AddModelError("CalculationId", "The Calculation field is required");

            if (ModelState.IsValid)
            {

                if (vm.SaleInvoiceSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleInvoiceSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForReturn", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleInvoiceSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "SaleInvoiceReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleInvoiceSetting temp = _SaleInvoiceSettingService.Find(pt.SaleInvoiceSettingId);

                    SaleInvoiceSetting ExRec = Mapper.Map<SaleInvoiceSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.DocTypeDispatchReturnId = pt.DocTypeDispatchReturnId;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.DocTypePackingHeaderId = pt.DocTypePackingHeaderId;
                    temp.SaleDispatchDocTypeId = pt.SaleDispatchDocTypeId;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleAgent = pt.isVisibleAgent;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleProductUid = pt.isVisibleProductUid;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleBaleNo = pt.isVisibleBaleNo;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.isVisibleForSaleOrder = pt.isVisibleForSaleOrder;
                    temp.isVisibleWeight = pt.isVisibleWeight;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;
                    temp.isVisibleFreeQty = pt.isVisibleFreeQty;
                    temp.isVisibleRewardPoints = pt.isVisibleRewardPoints;
                    temp.isVisibleCreditDays = pt.isVisibleCreditDays;
                    temp.isVisibleTermsAndConditions = pt.isVisibleTermsAndConditions;
                    temp.isVisibleShipToPartyAddress = pt.isVisibleShipToPartyAddress;
                    temp.DoNotUpdateProductUidStatus = pt.DoNotUpdateProductUidStatus;
                    temp.CurrencyId = pt.CurrencyId;
                    temp.DeliveryTermsId = pt.DeliveryTermsId;
                    temp.ShipMethodId = pt.ShipMethodId;
                    temp.ProcessId = pt.ProcessId;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.GodownId = pt.GodownId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleInvoiceSettingService.Update(temp);

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
                        return View("CreateForReturn", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleInvoiceSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "SaleInvoiceReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForReturn", vm);
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
