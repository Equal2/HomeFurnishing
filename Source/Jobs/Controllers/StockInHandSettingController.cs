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
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class StockInHandSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        IStockInHandSettingService _StockInHandSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public StockInHandSettingController(IStockInHandSettingService StockInHandSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _StockInHandSettingService = StockInHandSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }
        // GET: /DeliveryTermsMaster/       

        private void PrepareViewBag(int ProductTypeId,string Routeid)
        {
            var Caption= new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettings(ProductTypeId);
            
            List<SelectListItem> temp = new List<SelectListItem>();
            temp.Add(new SelectListItem { Text = Caption.Dimension1Caption.ToString()!="" ? Caption.Dimension1Caption.ToString() : StockInHandGroupOnConstants.Dimension1, Value = StockInHandGroupOnConstants.Dimension1 });
            temp.Add(new SelectListItem { Text = Caption.Dimension2Caption.ToString() != "" ? Caption.Dimension2Caption.ToString() : StockInHandGroupOnConstants.Dimension2, Value = StockInHandGroupOnConstants.Dimension2 });
            temp.Add(new SelectListItem { Text =Caption.Dimension3Caption.ToString() != "" ? Caption.Dimension3Caption.ToString() : StockInHandGroupOnConstants.Dimension3, Value = StockInHandGroupOnConstants.Dimension3 });
            temp.Add(new SelectListItem { Text =Caption.Dimension4Caption.ToString() != "" ? Caption.Dimension4Caption.ToString() : StockInHandGroupOnConstants.Dimension4, Value = StockInHandGroupOnConstants.Dimension4 });
            if (Routeid == "Stock")
            {
                temp.Add(new SelectListItem { Text = StockInHandGroupOnConstants.Godown, Value = StockInHandGroupOnConstants.Godown });
            }
            else
            {
                temp.Add(new SelectListItem { Text = StockInHandGroupOnConstants.Person, Value = StockInHandGroupOnConstants.Person });
            }
            temp.Add(new SelectListItem { Text = StockInHandGroupOnConstants.LotNo, Value = StockInHandGroupOnConstants.LotNo });
            temp.Add(new SelectListItem { Text = StockInHandGroupOnConstants.Process, Value = StockInHandGroupOnConstants.Process });
            temp.Add(new SelectListItem { Text = StockInHandGroupOnConstants.Product, Value = StockInHandGroupOnConstants.Product });
            ViewBag.GroupOnList = new SelectList(temp, "Value", "Text");



            List<SelectListItem> shwBal = new List<SelectListItem>();
            shwBal.Add(new SelectListItem { Text = StockInHandShowBalanceConstants.GreaterThanZero, Value = StockInHandShowBalanceConstants.GreaterThanZero });
            shwBal.Add(new SelectListItem { Text = StockInHandShowBalanceConstants.LessThanZero, Value = StockInHandShowBalanceConstants.LessThanZero });
            shwBal.Add(new SelectListItem { Text = StockInHandShowBalanceConstants.NotZero, Value = StockInHandShowBalanceConstants.NotZero });
            shwBal.Add(new SelectListItem { Text = StockInHandShowBalanceConstants.PeriodNegative, Value = StockInHandShowBalanceConstants.PeriodNegative });
            shwBal.Add(new SelectListItem { Text = StockInHandShowBalanceConstants.Zero, Value = StockInHandShowBalanceConstants.Zero });
            shwBal.Add(new SelectListItem { Text = StockInHandShowBalanceConstants.All, Value = StockInHandShowBalanceConstants.All });

            ViewBag.ShowBalanceList = new SelectList(shwBal, "Value", "Text");

        


           

        }

        public ActionResult Create(int? LedgerAccountGroupId, int? LedgerAccountId, int ProductTypeId,string ControllerName,string Route)
        {

            //if (LedgerAccountGroupId.HasValue && LedgerAccountGroupId.Value > 0)
            //    System.Web.HttpContext.Current.Session["LedgerAccountGroupId"] = LedgerAccountGroupId.Value;
            //if (LedgerAccountId.HasValue && LedgerAccountId.Value > 0)
            //    System.Web.HttpContext.Current.Session["LedgerAccountId"] = LedgerAccountId.Value;
            ViewBag.id = ProductTypeId;
            System.Web.HttpContext.Current.Session["ProductTypeId"] = ProductTypeId;
            System.Web.HttpContext.Current.Session["Route"] = Route;
            System.Web.HttpContext.Current.Session["ControllerName"] = ControllerName.ToString();
            
            string UserName = User.Identity.Name;

            var Settings = _StockInHandSettingService.GetTrailBalanceSetting(UserName, ProductTypeId, Route);
            PrepareViewBag(ProductTypeId, Route);
            if (Settings == null)
            {
                StockInHandSetting settings = new StockInHandSetting();
                settings.UserName = UserName;
                settings.SiteIds = Convert.ToString((int)System.Web.HttpContext.Current.Session["SiteId"]);
                settings.ToDate = DateTime.Now;
                settings.FromDate = new DocumentTypeService(_unitOfWork).GetFinYearStart();
                settings.GroupOn = StockInHandGroupOnConstants.Product;
                settings.ShowBalance= StockInHandShowBalanceConstants.All;
                settings.ProductTypeId = ProductTypeId;
                settings.TableName = Route;
                settings.ShowOpening = false;
                return View("Create", settings);
            }
            else
            {
                
                return View("Create", Settings);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(StockInHandSetting vm)
        {
            int? LedgerAccountGroupId = null;
            int? LedgerAccountId = null;
            //if(System.Web.HttpContext.Current.Session["LedgerAccountGroupId"] != null)
            //LedgerAccountGroupId=(int)System.Web.HttpContext.Current.Session["LedgerAccountGroupId"];

            //if (System.Web.HttpContext.Current.Session["LedgerAccountId"] != null)
            //LedgerAccountId=(int)System.Web.HttpContext.Current.Session["LedgerAccountId"];

            int ProductTypeId = (int)System.Web.HttpContext.Current.Session["ProductTypeId"];
            string ControllerName= (string)System.Web.HttpContext.Current.Session["ControllerName"];
            string Routeid=(string)System.Web.HttpContext.Current.Session["Route"];
            ViewBag.id = ProductTypeId;

            if (!vm.FromDate.HasValue)
                ModelState.AddModelError("FromDate", "The From date field is required.");

            if (!vm.ToDate.HasValue)
                ModelState.AddModelError("ToDate", "The To date field is required.");

            if (string.IsNullOrEmpty(vm.GroupOn))
                ModelState.AddModelError("GroupOn", "The GroupOn field is required.");

            if (ModelState.IsValid)
            {

                if (vm.StockInHandSettingId <= 0)
                {
                    vm.ObjectState = Model.ObjectState.Added;
                    _StockInHandSettingService.Create(vm);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(ProductTypeId, Routeid);
                        return View("Create", vm);
                    }

                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountGroupId");
                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountId");

                    //if (LedgerAccountGroupId.HasValue && LedgerAccountGroupId.Value > 0)
                    //    return RedirectToAction("GetSubStockInHand", "StockInHand", new { id = LedgerAccountGroupId }).Success("Data saved successfully");
                    //else if (LedgerAccountId.HasValue && LedgerAccountId.Value > 0)
                    //    return RedirectToAction("GetLedgerBalance", "StockInHand", new { id = LedgerAccountId }).Success("Data saved successfully");
                    //else
                    System.Web.HttpContext.Current.Session.Remove("ProductTypeId");
                    System.Web.HttpContext.Current.Session.Remove("ControllerName");
                    if (ControllerName == "StockProcessDisplay")
                    {
                        return RedirectToAction("GetStockInHand", "StockProcessDisplay", new { id = ProductTypeId }).Success("Data saved successfully");
                    }
                    else if (ControllerName == "StockInHandDisplay")
                    {
                        return RedirectToAction("GetStockInHand", "StockInHandDisplay", new { id = ProductTypeId, Routeid = Routeid }).Success("Data saved successfully");
                    }
                    else
                    {
                        return RedirectToAction("GetStockInHand", "StockInHand", new { id = ProductTypeId }).Success("Data saved successfully");
                    }
                }
                else
                {
                    vm.ObjectState = Model.ObjectState.Modified;
                    _StockInHandSettingService.Update(vm);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(ProductTypeId, Routeid);
                        return View("Create", vm);
                    }

                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountGroupId");
                    //System.Web.HttpContext.Current.Session.Remove("LedgerAccountId");

                    //if (LedgerAccountGroupId.HasValue && LedgerAccountGroupId.Value > 0)
                    //    return RedirectToAction("GetSubStockInHand", "StockInHand", new { id = LedgerAccountGroupId }).Success("Data saved successfully");
                    //else if (LedgerAccountId.HasValue && LedgerAccountId.Value > 0)
                    //    return RedirectToAction("GetLedgerBalance", "StockInHand", new { id = LedgerAccountId }).Success("Data saved successfully");
                    //else
                    System.Web.HttpContext.Current.Session.Remove("ProductTypeId");
                    System.Web.HttpContext.Current.Session.Remove("ControllerName");
                    if (ControllerName == "StockProcessDisplay")
                    {
                        return RedirectToAction("GetStockInHand", "StockProcessDisplay", new { id = ProductTypeId }).Success("Data saved successfully");
                    }
                    else if (ControllerName == "StockInHandDisplay")
                    {
                        return RedirectToAction("GetStockInHand", "StockInHandDisplay", new { id = ProductTypeId,Routeid = Routeid }).Success("Data saved successfully");
                    }
                    else
                    {
                        return RedirectToAction("GetStockInHand", "StockInHand", new { id = ProductTypeId }).Success("Data saved successfully");
                    }
                   // return RedirectToAction("GetStockInHand", "StockInHand", new { id = ProductTypeId }).Success("Data saved successfully");

                }

            }
            else
            {
                PrepareViewBag(ProductTypeId, Routeid);
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
