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
    public class StockHeaderSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IStockHeaderSettingsService _StockHeaderSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public StockHeaderSettingsController(IStockHeaderSettingsService StockHeaderSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockHeaderSettingsService = StockHeaderSettingsService;
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

        // GET: /StockHeaderSettingsMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

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
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "StockIssueHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.SqlFuncCurrentStock = pt.SqlFuncCurrentStock;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.isVisibleMaterialRequest = pt.isVisibleMaterialRequest;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "StockIssueHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("Create", vm);
        }



        public ActionResult CreateStockHeader(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateStockHeader", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateStockHeader", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostStockHeader(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateStockHeader", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "StockHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.SqlFuncCurrentStock = pt.SqlFuncCurrentStock;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.isVisibleMaterialRequest = pt.isVisibleMaterialRequest;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateStockHeader", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "StockHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateStockHeader", vm);
        }



        public ActionResult CreateForReceive(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForReceive", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForReceive", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostReceive(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForReceive", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "StockReceiveHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.SqlFuncCurrentStock = pt.SqlFuncCurrentStock;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.isVisibleMaterialRequest = pt.isVisibleMaterialRequest;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isProductHelpFromStockProcess = pt.isProductHelpFromStockProcess;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateForReceive", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "StockReceiveHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForReceive", vm);
        }



        public ActionResult CreateForExchange(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForExchange", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForExchange", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostExchange(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForExchange", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "StockExchange", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.isVisibleMaterialRequest = pt.isVisibleMaterialRequest;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateForExchange", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "StockExchange", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForExchange", vm);
        }




        public ActionResult CreateForProcessTransfer(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForProcessTransfer", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForProcessTransfer", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostProcessTransfer(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForProcessTransfer", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "StockProcessTransfer", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.isVisibleMaterialRequest = pt.isVisibleMaterialRequest;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateForProcessTransfer", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "StockProcessTransfer", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForProcessTransfer", vm);
        }



        public ActionResult CreateForRateConversion(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForRateConversion", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForRateConversion", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostForRateConversion(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (vm.isPostedInLedger && !vm.AdjLedgerAccountId.HasValue)
                ModelState.AddModelError("AdjLedgerAccountId", "Ledger A/C field is required");

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForRateConversion", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "RateConversionHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.AdjLedgerAccountId = pt.AdjLedgerAccountId;
                    temp.isPostedInLedger = pt.isPostedInLedger;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateForRateConversion", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "RateConversionHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForRateConversion", vm);
        }


        public ActionResult CreateForJobConsumption(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForJobConsumption", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForJobConsumption", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostForJobConsumption(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForJobConsumption", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobConsumptionHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateForJobConsumption", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobConsumptionHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForJobConsumption", vm);
        }







        public ActionResult CreateForMaterialTransfer(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new StockHeaderSettingsService(_unitOfWork).GetStockHeaderSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                StockHeaderSettingsViewModel vm = new StockHeaderSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForMaterialTransfer", vm);
            }
            else
            {
                StockHeaderSettingsViewModel temp = AutoMapper.Mapper.Map<StockHeaderSettings, StockHeaderSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForMaterialTransfer", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostForMaterialTransfer(StockHeaderSettingsViewModel vm)
        {
            StockHeaderSettings pt = AutoMapper.Mapper.Map<StockHeaderSettingsViewModel, StockHeaderSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.StockHeaderSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _StockHeaderSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag();
                        return View("CreateForMaterialTransfer", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "MaterialTransferHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    StockHeaderSettings temp = _StockHeaderSettingsService.Find(pt.StockHeaderSettingsId);

                    StockHeaderSettings ExRec = Mapper.Map<StockHeaderSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.BarcodeStatusUpdate = pt.BarcodeStatusUpdate;
                    temp.isEditableRate = pt.isEditableRate;
                    temp.isVisibleSpecification = pt.isVisibleSpecification;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.isMandatoryRate = pt.isMandatoryRate;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.SqlFuncCurrentStock = pt.SqlFuncCurrentStock;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.filterContraProductDivisions = pt.filterContraProductDivisions;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleProductCode = pt.isVisibleProductCode;
                    temp.isVisibleRate = pt.isVisibleRate;
                    temp.isVisibleProcessLine = pt.isVisibleProcessLine;
                    temp.isVisibleProcessHeader = pt.isVisibleProcessHeader;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcHelpListReferenceDocId = pt.SqlProcHelpListReferenceDocId;
                    temp.SqlProcProductUidHelpList = pt.SqlProcProductUidHelpList;
                    temp.SqlProcGatePass = pt.SqlProcGatePass;
                    temp.isVisibleMaterialRequest = pt.isVisibleMaterialRequest;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.PersonFieldHeading = pt.PersonFieldHeading;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.NoOfPrintCopies = pt.NoOfPrintCopies;
                    temp.isVisibleStockIn = pt.isVisibleStockIn;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrintReportHeaderId = pt.DocumentPrintReportHeaderId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _StockHeaderSettingsService.Update(temp);

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
                        return View("CreateForMaterialTransfer", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.StockHeaderSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "MaterialTransferHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
            return View("CreateForMaterialTransfer", vm);
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
