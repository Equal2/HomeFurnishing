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
    public class PackingSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        List<string> UserRoles = new List<string>();
        ActiivtyLogViewModel LogVm = new ActiivtyLogViewModel();

        IPackingSettingService _PackingSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public PackingSettingController(IPackingSettingService PackingSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _PackingSettingService = PackingSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;

            UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];

            //Log Initialization
            LogVm.SessionId = 0;
            LogVm.ControllerName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");
            LogVm.ActionName = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
            LogVm.User = System.Web.HttpContext.Current.Request.RequestContext.HttpContext.User.Identity.Name;

        }

        private void PrepareViewBag()
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
                
            ViewBag.ShipMethodList = new ShipMethodService(_unitOfWork).GetShipMethodList().ToList();
            ViewBag.CurrencyList = new CurrencyService(_unitOfWork).GetCurrencyList().ToList();
            ViewBag.GodownList = new GodownService(_unitOfWork).GetGodownList(SiteId).ToList();


            ViewBag.UnitConvForList = (from p in db.UnitConversonFor
                                       select p).ToList();

            ViewBag.DealUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
            ViewBag.SalesTaxGroupList = new ChargeGroupPersonService(_unitOfWork).GetChargeGroupPersonList((int)(TaxTypeConstants.SalesTax)).ToList();
            ViewBag.DeliveryTermsList = new DeliveryTermsService(_unitOfWork).GetDeliveryTermsList().ToList();
        }


        // GET: /PackingSettingMaster/Create

        public ActionResult Create(int id)//DocTypeId
        {
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            //ViewBag.UnitConvForList = (from p in db.UnitConversonFor
            //                           select p).ToList();
            var settings = new PackingSettingService(_unitOfWork).GetPackingSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                PackingSettingsViewModel vm = new PackingSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                PrepareViewBag();
                return View("Create", vm);
            }
            else
            {
                PackingSettingsViewModel temp = AutoMapper.Mapper.Map<PackingSetting, PackingSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                PrepareViewBag();
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(PackingSettingsViewModel vm)
        {
            PackingSetting pt = AutoMapper.Mapper.Map<PackingSettingsViewModel, PackingSetting>(vm);


            if (ModelState.IsValid)
            {

                if (vm.PackingSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _PackingSettingService.Create(pt);

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

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = pt.DocTypeId,
                        DocId = pt.PackingSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsAdded,
                    }));



                    return RedirectToAction("Index", "PackingHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {
                    List<LogTypeViewModel> LogList = new List<LogTypeViewModel>();

                    PackingSetting temp = _PackingSettingService.Find(pt.PackingSettingId);

                    PackingSetting ExRec = Mapper.Map<PackingSetting>(temp);

                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterLedgerAccountGroups = pt.filterLedgerAccountGroups;
                    temp.filterLedgerAccounts = pt.filterLedgerAccounts;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterProductDivision = pt.filterProductDivision;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.ProcessId = pt.ProcessId;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isVisibleBaleCount = pt.isVisibleBaleCount;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.ImportMenuId = pt.ImportMenuId;
                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _PackingSettingService.Update(temp);

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
                        PrepareViewBag();
                        return View("Create", pt);
                    }

                    LogActivity.LogActivityDetail(LogVm.Map(new ActiivtyLogViewModel
                    {
                        DocTypeId = temp.DocTypeId,
                        DocId = temp.PackingSettingId,
                        ActivityType = (int)ActivityTypeContants.SettingsModified,
                        xEModifications = Modifications,
                    }));

                    return RedirectToAction("Index", "PackingHeader", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag();
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
