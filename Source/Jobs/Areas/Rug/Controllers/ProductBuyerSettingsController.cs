using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation;
using Core.Common;
using Model.ViewModel;
using System.Xml.Linq;
using AutoMapper;
using Jobs.Helpers;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class ProductBuyerSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductBuyerSettingsService _ProductBuyerSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductBuyerSettingsController(IProductBuyerSettingsService ProductBuyerSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductBuyerSettingsService = ProductBuyerSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(ProductBuyerSettingsViewModel s)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        }


        // GET: /ProductBuyerSettingMaster/Create

        public ActionResult Create(int id)
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new ProductBuyerSettingsService(_unitOfWork).GetProductBuyerSettings(DivisionId, SiteId);
            int DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Carpet).DocumentTypeId;



            if (settings == null)
            {
                ProductBuyerSettingsViewModel vm = new ProductBuyerSettingsViewModel();
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.ProductId = id;
                PrepareViewBag(vm);
                ViewBag.DocTypeId = DocTypeId;
                return View("Create", vm);
            }
            else
            {
                ProductBuyerSettingsViewModel temp = AutoMapper.Mapper.Map<ProductBuyerSettings, ProductBuyerSettingsViewModel>(settings);
                temp.ProductId = id;
                PrepareViewBag(temp);
                ViewBag.DocTypeId = DocTypeId;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductBuyerSettingsViewModel vm)
        {
            ProductBuyerSettings pt = AutoMapper.Mapper.Map<ProductBuyerSettingsViewModel, ProductBuyerSettings>(vm);


            if (ModelState.IsValid)
            {

                if (vm.ProductBuyerSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductBuyerSettingsService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm);
                        return View("Create", vm);
                    }

                    int DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Carpet).DocumentTypeId;

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocId = pt.ProductBuyerSettingsId,
                        DocTypeId = DocTypeId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "ProductBuyer", new { id = vm.ProductId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductBuyerSettings temp = _ProductBuyerSettingsService.Find(pt.ProductBuyerSettingsId);

                    ProductBuyerSettings ExRec = Mapper.Map<ProductBuyerSettings>(temp);

                    temp.BuyerSpecificationDisplayName = vm.BuyerSpecificationDisplayName;
                    temp.BuyerSpecification1DisplayName = vm.BuyerSpecification1DisplayName;
                    temp.BuyerSpecification2DisplayName = vm.BuyerSpecification2DisplayName;
                    temp.BuyerSpecification3DisplayName = vm.BuyerSpecification3DisplayName;
                    temp.BuyerSpecification4DisplayName = vm.BuyerSpecification4DisplayName;
                    temp.BuyerSpecification5DisplayName = vm.BuyerSpecification5DisplayName;
                    temp.BuyerSpecification6DisplayName = vm.BuyerSpecification6DisplayName;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductBuyerSettingsService.Update(temp);

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
                        PrepareViewBag(vm);
                        return View("Create", pt);
                    }

                    int DocTypeId = new DocumentTypeService(_unitOfWork).Find(MasterDocTypeConstants.Carpet).DocumentTypeId;

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocId = temp.ProductBuyerSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        DocTypeId = DocTypeId,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "ProductBuyer", new { id = vm.ProductId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm);
            return View("Create", vm);
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
