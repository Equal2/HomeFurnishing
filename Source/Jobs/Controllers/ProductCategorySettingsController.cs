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

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductCategorySettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductCategorySettingsService _ProductCategorySettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductCategorySettingsController(IProductCategorySettingsService ProductCategorySettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductCategorySettingsService = ProductCategorySettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(ProductCategorySettingsViewModel s)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        }


        // GET: /ProductCategorySettingMaster/Create

        public ActionResult Create(int id)//ProductCategoryId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new ProductCategorySettingsService(_unitOfWork).GetProductCategorySettings(id);

            if (settings == null)
            {
                ProductCategorySettingsViewModel vm = new ProductCategorySettingsViewModel();
                vm.ProductCategoryId = id;
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                ProductCategorySettingsViewModel temp = AutoMapper.Mapper.Map<ProductCategorySettings, ProductCategorySettingsViewModel>(settings);
                PrepareViewBag(temp);
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductCategorySettingsViewModel vm)
        {
            ProductCategorySettings pt = AutoMapper.Mapper.Map<ProductCategorySettingsViewModel, ProductCategorySettings>(vm);


            if (ModelState.IsValid)
            {
                ProductCategory ProductCategory = new ProductCategoryService(_unitOfWork).Find(vm.ProductCategoryId);
                if (vm.ProductCategorySettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductCategorySettingsService.Create(pt);

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
                        DocId = pt.ProductCategorySettingsId,
                        DocTypeId = DocTypeId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "ProductCategory", new { id = ProductCategory.ProductTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductCategorySettings temp = _ProductCategorySettingsService.Find(pt.ProductCategorySettingsId);

                    ProductCategorySettings ExRec = Mapper.Map<ProductCategorySettings>(temp);

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductCategorySettingsService.Update(temp);

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
                        DocId = temp.ProductCategorySettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        DocTypeId = DocTypeId,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "ProductCategory", new { id = ProductCategory.ProductTypeId }).Success("Data saved successfully");

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
