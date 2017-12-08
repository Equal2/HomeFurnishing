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
    public class CarpetSkuSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        ICarpetSkuSettingsService _CarpetSkuSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public CarpetSkuSettingsController(ICarpetSkuSettingsService CarpetSkuSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _CarpetSkuSettingsService = CarpetSkuSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag(CarpetSkuSettingsViewModel s)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            ViewBag.ProductSizeTypeList = (from p in db.ProductSizeType
                                       select p).ToList();

        }


        // GET: /CarpetSkuSettingMaster/Create

        public ActionResult Create()
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            var settings = new CarpetSkuSettingsService(_unitOfWork).GetCarpetSkuSettings(DivisionId, SiteId);

            if (settings == null)
            {
                CarpetSkuSettingsViewModel vm = new CarpetSkuSettingsViewModel();
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                PrepareViewBag(vm);
                return View("Create", vm);
            }
            else
            {
                CarpetSkuSettingsViewModel temp = AutoMapper.Mapper.Map<CarpetSkuSettings, CarpetSkuSettingsViewModel>(settings);
                PrepareViewBag(temp);
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(CarpetSkuSettingsViewModel vm)
        {
            CarpetSkuSettings pt = AutoMapper.Mapper.Map<CarpetSkuSettingsViewModel, CarpetSkuSettings>(vm);


            if (ModelState.IsValid)
            {

                if (vm.CarpetSkuSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _CarpetSkuSettingsService.Create(pt);

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
                        DocId = pt.CarpetSkuSettingsId,
                        DocTypeId = DocTypeId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "CarpetMaster", new { id = 0 }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    CarpetSkuSettings temp = _CarpetSkuSettingsService.Find(pt.CarpetSkuSettingsId);

                    CarpetSkuSettings ExRec = Mapper.Map<CarpetSkuSettings>(temp);

                    temp.isVisibleProductDesign = vm.isVisibleProductDesign;
                    temp.isVisibleProductStyle = vm.isVisibleProductStyle;
                    temp.isVisibleProductManufacturer = vm.isVisibleProductManufacturer;
                    temp.isVisibleProductDesignPattern = vm.isVisibleProductDesignPattern;
                    temp.isVisibleContent = vm.isVisibleContent;
                    temp.isVisibleOriginCountry = vm.isVisibleOriginCountry;
                    temp.isVisibleInvoiceGroup = vm.isVisibleInvoiceGroup;
                    temp.isVisibleDrawbackTarrif = vm.isVisibleDrawbackTarrif;
                    temp.isVisibleStandardCost = vm.isVisibleStandardCost;
                    temp.isVisibleStandardWeight = vm.isVisibleStandardWeight;
                    temp.isVisibleGrossWeight = vm.isVisibleGrossWeight;
                    temp.isVisibleSupplierDetail = vm.isVisibleSupplierDetail;
                    temp.isVisibleSample = vm.isVisibleSample;
                    temp.isVisibleCounterNo = vm.isVisibleCounterNo;
                    temp.isVisibleTags = vm.isVisibleTags;
                    temp.isVisibleDivision = vm.isVisibleDivision;
                    temp.isVisibleColour = vm.isVisibleColour;
                    temp.isVisibleProductionRemark = vm.isVisibleProductionRemark;
                    temp.ProductDesignId = pt.ProductDesignId;
                    temp.OriginCountryId = pt.OriginCountryId;
                    temp.UnitConversions = pt.UnitConversions;
                    temp.PerimeterSizeTypeId = pt.PerimeterSizeTypeId;
                    temp.isVisibleCBM = pt.isVisibleCBM;
                    temp.isVisibleMapScale = pt.isVisibleMapScale;
                    temp.isVisibleTraceType = pt.isVisibleTraceType;
                    temp.isVisibleMapType = pt.isVisibleMapType;
                    temp.isVisibleStencilSize = pt.isVisibleStencilSize;
                    temp.isVisibleSalesTaxProductCode = pt.isVisibleSalesTaxProductCode;
                    temp.SalesTaxProductCodeCaption = pt.SalesTaxProductCodeCaption;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _CarpetSkuSettingsService.Update(temp);

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
                        DocId = temp.CarpetSkuSettingsId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        DocTypeId = DocTypeId,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "CarpetMaster", new { id = 0 }).Success("Data saved successfully");

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
