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
using System.Net;
using Presentation.ViewModels;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductCategoryProcessSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductCategoryProcessSettingsService _ProductCategoryProcessSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductCategoryProcessSettingsController(IProductCategoryProcessSettingsService ProductCategoryProcessSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductCategoryProcessSettingsService = ProductCategoryProcessSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(ProductCategoryProcessSettingsViewModel s)
        {
            ViewBag.Name = new ProductCategoryService(_unitOfWork).Find(s.ProductCategoryId).ProductCategoryName;
            ViewBag.id = s.ProductCategoryId;
        }

        public ActionResult Index(int id)//ProductCategoryId
        {
            var p = _ProductCategoryProcessSettingsService.GetProductCategoryProcessSettingList(id);
            ViewBag.id = id;
            ViewBag.Name = new ProductCategoryService(_unitOfWork).Find(id).ProductCategoryName;
            return View(p);
        }


        // GET: /ProductCategorySettingMaster/Create

        public ActionResult Create(int id)//ProductCategoryId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            ProductCategoryProcessSettingsViewModel vm = new ProductCategoryProcessSettingsViewModel();
            vm.ProductCategoryId = id;
            PrepareViewBag(vm);
            return View("Create", vm);

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductCategoryProcessSettingsViewModel vm)
        {
            ProductCategoryProcessSettings pt = AutoMapper.Mapper.Map<ProductCategoryProcessSettingsViewModel, ProductCategoryProcessSettings>(vm);


            if (ModelState.IsValid)
            {
                ProductCategory ProductCategory = new ProductCategoryService(_unitOfWork).Find(vm.ProductCategoryId);
                if (vm.ProductCategoryProcessSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductCategoryProcessSettingsService.Create(pt);

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
                        DocId = pt.ProductCategoryProcessSettingsId,
                        DocTypeId = DocTypeId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "ProductCategoryProcessSettings", new { id = ProductCategory.ProductCategoryId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductCategoryProcessSettings temp = _ProductCategoryProcessSettingsService.Find(pt.ProductCategoryProcessSettingsId);

                    ProductCategoryProcessSettings ExRec = Mapper.Map<ProductCategoryProcessSettings>(temp);

                    temp.ProcessId = vm.ProcessId;
                    temp.QAGroupId = vm.QAGroupId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductCategoryProcessSettingsService.Update(temp);

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
                        DocId = temp.ProductCategoryProcessSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        DocTypeId = DocTypeId,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "ProductCategoryProcessSettings", new { id = ProductCategory.ProductCategoryId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm);
            return View("Create", vm);
        }


        public ActionResult Edit(int id)//ProductCategoryProcessSettingsId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var settings = new ProductCategoryProcessSettingsService(_unitOfWork).Find(id);
            ProductCategoryProcessSettingsViewModel temp = AutoMapper.Mapper.Map<ProductCategoryProcessSettings, ProductCategoryProcessSettingsViewModel>(settings);

            PrepareViewBag(temp);
            return View("Create", temp);
        }

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductCategoryProcessSettings ProductCategoryProcessSettings = _ProductCategoryProcessSettingsService.Find(id);
            if (ProductCategoryProcessSettings == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductCategoryMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {
            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();
            if (ModelState.IsValid)
            {
                var temp = _ProductCategoryProcessSettingsService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                {
                    ExObj = temp,
                });

                _ProductCategoryProcessSettingsService.Delete(vm.id);
                XElement Modifications = new ModificationsCheckService().CheckChanges(LogList);
                try
                {
                    _unitOfWork.Save();
                }

                catch (Exception ex)
                {
                    string message = _exception.HandleException(ex);
                    ModelState.AddModelError("", message);
                    return PartialView("_Reason", vm);
                }

                LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                {
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
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
