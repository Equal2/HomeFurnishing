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
    public class UnitController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IUnitService _UnitService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public UnitController(IUnitService UnitService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _UnitService = UnitService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/

        public ActionResult Index()
        {
            //return RedirectToAction("Create");
            var vm = _UnitService.GetUnitList().ToList();
            return View(vm);
        }


        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            UnitViewModel vm = new UnitViewModel();
            Unit vmu = new Unit();
            vm.Unit = vmu;
            vm.Unit.IsActive = true;
            return View(vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UnitViewModel vm)
        {
            Unit pt = vm.Unit;
            if (ModelState.IsValid)
            {
                pt.CreatedDate = DateTime.Now;
                pt.ModifiedDate = DateTime.Now;
                pt.CreatedBy = User.Identity.Name;
                pt.ModifiedBy = User.Identity.Name;
                pt.ObjectState = Model.ObjectState.Added;
                _UnitService.Create(pt);

                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return View(pt);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Unit).DocumentTypeId,
                    DocNo = pt.UnitName,
                    ActivityType = (int)ActivityTypeContants.Added,
                }));


                return RedirectToAction("Create").Success("Data saved successfully");
            }

            // PrepareViewBag();
            return View(vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(string id)
        {
            Unit pt = _UnitService.Find(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View(pt);
        }

        // POST: /ProductMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Unit pt)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {
                Unit temp = _UnitService.Find(pt.UnitId);

                Unit ExRec = Mapper.Map<Unit>(temp);

                temp.UnitName = pt.UnitName;
                temp.Symbol = pt.Symbol;
                temp.FractionName = pt.FractionName;
                temp.FractionUnits = pt.FractionUnits;
                temp.FractionSymbol = pt.FractionSymbol;
                temp.DecimalPlaces = pt.DecimalPlaces;
                temp.IsActive = pt.IsActive;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = User.Identity.Name;
                temp.ObjectState = Model.ObjectState.Modified;
                _UnitService.Update(temp);

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
                    return View(pt);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Unit).DocumentTypeId,
                    DocNo = temp.UnitName,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    xEModifications = Modifications,
                }));

                return RedirectToAction("Index").Success("Data saved successfully");
            }
            return View(pt);
        }

        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Unit Unit = _UnitService.Find(id);
            if (Unit == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                sid = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (ModelState.IsValid)
            {
                var temp = _UnitService.Find(vm.sid);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _UnitService.Delete(vm.sid);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Unit).DocumentTypeId,
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
        public ActionResult NextPage(string id)//CurrentHeaderId
        {
            var nextId = _UnitService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(string id)//CurrentHeaderId
        {
            var nextId = _UnitService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Unit);

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
