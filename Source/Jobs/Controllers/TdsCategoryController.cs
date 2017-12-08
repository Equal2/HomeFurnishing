using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class TdsCategoryController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ITdsCategoryService _TdsCategoryService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public TdsCategoryController(ITdsCategoryService TdsCategoryService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _TdsCategoryService = TdsCategoryService;
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
            var TdsCategory = _TdsCategoryService.GetTdsCategoryList().ToList();
            return View(TdsCategory);
        }

        // GET: /ProductMaster/Create

        public ActionResult Create()
        {
            TdsCategory vm = new TdsCategory();
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TdsCategory vm)
        {
            TdsCategory pt = vm;

            if (ModelState.IsValid)
            {
                pt.CreatedDate = DateTime.Now;
                pt.ModifiedDate = DateTime.Now;
                pt.CreatedBy = User.Identity.Name;
                pt.ModifiedBy = User.Identity.Name;
                pt.ObjectState = Model.ObjectState.Added;
                _TdsCategoryService.Create(pt);

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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.TdsCategory).DocumentTypeId,
                    DocId = pt.TdsCategoryId,
                    ActivityType = (int)ActivityTypeContants.Added,
                }));

                return RedirectToAction("Create").Success("Data saved successfully");
            }
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            TdsCategory pt = _TdsCategoryService.GetTdsCategory(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            return View("Create", pt);
        }

        // POST: /ProductMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TdsCategory pt)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (ModelState.IsValid)
            {
                TdsCategory temp = _TdsCategoryService.Find(pt.TdsCategoryId);

                TdsCategory ExRec = Mapper.Map<TdsCategory>(temp);

                temp.TdsCategoryName = pt.TdsCategoryName;
                temp.ModifiedDate = DateTime.Now;
                temp.ModifiedBy = User.Identity.Name;
                temp.ObjectState = Model.ObjectState.Modified;
                _TdsCategoryService.Update(temp);

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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.TdsCategory).DocumentTypeId,
                    DocId = temp.TdsCategoryId,
                    ActivityType = (int)ActivityTypeContants.Modified,
                    xEModifications = Modifications,
                }));

                return RedirectToAction("Index").Success("Data saved successfully");
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
            TdsCategory TdsCategory = _TdsCategoryService.GetTdsCategory(id);

            if (TdsCategory == null)
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
            if (ModelState.IsValid)
            {
                var temp = _TdsCategoryService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                _TdsCategoryService.Delete(vm.id);
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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.TdsCategory).DocumentTypeId,
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
        public ActionResult NextPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _TdsCategoryService.NextId(id);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, string name)//CurrentHeaderId
        {
            var nextId = _TdsCategoryService.PrevId(id);
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
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.TdsCategory);

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
