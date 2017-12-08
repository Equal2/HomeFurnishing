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
using System.Reflection;

namespace Jobs.Controllers
{
    [Authorize]
    public class TrialBalanceSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        ITrialBalanceSettingService _TrialBalanceSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public TrialBalanceSettingController(ITrialBalanceSettingService TrialBalanceSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _TrialBalanceSettingService = TrialBalanceSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /DeliveryTermsMaster/       

        private void PrepareViewBag()
        {

            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = DisplayTypeConstants.Balance, Value = DisplayTypeConstants.Balance });
            temp.Add(new SelectListItem { Text = DisplayTypeConstants.Summary, Value = DisplayTypeConstants.Summary });

            ViewBag.DisplayTypeList = new SelectList(temp, "Value", "Text");

            List<SelectListItem> DRCR = new List<SelectListItem>();
            DRCR.Add(new SelectListItem { Text = DrCrConstants.Debit, Value = DrCrConstants.Debit });
            DRCR.Add(new SelectListItem { Text = DrCrConstants.Credit, Value = DrCrConstants.Credit });
            DRCR.Add(new SelectListItem { Text = DrCrConstants.Both, Value = DrCrConstants.Both });

            ViewBag.DrCrList = new SelectList(DRCR, "Value", "Text");

        }

        public ActionResult Create(int? LedgerAccountGroupId, int? LedgerAccountId, string returnUrl)
        {

            if (LedgerAccountGroupId.HasValue && LedgerAccountGroupId.Value > 0)
                System.Web.HttpContext.Current.Session["LedgerAccountGroupId"] = LedgerAccountGroupId.Value;
            if (LedgerAccountId.HasValue && LedgerAccountId.Value > 0)
                System.Web.HttpContext.Current.Session["LedgerAccountId"] = LedgerAccountId.Value;
            if (returnUrl != null)
                System.Web.HttpContext.Current.Session["returnUrl"] = returnUrl;

            string UserName = User.Identity.Name;

            var Settings = _TrialBalanceSettingService.GetTrailBalanceSetting(UserName);
            PrepareViewBag();
            if (Settings == null)
            {
                TrialBalanceSetting settings = new TrialBalanceSetting();
                settings.UserName = UserName;
                settings.ToDate = DateTime.Now;
                settings.SiteIds = Convert.ToString((int)System.Web.HttpContext.Current.Session["SiteId"]);
                settings.FromDate = new DocumentTypeService(_unitOfWork).GetFinYearStart();
                return View("Create", settings);
            }
            else
            {
                return View("Create", Settings);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(TrialBalanceSetting vm)
        {
            int? LedgerAccountGroupId = null;
            int? LedgerAccountId = null;
            string returnUrl = "";
            if (System.Web.HttpContext.Current.Session["LedgerAccountGroupId"] != null)
                LedgerAccountGroupId = (int)System.Web.HttpContext.Current.Session["LedgerAccountGroupId"];

            if (System.Web.HttpContext.Current.Session["LedgerAccountId"] != null)
                LedgerAccountId = (int)System.Web.HttpContext.Current.Session["LedgerAccountId"];

            if (System.Web.HttpContext.Current.Session["returnUrl"] != null)
                returnUrl = (string)System.Web.HttpContext.Current.Session["returnUrl"];

            if (!vm.FromDate.HasValue)
                ModelState.AddModelError("FromDate", "From date field is required.");

            if (!vm.ToDate.HasValue)
                ModelState.AddModelError("ToDate", "To date field is required.");

            if (ModelState.IsValid)
            {

                if (vm.TrialBalanceSettingId <= 0)
                {
                    vm.ObjectState = Model.ObjectState.Added;
                    _TrialBalanceSettingService.Create(vm);

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

                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountGroupId");
                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountId");


                    return Redirect(System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + returnUrl);

                    //if (LedgerAccountGroupId.HasValue && LedgerAccountGroupId.Value > 0)
                    //    return RedirectToAction("GetSubTrialBalance", "TrialBalance", new { id = LedgerAccountGroupId }).Success("Data saved successfully");
                    //else if (LedgerAccountId.HasValue && LedgerAccountId.Value > 0)
                    //    return RedirectToAction("GetLedgerBalance", "TrialBalance", new { id = LedgerAccountId }).Success("Data saved successfully");
                    //else
                    //    return RedirectToAction("GetTrialBalance", "TrialBalance").Success("Data saved successfully");
                }
                else
                {
                    vm.ObjectState = Model.ObjectState.Modified;
                    _TrialBalanceSettingService.Update(vm);

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

                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountGroupId");
                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountId");

                    return Redirect(System.Configuration.ConfigurationManager.AppSettings["CurrentDomain"] + returnUrl);

                    //if (LedgerAccountGroupId.HasValue && LedgerAccountGroupId.Value > 0)
                    //    return RedirectToAction("GetSubTrialBalance", "TrialBalance", new { id = LedgerAccountGroupId }).Success("Data saved successfully");
                    //else if (LedgerAccountId.HasValue && LedgerAccountId.Value > 0)
                    //    return RedirectToAction("GetLedgerBalance", "TrialBalance", new { id = LedgerAccountId }).Success("Data saved successfully");
                    //else
                    //    return RedirectToAction("GetTrialBalance", "TrialBalance").Success("Data saved successfully");

                }

            }
            else
            {
                PrepareViewBag();
                return View("Create", vm);
            }
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
