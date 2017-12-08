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
    public class JobConsumptionSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IJobConsumptionSettingsService _JobConsumptionSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public JobConsumptionSettingsController(IJobConsumptionSettingsService JobConsumptionSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _JobConsumptionSettingsService = JobConsumptionSettingsService;
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

        // GET: /JobConsumptionSettingsMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new JobConsumptionSettingsService(_unitOfWork).GetJobConsumptionSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                JobConsumptionSettingsViewModel vm = new JobConsumptionSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                JobConsumptionSettingsViewModel temp = AutoMapper.Mapper.Map<JobConsumptionSettings, JobConsumptionSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(JobConsumptionSettingsViewModel vm)
        {
            JobConsumptionSettings pt = AutoMapper.Mapper.Map<JobConsumptionSettingsViewModel, JobConsumptionSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.JobConsumptionSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _JobConsumptionSettingsService.Create(pt);

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
                        DocId = pt.JobConsumptionSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));


                    return RedirectToAction("Index", "JobConsumptionHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    //JobConsumptionSettings temp = _JobConsumptionSettingsService.Find(pt.JobConsumptionSettingsId);

                    JobConsumptionSettings temp = db.JobConsumptionSettings.Find(pt.JobConsumptionSettingsId);

                    JobConsumptionSettings ExRec = Mapper.Map<JobConsumptionSettings>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;                    
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;

                    _JobConsumptionSettingsService.Update(temp);


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
                        DocId = temp.JobConsumptionSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,                        
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "JobConsumptionHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
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
