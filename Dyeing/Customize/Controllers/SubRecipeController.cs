using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using System.Configuration;
using Presentation;
using Components.ExceptionHandlers;
using Services.BasicSetup;
using Models.Customize.ViewModels;
using Models.Customize.Models;
using Services.Customize;
using TimePlanValidator;
using TimePlanValidator.ViewModels;
using TimePlanValidator.Common;
using ProjLib.Constants;
using Models.BasicSetup.ViewModels;
using CookieNotifier;
using Presentation.Helper;

namespace Customize.Controllers
{

    [Authorize]
    public class SubRecipeController : System.Web.Mvc.Controller
    {
        List<string> UserRoles = new List<string>();

        bool TimePlanValidation = true;
        string ExceptionMsg = "";
        bool Continue = true;

        ISubRecipeService _SubRecipeService;
        IRecipeHeaderService _RecipeHeaderService;
        IDocumentTypeService _documentTypeService;
        IExceptionHandler _exception;


        public SubRecipeController(ISubRecipeService SubRecipeService, RecipeHeaderService RecipeHeaderService, IExceptionHandler exec, IDocumentTypeService DocumentTypeServ)
        {
            _SubRecipeService = SubRecipeService;
            _RecipeHeaderService = RecipeHeaderService;
            _exception = exec;
            _documentTypeService = DocumentTypeServ;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
        }

        // GET: /JobReceiveHeader/




        // GET: /JobReceiveHeader/Create

        public ActionResult Create()
        {
            SubRecipeViewModel p = new SubRecipeViewModel();

            ViewBag.Mode = "Add";
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(SubRecipeViewModel svm)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _SubRecipeService.Create(svm, User.Identity.Name);
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    TempData["CSEXC"] += message;
                    ViewBag.Mode = "Add";
                    return View("Create", svm);
                }

                return RedirectToAction("Create", "SubRecipe", null).Success("Data saved successfully");
            }
            ViewBag.Mode = "Add";
            return View("Create", svm);
        }


        public JsonResult GetRecipeDetailJson(int JobOrderHeaderId)
        {
            Decimal temp = _SubRecipeService.GetRecipeBalanceForSubRecipe(JobOrderHeaderId);

            return Json(temp);
        }


        public ActionResult GetRecipeList(string searchTerm, int pageSize, int pageNum, int filter)//DocTypeId
        {
            var Query = _SubRecipeService.GetRecipeHelpList(filter, searchTerm);
            var temp = Query.Skip(pageSize * (pageNum - 1))
                .Take(pageSize)
                .ToList();

            var count = Query.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return new JsonpResult
            {
                Data = Data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        protected override void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty((string)TempData["CSEXC"]))
            {
                GenerateCookie.CreateNotificationCookie(NotificationTypeConstants.Danger, (string)TempData["CSEXC"]);
                TempData.Remove("CSEXC");
            }
            if (disposing)
            {
                _SubRecipeService.Dispose();
            }
            base.Dispose(disposing);
        }



    }
}
