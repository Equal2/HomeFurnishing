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
using System.Xml.Linq;
using AutoMapper;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobOrderInspectionSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobOrderInspectionSettingsService _JobOrderInspectionSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobOrderInspectionSettingsController(IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobOrderInspectionSettingsService = new JobOrderInspectionSettingsService(db);
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /JobOrderInspectionSettingsMaster/Create

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
            var settings = _JobOrderInspectionSettingsService.GetJobOrderInspectionSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobOrderInspectionSettingsViewModel vm = new JobOrderInspectionSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobOrderInspectionSettingsViewModel temp = AutoMapper.Mapper.Map<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobOrderInspectionSettingsViewModel vm)
        {
            JobOrderInspectionSettings pt = AutoMapper.Mapper.Map<JobOrderInspectionSettingsViewModel, JobOrderInspectionSettings>(vm);

            if (vm.ProcessId == 0)
                ModelState.AddModelError("ProcessId", "The Process field is required.");

            if (ModelState.IsValid)
            {

                if (vm.JobOrderInspectionSettingsId <= 0)
                {

                    _JobOrderInspectionSettingsService.Create(pt, User.Identity.Name);

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
                        DocId = pt.JobOrderInspectionSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobOrderInspectionHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobOrderInspectionSettings temp = _JobOrderInspectionSettingsService.Find(pt.JobOrderInspectionSettingsId);

                    JobOrderInspectionSettings ExRec = Mapper.Map<JobOrderInspectionSettings>(temp);

                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;

                    _JobOrderInspectionSettingsService.Update(temp, User.Identity.Name);

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
                        DocId = temp.JobOrderInspectionSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobOrderInspectionHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
            }
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
    }
}
