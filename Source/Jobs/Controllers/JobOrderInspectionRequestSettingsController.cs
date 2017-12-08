using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Model.ViewModels;
using Model.ViewModel;
using Core.Common;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderInspectionRequestSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobOrderInspectionRequestSettingsService _JobOrderInspectionRequestSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobOrderInspectionRequestSettingsController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderInspectionRequestSettingsService = new JobOrderInspectionRequestSettingsService(db);
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /JobOrderInspectionRequestSettingsMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = _JobOrderInspectionRequestSettingsService.GetJobOrderInspectionRequestSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobOrderInspectionRequestSettingsViewModel vm = new JobOrderInspectionRequestSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobOrderInspectionRequestSettingsViewModel temp = AutoMapper.Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderInspectionRequestSettingsViewModel vm)
        {
            JobOrderInspectionRequestSettings pt = AutoMapper.Mapper.Map<JobOrderInspectionRequestSettingsViewModel, JobOrderInspectionRequestSettings>(vm);

            if (vm.ProcessId == 0)
                ModelState.AddModelError("ProcessId", "The Process field is required.");

            if (ModelState.IsValid)
            {

                if (vm.JobOrderInspectionRequestSettingsId <= 0)
                {
                    
                    _JobOrderInspectionRequestSettingsService.Create(pt,User.Identity.Name);

                    try
                    {
                        db.SaveChanges();
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
                        DocId = pt.JobOrderInspectionRequestSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobOrderInspectionRequestHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobOrderInspectionRequestSettings temp = _JobOrderInspectionRequestSettingsService.Find(pt.JobOrderInspectionRequestSettingsId);

                    JobOrderInspectionRequestSettings ExRec = Mapper.Map<JobOrderInspectionRequestSettings>(temp);                    
                   
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    
                    _JobOrderInspectionRequestSettingsService.Update(temp,User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        //PrepareViewBag();
                        return View("Create", pt);
                    }


                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.JobOrderInspectionRequestSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobOrderInspectionRequestHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("Create", vm);
        }

        // GET: /JobOrderInspectionRequestSettingsMaster/Create
        
        public ActionResult CreateJobOrderInspectionRequestCancel(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //PrepareViewBag();
            var settings = _JobOrderInspectionRequestSettingsService.GetJobOrderInspectionRequestSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobOrderInspectionRequestSettingsViewModel vm = new JobOrderInspectionRequestSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("JobOrderInspectionRequestCancel", vm);
            }
            else
            {
                JobOrderInspectionRequestSettingsViewModel temp = AutoMapper.Mapper.Map<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("JobOrderInspectionRequestCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostJobOrderInspectionRequestCancel(JobOrderInspectionRequestSettingsViewModel vm)
        {
            JobOrderInspectionRequestSettings pt = AutoMapper.Mapper.Map<JobOrderInspectionRequestSettingsViewModel, JobOrderInspectionRequestSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.JobOrderInspectionRequestSettingsId <= 0)
                {
                    
                    _JobOrderInspectionRequestSettingsService.Create(pt,User.Identity.Name);

                    try
                    {
                        db.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return View("JobOrderInspectionRequestCancel", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.JobOrderInspectionRequestSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobOrderInspectionRequestCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobOrderInspectionRequestSettings temp = _JobOrderInspectionRequestSettingsService.Find(pt.JobOrderInspectionRequestSettingsId);

                    JobOrderInspectionRequestSettings ExRec = Mapper.Map<JobOrderInspectionRequestSettings>(temp);

                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                   
                    _JobOrderInspectionRequestSettingsService.Update(temp,User.Identity.Name);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp,
                    });

                    XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                    try
                    {
                        db.SaveChanges();
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
                        DocId = temp.JobOrderInspectionRequestSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobOrderInspectionRequestCancelHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("JobOrderInspectionRequestCancel", vm);
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
