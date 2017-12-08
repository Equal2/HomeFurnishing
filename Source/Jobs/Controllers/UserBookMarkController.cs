using System.Collections.Generic;
using System.Web.Mvc;
using Service;
using Model.ViewModel;
using Model.Models;

//using ProjLib.ViewModels;

namespace Jobs.Controllers
{
    [Authorize]
    public class UserBookMarkController : Controller
    {
        IUserBookMarkService _userBookMarkService;
        IMenuService _menuService;
        public UserBookMarkController(IUserBookMarkService userBookMarkService, IMenuService menuServ)
        {
            _userBookMarkService = userBookMarkService;
            _menuService = menuServ;
        }


        public JsonResult AddBookMark(int caid)
        {

            string appuserid = User.Identity.Name;

            if (!_userBookMarkService.CheckBookMarkExists(appuserid, caid))
            {

                _userBookMarkService.AddUserBookMark(appuserid, caid);

                List<UserBookMarkViewModel> bookmark = (List<UserBookMarkViewModel>)(System.Web.HttpContext.Current.Session["BookMarks"]);

                Menu menu = _menuService.Find(caid);

                bookmark.Add(new UserBookMarkViewModel()
                {
                    IconName = menu.IconName,
                    MenuId = menu.MenuId,
                    MenuName = menu.MenuName,
                });
                System.Web.HttpContext.Current.Session["BookMarks"] = bookmark;

            }

            return Json(new { success = true });

        }

        public JsonResult RemoveBookMark(int caid)
        {

            string appuserid = User.Identity.Name;

            _userBookMarkService.DeleteUserBookMark(appuserid, caid);

            List<UserBookMarkViewModel> bookmark = (List<UserBookMarkViewModel>)(System.Web.HttpContext.Current.Session["BookMarks"]);

            bookmark.RemoveAll(m => m.MenuId == caid);

            System.Web.HttpContext.Current.Session["BookMarks"] = bookmark;

            return Json(new { success = true });

        }


    }
}