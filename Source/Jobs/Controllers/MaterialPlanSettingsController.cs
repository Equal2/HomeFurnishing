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
    public class MaterialPlanSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IMaterialPlanSettingsService _MaterialPlanSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public MaterialPlanSettingsController(IMaterialPlanSettingsService MaterialPlanSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _MaterialPlanSettingsService = MaterialPlanSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }


        //public void PrepareViewBag()
        //{
        //    ViewBag.UnitConversionForList = (from p in db.UnitConversonFor
        //                                     select p).ToList();
        //}

        // GET: /MaterialPlanSettingsMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                MaterialPlanSettingsViewModel vm = new MaterialPlanSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                MaterialPlanSettingsViewModel temp = AutoMapper.Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(MaterialPlanSettingsViewModel vm)
        {
            MaterialPlanSettings pt = AutoMapper.Mapper.Map<MaterialPlanSettingsViewModel, MaterialPlanSettings>(vm);

            if (vm.DocTypeProductionOrderId == 0 || !vm.DocTypeProductionOrderId.HasValue)
                ModelState.AddModelError("DocTypeProductionOrderId", "Production Order Doctype field is required.");

            if (vm.DocTypePurchaseIndentId == 0 || !vm.DocTypePurchaseIndentId.HasValue)
                ModelState.AddModelError("DocTypePurchaseIndentId", "Production Order Doctype field is required.");

            if (ModelState.IsValid)
            {

                if (vm.MaterialPlanSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _MaterialPlanSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.MaterialPlanSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "MaterialPlanHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    MaterialPlanSettings temp = _MaterialPlanSettingsService.Find(pt.MaterialPlanSettingsId);

                    MaterialPlanSettings ExRec = Mapper.Map<MaterialPlanSettings>(temp);


                    temp.filterContraDocTypes = pt.filterContraDocTypes;                   
                    temp.filterProcesses = pt.filterProcesses;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;                  
                    temp.SqlProcConsumption = pt.SqlProcConsumption;                    
                    temp.PendingProdOrderList = pt.PendingProdOrderList;
                    temp.DocTypeProductionOrderId = pt.DocTypeProductionOrderId;
                    temp.DocTypePurchaseIndentId = pt.DocTypePurchaseIndentId;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isVisiblePurchPlanQty = pt.isVisiblePurchPlanQty;
                    temp.isVisibleProdPlanQty = pt.isVisibleProdPlanQty;
                    temp.PlanType = pt.PlanType;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _MaterialPlanSettingsService.Update(temp);

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
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.MaterialPlanSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "MaterialPlanHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }            
            return View("Create", vm);
        }

        public ActionResult CreateForCancel(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new MaterialPlanSettingsService(_unitOfWork).GetMaterialPlanSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                MaterialPlanSettingsViewModel vm = new MaterialPlanSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForCancel", vm);
            }
            else
            {
                MaterialPlanSettingsViewModel temp = AutoMapper.Mapper.Map<MaterialPlanSettings, MaterialPlanSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostForCancel(MaterialPlanSettingsViewModel vm)
        {
            MaterialPlanSettings pt = AutoMapper.Mapper.Map<MaterialPlanSettingsViewModel, MaterialPlanSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.MaterialPlanSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _MaterialPlanSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("CreateForCancel", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.MaterialPlanSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "MaterialPlanCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    MaterialPlanSettings temp = _MaterialPlanSettingsService.Find(pt.MaterialPlanSettingsId);

                    MaterialPlanSettings ExRec = Mapper.Map<MaterialPlanSettings>(temp);


                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterProcesses = pt.filterProcesses;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.SqlProcConsumption = pt.SqlProcConsumption;
                    temp.PendingProdOrderList = pt.PendingProdOrderList;
                    temp.DocTypeProductionOrderId = pt.DocTypeProductionOrderId;
                    temp.DocTypePurchaseIndentId = pt.DocTypePurchaseIndentId;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.PlanType = pt.PlanType;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _MaterialPlanSettingsService.Update(temp);

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
                        return View("CreateForCancel", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.MaterialPlanSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "MaterialPlanCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
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
