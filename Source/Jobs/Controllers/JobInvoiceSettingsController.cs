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
    public class JobInvoiceSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobInvoiceSettingsService _JobInvoiceSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobInvoiceSettingsController(IJobInvoiceSettingsService JobInvoiceSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobInvoiceSettingsService = JobInvoiceSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        public void PrepareViewBag()
        {
            ViewBag.UnitConversionForList = (from p in db.UnitConversonFor
                                             select p).ToList();
        }

        // GET: /JobInvoiceSettingsMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobInvoiceSettingsViewModel vm = new JobInvoiceSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobInvoiceSettingsViewModel temp = AutoMapper.Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobInvoiceSettingsViewModel vm)
        {
            JobInvoiceSettings pt = AutoMapper.Mapper.Map<JobInvoiceSettingsViewModel, JobInvoiceSettings>(vm);


            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobInvoiceSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobInvoiceSettingsService.Create(pt);

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
                        DocId = pt.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobInvoiceHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceSettings temp = _JobInvoiceSettingsService.Find(pt.JobInvoiceSettingsId);

                    JobInvoiceSettings ExRec = Mapper.Map<JobInvoiceSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryJobOrder = pt.isMandatoryJobOrder;
                    temp.isMandatoryJobReceive = pt.isMandatoryJobReceive;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isVisibleHeaderJobWorker = pt.isVisibleHeaderJobWorker;
                    temp.isPostedInStock = pt.isPostedInStock;
                    
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleWeight = pt.isVisibleWeight;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleGovtInvoiceNo = pt.isVisibleGovtInvoiceNo;


                    temp.IsVisibleDocQty = pt.IsVisibleDocQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.IsVisibleReceiveQty = pt.IsVisibleReceiveQty;
                    temp.IsVisiblePassQty = pt.IsVisiblePassQty;
                    temp.IsVisibleRate = pt.IsVisibleRate;
                    temp.IsVisibleAdditionalCharges = pt.IsVisibleAdditionalCharges;

                    temp.isVisibleIncentive = pt.isVisibleIncentive;
                    temp.isVisiblePenalty = pt.isVisiblePenalty;
                    temp.isVisibleJobOrder = pt.isVisibleJobOrder;
                    temp.isVisibleJobReceive = pt.isVisibleJobReceive;

                    temp.isVisibleRateDiscountPer = pt.isVisibleRateDiscountPer;
                    temp.isVisibleMfgDate = pt.isVisibleMfgDate;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;

                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleJobReceiveBy = pt.isVisibleJobReceiveBy;



                    temp.isGenerateProductUid = pt.isGenerateProductUid;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;

                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.isAutoCreateJobReceive = pt.isAutoCreateJobReceive;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.CalculationId = pt.CalculationId;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobInvoiceSettingsService.Update(temp);

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
                        DocId = temp.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobInvoiceHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("Create", vm);
        }





        public ActionResult CreateInvoiceReceive(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobInvoiceSettingsViewModel vm = new JobInvoiceSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateInvoiceReceive", vm);
            }
            else
            {
                JobInvoiceSettingsViewModel temp = AutoMapper.Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateInvoiceReceive", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostInvoiceReceive(JobInvoiceSettingsViewModel vm)
        {
            JobInvoiceSettings pt = AutoMapper.Mapper.Map<JobInvoiceSettingsViewModel, JobInvoiceSettings>(vm);


            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (!vm.JobReceiveDocTypeId.HasValue)
                ModelState.AddModelError("JobReceiveDocTypeId", "The Receive DocType field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobInvoiceSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobInvoiceSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateInvoiceReceive", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobInvoiceReceiveHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceSettings temp = _JobInvoiceSettingsService.Find(pt.JobInvoiceSettingsId);

                    JobInvoiceSettings ExRec = Mapper.Map<JobInvoiceSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryJobOrder = pt.isMandatoryJobOrder;
                    temp.isMandatoryJobReceive = pt.isMandatoryJobReceive;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.JobReceiveDocTypeId = pt.JobReceiveDocTypeId;
                    temp.isVisibleHeaderJobWorker = pt.isVisibleHeaderJobWorker;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.isVisibleWeight = pt.isVisibleWeight;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleIncentive = pt.isVisibleIncentive;
                    temp.isVisiblePenalty = pt.isVisiblePenalty;
                    temp.isVisibleJobOrder = pt.isVisibleJobOrder;
                    temp.isVisibleJobReceive = pt.isVisibleJobReceive;
                    temp.isVisibleRateDiscountPer = pt.isVisibleRateDiscountPer;
                    temp.isVisibleMfgDate = pt.isVisibleMfgDate;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleJobReceiveBy = pt.isVisibleJobReceiveBy;
                    temp.isVisibleGovtInvoiceNo = pt.isVisibleGovtInvoiceNo;

                    temp.IsVisibleDocQty = pt.IsVisibleDocQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.IsVisibleReceiveQty = pt.IsVisibleReceiveQty;
                    temp.IsVisiblePassQty = pt.IsVisiblePassQty;
                    temp.IsVisibleRate = pt.IsVisibleRate;
                    temp.IsVisibleAdditionalCharges = pt.IsVisibleAdditionalCharges;

                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.isAutoCreateJobReceive = pt.isAutoCreateJobReceive;
                    temp.isGenerateProductUid = pt.isGenerateProductUid;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.CalculationId = pt.CalculationId;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobInvoiceSettingsService.Update(temp);

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
                        return View("CreateInvoiceReceive", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobInvoiceReceiveHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateInvoiceReceive", vm);
        }

        // GET: /JobInvoiceSettingsMaster/Create

        public ActionResult CreateJobInvoiceAmendment(int id)//DocTypeId
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobInvoiceSettingsViewModel vm = new JobInvoiceSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateJobInvoiceAmendment", vm);
            }
            else
            {
                JobInvoiceSettingsViewModel temp = AutoMapper.Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateJobInvoiceAmendment", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostJobInvoiceAmendment(JobInvoiceSettingsViewModel vm)
        {
            JobInvoiceSettings pt = AutoMapper.Mapper.Map<JobInvoiceSettingsViewModel, JobInvoiceSettings>(vm);

            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobInvoiceSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobInvoiceSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateJobInvoiceAmendment", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobInvoiceAmendmentHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceSettings temp = _JobInvoiceSettingsService.Find(pt.JobInvoiceSettingsId);

                    JobInvoiceSettings ExRec = Mapper.Map<JobInvoiceSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryJobOrder = pt.isMandatoryJobOrder;
                    temp.isMandatoryJobReceive = pt.isMandatoryJobReceive;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.JobReceiveDocTypeId = pt.JobReceiveDocTypeId;
                    temp.isVisibleHeaderJobWorker = pt.isVisibleHeaderJobWorker;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleIncentive = pt.isVisibleIncentive;
                    temp.isVisiblePenalty = pt.isVisiblePenalty;
                    temp.isVisibleJobOrder = pt.isVisibleJobOrder;
                    temp.isVisibleJobReceive = pt.isVisibleJobReceive;
                    temp.isVisibleRateDiscountPer = pt.isVisibleRateDiscountPer;
                    temp.isVisibleMfgDate = pt.isVisibleMfgDate;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleJobReceiveBy = pt.isVisibleJobReceiveBy;
                    temp.isVisibleGovtInvoiceNo = pt.isVisibleGovtInvoiceNo;

                    temp.IsVisibleDocQty = pt.IsVisibleDocQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.IsVisibleReceiveQty = pt.IsVisibleReceiveQty;
                    temp.IsVisiblePassQty = pt.IsVisiblePassQty;
                    temp.IsVisibleRate = pt.IsVisibleRate;
                    temp.IsVisibleAdditionalCharges = pt.IsVisibleAdditionalCharges;

                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.isAutoCreateJobReceive = pt.isAutoCreateJobReceive;
                    temp.isGenerateProductUid = pt.isGenerateProductUid;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.CalculationId = pt.CalculationId;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobInvoiceSettingsService.Update(temp);

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
                        return View("CreateJobInvoiceAmendment", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobInvoiceAmendmentHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateJobInvoiceAmendment", vm);
        }


        public ActionResult CreateJobInvoiceReturn(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new JobInvoiceSettingsService(_unitOfWork).GetJobInvoiceSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobInvoiceSettingsViewModel vm = new JobInvoiceSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateJobInvoiceReturn", vm);
            }
            else
            {
                JobInvoiceSettingsViewModel temp = AutoMapper.Mapper.Map<JobInvoiceSettings, JobInvoiceSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateJobInvoiceReturn", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostJobInvoiceReturn(JobInvoiceSettingsViewModel vm)
        {
            JobInvoiceSettings pt = AutoMapper.Mapper.Map<JobInvoiceSettingsViewModel, JobInvoiceSettings>(vm);


            if (vm.ProcessId <= 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (!vm.JobReturnDocTypeId.HasValue)
                ModelState.AddModelError("DocTypeJobReturnId", "The return doctype is required");

            if (ModelState.IsValid)
            {

                if (vm.JobInvoiceSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobInvoiceSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateJobInvoiceReturn", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobInvoiceReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobInvoiceSettings temp = _JobInvoiceSettingsService.Find(pt.JobInvoiceSettingsId);

                    JobInvoiceSettings ExRec = Mapper.Map<JobInvoiceSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryJobOrder = pt.isMandatoryJobOrder;
                    temp.isMandatoryJobReceive = pt.isMandatoryJobReceive;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.JobReturnDocTypeId = pt.JobReturnDocTypeId;
                    temp.isVisibleHeaderJobWorker = pt.isVisibleHeaderJobWorker;
                    temp.isPostedInStock = pt.isPostedInStock;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleIncentive = pt.isVisibleIncentive;
                    temp.isVisiblePenalty = pt.isVisiblePenalty;
                    temp.isVisibleJobOrder = pt.isVisibleJobOrder;
                    temp.isVisibleJobReceive = pt.isVisibleJobReceive;
                    temp.isVisibleRateDiscountPer = pt.isVisibleRateDiscountPer;
                    temp.isVisibleMfgDate = pt.isVisibleMfgDate;
                    temp.isVisibleFinancier = pt.isVisibleFinancier;
                    temp.isVisibleSalesTaxGroupPerson = pt.isVisibleSalesTaxGroupPerson;
                    temp.isVisibleSalesTaxGroupProduct = pt.isVisibleSalesTaxGroupProduct;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleJobReceiveBy = pt.isVisibleJobReceiveBy;
                    temp.isVisibleGovtInvoiceNo = pt.isVisibleGovtInvoiceNo;

                    temp.IsVisibleDocQty = pt.IsVisibleDocQty;
                    temp.isVisibleLoss = pt.isVisibleLoss;
                    temp.IsVisibleReceiveQty = pt.IsVisibleReceiveQty;
                    temp.IsVisiblePassQty = pt.IsVisiblePassQty;
                    temp.IsVisibleRate = pt.IsVisibleRate;
                    temp.IsVisibleAdditionalCharges = pt.IsVisibleAdditionalCharges;

                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.isAutoCreateJobReceive = pt.isAutoCreateJobReceive;
                    temp.isGenerateProductUid = pt.isGenerateProductUid;
                    temp.SalesTaxGroupPersonId = pt.SalesTaxGroupPersonId;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.CalculationId = pt.CalculationId;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.AmountRoundOff = pt.AmountRoundOff;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobInvoiceSettingsService.Update(temp);

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
                        return View("CreateJobInvoiceReturn", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobInvoiceSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobInvoiceReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateJobInvoiceReturn", vm);
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
