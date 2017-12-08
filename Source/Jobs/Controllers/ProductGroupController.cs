using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Model.Models;
using Data.Models;
using Service;
using Data.Infrastructure;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using Model.ViewModel;
using AutoMapper;
using System.Xml.Linq;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class ProductGroupController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();

        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        //IGenericService<ProductGroup> _GenericService<ProductGroup>;
        IProductGroupService _ProductGroupService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductGroupController(IProductGroupService ProductGroupService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductGroupService = ProductGroupService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;
        }
        // GET: /ProductMaster/

        public ActionResult Index(int id)//ProductTypeId
        {
            //return RedirectToAction("Create");
            ViewBag.id = id;
            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "ProductTypeSettings", new { id = id }).Warning("Please create Product Type Settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            string ProductTypeName = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            ViewBag.Name = (settings.ProductGroupCaption ?? "Product Group") + "-" + ProductTypeName;


            //ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            var p = _ProductGroupService.GetProductGroupList(id);
            return View(p);
        }
        public ActionResult ProductTypeIndex(int id)//NatureId
        {
            var producttype = new ProductTypeService(_unitOfWork).GetProductTypeListForGroup(id).Where(m => m.IsActive != false).ToList();

            ViewBag.ProductNatureName = new ProductNatureService(_unitOfWork).Find(id).ProductNatureName;

            if (producttype.Count() == 0)
            {
                ViewBag.PrevLink = Request.UrlReferrer.ToString();
                ViewBag.Message = "No ProductType found for this section.";
                return View("~/Views/Shared/NotFound.cshtml");
            }
            if (producttype.Count() == 1)
                return RedirectToAction("Index", new { id = producttype.FirstOrDefault().ProductTypeId });
            else
                return View("ProductTypeIndex", producttype);
        }

        // GET: /ProductMaster/Create

        public ActionResult Create(int id)
        {
            ProductGroup vm = new ProductGroup();
            vm.IsActive = true;
            vm.ProductTypeId = id;
            ViewBag.id = id;

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);

            if (settings == null && UserRoles.Contains("SysAdmin"))
            {
                return RedirectToAction("Create", "ProductTypeSettings", new { id = id }).Warning("Please create Product Type Settings");
            }
            else if (settings == null && !UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            //var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(id);
            string ProductTypeName = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
            ViewBag.Name = (settings.ProductGroupCaption ?? "Product Group") + "-" + ProductTypeName;
            ViewBag.ProductGroupCaption = settings.ProductGroupCaption ?? "Product Group";
            ViewBag.SalesTaxProductCodeCaption = "Default " + (settings.SalesTaxProductCodeCaption ?? "Sales Tax Product Code");
            ViewBag.IsVisibleSalesTaxProductCode = settings.isVisibleSalesTaxProductCode ?? false;
            //ViewBag.Name = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
            return View("Create", vm);

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductGroup vm)
        {
            ProductGroup pt = vm;
            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(vm.ProductTypeId);
            if (ModelState.IsValid)
            {
                if (vm.ProductGroupId <= 0)
                {

                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductGroupService.Create(pt);


                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        ViewBag.id = vm.ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(vm.ProductTypeId).ProductTypeName;
                        ViewBag.ProductGroupCaption = settings.ProductGroupCaption ?? "Product Group";
                        ViewBag.SalesTaxProductCodeCaption = "Default " + (settings.SalesTaxProductCodeCaption ?? "Sales Tax Product Code");
                        ViewBag.IsVisibleSalesTaxProductCode = settings.isVisibleSalesTaxProductCode ?? false;
                        return View("Create", vm);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId,
                        DocId = pt.ProductGroupId,
                        ActivityType = (int)ActivityTypeContants.Added,
                    }));

                    return RedirectToAction("Create", new { id = vm.ProductTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductGroup temp = _ProductGroupService.Find(pt.ProductGroupId);

                    ProductGroup ExRec = Mapper.Map<ProductGroup>(temp);

                    temp.ProductGroupName = pt.ProductGroupName;
                    temp.ProductTypeId = pt.ProductTypeId;
                    temp.DefaultSalesTaxProductCodeId = pt.DefaultSalesTaxProductCodeId;
                    temp.DefaultSalesTaxGroupProductId = pt.DefaultSalesTaxGroupProductId;
                    temp.IsActive = pt.IsActive;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductGroupService.Update(temp);

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
                        ViewBag.id = pt.ProductTypeId;
                        ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
                        ViewBag.ProductGroupCaption = settings.ProductGroupCaption ?? "Product Group";
                        ViewBag.SalesTaxProductCodeCaption = "Default " + (settings.SalesTaxProductCodeCaption ?? "Sales Tax Product Code");
                        ViewBag.IsVisibleSalesTaxProductCode = settings.isVisibleSalesTaxProductCode ?? false;
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId,
                        DocId = temp.ProductGroupId,
                        ActivityType = (int)ActivityTypeContants.Modified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", new { id = vm.ProductTypeId }).Success("Data saved successfully");
                }
            }

            ViewBag.id = pt.ProductTypeId;
            ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
            return View("Create", vm);
        }


        // GET: /ProductMaster/Edit/5

        public ActionResult Edit(int id)
        {
            ProductGroup pt = _ProductGroupService.GetProductGroup(id);
            if (pt == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = pt.ProductTypeId;

            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettingsForDocument(pt.ProductTypeId);
            string ProductTypeName = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
            ViewBag.Name = (settings.ProductGroupCaption ?? "Product Group") + "-" + ProductTypeName;
            ViewBag.ProductGroupCaption = settings.ProductGroupCaption ?? "Product Group";
            ViewBag.SalesTaxProductCodeCaption = "Default " + (settings.SalesTaxProductCodeCaption ?? "Sales Tax Product Code");
            ViewBag.IsVisibleSalesTaxProductCode = settings.isVisibleSalesTaxProductCode ?? false;


            //ViewBag.Name = new ProductTypeService(_unitOfWork).Find(pt.ProductTypeId).ProductTypeName;
            return View("Create", pt);
        }


        // GET: /ProductMaster/Delete/5

        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductGroup ProductGroup = _ProductGroupService.GetProductGroup(id);
            if (ProductGroup == null)
            {
                return HttpNotFound();
            }

            ReasonViewModel vm = new ReasonViewModel()
            {
                id = id,
            };

            return PartialView("_Reason", vm);
        }

        // POST: /ProductGroupMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(ReasonViewModel vm)
        {

            List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

            if (ModelState.IsValid)
            {

                var temp = _ProductGroupService.Find(vm.id);

                LogList.Add(new LogTypeViewModel
                 {
                     ExObj = temp,
                 });

                _ProductGroupService.Delete(vm.id);

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
                    DocTypeId = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup).DocumentTypeId,
                    DocId = vm.id,
                    ActivityType = (int)ActivityTypeContants.Deleted,
                    UserRemark = vm.Reason,
                    xEModifications = Modifications,
                }));

                return Json(new { success = true });

            }
            return PartialView("_Reason", vm);
        }



        [HttpGet]
        public ActionResult NextPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductGroupService.NextId(id, ptypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }
        [HttpGet]
        public ActionResult PrevPage(int id, int ptypeid)//CurrentHeaderId
        {
            var nextId = _ProductGroupService.PrevId(id, ptypeid);
            return RedirectToAction("Edit", new { id = nextId });
        }

        [HttpGet]
        public ActionResult History()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Print()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }
        [HttpGet]
        public ActionResult Email()
        {
            //To Be Implemented
            return View("~/Views/Shared/UnderImplementation.cshtml");
        }

        [HttpGet]
        public ActionResult Report()
        {

            DocumentType Dt = new DocumentType();
            Dt = new DocumentTypeService(_unitOfWork).FindByName(MasterDocTypeConstants.ProductGroup);

            return Redirect((string)System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/Report_ReportPrint/ReportPrint/?MenuId=" + Dt.ReportMenuId);

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
