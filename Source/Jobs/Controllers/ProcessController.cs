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
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProcessController : System.Web.Mvc.Controller
    {
          private ApplicationDbContext db = new ApplicationDbContext();

          IProcessService _ProcessService;
          IUnitOfWork _unitOfWork;
          IExceptionHandlingService _exception;
          ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

          public ProcessController(IProcessService ProcessService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
          {
              _ProcessService = ProcessService;
              _unitOfWork = unitOfWork;
              _exception = exec;

              //Log Initialization
              LogVm.SessionId = 0;
              LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
              LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
              LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
          }
        // GET: /ProcessMaster/
        
          public ActionResult Index()
          { 
              var Process = _ProcessService.GetProcessList().ToList();
              return View(Process);
              //return RedirectToAction("Create");
          }

          // GET: /ProcessMaster/Create
        
          public ActionResult Create()
          {
              Process vm = new Process();
              vm.IsActive = true;
              vm.IsAffectedStock = true;
              return View("Create",vm);
          }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
          public ActionResult Create(Process vm)
          {
              Process pt = vm; 
              if (ModelState.IsValid)
              {
                  if (vm.ProcessId == 0)
                  {
                      pt.CreatedDate = DateTime.Now;
                      pt.ModifiedDate = DateTime.Now;
                      pt.CreatedBy = User.Identity.Name;
                      pt.ModifiedBy = User.Identity.Name;
                      pt.ObjectState = Model.ObjectState.Added;
                      _ProcessService.Create(pt);
                   
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
                          DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Process).DocumentTypeId,
                          DocId = pt.ProcessId,
                          ActivityType = (int)ActivityTypeContants.Added,
                      }));

                      return RedirectToAction("Create").Success("Data saved successfully");
                  }
                  else
                  {
                      List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                      Process temp = _ProcessService.Find(pt.ProcessId);

                      Process ExRec = Mapper.Map<Process>(temp);

                      temp.ProcessName = pt.ProcessName;
                      temp.ProcessCode = pt.ProcessCode;
                      temp.ParentProcessId = pt.ParentProcessId;
                      temp.AccountId = pt.AccountId;
                      temp.IsActive = pt.IsActive;
                      temp.IsSystemDefine = pt.IsSystemDefine;
                      temp.IsAffectedStock = pt.IsAffectedStock;
                      temp.ModifiedDate = DateTime.Now;
                      temp.ModifiedBy = User.Identity.Name;
                      temp.ObjectState = Model.ObjectState.Modified;
                      _ProcessService.Update(temp);

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
                          DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Process).DocumentTypeId,
                          DocId = temp.ProcessId,
                          ActivityType = (int)ActivityTypeContants.Modified,
                          xEModifications = Modifications,
                      }));

                      return RedirectToAction("Index").Success("Data saved successfully");
                  }
              }
              return View(vm);
          }


        // GET: /ProductMaster/Edit/5
        
        public ActionResult Edit(int id)
        {
            Process pt = _ProcessService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

       // GET: /ProductMaster/Delete/5
        
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Process Process = db.Process.Find(id);
            if (Process == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if(ModelState.IsValid)
            {
                var temp = _ProcessService.Find(vm.id);
                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });              

                _ProcessService.Delete(vm.id);

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);

                try
                {
                    _unitOfWork.Save();
                }
                
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Process).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));              

                return Json(new { success = true });
            }
            return PartialView("_Reason", vm);
        }

        [HttpGet]
        public ActionResult NextPage(int id)//CurrentHeaderId
        {
            var nextId = _ProcessService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id)//CurrentHeaderId
        {
            var nextId = _ProcessService.PrevId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Process);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

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
