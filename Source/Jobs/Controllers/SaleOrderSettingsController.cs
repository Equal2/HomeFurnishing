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
    public class SaleOrderSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleOrderSettingsService _SaleOrderSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleOrderSettingsController(ISaleOrderSettingsService SaleOrderSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleOrderSettingsService = SaleOrderSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(SaleOrderSettingsViewModel s)
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

            ViewBag.id = s.DocTypeId;


            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), -10), Value = ((int)(SaleOrderPriority.Low)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), 0), Value = ((int)(SaleOrderPriority.Normal)).ToString() });
            temp.Add(new SelectListItem { Text = Enum.GetName(typeof(SaleOrderPriority), 10), Value = ((int)(SaleOrderPriority.High)).ToString() });

            if (s == null)
                ViewBag.Priority = new SelectList(temp, "Value", "Text");
            else
                ViewBag.Priority = new SelectList(temp, "Value", "Text", s.Priority);

        }


        // GET: /SaleOrderSettingMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(id, DivisionId, SiteId);

            if (settings == null)
            {
                SaleOrderSettingsViewModel vm = new SaleOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                SaleOrderSettingsViewModel temp = AutoMapper.Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                PrepareViewBag(temp);
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleOrderSettingsViewModel vm)
        {
            SaleOrderSettings pt = AutoMapper.Mapper.Map<SaleOrderSettingsViewModel, SaleOrderSettings>(vm);

            if (pt.CalculationId <= 0)
                ModelState.AddModelError("CalculationId", "The Calculation field is required");

            if (ModelState.IsValid)
            {

                if (vm.SaleOrderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleOrderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "SaleOrderHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleOrderSettings temp = _SaleOrderSettingsService.Find(pt.SaleOrderSettingsId);

                    SaleOrderSettings ExRec = Mapper.Map<SaleOrderSettings>(temp);

                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleAdvance = pt.isVisibleAdvance;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisiblePriority = pt.isVisiblePriority;
                    temp.isVisibleUnitConversionFor = pt.isVisibleUnitConversionFor;
                    temp.isVisibleBillToParty = pt.isVisibleBillToParty;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.CurrencyId = pt.CurrencyId;
                    temp.DeliveryTermsId = pt.DeliveryTermsId;
                    temp.ShipMethodId = pt.ShipMethodId;
                    temp.Priority = pt.Priority;
                    temp.ProcessId = pt.ProcessId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleOrderSettingsService.Update(temp);

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
                        PrepareViewBag(vm);
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "SaleOrderHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm);
            return View("Create", vm);
        }


        // GET: /SaleOrderSettingMaster/Create

        public ActionResult CreateForCancel(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new SaleOrderSettingsService(_unitOfWork).GetSaleOrderSettings(id, DivisionId, SiteId);

            if (settings == null)
            {
                SaleOrderSettingsViewModel vm = new SaleOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                PrepareViewBag(vm);
                return View("CreateForCancel", vm);
            }
            else
            {
                SaleOrderSettingsViewModel temp = AutoMapper.Mapper.Map<SaleOrderSettings, SaleOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                PrepareViewBag(temp);
                return View("CreateForCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostCancel(SaleOrderSettingsViewModel vm)
        {
            SaleOrderSettings pt = AutoMapper.Mapper.Map<SaleOrderSettingsViewModel, SaleOrderSettings>(vm);

            if (pt.CalculationId <= 0)
                ModelState.AddModelError("CalculationId", "The Calculation field is required");

            if (ModelState.IsValid)
            {

                if (vm.SaleOrderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleOrderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return View("CreateForCancel", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "SaleOrderCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleOrderSettings temp = _SaleOrderSettingsService.Find(pt.SaleOrderSettingsId);

                    SaleOrderSettings ExRec = Mapper.Map<SaleOrderSettings>(temp);

                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleAdvance = pt.isVisibleAdvance;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisiblePriority = pt.isVisiblePriority;
                    temp.isVisibleUnitConversionFor = pt.isVisibleUnitConversionFor;
                    temp.isVisibleBillToParty = pt.isVisibleBillToParty;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.CurrencyId = pt.CurrencyId;
                    temp.DeliveryTermsId = pt.DeliveryTermsId;
                    temp.ShipMethodId = pt.ShipMethodId;
                    temp.Priority = pt.Priority;
                    temp.ProcessId = pt.ProcessId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _SaleOrderSettingsService.Update(temp);

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
                        PrepareViewBag(vm);
                        return View("CreateForCancel", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "SaleOrderCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm);
            return View("CreateForCancel", vm);
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
