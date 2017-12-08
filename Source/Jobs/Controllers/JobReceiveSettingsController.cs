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
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobReceiveSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobReceiveSettingsService _JobReceiveSettingservice;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobReceiveSettingsController(IJobReceiveSettingsService JobReceiveSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobReceiveSettingservice = JobReceiveSettingsService;
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

        // GET: /JobReceiveSettingsMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobReceiveSettingsViewModel vm = new JobReceiveSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobReceiveSettingsViewModel temp = AutoMapper.Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobReceiveSettingsViewModel vm)
        {
            JobReceiveSettings pt = AutoMapper.Mapper.Map<JobReceiveSettingsViewModel, JobReceiveSettings>(vm);

            if (vm.ProcessId == 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobReceiveSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobReceiveSettingservice.Create(pt);

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
                        DocId = pt.JobReceiveSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobReceiveHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveSettings temp = _JobReceiveSettingservice.Find(pt.JobReceiveSettingsId);

                    JobReceiveSettings ExRec = Mapper.Map<JobReceiveSettings>(temp);

                    temp.DocTypeId = pt.DocTypeId;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;  
                    temp.isMandatoryMachine = pt.isMandatoryMachine;                                                      
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.isPostedInStock = pt.isPostedInStock;                   
                    temp.isVisibleLoss = pt.isVisibleLoss;                    
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.IsVisibleForOrderMultiple = pt.IsVisibleForOrderMultiple;
                    temp.IsMandatoryWeight = pt.IsMandatoryWeight;
                    temp.IsVisibleWeight = pt.IsVisibleWeight;
                    temp.StockQty = pt.StockQty;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.CalculationId = pt.CalculationId;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.IsVisibleIncentive = pt.IsVisibleIncentive;
                    temp.IsVisiblePenalty = pt.IsVisiblePenalty;
                    temp.IsVisiblePassQty = pt.IsVisiblePassQty;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleConsumptionDetail = pt.isVisibleConsumptionDetail;
                    temp.isVisibleByProductDetail = pt.isVisibleByProductDetail;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;

                    temp.ConsumptionProductCaption = pt.ConsumptionProductCaption;
                    temp.ConsumptionDimension1Caption = pt.ConsumptionDimension1Caption;
                    temp.ConsumptionDimension2Caption = pt.ConsumptionDimension2Caption;
                    temp.ConsumptionDimension3Caption = pt.ConsumptionDimension3Caption;
                    temp.ConsumptionDimension4Caption = pt.ConsumptionDimension4Caption;
                    temp.ByProductCaption = pt.ByProductCaption;
                    temp.ByProductDimension1Caption = pt.ByProductDimension1Caption;
                    temp.ByProductDimension2Caption = pt.ByProductDimension2Caption;
                    temp.ByProductDimension3Caption = pt.ByProductDimension3Caption;
                    temp.ByProductDimension4Caption = pt.ByProductDimension4Caption;
                    temp.isVisibleConsumptionDimension1 = pt.isVisibleConsumptionDimension1;
                    temp.isVisibleConsumptionDimension2 = pt.isVisibleConsumptionDimension2;
                    temp.isVisibleConsumptionDimension3 = pt.isVisibleConsumptionDimension3;
                    temp.isVisibleConsumptionDimension4 = pt.isVisibleConsumptionDimension4;
                    temp.isVisibleByProductDimension1 = pt.isVisibleByProductDimension1;
                    temp.isVisibleByProductDimension2 = pt.isVisibleByProductDimension2;
                    temp.isVisibleByProductDimension3 = pt.isVisibleByProductDimension3;
                    temp.isVisibleByProductDimension4 = pt.isVisibleByProductDimension4;


                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.LossPer = pt.LossPer;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ImportMenuId = pt.ImportMenuId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobReceiveSettingservice.Update(temp);

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
                        DocId = temp.JobReceiveSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobReceiveHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("Create", vm);
        }

        public ActionResult CreateJobReturn(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new JobReceiveSettingsService(_unitOfWork).GetJobReceiveSettingsForDocument(id, DivisionId, SiteId);
            PrepareViewBag();
            if (settings == null)
            {
                JobReceiveSettingsViewModel vm = new JobReceiveSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateJobReturn", vm);
            }
            else
            {
                JobReceiveSettingsViewModel temp = AutoMapper.Mapper.Map<JobReceiveSettings, JobReceiveSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateJobReturn", temp);
            }


        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostJobReturn(JobReceiveSettingsViewModel vm)
        {
            JobReceiveSettings pt = AutoMapper.Mapper.Map<JobReceiveSettingsViewModel, JobReceiveSettings>(vm);

            if (vm.ProcessId == 0)
                ModelState.AddModelError("ProcessId", "The Process field is required");

            if (ModelState.IsValid)
            {

                if (vm.JobReceiveSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobReceiveSettingservice.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateJobReturn", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobReceiveSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveSettings temp = _JobReceiveSettingservice.Find(pt.JobReceiveSettingsId);

                    JobReceiveSettings ExRec = Mapper.Map<JobReceiveSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.IsVisibleForOrderMultiple = pt.IsVisibleForOrderMultiple;
                    temp.IsMandatoryWeight = pt.IsMandatoryWeight;
                    temp.IsVisibleWeight = pt.IsVisibleWeight;
                    temp.StockQty = pt.StockQty;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcGenProductUID = pt.SqlProcGenProductUID;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isPostedInStockVirtual = pt.isPostedInStockVirtual;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.IsVisibleIncentive = pt.IsVisibleIncentive;
                    temp.IsVisiblePenalty = pt.IsVisiblePenalty;
                    temp.IsVisiblePassQty = pt.IsVisiblePassQty;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleConsumptionDetail = pt.isVisibleConsumptionDetail;
                    temp.isVisibleByProductDetail = pt.isVisibleByProductDetail;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;

                    temp.ConsumptionProductCaption = pt.ConsumptionProductCaption;
                    temp.ConsumptionDimension1Caption = pt.ConsumptionDimension1Caption;
                    temp.ConsumptionDimension2Caption = pt.ConsumptionDimension2Caption;
                    temp.ConsumptionDimension3Caption = pt.ConsumptionDimension3Caption;
                    temp.ConsumptionDimension4Caption = pt.ConsumptionDimension4Caption;
                    temp.ByProductCaption = pt.ByProductCaption;
                    temp.ByProductDimension1Caption = pt.ByProductDimension1Caption;
                    temp.ByProductDimension2Caption = pt.ByProductDimension2Caption;
                    temp.ByProductDimension3Caption = pt.ByProductDimension3Caption;
                    temp.ByProductDimension4Caption = pt.ByProductDimension4Caption;
                    temp.isVisibleConsumptionDimension1 = pt.isVisibleConsumptionDimension1;
                    temp.isVisibleConsumptionDimension2 = pt.isVisibleConsumptionDimension2;
                    temp.isVisibleConsumptionDimension3 = pt.isVisibleConsumptionDimension3;
                    temp.isVisibleConsumptionDimension4 = pt.isVisibleConsumptionDimension4;
                    temp.isVisibleByProductDimension1 = pt.isVisibleByProductDimension1;
                    temp.isVisibleByProductDimension2 = pt.isVisibleByProductDimension2;
                    temp.isVisibleByProductDimension3 = pt.isVisibleByProductDimension3;
                    temp.isVisibleByProductDimension4 = pt.isVisibleByProductDimension4;

                    temp.LossPer = pt.LossPer;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;


                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _JobReceiveSettingservice.Update(temp);

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
                        return View("CreateJobReturn", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobReceiveSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobReturnHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("CreateJobReturn", vm);
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
