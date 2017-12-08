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
    public class ProductTypeSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IProductTypeSettingsService _ProductTypeSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public ProductTypeSettingsController(IProductTypeSettingsService ProductTypeSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _ProductTypeSettingsService = ProductTypeSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(ProductTypeSettingsViewModel s)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.UnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.id = s.ProductTypeId;
        }


        // GET: /ProductTypeSettingMaster/Create

        public ActionResult Create(int id)//ProductTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var settings = new ProductTypeSettingsService(_unitOfWork).GetProductTypeSettings(id);

            if (settings == null)
            {
                ProductTypeSettingsViewModel vm = new ProductTypeSettingsViewModel();
                vm.ProductTypeName = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
                vm.ProductTypeId = id;
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                ProductTypeSettingsViewModel temp = AutoMapper.Mapper.Map<ProductTypeSettings, ProductTypeSettingsViewModel>(settings);
                temp.ProductTypeName = new ProductTypeService(_unitOfWork).Find(id).ProductTypeName;
                PrepareViewBag(temp);
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(ProductTypeSettingsViewModel vm)
        {
            ProductTypeSettings pt = AutoMapper.Mapper.Map<ProductTypeSettingsViewModel, ProductTypeSettings>(vm);
            DocumentType DocType = new DocumentTypeService(_unitOfWork).Find(TransactionDoctypeConstants.ProductType);

            if (ModelState.IsValid)
            {

                if (vm.ProductTypeSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _ProductTypeSettingsService.Create(pt);

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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DocType.DocumentTypeId,
                        DocId = pt.ProductTypeSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("MaterialIndex", "Product", new { id = vm.ProductTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    ProductTypeSettings temp = _ProductTypeSettingsService.Find(pt.ProductTypeSettingsId);

                    ProductTypeSettings ExRec = Mapper.Map<ProductTypeSettings>(temp);


                    temp.UnitId = pt.UnitId;
                    temp.isShowMappedDimension1 = pt.isShowMappedDimension1;
                    temp.isShowUnMappedDimension1 = pt.isShowUnMappedDimension1;
                    temp.isApplicableDimension1 = pt.isApplicableDimension1;
                    temp.Dimension1Caption = pt.Dimension1Caption;
                    temp.isShowMappedDimension2 = pt.isShowMappedDimension2;
                    temp.isShowUnMappedDimension2 = pt.isShowUnMappedDimension2;
                    temp.isApplicableDimension2 = pt.isApplicableDimension2;
                    temp.Dimension2Caption = pt.Dimension2Caption;
                    temp.isShowMappedDimension3 = pt.isShowMappedDimension3;
                    temp.isShowUnMappedDimension3 = pt.isShowUnMappedDimension3;
                    temp.isApplicableDimension3 = pt.isApplicableDimension3;
                    temp.Dimension3Caption = pt.Dimension3Caption;
                    temp.isShowMappedDimension4 = pt.isShowMappedDimension4;
                    temp.isShowUnMappedDimension4 = pt.isShowUnMappedDimension4;
                    temp.isApplicableDimension4 = pt.isApplicableDimension4;
                    temp.Dimension4Caption = pt.Dimension4Caption;


                    temp.isVisibleProductDescription = pt.isVisibleProductDescription;
                    temp.isVisibleProductSpecification = pt.isVisibleProductSpecification;
                    temp.isVisibleProductCategory = pt.isVisibleProductCategory;
                    temp.isVisibleSalesTaxGroup = pt.isVisibleSalesTaxGroup;
                    temp.isVisibleSaleRate = pt.isVisibleSaleRate;
                    temp.isVisibleStandardCost = pt.isVisibleStandardCost;
                    temp.isVisibleTags = pt.isVisibleTags;
                    temp.isVisibleMinimumOrderQty = pt.isVisibleMinimumOrderQty;
                    temp.isVisibleReOrderLevel = pt.isVisibleReOrderLevel;
                    temp.isVisibleGodownId = pt.isVisibleGodownId;
                    temp.isVisibleBinLocationId = pt.isVisibleBinLocationId;
                    temp.isVisibleProfitMargin = pt.isVisibleProfitMargin;
                    temp.isVisibleCarryingCost = pt.isVisibleCarryingCost;
                    temp.isVisibleLotManagement = pt.isVisibleLotManagement;
                    temp.isVisibleConsumptionDetail = pt.isVisibleConsumptionDetail;
                    temp.isVisibleProductProcessDetail = pt.isVisibleProductProcessDetail;


                    temp.isVisibleDefaultDimension1 = pt.isVisibleDefaultDimension1;
                    temp.isVisibleDefaultDimension2 = pt.isVisibleDefaultDimension2;
                    temp.isVisibleDefaultDimension3 = pt.isVisibleDefaultDimension3;
                    temp.isVisibleDefaultDimension4 = pt.isVisibleDefaultDimension4;

                    temp.isVisibleDiscontinueDate = pt.isVisibleDiscontinueDate;
                    temp.isVisibleSalesTaxProductCode = pt.isVisibleSalesTaxProductCode;

                    temp.IndexFilterParameter = pt.IndexFilterParameter;
                    temp.ProductNameCaption = pt.ProductNameCaption;
                    temp.ProductCodeCaption = pt.ProductCodeCaption;
                    temp.ProductDescriptionCaption = pt.ProductDescriptionCaption;
                    temp.ProductSpecificationCaption = pt.ProductSpecificationCaption;
                    temp.ProductGroupCaption = pt.ProductGroupCaption;
                    temp.ProductCategoryCaption = pt.ProductCategoryCaption;
                    temp.SalesTaxProductCodeCaption = pt.SalesTaxProductCodeCaption;
                    temp.SqlProcProductCode = pt.SqlProcProductCode;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _ProductTypeSettingsService.Update(temp);

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

                    

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = DocType.DocumentTypeId,
                        DocId = temp.ProductTypeSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("MaterialIndex", "Product", new { id = vm.ProductTypeId }).Success("Data saved successfully");

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
