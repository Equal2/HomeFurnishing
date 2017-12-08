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
using Model.ViewModel;
using Jobs.Helpers;

namespace Jobs.Controllers
{
    [Authorize]
    public class RateConversionSettingsController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        IRateConversionSettingsService _RateConversionSettingsService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public RateConversionSettingsController(IRateConversionSettingsService RateConversionSettingsService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _RateConversionSettingsService = RateConversionSettingsService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }


        public void PrepareViewBag()
        {
            ViewBag.UnitConversionForList = (from p in db.UnitConversonFor
                                             select p).ToList();
        }

        // GET: /RateConversionSettingsMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag();
            var settings = new RateConversionSettingsService(_unitOfWork).GetRateConversionSettingsForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                RateConversionSettingsViewModel vm = new RateConversionSettingsViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                RateConversionSettingsViewModel temp = AutoMapper.Mapper.Map<RateConversionSettings, RateConversionSettingsViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(RateConversionSettingsViewModel vm)
        {
            RateConversionSettings pt = AutoMapper.Mapper.Map<RateConversionSettingsViewModel, RateConversionSettings>(vm);

            if (ModelState.IsValid)
            {

                if (vm.RateConversionSettingsId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _RateConversionSettingsService.Create(pt);

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


                    return RedirectToAction("Index", "RateConversionHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {

                    RateConversionSettings temp = _RateConversionSettingsService.Find(pt.RateConversionSettingsId);                    
                    temp.filterContraDocTypes = pt.filterContraDocTypes;                    
                    temp.ProcessId = pt.ProcessId;
                    temp.filterProductGroups = pt.filterProductGroups;
                    temp.filterProducts = pt.filterProducts;
                    temp.filterProductTypes = pt.filterProductTypes;
                    temp.filterPersonRoles = pt.filterPersonRoles;
                    temp.isMandatoryCostCenter = pt.isMandatoryCostCenter;
                    temp.isMandatoryMachine = pt.isMandatoryMachine;
                    temp.isMandatoryProcessLine = pt.isMandatoryProcessLine;
                    temp.isVisibleCostCenter = pt.isVisibleCostCenter;
                    temp.isVisibleDimension1 = pt.isVisibleDimension1;
                    temp.isVisibleDimension2 = pt.isVisibleDimension2;
                    temp.isVisibleDimension3 = pt.isVisibleDimension3;
                    temp.isVisibleDimension4 = pt.isVisibleDimension4;
                    temp.isPostedInStockProcess = pt.isPostedInStockProcess;
                    temp.isVisibleLotNo = pt.isVisibleLotNo;
                    temp.isVisibleMachine = pt.isVisibleMachine;
                    temp.isVisibleProductUID = pt.isVisibleProductUID;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;


                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _RateConversionSettingsService.Update(temp);

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

                    return RedirectToAction("Index", "RateConversionHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

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
