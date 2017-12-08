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
    public class LedgerSettingController : System.Web.Mvc.Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        ILedgerSettingService _LedgerSettingService;
        IUnitOfWork _unitOfWork;
        IExceptionHandlingService _exception;
        public LedgerSettingController(ILedgerSettingService LedgerSettingService, IUnitOfWork unitOfWork, IExceptionHandlingService exec)
        {
            _LedgerSettingService = LedgerSettingService;
            _unitOfWork = unitOfWork;
            _exception = exec;
        }


        public void PrepareViewBag(int id)
        {
            ViewBag.Name = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
            ViewBag.id = id;
            ViewBag.UnitConversionForList = (from p in db.UnitConversonFor
                                             select p).ToList();
            ViewBag.DeliveryUnitList = new UnitService(_unitOfWork).GetUnitList().ToList();
        }

        // GET: /LedgerSettingMaster/Create
        
        public ActionResult Create(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag(id);
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                LedgerSettingViewModel vm = new LedgerSettingViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("Create", vm);
            }
            else
            {
                LedgerSettingViewModel temp = AutoMapper.Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("Create", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(LedgerSettingViewModel vm)
        {
            LedgerSetting pt = AutoMapper.Mapper.Map<LedgerSettingViewModel, LedgerSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.LedgerSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _LedgerSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("Create", vm);
                    }


                    return RedirectToAction("Index", "LedgerHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");
                }
                else
                {

                    LedgerSetting temp = _LedgerSettingService.Find(pt.LedgerSettingId);
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterDocTypeCostCenter = pt.filterDocTypeCostCenter;
                    temp.filterLedgerAccountGroupHeaders = pt.filterLedgerAccountGroupHeaders;
                    temp.filterExcludeLedgerAccountGroupHeaders = pt.filterExcludeLedgerAccountGroupHeaders;
                    temp.filterLedgerAccountGroupLines = pt.filterLedgerAccountGroupLines;
                    temp.filterExcludeLedgerAccountGroupLines = pt.filterExcludeLedgerAccountGroupLines;
                    temp.isMandatoryChqNo = pt.isMandatoryChqNo;
                    temp.BaseRateText = pt.BaseRateText;
                    temp.BaseValueText = pt.BaseValueText;
                    temp.isMandatoryGodown = pt.isMandatoryGodown;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleProcess = pt.isVisibleProcess;
                    temp.isMandatoryProcess = pt.isMandatoryProcess;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.ProcessId = pt.ProcessId;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isMandatoryRefNo = pt.isMandatoryRefNo;
                    temp.SqlProcReferenceNo = pt.SqlProcReferenceNo;
                    temp.isVisibleChqNo = pt.isVisibleChqNo;
                    temp.WizardMenuId = pt.WizardMenuId;
                    temp.filterPersonProcessHeaders = pt.filterPersonProcessHeaders;
                    temp.filterPersonProcessLines = pt.filterPersonProcessLines;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isVisibleRefNo = pt.isVisibleRefNo;
                    temp.isVisibleProductUid = pt.isVisibleProductUid;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.isVisibleReferenceDocTypeId = pt.isVisibleReferenceDocTypeId;
                    temp.isVisibleDrCr = pt.isVisibleDrCr;
                    temp.isVisibleLineDrCr = pt.isVisibleLineDrCr;
                    temp.isVisibleAdjustmentType = pt.isVisibleAdjustmentType;
                    temp.isVisiblePaymentFor = pt.isVisiblePaymentFor;
                    temp.isVisiblePartyDocNo = pt.isVisiblePartyDocNo;
                    temp.isVisiblePartyDocDate = pt.isVisiblePartyDocDate;
                    temp.isVisibleLedgerAdj = pt.isVisibleLedgerAdj;
                    temp.IsAutoDocNo = pt.IsAutoDocNo;
                    temp.PartyDocNoCaption = pt.PartyDocNoCaption;
                    temp.PartyDocDateCaption = pt.PartyDocDateCaption;
                    temp.CancelDocTypeId = pt.CancelDocTypeId;
                    temp.filterReferenceDocTypes = pt.filterReferenceDocTypes;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;                    

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _LedgerSettingService.Update(temp);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("Create", pt);

                    }

                    return RedirectToAction("Index", "LedgerHeader", new { id=vm.DocTypeId}).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm.DocTypeId);
            return View("Create", vm);
        }


        public ActionResult CreateForCancel(int id)//DocTypeId
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            if (!UserRoles.Contains("SysAdmin"))
            {
                return View("~/Views/Shared/InValidSettings.cshtml");
            }

            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            PrepareViewBag(id);
            var settings = new LedgerSettingService(_unitOfWork).GetLedgerSettingForDocument(id, DivisionId, SiteId);

            if (settings == null)
            {
                LedgerSettingViewModel vm = new LedgerSettingViewModel();
                vm.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                vm.SiteId = SiteId;
                vm.DivisionId = DivisionId;
                vm.DocTypeId = id;
                return View("CreateForCancel", vm);
            }
            else
            {
                LedgerSettingViewModel temp = AutoMapper.Mapper.Map<LedgerSetting, LedgerSettingViewModel>(settings);
                temp.DocTypeName = new DocumentTypeService(_unitOfWork).Find(id).DocumentTypeName;
                return View("CreateForCancel", temp);
            }

        }

        // POST: /ProductMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostForCancel(LedgerSettingViewModel vm)
        {
            LedgerSetting pt = AutoMapper.Mapper.Map<LedgerSettingViewModel, LedgerSetting>(vm);

            if (ModelState.IsValid)
            {

                if (vm.LedgerSettingId <= 0)
                {
                    pt.CreatedDate = DateTime.Now;
                    pt.ModifiedDate = DateTime.Now;
                    pt.CreatedBy = User.Identity.Name;
                    pt.ModifiedBy = User.Identity.Name;
                    pt.ObjectState = Model.ObjectState.Added;
                    _LedgerSettingService.Create(pt);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("CreateForCancel", vm);
                    }


                    return RedirectToAction("PaymentCancelWizard", "PaymentCancelWizard", new { id = vm.DocTypeId }).Success("Data saved successfully");
                }
                else
                {

                    LedgerSetting temp = _LedgerSettingService.Find(pt.LedgerSettingId);
                    temp.filterContraDivisions = pt.filterContraDivisions;
                    temp.filterContraDocTypes = pt.filterContraDocTypes;
                    temp.filterContraSites = pt.filterContraSites;
                    temp.filterDocTypeCostCenter = pt.filterDocTypeCostCenter;
                    temp.filterLedgerAccountGroupHeaders = pt.filterLedgerAccountGroupHeaders;
                    temp.filterExcludeLedgerAccountGroupHeaders = pt.filterExcludeLedgerAccountGroupHeaders;
                    temp.filterLedgerAccountGroupLines = pt.filterLedgerAccountGroupLines;
                    temp.filterExcludeLedgerAccountGroupLines = pt.filterExcludeLedgerAccountGroupLines;
                    temp.isMandatoryChqNo = pt.isMandatoryChqNo;
                    temp.BaseRateText = pt.BaseRateText;
                    temp.BaseValueText = pt.BaseValueText;
                    temp.isMandatoryGodown = pt.isMandatoryGodown;
                    temp.isVisibleGodown = pt.isVisibleGodown;
                    temp.isVisibleProcess = pt.isVisibleProcess;
                    temp.isMandatoryProcess = pt.isMandatoryProcess;
                    temp.isMandatoryHeaderCostCenter = pt.isMandatoryHeaderCostCenter;
                    temp.isVisibleHeaderCostCenter = pt.isVisibleHeaderCostCenter;
                    temp.ProcessId = pt.ProcessId;
                    temp.isMandatoryLineCostCenter = pt.isMandatoryLineCostCenter;
                    temp.isMandatoryRefNo = pt.isMandatoryRefNo;
                    temp.SqlProcReferenceNo = pt.SqlProcReferenceNo;
                    temp.isVisibleChqNo = pt.isVisibleChqNo;
                    temp.filterPersonProcessHeaders = pt.filterPersonProcessHeaders;
                    temp.filterPersonProcessLines = pt.filterPersonProcessLines;
                    temp.isVisibleLineCostCenter = pt.isVisibleLineCostCenter;
                    temp.isVisibleRefNo = pt.isVisibleRefNo;
                    temp.isVisibleProductUid = pt.isVisibleProductUid;
                    temp.isVisibleReferenceDocId = pt.isVisibleReferenceDocId;
                    temp.isVisibleReferenceDocTypeId = pt.isVisibleReferenceDocTypeId;
                    temp.isVisibleDrCr = pt.isVisibleDrCr;
                    temp.isVisibleLineDrCr = pt.isVisibleLineDrCr;
                    temp.isVisibleAdjustmentType = pt.isVisibleAdjustmentType;
                    temp.isVisiblePaymentFor = pt.isVisiblePaymentFor;
                    temp.isVisiblePartyDocNo = pt.isVisiblePartyDocNo;
                    temp.isVisiblePartyDocDate = pt.isVisiblePartyDocDate;
                    temp.isVisibleLedgerAdj = pt.isVisibleLedgerAdj;
                    temp.IsAutoDocNo = pt.IsAutoDocNo;
                    temp.PartyDocNoCaption = pt.PartyDocNoCaption;
                    temp.PartyDocDateCaption = pt.PartyDocDateCaption;
                    temp.filterReferenceDocTypes = pt.filterReferenceDocTypes;
                    temp.CancelDocTypeId = pt.CancelDocTypeId;
                    temp.SqlProcDocumentPrint = pt.SqlProcDocumentPrint;
                    temp.SqlProcDocumentPrint_AfterApprove = pt.SqlProcDocumentPrint_AfterApprove;
                    temp.SqlProcDocumentPrint_AfterSubmit = pt.SqlProcDocumentPrint_AfterSubmit;

                    temp.ModifiedDate = DateTime.Now;
                    temp.ModifiedBy = User.Identity.Name;
                    temp.ObjectState = Model.ObjectState.Modified;
                    _LedgerSettingService.Update(temp);

                    try
                    {
                        _unitOfWork.Save();
                    }

                    catch (Exception ex)
                    {
                        string message = _exception.HandleException(ex);
                        ModelState.AddModelError("", message);
                        PrepareViewBag(vm.DocTypeId);
                        return View("CreateForCancel", pt);

                    }

                    return RedirectToAction("PaymentCancelWizard", "PaymentCancelWizard", new { id = vm.DocTypeId }).Success("Data saved successfully");

                }

            }
            PrepareViewBag(vm.DocTypeId);
            return View("CreateForCancel", vm);
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
