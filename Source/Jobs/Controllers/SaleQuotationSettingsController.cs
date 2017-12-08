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
    public class SaleQuotationSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ISaleQuotationSettingsService _SaleQuotationSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public SaleQuotationSettingsController(ISaleQuotationSettingsService SaleQuotationSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _SaleQuotationSettingsService = SaleQuotationSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.UnitConversionForList = (from p in db.UnitConversonFor
                                             select p).ToList();
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        // GET: /SaleQuotationSettingsMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag(id);
            var settings = new SaleQuotationSettingsService(_unitOfWork).GetSaleQuotationSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                SaleQuotationSettingsViewModel vm = new SaleQuotationSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                SaleQuotationSettingsViewModel temp = AutoMapper.Mapper.Map<SaleQuotationSettings, SaleQuotationSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SaleQuotationSettingsViewModel vm)
        {
            SaleQuotationSettings pt = AutoMapper.Mapper.Map<SaleQuotationSettingsViewModel, SaleQuotationSettings>(vm);

            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");


            if (ModelState.IsValid)
            {

                if (vm.SaleQuotationSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _SaleQuotationSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.SaleQuotationSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "SaleQuotationHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    SaleQuotationSettings temp = db.SaleQuotationSettings.Find(pt.SaleQuotationSettingsId);

                    SaleQuotationSettings ExRec = Mapper.Map<SaleQuotationSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.CalculationId = pt.CalculationId;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleUnitConversionFor = pt.isVisibleUnitConversionFor;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleCreditDays = pt.isVisibleCreditDays;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleAgent = pt.isVisibleAgent;
                    temp.isVisiblePaymentTerms = pt.isVisiblePaymentTerms;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleDoorDelivery = pt.isVisibleDoorDelivery;
                    temp.isVisibleFromSaleEnquiry = pt.isVisibleFromSaleEnquiry;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;

                    GetChanges(db, temp);


                    _SaleQuotationSettingsService.Update(temp);

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
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.SaleQuotationSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "SaleQuotationHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm.DocTypeId);
            return View("Create", vm);
        }











        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        private void GetChanges(ApplicationDbContext con, SaleQuotationSettings settings)
        {

            var Entry = con.Entry<SaleQuotationSettings>(settings);

            var PropNames = Entry.CurrentValues.PropertyNames;

            var ModifiedRecords = (from name in PropNames
                                   where Entry.Property(name).IsModified
                                   select name).ToList();

        }
    }
}
