using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Service;
using Presentation;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Xml.Linq;
using Components.ExceptionHandlers;
using AdminSetup.Models.ViewModels;

namespace Web
{
    [Authorize]
    public class UserRolesController : Controller
    {
        IExceptionHandler _exception;
        IUserRolesService _userRolesService;
        IUsersService _userService;
        public UserRolesController(IExceptionHandler exec, IUserRolesService userRoleServ, IUsersService userServ)
        {
            _exception = exec;
            _userRolesService = userRoleServ;
            _userService = userServ;
        }
        // GET: /UserRoleMaster/

        public ActionResult Index()
        {
            var List = _userRolesService.GetRolesListForIndex();

            return View(List);
        }

        // GET: /UserRoleMaster/Create

        public ActionResult UpdateRoles(string UserId)
        {
            var vm = _userRolesService.GetUserRoleDeail(UserId);
            return View("Create", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(UserRoleViewModel vm)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _userRolesService.UpdateUserRoles(vm);
                }
                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return View("Create", vm);
                }

                return RedirectToAction("Index").Success("Data saved successfully");

            }
            return View("Create", vm);
        }


        public ActionResult TempRolesIndex()
        {

            var UsersList = _userService.GetUsersList();

            var Users = (from p in UsersList
                         orderby p.UserName
                         select new UserRoleViewModel
                         {
                             UserId = p.Id,
                             UserName = p.UserName,
                             Email = p.Email,
                         });

            return View(Users);
        }

        // GET: /UserRoleMaster/Create

        public ActionResult UpdateTempRoles(string UserId)
        {
            UserRoleViewModel vm = new UserRoleViewModel();
            vm.UserId = UserId;
            vm.ExpiryDate = DateTime.Now;
            vm.UserName = _userService.Find(UserId).UserName;

            return View("CreateTempRoles", vm);
        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostTempRoles(UserRoleViewModel vm)
        {

            if (!vm.ExpiryDate.HasValue || vm.ExpiryDate < DateTime.Now.Date)
                ModelState.AddModelError("ExpiryDate", "Expiry date field is required.");
            if (ModelState.IsValid)
            {

                try
                {
                    _userRolesService.UpdateUserTempRoles(vm);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return View("CreateTempRoles", vm);
                }

                return RedirectToAction("TempRolesIndex").Success("Data saved successfully");

            }
            return View("CreateTempRoles", vm);
        }



        [HttpGet]
        public ActionResult Copy()
        {
            return PartialView("Copy");
        }


        [HttpPost]
        public ActionResult CopyFromExisting(UserRoleViewModel Vm)
        {
            if (!string.IsNullOrEmpty(Vm.UserId))
            {
                var RolesList = _userRolesService.GetUserRolesList(Vm.UserId);

                return Json(new { success = true, data = RolesList });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public ActionResult GetUserTempRoles(string UserId)
        {
            ViewBag.UserName = _userService.Find(UserId).UserName;

            var GroupList = _userRolesService.GetUserTempRoles(UserId);

            return PartialView("UserTempRoles", GroupList);
        }

        public ActionResult DeleteTempUserRole(DateTime ExpiryDate, string UserId)
        {

            if (!string.IsNullOrEmpty(UserId))
            {
                try
                {
                    _userRolesService.DeleteTempUserRoles(ExpiryDate, UserId);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false });
                }

                return Json(new { success = true });
            }
            else
                return Json(new { success = false });
        }

        public ActionResult Sync()
        {
            try
            {
                _userService.Syncs();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = _exception.HandleException(ex);
                return View("Error");
            }

            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userRolesService.Dispose();
            }
            base.Dispose(disposing);
        }



    }
}
