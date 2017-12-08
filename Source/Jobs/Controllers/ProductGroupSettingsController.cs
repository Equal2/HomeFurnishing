using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Jobs.Helpers;
using Core.Common;
using Model.ViewModel;
using System.Xml.Linq;
using AutoMapper;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductGroupSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductGroupSettingsService _ProductGroupSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductGroupSettingsController(IProductGroupSettingsService ProductGroupSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductGroupSettingsService = ProductGroupSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(ProductGroupSettingsViewModel s)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
        }


        // GET: /ProductGroupSettingMaster/Create

        public ActionResult Create(int id)//ProductGroupId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new ProductGroupSettingsService(_unitOfWork).GetProductGroupSettings(id);

            if (settings == null)
            {
                ProductGroupSettingsViewModel vm = new ProductGroupSettingsViewModel();
                vm.ProductGroupId = id;
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                ProductGroupSettingsViewModel temp = AutoMapper.Mapper.Map<ProductGroupSettings, ProductGroupSettingsViewModel>(settings);
                PrepareViewBag(temp);
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductGroupSettingsViewModel vm)
        {
            ProductGroupSettings pt = AutoMapper.Mapper.Map<ProductGroupSettingsViewModel, ProductGroupSettings>(vm);


            if (ModelState.IsValid)
            {
                ProductGroup ProductGroup = new ProductGroupService(_unitOfWork).Find(vm.ProductGroupId);
                if (vm.ProductGroupSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductGroupSettingsService.Create(pt);

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
                        DocId = pt.ProductGroupSettingsId,
                        DocTypeId = DocTypeId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "ProductGroup", new { id = ProductGroup.ProductTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductGroupSettings temp = _ProductGroupSettingsService.Find(pt.ProductGroupSettingsId);

                    ProductGroupSettings ExRec = Mapper.Map<ProductGroupSettings>(temp);

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductGroupSettingsService.Update(temp);

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
                        DocId = temp.ProductGroupSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        DocTypeId = DocTypeId,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "ProductGroup", new { id = ProductGroup.ProductTypeId }).Success("Data saved successfully");

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
