using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Model.ViewModel;
using Model.ViewModels;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class JobReceiveQASettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobReceiveQASettingsService _JobReceiveQASettingsService;
        IExceptionHandlingService _exception;
        public JobReceiveQASettingsController(IExceptionHandlingService exec)
        {
            _JobReceiveQASettingsService = new JobReceiveQASettingsService(db);
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        // GET: /JobReceiveQASettingsMaster/Create
        
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
            var settings = _JobReceiveQASettingsService.GetJobReceiveQASettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobReceiveQASettingsViewModel vm = new JobReceiveQASettingsViewModel();
                vm.DocTypeName = db.DocumentType.Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobReceiveQASettingsViewModel temp = AutoMapper.Mapper.Map<JobReceiveQASettings, JobReceiveQASettingsViewModel>(settings);
                temp.DocTypeName = db.DocumentType.Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobReceiveQASettingsViewModel vm)
        {
            JobReceiveQASettings pt = AutoMapper.Mapper.Map<JobReceiveQASettingsViewModel, JobReceiveQASettings>(vm);

            if (vm.ProcessId == 0)
                ModelState.AddModelError("ProcessId", "The Process field is required.");

            if (ModelState.IsValid)
            {

                if (vm.JobReceiveQASettingsId <= 0)
                {
                    
                    _JobReceiveQASettingsService.Create(pt,User.Identity.Name);

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
                        DocId = pt.JobReceiveQASettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));

                    return RedirectToAction("Index", "JobReceiveQAHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    JobReceiveQASettings temp = _JobReceiveQASettingsService.Find(pt.JobReceiveQASettingsId);

                    JobReceiveQASettings ExRec = Mapper.Map<JobReceiveQASettings>(temp);
                   
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.DocumentPrint = pt.DocumentPrint;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isMandatoryProductUID = pt.isMandatoryProductUID;
                    temp.isVisibleMarks = pt.isVisibleMarks;
                    temp.isVisibleDealUnit = pt.isVisibleDealUnit;
                    temp.IsVisibleInspectedQty = pt.IsVisibleInspectedQty;
                    temp.IsVisiblePenalty = pt.IsVisiblePenalty;
                    temp.IsVisibleSpecification = pt.IsVisibleSpecification;
                    temp.IsVisibleWeight = pt.IsVisibleWeight;
                    temp.IsVisibleLength = pt.IsVisibleLength;
                    temp.IsVisibleWidth = pt.IsVisibleWidth;
                    temp.IsVisibleHeight = pt.IsVisibleHeight;
                    
                    _JobReceiveQASettingsService.Update(temp,User.Identity.Name);

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
                        DocId = temp.JobReceiveQASettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobReceiveQAHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            //PrepareViewBag();
            return View("Create", vm);
        }  

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            GC.SuppressFinalize(this);
            base.Dispose(disposing);
        }
    }
}
