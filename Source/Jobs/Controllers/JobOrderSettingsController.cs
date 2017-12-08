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
    public class JobOrderSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobOrderSettingsService _JobOrderSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobOrderSettingsController(IJobOrderSettingsService JobOrderSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderSettingsService = JobOrderSettingsService;
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

        // GET: /JobOrderSettingsMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag(id);
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobOrderSettingsViewModel vm = new JobOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobOrderSettingsViewModel temp = AutoMapper.Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderSettingsViewModel vm)
        {
            JobOrderSettings pt = AutoMapper.Mapper.Map<JobOrderSettingsViewModel, JobOrderSettings>(vm);

            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (vm.MaxDays.HasValue && vm.DueDays > vm.MaxDays.Value)
                ModelState.AddModelError("DueDays", "DueDays Exceeding MaxDueDays");


            if (ModelState.IsValid)
            {

                if (vm.JobOrderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobOrderSettingsService.Create(pt);

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
                        DocId = pt.JobOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobOrderHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobOrderSettings temp = db.JobOrderSettings.Find(pt.JobOrderSettingsId);

                    JobOrderSettings ExRec = Mapper.Map<JobOrderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductCategories = pt.filterProductCategories;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.NonCountedQty = pt.NonCountedQty;
                    temp.isVisibleFromProdOrder = pt.isVisibleFromProdOrder;
                    temp.LossQty = pt.LossQty;
                    temp.JobUnitId = pt.JobUnitId;
                    temp.DealUnitId = pt.DealUnitId;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isMandatoryGodown = pt.isMandatoryGodown;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.CalculationId = pt.CalculationId;
                    temp.ExcessQtyAllowedPer = pt.ExcessQtyAllowedPer;
                    temp.isUniqueCostCenter = pt.isUniqueCostCenter;
                    temp.PersonWiseCostCenter = pt.PersonWiseCostCenter;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.Perks = pt.Perks;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.DueDays = pt.DueDays;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.isVisibleUncountableQty = pt.isVisibleUncountableQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleLineDueDate = pt.isVisibleLineDueDate;
                    temp.isVisibleBillToParty = pt.isVisibleBillToParty;
                    temp.isVisibleUnitConversionFor = pt.isVisibleUnitConversionFor;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleCreditDays = pt.isVisibleCreditDays;


                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;


                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;

                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleShipToAddress = pt.isVisibleShipToAddress;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleDocumentShipMethod = pt.isVisibleDocumentShipMethod;
                    temp.isVisibleTransporter = pt.isVisibleTransporter;
                    temp.isVisibleAgent = pt.isVisibleAgent;
                    temp.isVisibleDoorDelivery = pt.isVisibleDoorDelivery;
                    temp.isVisiblePaymentTerms = pt.isVisiblePaymentTerms;

                    temp.NonCountedQtyCaption = pt.NonCountedQtyCaption;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.MaxDays = pt.MaxDays;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;

                    GetChanges(db, temp);


                    _JobOrderSettingsService.Update(temp);

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
                        DocId = temp.JobOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobOrderHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm.DocTypeId);
            return View("Create", vm);
        }

        // GET: /JobOrderSettingsMaster/Create

        public ActionResult CreateJobOrderCancel(int id)//DocTypeId
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag(id);
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobOrderSettingsViewModel vm = new JobOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateJobOrderCancel", vm);
            }
            else
            {
                JobOrderSettingsViewModel temp = AutoMapper.Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateJobOrderCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostJobOrderCancel(JobOrderSettingsViewModel vm)
        {
            JobOrderSettings pt = AutoMapper.Mapper.Map<JobOrderSettingsViewModel, JobOrderSettings>(vm);

            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobOrderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobOrderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("CreateJobOrderCancel", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobOrderCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobOrderSettings temp = db.JobOrderSettings.Find(pt.JobOrderSettingsId);

                    JobOrderSettings ExRec = Mapper.Map<JobOrderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProductCategories = pt.filterProductCategories;
                    temp.filterProducts = pt.filterProducts;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.NonCountedQty = pt.NonCountedQty;
                    temp.isUniqueCostCenter = pt.isUniqueCostCenter;
                    temp.isVisibleFromProdOrder = pt.isVisibleFromProdOrder;
                    temp.LossQty = pt.LossQty;
                    temp.ExcessQtyAllowedPer = pt.ExcessQtyAllowedPer;
                    temp.JobUnitId = pt.JobUnitId;
                    temp.DealUnitId = pt.DealUnitId;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isMandatoryGodown = pt.isMandatoryGodown;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.Perks = pt.Perks;
                    temp.DueDays = pt.DueDays;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.isVisibleUncountableQty = pt.isVisibleUncountableQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleLineDueDate = pt.isVisibleLineDueDate;
                    temp.isVisibleBillToParty = pt.isVisibleBillToParty;
                    temp.isVisibleUnitConversionFor = pt.isVisibleUnitConversionFor;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleCreditDays = pt.isVisibleCreditDays;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;
                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleShipToAddress = pt.isVisibleShipToAddress;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleDocumentShipMethod = pt.isVisibleDocumentShipMethod;
                    temp.isVisibleTransporter = pt.isVisibleTransporter;
                    temp.isVisibleAgent = pt.isVisibleAgent;
                    temp.isVisibleDoorDelivery = pt.isVisibleDoorDelivery;
                    temp.isVisiblePaymentTerms = pt.isVisiblePaymentTerms;

                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.NonCountedQtyCaption = pt.NonCountedQtyCaption;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobOrderSettingsService.Update(temp);

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
                        return View("CreateJobOrderCancel", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobOrderCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm.DocTypeId);
            return View("CreateJobOrderCancel", vm);
        }





        // GET: /JobOrderSettingsMaster/Create

        public ActionResult CreateJobOrderAmendment(int id)//DocTypeId
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag(id);
            var settings = new JobOrderSettingsService(_unitOfWork).GetJobOrderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobOrderSettingsViewModel vm = new JobOrderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateJobOrderAmendment", vm);
            }
            else
            {
                JobOrderSettingsViewModel temp = AutoMapper.Mapper.Map<JobOrderSettings, JobOrderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateJobOrderAmendment", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostJobOrderAmendment(JobOrderSettingsViewModel vm)
        {
            JobOrderSettings pt = AutoMapper.Mapper.Map<JobOrderSettingsViewModel, JobOrderSettings>(vm);

            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobOrderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobOrderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("CreateJobOrderAmendment", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobOrderAmendmentHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobOrderSettings temp = db.JobOrderSettings.Find(pt.JobOrderSettingsId);

                    JobOrderSettings ExRec = Mapper.Map<JobOrderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProductCategories = pt.filterProductCategories;
                    temp.filterProducts = pt.filterProducts;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isVisibleJobWorkerLine = pt.isVisibleJobWorkerLine;
                    temp.NonCountedQty = pt.NonCountedQty;
                    temp.isMandatoryGodown = pt.isMandatoryGodown;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.CalculationId = pt.CalculationId;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.Perks = pt.Perks;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isVisibleFromProdOrder = pt.isVisibleFromProdOrder;
                    temp.LossQty = pt.LossQty;
                    temp.JobUnitId = pt.JobUnitId;
                    temp.ExcessQtyAllowedPer = pt.ExcessQtyAllowedPer;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.DueDays = pt.DueDays;
                    temp.isVisibleUncountableQty = pt.isVisibleUncountableQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.UnitConversionForId = pt.UnitConversionForId;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.DealUnitId = pt.DealUnitId;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleDiscountPer = pt.isVisibleDiscountPer;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.isUniqueCostCenter = pt.isUniqueCostCenter;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleRate = pt.isVisibleRate;

                    temp.isVisibleDeliveryTerms = pt.isVisibleDeliveryTerms;
                    temp.isVisibleShipToAddress = pt.isVisibleShipToAddress;
                    temp.isVisibleCurrency = pt.isVisibleCurrency;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleShipMethod = pt.isVisibleShipMethod;
                    temp.isVisibleDocumentShipMethod = pt.isVisibleDocumentShipMethod;
                    temp.isVisibleTransporter = pt.isVisibleTransporter;
                    temp.isVisibleAgent = pt.isVisibleAgent;
                    temp.isVisibleDoorDelivery = pt.isVisibleDoorDelivery;
                    temp.isVisiblePaymentTerms = pt.isVisiblePaymentTerms;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesExecutive = pt.isVisibleSalesExecutive;

                    temp.CalculateDiscountOnRate = pt.CalculateDiscountOnRate;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.NonCountedQtyCaption = pt.NonCountedQtyCaption;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobOrderSettingsService.Update(temp);

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
                        return View("CreateJobOrderAmendment", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobOrderAmendmentHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm.DocTypeId);
            return View("CreateJobOrderAmendment", vm);
        }






        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        private void GetChanges(ApplicationDbContext con, JobOrderSettings settings)
        {

            var Entry = con.Entry<JobOrderSettings>(settings);

            var PropNames = Entry.CurrentValues.PropertyNames;

            var ModifiedRecords = (from name in PropNames
                                   where Entry.Property(name).IsModified
                                   select name).ToList();

        }
    }
}
