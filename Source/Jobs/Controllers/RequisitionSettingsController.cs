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
    public class RequisitionSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IRequisitionSettingService _RequisitionSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public RequisitionSettingController(IRequisitionSettingService RequisitionSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _RequisitionSettingService = RequisitionSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /RequisitionSettingMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                RequisitionSettingsViewModel vm = new RequisitionSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                RequisitionSettingsViewModel temp = AutoMapper.Mapper.Map<RequisitionSetting, RequisitionSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(RequisitionSettingsViewModel vm)
        {
            RequisitionSetting pt = AutoMapper.Mapper.Map<RequisitionSettingsViewModel, RequisitionSetting>(vm);           

            if (ModelState.IsValid)
            {

                if (vm.RequisitionSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _RequisitionSettingService.Create(pt);

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
                        DocId = pt.RequisitionSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "MaterialRequestHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    RequisitionSetting temp = _RequisitionSettingService.Find(pt.RequisitionSettingId);

                    RequisitionSetting ExRec = Mapper.Map<RequisitionSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;                    
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.ProcessId = pt.ProcessId;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.OnSubmitMenuId = pt.OnSubmitMenuId;
                    temp.OnApproveMenuId = pt.OnApproveMenuId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _RequisitionSettingService.Update(temp);

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
                        DocId = temp.RequisitionSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));


                    return RedirectToAction("Index", "MaterialRequestHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("Create", vm);
        }

        // GET: /RequisitionSettingMaster/Create
        
        public ActionResult CreateRequisitionCancel(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.ReasonList = new ReasonService(_unitOfWork).GetReasonList(TransactionDocCategoryConstants.RequisitionCancel).ToList();
            //PrepareViewBag();
            var settings = new RequisitionSettingService(_unitOfWork).GetRequisitionSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                RequisitionSettingsViewModel vm = new RequisitionSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateRequisitionCancel", vm);
            }
            else
            {
                RequisitionSettingsViewModel temp = AutoMapper.Mapper.Map<RequisitionSetting, RequisitionSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateRequisitionCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostRequisitionCancel(RequisitionSettingsViewModel vm)
        {
            RequisitionSetting pt = AutoMapper.Mapper.Map<RequisitionSettingsViewModel, RequisitionSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.RequisitionSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _RequisitionSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("CreateRequisitionCancel", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.RequisitionSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "MaterialRequestCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    RequisitionSetting temp = _RequisitionSettingService.Find(pt.RequisitionSettingId);

                    RequisitionSetting ExRec = Mapper.Map<RequisitionSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.ProcessId = pt.ProcessId;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.DefaultReasonId = pt.DefaultReasonId;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.OnSubmitMenuId = pt.OnSubmitMenuId;
                    temp.OnApproveMenuId = pt.OnApproveMenuId;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _RequisitionSettingService.Update(temp);

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
                        DocId = temp.RequisitionSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "MaterialRequestCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("CreateRequisitionCancel", vm);
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
