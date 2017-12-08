using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using Model.ViewModels;
using AutoMapper;
using Presentation;
using System.Text;
using Model.ViewModel;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{

    [Authorize]
    public class RouteLineController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IRouteLineService _RouteLineService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public RouteLineController(IRouteLineService Route, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _RouteLineService = Route;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(int id)
        {
            var p = _RouteLineService.GetRouteLineListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        private void PrepareViewBag(RouteLineViewModel s)
        {
            if (s == null)
            {
                ViewBag.CityId = new SelectList(new CityService(_unitOfWork).GetCityList(), "CityId", "CityName");
            }
            else
            {
                ViewBag.CityId = new SelectList(new CityService(_unitOfWork).GetCityList(), "CityId", "CityName", s.CityId);
            }
        }

        public ActionResult _Create(int Id) //Id ==>City Sequence Header Id
        {
            Route H = new RouteService(_unitOfWork).Find(Id);
            RouteLineViewModel s = new RouteLineViewModel();
            s.RouteId = H.RouteId;
            PrepareViewBag(null);
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RouteLineViewModel svm, string Command)
        {
            RouteLine s = Mapper.Map<RouteLineViewModel, RouteLine>(svm);
            Route temp = new RouteService(_unitOfWork).Find(s.RouteId);

            if (ModelState.IsValid)
            {
                if (s.CityId == 0)
                {
                    PrepareViewBag(svm);
                    return View(svm).Danger("Please fill City");
                }


                s.CreatedDate = DateTime.Now;
                s.ModifiedDate = DateTime.Now;
                s.CreatedBy = User.Identity.Name;
                s.ModifiedBy = User.Identity.Name;
                s.ObjectState = Model.ObjectState.Added;
                _RouteLineService.Create(s);


                _unitOfWork.Save();
                return RedirectToAction("Create").Success("Data saved successfully");
            }

            PrepareViewBag(svm);
            return View(svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(RouteLineViewModel svm)
        {
            RouteLine s = Mapper.Map<RouteLineViewModel, RouteLine>(svm);
            Route temp = new RouteService(_unitOfWork).Find(s.RouteId);
            //if (Command == "Submit" && (s.ProductId == 0))
            //    return RedirectToAction("Submit", "Route", new { id = s.RouteId }).Success("Data saved successfully");
            if (s.CityId == 0)
            {
                PrepareViewBag(svm);
                return View(svm).Danger("Please fill City");
            }

            if (ModelState.IsValid)
            {
                if (svm.RouteLineId == 0)
                {
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                    s.CreatedBy = User.Identity.Name;
                    s.ModifiedBy = User.Identity.Name;
                    s.ObjectState = Model.ObjectState.Added;
                    _RouteLineService.Create(s);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Route).DocumentTypeId,
                        DocId = s.RouteId,
                        DocLineId = s.RouteLineId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_Create", new { id = s.RouteId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    Route header = new RouteService(_unitOfWork).Find(svm.RouteId);
                    StringBuilder logstring = new StringBuilder();
                    RouteLine temp1 = _RouteLineService.Find(svm.RouteLineId);

                    RouteLine ExRec = Mapper.Map<RouteLine>(temp1);

                    temp1.CityId = svm.CityId;
                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _RouteLineService.Update(temp1);

                    LogList.Add(new LogTypeViewModel
                    {
                        ExObj = ExRec,
                        Obj = temp1,
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
                        return PartialView("_Create", svm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Route).DocumentTypeId,
                        DocId = temp.RouteId,
                        DocLineId = temp1.RouteLineId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });

                }
            }
            PrepareViewBag(svm);
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int id)
        {
            RouteLine temp = _RouteLineService.GetRouteLine(id);
            RouteLineViewModel s = Mapper.Map<RouteLine, RouteLineViewModel>(temp);
            PrepareViewBag(s);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", s);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteLine RouteLine = _RouteLineService.GetRouteLine(id);
            if (RouteLine == null)
            {
                return HttpNotFound();
            }
            return View(RouteLine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(RouteLineViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            RouteLine RouteLine = _RouteLineService.GetRouteLine(vm.RouteLineId);

            LogList.Add(new LogTypeViewModel
            {
                ExObj = RouteLine,
            });


            _RouteLineService.Delete(vm.RouteLineId);
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("EditSize", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.Route).DocumentTypeId,
                DocId = RouteLine.RouteId,
                DocLineId = RouteLine.RouteLineId,
                ActivityType = (int)ActivityTypeContants.Deleted,
                xEModifications = Modifications,
            }));

            return Json(new { success = true });
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
