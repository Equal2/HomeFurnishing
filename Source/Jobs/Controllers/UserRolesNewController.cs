using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Core.Common;
using AutoMapper;
using Model.ViewModel;
using System.Xml.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Jobs.Controllers
{

    [Authorize]
    public class UserRolesNewController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IUserRolesNewService _UserRolesNewService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;

        public UserRolesNewController(IUserRolesNewService UserRoleServ, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _UserRolesNewService = UserRoleServ;
            _unitOfWork = unitOfWork;
            _exception = exec;

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }

        [HttpGet]
        public JsonResult Index(string id)
        {
            var p = _UserRolesNewService.GetUserRoleListForIndex(id);
            return Json(p, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _Create(string Id) //Id ==>Sale Order Header Id
        {
            UserRole s = new UserRole();
            s.UserId = Id;
            return PartialView("_Create", s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePost(UserRole svm)
        {
            if (svm.SiteId == 0)
                ModelState.AddModelError("SiteId", "Site is required.");

            if (svm.DivisionId == 0)
                ModelState.AddModelError("DivisionId", "Division is required.");


            if (ModelState.IsValid)
            {
                if (svm.UserRoleId == 0)
                {
                    svm.CreatedDate = DateTime.Now;
                    svm.ModifiedDate = DateTime.Now;
                    svm.CreatedBy = User.Identity.Name;
                    svm.ModifiedBy = User.Identity.Name;
                    svm.ObjectState = Model.ObjectState.Added;
                    _UserRolesNewService.Create(svm);

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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.User).DocumentTypeId,
                        DocId = svm.UserRoleId,
                        DocLineId = svm.UserRoleId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("_Create", new { id = svm.UserId });
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    UserRole temp1 = _UserRolesNewService.Find(svm.UserRoleId);

                    UserRole ExRec = Mapper.Map<UserRole>(temp1);

                    temp1.RoleId = svm.RoleId;
                    temp1.SiteId = svm.SiteId;
                    temp1.DivisionId = svm.DivisionId;

                    temp1.ModifiedDate = DateTime.Now;
                    temp1.ModifiedBy = User.Identity.Name;
                    _UserRolesNewService.Update(temp1);


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
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.User).DocumentTypeId,
                        DocId = temp1.UserRoleId,
                        DocLineId = temp1.UserRoleId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return Json(new { success = true });

                }
            }
            return PartialView("_Create", svm);
        }

        [HttpGet]
        public ActionResult _Edit(int id)
        {
            UserRole temp = _UserRolesNewService.GetUserRole(id);

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Create", temp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(UserRole vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            UserRole UserRolesNew = _UserRolesNewService.GetUserRole(vm.UserRoleId);
            LogList.Add(new LogTypeViewModel
            {
                ExObj = UserRolesNew,
            });

            _UserRolesNewService.Delete(vm.UserRoleId);
            XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
            try
            {
                _unitOfWork.Save();
            }

            catch (Exception ex)
            {
                string message = _exception.HandleException(ex);
                ModelState.AddModelError("", message);
                return PartialView("EditLine", vm);
            }

            LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
            {
                DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.User).DocumentTypeId,
                DocId = vm.UserRoleId,
                DocLineId = vm.UserRoleId,
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
