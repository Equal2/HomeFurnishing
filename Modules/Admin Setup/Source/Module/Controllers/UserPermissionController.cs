using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Service;

namespace Presentation.Controllers
{
    [Authorize]
    public class UserPermissionController : Controller
    {
        private IUserPermissionService _userPermissionService;
        public UserPermissionController(IUserPermissionService userPermissionService)
        {
            _userPermissionService = userPermissionService;
        }


        public JsonResult AddPermission(int caid)
        {
            _userPermissionService.AddUserPermission(caid, User.Identity.Name);

            return Json(new { success = true });
        }

        public JsonResult RemovePermission(int caid)
        {
            _userPermissionService.DeleteUserPermission(caid);

            return Json(new { success = true });
        }

        public JsonResult AddPermissionForAction(int ActionId)
        {
            _userPermissionService.AddPermissionForAction(ActionId, User.Identity.Name);

            return Json(new { success = true });
        }

        public JsonResult RemovePermissionForAction(int ActionId)
        {
            _userPermissionService.RemovePermissionForAction(ActionId);

            return Json(new { success = true });
        }

        public JsonResult AddPermissionForMenu(int MenuId)
        {
            _userPermissionService.AddPermissionForMenu(MenuId, User.Identity.Name);

            return Json(new { success = true });
        }

        public JsonResult AddLinePermissionForMenu(int MenuId)
        {
            _userPermissionService.AddLinePermissionForMenu(MenuId, User.Identity.Name);

            return Json(new { success = true });
        }

        public JsonResult RemovePermissionForMenu(int MenuId)
        {
            _userPermissionService.RemovePermissionForMenu(MenuId);

            return Json(new { success = true });
        }

        public JsonResult RemoveLinePermissionForMenu(int MenuId)
        {
            _userPermissionService.RemoveLinePermissionForMenu(MenuId);

            return Json(new { success = true });

        }
    }
}